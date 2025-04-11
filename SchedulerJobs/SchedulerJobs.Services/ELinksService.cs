using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingsApi.Client;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Common.ApiHelper;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Common.Models;
using SchedulerJobs.Services.HttpClients;
using SchedulerJobs.Services.Mappers;
using SchedulerJobs.Common.Logging;


namespace SchedulerJobs.Services
{
    public interface IELinksService
    {
        Task ImportJudiciaryPeopleAsync(DateTime fromDate);
        Task ImportLeaversJudiciaryPeopleAsync(DateTime fromDate);
        Task<DateTime> GetUpdatedSince();
    }

    public class ELinksService : IELinksService
    {
        private readonly IPeoplesClient _peoplesClient;
        private readonly ILeaversClient _leaversClient;
        private readonly IBookingsApiClient _bookingsApiClient;
        private readonly ILogger<ELinksService> _logger;
        private readonly IAzureStorageService _service;
        private readonly IFeatureToggles _featureToggles;
        private readonly IJobHistoryService _jobHistoryService;

        public ELinksService(IPeoplesClient peoplesClient, ILeaversClient leaversClient,
            IBookingsApiClient bookingsApiClient, ILogger<ELinksService> logger, IAzureStorageService service,
            IFeatureToggles featureToggles, IJobHistoryService jobHistoryService)
        {
            _peoplesClient = peoplesClient;
            _leaversClient = leaversClient;
            _bookingsApiClient = bookingsApiClient;
            _logger = logger;
            _service = service;
            _featureToggles = featureToggles;
            _jobHistoryService = jobHistoryService;
        }

        public async Task ImportJudiciaryPeopleAsync(DateTime fromDate)
        {
            var currentPage = 1;
            var invalidPeoplePersonalCode = new List<string>();
            var morePages = false;
            int results = 0;

            StringBuilder peopleListString = new StringBuilder();

            
            _logger.LogInformationImportJudiciaryPeopleRemoving();
            await _bookingsApiClient.RemoveAllJudiciaryPersonsStagingAsync();
            
            do
            {
                try
                {
                    
                    _logger.LogInformationImportJudiciaryPeopleExecutingPage(currentPage);
                    
                    var clientResponse = await _peoplesClient.GetPeopleJsonAsync(fromDate, currentPage);
                    
                    var peoples = ApiRequestHelper.Deserialise<PeopleResponse>(clientResponse);
                    
                    string fileName = $"page{currentPage}.json";
                    if (_featureToggles.StorePeopleIngestion())
                    {
                        peopleListString.Append(clientResponse);

                        byte[] fileToBytes = Encoding.ASCII.GetBytes(clientResponse);
                
                        // delete all the history only on the first page so we can keep the following file in storage
                        if (currentPage == 1) await _service.ClearBlobs();
                        await _service.UploadFile(fileName, fileToBytes);
                        _logger.LogInformationImportJudiciaryPeopleCreatePeopleJsonFile(currentPage);
                    }

                    morePages = peoples.Pagination.MorePages;
                    var pages = peoples.Pagination.Pages;
                    results = peoples.Pagination.Results;
                    
                    var peopleResult = peoples.Results
                        .Where(x => !string.IsNullOrEmpty(x.PersonalCode))
                        .ToList();
                    if (peopleResult.Count == 0)
                    {
                        _logger.LogWarningImportJudiciaryPeopleCountFromApi(currentPage);
                        break;
                    }

                    await BulkJudiciaryPersonStaging(peoples, invalidPeoplePersonalCode, currentPage, pages);
                    
                    _logger.LogInformationImportJudiciaryPeopleCallingApiWith(peopleResult.Count);
                    var response =
                        await _bookingsApiClient.BulkJudiciaryPersonsAsync(
                            peopleResult.Select(JudiciaryPersonRequestMapper.MapTo));
                    response?.ErroredRequests.ForEach(x =>
                        _logger.LogErrorImportJudiciaryPeopleMessage(x.Message));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "There was a problem importing judiciary people, on page {CurrentPage}", currentPage);
                }
                currentPage++;
                
            } while (morePages);
            
            // create a combined file for all pages

            if (peopleListString.Length > 0)
            {
                string fileName = "combined.json";
                byte[] fileToBytes = Encoding.ASCII.GetBytes(peopleListString.ToString());
                await _service.UploadFile(fileName, fileToBytes);
            }
            _logger.LogInformationNumberOfPaginationResults(results);
            _logger.LogWarningImportJudiciaryPeopleListOfPersonalCode(string.Join(",", invalidPeoplePersonalCode));
        }
        private async Task BulkJudiciaryPersonStaging(PeopleResponse peoples, List<string> invalidPeoplePersonalCode, int currentPage, int pages)
        {
            _logger.LogInformationImportJudiciaryAddingToJudiciaryPersonStaging(currentPage, peoples.Results.Count());
            await _bookingsApiClient.BulkJudiciaryPersonsStagingAsync(
                peoples.Results.Select(JudiciaryPersonStagingRequestMapper.MapTo));

            var invalidPersonList = peoples.Results.Where(x => string.IsNullOrEmpty(x.PersonalCode)).ToList();
            invalidPersonList.ForEach(x => invalidPeoplePersonalCode.Add(x.PersonalCode));
            _logger.LogWarningImportJudiciaryPeopleInvalidPeopleCount(invalidPersonList.Count, currentPage, pages);
        }

        public async Task ImportLeaversJudiciaryPeopleAsync(DateTime fromDate)
        {
            var currentPage = 1;
            var morePages = false;
            do
            {
                try
                {
                    _logger.LogInformationImportJudiciaryPeopleExecutingPage(currentPage);
                    var leavers = await _leaversClient.GetLeaversAsync(fromDate, currentPage);
                    morePages = leavers.Pagination.MorePages;
                    var leaversResult = leavers.Results
                        .Where(x => !string.IsNullOrEmpty(x.Id))
                        .ToList();

                    if (leaversResult.Count == 0)
                    {
                        _logger.LogWarningImportJudiciaryPeopleCountFromApi(currentPage);
                        break;
                    }

                    var invalidCount = leavers.Results.Count(x => string.IsNullOrEmpty(x.Id));
                    _logger.LogWarningImportJudiciaryPeopleInvalidPeopleCount(invalidCount, currentPage);
                    _logger.LogInformationImportJudiciaryLeaversCallingApi(leaversResult.Count);

                    var response =
                        await _bookingsApiClient.BulkJudiciaryLeaversAsync(
                            leaversResult.Select(x => JudiciaryLeaverRequestMapper.MapTo(x)));
                    response?.ErroredRequests.ForEach(x =>
                        _logger.LogErrorImportJudiciaryLeaversMessage(x.Message));
                }
                catch (Exception ex)
                {
                    _logger.LogErrorImportJudiciaryLeaversException(ex, currentPage);
                }
                currentPage++;
                
            } while (morePages);
        }
        
        public async Task<DateTime> GetUpdatedSince()
        {
            if (_featureToggles.ImportAllJudiciaryUsersToggle())
                return DateTime.MinValue;

            var lastRun = await _jobHistoryService.GetMostRecentSuccessfulRunDate(GetType().Name);
            return lastRun ?? DateTime.UtcNow.AddDays(-1);
        }
    }
}