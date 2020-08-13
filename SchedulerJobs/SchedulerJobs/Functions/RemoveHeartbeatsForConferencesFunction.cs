using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;
using System;
using System.Threading.Tasks;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace SchedulerJobs.Functions
{
    public static class RemoveHeartbeatsForConferencesFunction
    {
        /// <summary>
        /// This function will delete heartbeat data for conferences that are older than 14 days.
        /// </summary>
        /// <param name="myTimer">Set time to run every day at 5:40 AM</param>
        /// <param name="log"></param>
        /// <param name="removeHeartbeatsForConferencesService"></param>
        [FunctionName("RemoveHeartbeatsForConferencesFunction")]
        public static async Task Run([TimerTrigger("0 40 5 * * *")]TimerInfo myTimer, 
            ILogger log, 
            [Inject] IRemoveHeartbeatsForConferencesService removeHeartbeatsForConferencesService)
        {
            if (myTimer != null && myTimer.IsPastDue)
            {
                log.LogTrace("Remove heartbeats for conferences function running late");
            }
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");
            await removeHeartbeatsForConferencesService.RemoveHeartbeatsForConferencesAsync().ConfigureAwait(false);
            log.LogInformation("Removed heartbeats for conferences older than 14 days.");
        }
    }
}
