using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;
using System.Threading.Tasks;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace SchedulerJobs.Functions
{
    public static class DeleteAudiorecordingApplicationsFunction
    {
        [FunctionName("DeleteAudiorecordingApplicationsFunction")]
        public static async Task Run([TimerTrigger("0 0 2 * * *")]TimerInfo myTimer,
        ILogger log,
            [Inject]ICloseConferenceService closeConferenceService)
        {

            if (myTimer != null && myTimer.IsPastDue)
            {
                log.LogTrace("Delete audiorecording applications function running late");
            }

            var audioFilesCount = await closeConferenceService.DeleteAudiorecordingApplicationsAsync().ConfigureAwait(false);
            log.LogTrace($"Delete audiorecording applications function executed for {audioFilesCount} conferences");
        }
    }
}
