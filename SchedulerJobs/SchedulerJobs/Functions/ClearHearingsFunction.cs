using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace SchedulerJobs.Functions
{
    public static class ClearHearingsFunction
    {
        /// <summary>
        /// Function is cleaning video hearings
        /// </summary>
        /// <param name="myTimer">Set time to run every day at 4:40 AM</param>
        /// <param name="log"></param>
        /// <param name="closeConferenceService"></param>
        [FunctionName("ClearHearingsFunction")]
        public static async Task Run([TimerTrigger("0 0 23 * * *")]TimerInfo myTimer,
        ILogger log,
         [Inject]ICloseConferenceService closeConferenceService)
        {
            if (myTimer != null && myTimer.IsPastDue)
            {
                log.LogTrace("Closed hearings function running late");
            }

            var conferencesCount = await closeConferenceService.CloseConferencesAsync();
            log.LogTrace($"Close hearings function executed and  {conferencesCount} hearings closed");
        }
    }
}

