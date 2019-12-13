using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Service;
using SchedulerJobs.Services;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace SchedulerJobs
{
    public static class ClearHearingsFunction
    {
        /// <summary>
        /// Function is cleaning video hearings
        /// </summary>
        /// <param name="myTimer">Set time to run every day at 9:30 AM</param>
        /// <param name="log"></param>
        /// <param name="videoApiService"></param>
        [FunctionName("ClearHearingsFunction")]
        //  public static void Run([TimerTrigger("0 30 9 * * *")]TimerInfo myTimer,
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer,

         ILogger log,
         [Inject]IVideoApiService videoApiService)
        {
            if (myTimer.IsPastDue)
            {
                log.LogInformation("Timer is running late");

                ApplicationLogger.Trace("Function with time trigger", "Timer", "Timer is running late");
            }

            log.LogInformation($"Timer trigger function executed at: {DateTime.Now}");

            ApplicationLogger.Trace("Function with time trigger", "Timer", $"Timer trigger function executed at: {DateTime.Now}");

            videoApiService.ClearHearings();
        }
    }
}

