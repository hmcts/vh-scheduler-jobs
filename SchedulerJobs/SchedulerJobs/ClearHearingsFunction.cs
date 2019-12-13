using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace SchedulerJobs
{
    
    public static class ClearHearingsFunction
    {
        [FunctionName("ClearHearingsFunction")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer,
         ILogger log,
         [Inject]IBookingApiService bookingApiService
            )
        {
            if (myTimer.IsPastDue)
            {
                log.LogInformation("Timer is running late");
            }

            log.LogInformation($"Timer trigger function executed at: {DateTime.Now}");

            bookingApiService.ClearHearings();
        }
    }
}

