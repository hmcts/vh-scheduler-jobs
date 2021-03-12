using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;
using System.Threading.Tasks;

namespace SchedulerJobs.Functions
{
    public class RemoveHeartbeatsForConferencesFunction
    {
        private readonly IRemoveHeartbeatsForConferencesService _removeHeartbeatsForConferencesService;

        public RemoveHeartbeatsForConferencesFunction(IRemoveHeartbeatsForConferencesService removeHeartbeatsForConferencesService)
        {
            _removeHeartbeatsForConferencesService = removeHeartbeatsForConferencesService;
        }

        /// <summary>
        /// This function will delete heartbeat data for conferences that are older than 14 days.
        /// </summary>
        /// <param name="myTimer">Set time to run every day at 5:40 AM</param>
        /// <param name="log"></param>
        /// <param name="removeHeartbeatsForConferencesService"></param>
        [FunctionName("RemoveHeartbeatsForConferencesFunction")]
        public async Task Run([TimerTrigger("0 40 5 * * *")]TimerInfo myTimer, ILogger log)
        {
            if (myTimer?.IsPastDue ?? true)
            {
                log.LogTrace("Remove heartbeats for conferences function running late");
            }

            await _removeHeartbeatsForConferencesService.RemoveHeartbeatsForConferencesAsync().ConfigureAwait(false);
            log.LogInformation("Removed heartbeats for conferences older than 14 days.");
        }
    }
}
