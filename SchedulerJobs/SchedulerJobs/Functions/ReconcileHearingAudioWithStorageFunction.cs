using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services.Interfaces;
using System.Threading.Tasks;

namespace SchedulerJobs.Functions
{
    public class ReconcileHearingAudioWithStorageFunction
    {
        private readonly IReconcileHearingAudioService _reconcileHearingAudioService;

        public ReconcileHearingAudioWithStorageFunction(IReconcileHearingAudioService reconcileHearingAudioService)
        {
            _reconcileHearingAudioService = reconcileHearingAudioService;
        }

        /// <summary>
        /// Function is reconcile audio files in wowza with conferences (InSession) for a date 
        /// </summary>
        /// <param name="myTimer">Set time to run every day at 10:00 PM</param>
        /// <param name="log"></param>
        /// //public async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log) for local
        /// 
        [FunctionName("ReconcileHearingAudioWithStorageFunction")]
        public async Task RunAsync([TimerTrigger("0 0 22 * * *")] TimerInfo myTimer, ILogger log)
        {

            if (myTimer?.IsPastDue ?? true)
            {
                log.LogTrace("Reconcile audio recording files with number of conferences for the day");
            }

            await _reconcileHearingAudioService.ReconcileAudiorecordingsWithConferencesAsync();

            log.LogTrace("Reconcile audio recording files with number of conferences for the day - Done");
        }
    }
}
