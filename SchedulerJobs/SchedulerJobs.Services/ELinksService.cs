using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingsApi.Client;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Common.Models;
using SchedulerJobs.Services.HttpClients;

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
        private readonly ILogger _logger;

        public ELinksService(IELinksApiClient eLinksApiClient, IBookingsApiClient bookingsApiClient, ILogger logger)
        {
            _eLinksApiClient = eLinksApiClient;
            _bookingsApiClient = bookingsApiClient;
            _logger = logger;
        }

        public async Task ImportJudiciaryPeople(DateTime fromDate)
        {
            const int maxPages = 10;
            var currentPage = 1;
            IEnumerable<JudiciaryPersonModel> peopleResult;

            try
            {
                while (currentPage < maxPages)
                {
                    peopleResult = await _eLinksApiClient.GetPeopleAsync(fromDate);

                    if (peopleResult == null || !peopleResult.Any())
                    {
                        _logger.LogWarning($"{GetType().Name}: ImportJudiciaryPeople: No results from api for page: {currentPage}");
                        break;
                    }
                    
                    // peopleResult o BookingApi

                    currentPage++;
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