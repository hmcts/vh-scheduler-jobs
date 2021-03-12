using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;
using System.Threading.Tasks;

namespace SchedulerJobs.Functions
{
    public class DeleteAudiorecordingApplicationsFunction
    {
        private readonly ICloseConferenceService _closeConferenceService;

        public DeleteAudiorecordingApplicationsFunction(ICloseConferenceService closeConferenceService)
        {
            _closeConferenceService = closeConferenceService;
        }

        [FunctionName("DeleteAudiorecordingApplicationsFunction")]
        public async Task Run([TimerTrigger("0 0 22 * * *")]TimerInfo myTimer, ILogger log)
        {

            if (myTimer != null && myTimer.IsPastDue)
            {
                log.LogTrace("Delete audiorecording applications function running late");
            }

            var audioFilesCount = await _closeConferenceService.DeleteAudiorecordingApplicationsAsync().ConfigureAwait(false);
            log.LogTrace($"Delete audiorecording applications function executed for {audioFilesCount} conferences");
        }
    }
}
