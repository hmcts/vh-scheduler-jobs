using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace SchedulerJobs.Functions
{
    public class GetJudiciaryUsersFunction
    {
        private readonly IELinksService _eLinksService;
        private readonly IJobHistoryService _jobHistoryService;
        private bool jobSucceeded;
        public GetJudiciaryUsersFunction(IELinksService eLinksService, IJobHistoryService jobHistoryService)
        {
            _eLinksService = eLinksService;
            _jobHistoryService = jobHistoryService;
        }
        
        /// <summary>
        /// Get eJudiciary user from external system.
        /// 1) eLinks API
        /// </summary>
        /// <param name="myTimer"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("GetJudiciaryUsersFunction")]
        public async Task RunAsync([TimerTrigger("0 0 2 * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            var lastRun = await _jobHistoryService.GetMostRecentSuccessfulRunDate(GetType().Name);
            var updatedSince = lastRun ?? DateTime.UtcNow.AddDays(-1);
            log.LogInformation("Started GetJudiciaryUsersFunction at: {Now} - param UpdatedSince: {UpdatedSince}",
            DateTime.UtcNow, updatedSince.ToString("yyyy-MM-dd"));

            try
            {
                await _eLinksService.ImportJudiciaryPeopleAsync(updatedSince);
                await _eLinksService.ImportLeaversJudiciaryPeopleAsync(updatedSince);
                jobSucceeded = true;
            }
            catch (Exception ex)
            {
                jobSucceeded = false;
                log.LogError(ex, ex.Message);
                throw;
            }
            finally
            {
                await _jobHistoryService.UpdateJobHistory(GetType().Name, jobSucceeded);
            }
            log.LogInformation("Finished GetJudiciaryUsersFunction at: {Now} - param UpdatedSince: {UpdatedSince}",
                DateTime.UtcNow, updatedSince.ToString("yyyy-MM-dd"));
        }

        /// <summary>
        /// Get eJudiciary user from external system.
        /// 1) eLinks API
        /// </summary>
        /// <param name="log"></param>
        [FunctionName("GetJudiciaryUsersFunctionHttp")]
        public async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req, ILogger log)
        {
            var lastRun = await _jobHistoryService.GetMostRecentSuccessfulRunDate(GetType().Name);
            var updatedSince = lastRun ?? DateTime.UtcNow.AddDays(-1);
            log.LogInformation("Started GetJudiciaryUsersFunction at: {Now} - param UpdatedSince: {UpdatedSince}",
            DateTime.UtcNow, updatedSince.ToString("yyyy-MM-dd"));

            try
            {
                await _eLinksService.ImportJudiciaryPeopleAsync(updatedSince);
                await _eLinksService.ImportLeaversJudiciaryPeopleAsync(updatedSince);
                jobSucceeded = true;
            }
            catch (Exception ex)
            {
                jobSucceeded = false;
                log.LogError(ex, ex.Message);
                throw;
            }
            finally
            {
                await _jobHistoryService.UpdateJobHistory(GetType().Name, jobSucceeded);
            }
            log.LogInformation("Finished GetJudiciaryUsersFunction at: {Now} - param UpdatedSince: {UpdatedSince}",
                DateTime.UtcNow, updatedSince.ToString("yyyy-MM-dd"));
        }
    }
}