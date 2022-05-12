using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services.Interfaces;

namespace SchedulerJobs.Functions
{
    public  class SendHearingNotificationsFunction
    {

        private readonly IHearingNotificationService _hearingNotificationService;

        public SendHearingNotificationsFunction(IHearingNotificationService hearingNotificationService)
        {
            _hearingNotificationService = hearingNotificationService;
        }

        /// <summary>
        /// Function is send notifications for hearing in next 48 to 72 hrs  
        /// </summary>
        /// <param name="myTimer">Set time to run every day at 10:00 AM</param>
        /// <param name="log"></param>
        /// //public async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)  // for local
        /// 
        [FunctionName("SendHearingNotificationsFunction")]
        public async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)
        //public async Task RunAsync([TimerTrigger("0 0 10 * * *")] TimerInfo myTimer, ILogger log)
        {
            /*
            if (myTimer?.IsPastDue ?? true)
            {
                log.LogInformation($"Send hearing notifications function triggered at: {DateTime.Now} ");
            }*/
                        
            await _hearingNotificationService.SendNotificationsAsync();

            log.LogInformation($"Send hearing notifications - Completed at:{DateTime.Now} ");
        }
    }
}
