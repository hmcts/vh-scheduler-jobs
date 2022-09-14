using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;

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

            if (myTimer?.IsPastDue ?? true)
            {
                log.LogTrace("Delete audiorecording applications function running late");
            }

            var audioFilesCount = await _closeConferenceService.DeleteAudiorecordingApplicationsAsync();
            log.LogTrace($"Delete audiorecording applications function executed for {audioFilesCount} conferences");
        }

        /// <summary>
        ///     Function to Delete Audio recording Applications
        /// </summary>
        /// <param name="log"></param>
        [FunctionName("DeleteAudiorecordingApplicationsFunctionHttp")]
        public async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req, ILogger log)
        {
            var audioFilesCount = await _closeConferenceService.DeleteAudiorecordingApplicationsAsync();
            log.LogTrace($"Delete audiorecording applications function executed for {audioFilesCount} conferences");
        }
    }
}
