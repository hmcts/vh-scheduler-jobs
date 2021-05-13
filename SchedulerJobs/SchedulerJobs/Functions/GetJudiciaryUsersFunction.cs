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
        public Task RunAsync([TimerTrigger("0 0 2 * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            return Task.CompletedTask;
        }
    }
}