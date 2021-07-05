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
        Task ImportJudiciaryPeopleAsync(DateTime fromDate);
        Task ImportLeaversJudiciaryPeopleAsync(DateTime fromDate);
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

        public async Task ImportJudiciaryPeopleAsync(DateTime fromDate)
        {
            var currentPage = 1;

            try
            {
                while (true)
                {
                    _logger.LogInformation("ImportJudiciaryPeople: executing page {CurrentPage}", currentPage);
                    
                    var peopleResult = (await _eLinksApiClient.GetPeopleAsync(fromDate, currentPage))
                        .Where(x => x.Id.HasValue)
                        .ToList();

                    if (!peopleResult.Any())
                    {
                        _logger.LogWarning("ImportJudiciaryPeople: No results from api for page: {CurrentPage}", currentPage);
                        break;
                    }
                    
                    _logger.LogInformation("ImportJudiciaryPeople: Calling bookings API with {PeopleResultCount} people", peopleResult.Count);
                    var response = await _bookingsApiClient.BulkJudiciaryPersonsAsync(peopleResult.Select(JudiciaryPersonRequestMapper.MapTo));
                    response?.ErroredRequests.ForEach(x => _logger.LogError("ImportJudiciaryPeople: {ErrorResponseMessage}", x.Message));

                    currentPage++;
                }  
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was a problem importing judiciary people");
                throw;
            }   
        }

        public async Task ImportLeaversJudiciaryPeopleAsync(DateTime fromDate)
        {
            var currentPage = 1;

            try
            {
                while (true)
                {
                    _logger.LogInformation("ImportJudiciaryLeavers: executing page {CurrentPage}", currentPage);

                    var leaversResult = (await _eLinksApiClient.GetLeaversAsync(fromDate, currentPage))
                        .Where(x => x.Id.HasValue)
                        .ToList();

                    if (!leaversResult.Any())
                    {
                        _logger.LogWarning("ImportJudiciaryLeavers: No results from api for page: {CurrentPage}", currentPage);
                        break;
                    }

                    _logger.LogInformation("ImportJudiciaryLeavers: Calling bookings API with {LeaversResultCount} people", leaversResult.Count);
                    var response = await _bookingsApiClient.BulkJudiciaryPersonsAsync(leaversResult.Select(x =>
                    {
                        x.HasLeft = true;
                        return JudiciaryPersonRequestMapper.MapTo(x);
                    }));
                    response?.ErroredRequests.ForEach(x => _logger.LogError("ImportJudiciaryPeople: {ErrorResponseMessage}", x.Message));

                    currentPage++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was a problem importing judiciary people leavers");
                throw;
            }
        }
    }
}