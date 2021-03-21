using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Services;

namespace SchedulerJobs.Functions
{
    public class GetJudiciaryUsersFunction
    {
        private readonly IELinksService _eLinksService;
        private readonly ServicesConfiguration _servicesConfiguration;

        public GetJudiciaryUsersFunction(IELinksService eLinksService, IOptions<ServicesConfiguration> servicesConfiguration)
        {
            _eLinksService = eLinksService;
            _servicesConfiguration = servicesConfiguration.Value;
        }
        
        /// <summary>
        /// Get eJudiciary user from external system.
        /// 1) eLinks API
        /// </summary>
        /// <param name="myTimer"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("GetJudiciaryUsersFunction")]
        public async Task RunAsync([TimerTrigger("0 40 5 * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            var updatedSince = DateTime.UtcNow.AddDays(-_servicesConfiguration.ELinksApiGetPeopleUpdatedSinceDays);
            
            log.LogInformation($"Started GetJudiciaryUsersFunction at: {DateTime.UtcNow} - param UpdatedSince: {updatedSince:yyyy-MM-dd}");

            try
            {
                await _eLinksService.ImportJudiciaryPeople(updatedSince);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"***** {ex.Message}");
                log.LogError(ex, ex.Message);
                throw;
            }
            
            log.LogInformation($"Finished GetJudiciaryUsersFunction at: {DateTime.UtcNow} - param UpdatedSince: {updatedSince:yyyy-MM-dd}");
        }
    }
}