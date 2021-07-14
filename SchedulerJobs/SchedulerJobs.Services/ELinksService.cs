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
        private readonly IPeoplesClient _peoplesClient;
        private readonly ILeaversClient _leaversClient;
        private readonly IBookingsApiClient _bookingsApiClient;
        private readonly ILogger<ELinksService> _logger;

        public ELinksService(IPeoplesClient peoplesClient, ILeaversClient leaversClient, IBookingsApiClient bookingsApiClient, ILogger<ELinksService> logger)
        {
            _peoplesClient = peoplesClient;
            _leaversClient = leaversClient;
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
                    _logger.LogInformation("ImportJudiciaryPeople: Executing page {CurrentPage}", currentPage);
                    var people = await _peoplesClient.GetPeopleAsync(fromDate, currentPage);
                    var peopleResult = people
                        .Where(x => x.Id.HasValue)
                        .ToList();

                    if (peopleResult.Count == 0)
                    {
                        _logger.LogWarning("ImportJudiciaryPeople: No results from api for page: {CurrentPage}", currentPage);
                        break;
                    }
                    var invalidCount = people.Count(x => !x.Id.HasValue);
                    _logger.LogWarning($"ImportJudiciaryPeople: No of people who are invalid '{invalidCount}' in page '{currentPage}'.");
                    _logger.LogInformation($"ImportJudiciaryPeople: Calling bookings API with '{peopleResult.Count}' people");
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
                    _logger.LogInformation("ImportJudiciaryLeavers: Executing page {CurrentPage}", currentPage);
                    var leavers = await _leaversClient.GetLeaversAsync(fromDate, currentPage);
                    var leaversResult = leavers
                        .Where(x => !string.IsNullOrEmpty(x.Id))
                        .ToList();

                    if (leaversResult.Count == 0)
                    {
                        _logger.LogWarning("ImportJudiciaryLeavers: No results from api for page: {CurrentPage}", currentPage);
                        break;
                    }

                    var invalidCount = leavers.Count(x => string.IsNullOrEmpty(x.Id));
                    _logger.LogWarning($"ImportJudiciaryLeavers: No of leavers who are invalid '{invalidCount}' in page '{currentPage}'.");
                    _logger.LogInformation($"ImportJudiciaryLeavers: Calling bookings API with '{leaversResult.Count}' leavers");

                    var response = await _bookingsApiClient.BulkJudiciaryLeaversAsync(leaversResult.Select(x =>  JudiciaryLeaverRequestMapper.MapTo(x)));
                    response?.ErroredRequests.ForEach(x => _logger.LogError("ImportJudiciaryLeavers: {ErrorResponseMessage}", x.Message));

                    currentPage++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was a problem importing judiciary leavers");
                throw;
            }
        }
    }
}