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
        /// <param name="myTimer">Set time to run every day at 9:30 AM</param>
        /// <param name="closeConferenceService"></param>
        [FunctionName("ClearHearingsFunction")]
        //  public static void Run([TimerTrigger("0 30 9 * * *")]TimerInfo myTimer,
        public static async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer,
         ILogger log,
         [Inject]ICloseConferenceService closeConferenceService)
        {
            if (myTimer.IsPastDue)
            {
                 log.LogInformation("Timer is running late");
            }

            log.LogInformation($"Timer trigger function executed at: {DateTime.Now}");

            await closeConferenceService.CloseConferencesAsync(DateTime.UtcNow).ConfigureAwait(false);
        }
    }
}

