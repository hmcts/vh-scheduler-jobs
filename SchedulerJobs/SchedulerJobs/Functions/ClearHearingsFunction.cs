using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;

namespace SchedulerJobs.Functions
{
    public class ClearHearingsFunction
    {
        private readonly ICloseConferenceService _closeConferenceService;

        public ClearHearingsFunction(ICloseConferenceService closeConferenceService)
        {
            _closeConferenceService = closeConferenceService;
        }

        /// <summary>
        /// Function is cleaning video hearings
        /// </summary>
        /// <param name="myTimer">Set time to run every day at 4:40 AM</param>
        /// <param name="log"></param>
        [FunctionName("ClearHearingsFunction")]
        public async Task Run([TimerTrigger("0 0 23 * * *")]TimerInfo myTimer, ILogger log)
        {
            if (myTimer != null && myTimer.IsPastDue)
            {
                log.LogTrace("Closed hearings function running late");
            }

            var conferencesCount = await _closeConferenceService.CloseConferencesAsync();
            log.LogTrace($"Close hearings function executed and  {conferencesCount} hearings closed");
        }
    }
}

