using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;

namespace SchedulerJobs.Functions
{
    public class GetJudiciaryUsersFunction
    {
        private readonly IELinksService _eLinksService;

        public GetJudiciaryUsersFunction(IELinksService eLinksService)
        {
            _eLinksService = eLinksService;
        }
        
        /// <summary>
        /// Get eJudiciary user from external system.
        /// 1) eLinks API
        /// </summary>
        /// <param name="myTimer"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("GetJudiciaryUsersFunction")]
        public async Task RunAsync([TimerTrigger("0 40 5 * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Started GetJudiciaryUsersFunction at: {DateTime.UtcNow}");

            try
            {
                await _eLinksService.ImportJudiciaryPeople(DateTime.UtcNow.AddDays(-1));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                log.LogError(ex, ex.Message);
                throw;
            }
            
        }
    }
}