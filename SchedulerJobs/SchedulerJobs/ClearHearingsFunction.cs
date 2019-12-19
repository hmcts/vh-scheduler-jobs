using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace SchedulerJobs
{
    public static class ClearHearingsFunction
    {
        /// <summary>
        /// Function is cleaning video hearings
        /// </summary>
        /// <param name="myTimer">Set time to run every day at 5:30 AM</param>
        /// <param name="closeConferenceService"></param>
        [FunctionName("ClearHearingsFunction")]
         public static async Task Run([TimerTrigger("0 30 5 * * *")]TimerInfo myTimer,
         ILogger log,
         [Inject]ICloseConferenceService closeConferenceService)
        {
            if (myTimer != null && myTimer.IsPastDue)
            {
                 log.LogTrace("Timer is running late");
            }

            var fromDate = DateTime.UtcNow;
            log.LogTrace($"Timer trigger function executed at: {fromDate}");

            await closeConferenceService.CloseConferencesAsync(fromDate).ConfigureAwait(false);
        }
    }
}

