using System;
using System.Linq;
using System.Threading.Tasks;
using BookingsApi.Client;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services.HttpClients;
using SchedulerJobs.Services.Mappers;

namespace SchedulerJobs.Services
{
    public interface IELinksService
    {
        Task ImportJudiciaryPeople(DateTime fromDate);
    }
    
    public class ELinksService : IELinksService
    {
        private readonly IELinksApiClient _eLinksApiClient;
        private readonly IBookingsApiClient _bookingsApiClient;
        private readonly ILogger<ELinksService> _logger;

        public ELinksService(IELinksApiClient eLinksApiClient, IBookingsApiClient bookingsApiClient, ILogger<ELinksService> logger)
        {
            _eLinksApiClient = eLinksApiClient;
            _bookingsApiClient = bookingsApiClient;
            _logger = logger;
        }

        public async Task ImportJudiciaryPeople(DateTime fromDate)
        {
            var currentPage = 1;

            try
            {
                while (true)
                {
                    _logger.LogInformation($"{GetType().Name}: ImportJudiciaryPeople: executing page {currentPage}");
                    
                    var peopleResult = (await _eLinksApiClient.GetPeopleAsync(fromDate, currentPage))
                        .Where(x => x.Id.HasValue)
                        .ToList();

                    if (!peopleResult.Any())
                    {
                        _logger.LogWarning($"{GetType().Name}: ImportJudiciaryPeople: No results from api for page: {currentPage}");
                        break;
                    }
                    
                    var response = await _bookingsApiClient.BulkJudiciaryPersonsAsync(peopleResult.Select(JudiciaryPersonRequestMapper.MapTo));

                    if (response != null)
                    {
                        foreach (var errorResponse in response.ErroredRequests)
                        {
                            _logger.LogError($"{GetType().Name}: ImportJudiciaryPeople: {errorResponse.Message}");
                        }
                    }
                    
                    currentPage++;
                    await Task.Delay(250);
                }  
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }   
        }
    }
}