using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BookingsApi.Client;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Common.Configuration;
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
        private readonly IAzureStorageService _service;
        private readonly IFeatureToggles _featureToggles;

        public ELinksService(IPeoplesClient peoplesClient, ILeaversClient leaversClient,
            IBookingsApiClient bookingsApiClient, ILogger<ELinksService> logger, IAzureStorageService service,
            IFeatureToggles featureToggles)
        {
            _peoplesClient = peoplesClient;
            _leaversClient = leaversClient;
            _bookingsApiClient = bookingsApiClient;
            _logger = logger;
            _service = service;
            _featureToggles = featureToggles;
        }

        public async Task ImportJudiciaryPeopleAsync(DateTime fromDate)
        {
            var currentPage = 1;
            var invalidPeoplePersonalCode = new List<string>();
            var morePages = false;
            int results = 0;
            StringBuilder combinedFile = new StringBuilder();
            
            _logger.LogInformation("ImportJudiciaryPeople: Removing all records from JudiciaryPersonsStaging");
            await _bookingsApiClient.RemoveAllJudiciaryPersonsStagingAsync();
            
            do
            {
                try
                {
                    
                    _logger.LogInformation("ImportJudiciaryPeople: Executing page {CurrentPage}", currentPage);
                    var peoples = await _peoplesClient.GetPeopleAsync(fromDate, currentPage);
                    
                    string fileName = $"page{currentPage}.json";
                    if (_featureToggles.StorePeopleIngestion())
                    {
                        var clientResponse = await _peoplesClient.GetPeopleJsonAsync(fromDate, currentPage);

                        combinedFile.Append(clientResponse);
                        
                        byte[] fileToBytes = Encoding.ASCII.GetBytes(clientResponse);
                
                        // delete all the history only on the first page so we can keep the following file in storage
                        if (currentPage == 1) await _service.ClearBlobs();
                        await _service.UploadFile(fileName, fileToBytes);
                        _logger.LogInformation(
                            "ImportJudiciaryPeople: Create people json file page-'{currentPage}'", currentPage);
                    }

                    morePages = peoples.Pagination.MorePages;
                    var pages = peoples.Pagination.Pages;
                    results = peoples.Pagination.Results;
                    
                    var peopleResult = peoples.Results
                        .Where(x => !string.IsNullOrEmpty(x.Id))
                        .ToList();
                    if (peopleResult.Count == 0)
                    {
                        _logger.LogWarning("ImportJudiciaryPeople: No results from api for page: {CurrentPage}",
                            currentPage);
                        break;
                    }

                    _logger.LogInformation("ImportJudiciaryPeople: Adding raw data to JudiciaryPersonStaging from page: {CurrentPage}, total records: {Records}", currentPage, peoples.Results.Count());
                    await _bookingsApiClient.BulkJudiciaryPersonsStagingAsync(
                        peoples.Results.Select(JudiciaryPersonStagingRequestMapper.MapTo));

                    var invalidPersonList = peoples.Results.Where(x => string.IsNullOrEmpty(x.Id)).ToList();
                    invalidPersonList.ForEach(x => invalidPeoplePersonalCode.Add(x.PersonalCode));

                    _logger.LogWarning(
                        "ImportJudiciaryPeople: No of people who are invalid '{Count}' in page '{CurrentPage}'. Pages: {Pages}", 
                        invalidPersonList.Count, currentPage, pages);
                    _logger.LogInformation(
                        "ImportJudiciaryPeople: Calling bookings API with '{Count}' people", peopleResult.Count);
                    var response =
                        await _bookingsApiClient.BulkJudiciaryPersonsAsync(
                            peopleResult.Select(JudiciaryPersonRequestMapper.MapTo));
                    response?.ErroredRequests.ForEach(x =>
                        _logger.LogError("ImportJudiciaryPeople: {ErrorResponseMessage}", x.Message));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "There was a problem importing judiciary people, on page {CurrentPage}", currentPage);
                }
                currentPage++;
                
            } while (morePages);
            
            // create a combined file for all pages
            if (combinedFile.Length > 0)
            {
                string fileName = "combined.json";
                byte[] fileToBytes = Encoding.ASCII.GetBytes(combinedFile.ToString());
                await _service.UploadFile(fileName, fileToBytes);
            }
            _logger.LogInformation("Number of pagination results: {Results}", results);
            _logger.LogWarning(
                $"ImportJudiciaryPeople: List of Personal code which are failed to insert '{string.Join(",", invalidPeoplePersonalCode)}'");
        }

        public async Task ImportLeaversJudiciaryPeopleAsync(DateTime fromDate)
        {
            var currentPage = 1;
            var morePages = false;
            do
            {
                try
                {
                    _logger.LogInformation("ImportJudiciaryLeavers: Executing page {CurrentPage}", currentPage);
                    var leavers = await _leaversClient.GetLeaversAsync(fromDate, currentPage);
                    morePages = leavers.Pagination.MorePages;
                    var leaversResult = leavers.Results
                        .Where(x => !string.IsNullOrEmpty(x.Id))
                        .ToList();

                    if (leaversResult.Count == 0)
                    {
                        _logger.LogWarning("ImportJudiciaryLeavers: No results from api for page: {CurrentPage}",
                            currentPage);
                        break;
                    }

                    var invalidCount = leavers.Results.Count(x => string.IsNullOrEmpty(x.Id));
                    _logger.LogWarning(
                        $"ImportJudiciaryLeavers: No of leavers who are invalid '{invalidCount}' in page '{currentPage}'.");
                    _logger.LogInformation(
                        $"ImportJudiciaryLeavers: Calling bookings API with '{leaversResult.Count}' leavers");

                    var response =
                        await _bookingsApiClient.BulkJudiciaryLeaversAsync(
                            leaversResult.Select(x => JudiciaryLeaverRequestMapper.MapTo(x)));
                    response?.ErroredRequests.ForEach(x =>
                        _logger.LogError("ImportJudiciaryLeavers: {ErrorResponseMessage}", x.Message));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"There was a problem importing judiciary leavers in page: '{currentPage}'");
                }
                currentPage++;
                
            } while (morePages);
        }
    }
}