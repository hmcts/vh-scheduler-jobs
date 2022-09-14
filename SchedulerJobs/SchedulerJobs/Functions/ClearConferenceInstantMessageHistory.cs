using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace SchedulerJobs.Functions
{
    public class ClearConferenceInstantMessageHistory
    {
        private readonly IClearConferenceChatHistoryService _clearConferenceChatHistoryService;

        public ClearConferenceInstantMessageHistory(IClearConferenceChatHistoryService clearConferenceChatHistoryService)
        {
            _clearConferenceChatHistoryService = clearConferenceChatHistoryService;
        }

        [FunctionName("ClearConferenceInstantMessageHistory")]
        public async Task RunAsync([TimerTrigger("0 0 * * * *"
#if DEBUG
                , RunOnStartup=true
#endif
            ),] 
            TimerInfo myTimer, ILogger log
        )
        {
            if (myTimer?.IsPastDue ?? true)
            {
                log.LogInformation("Timer is running late!");
            }
            
            await _clearConferenceChatHistoryService.ClearChatHistoryForClosedConferences();
            log.LogInformation("Cleared chat history for closed conferences");

        }

        /// <summary>
        /// Function is cleaning Conference Instance Messages History
        /// </summary>
        /// <param name="log"></param>
        [FunctionName("ClearConferenceInstantMessageHistoryHttp")]
        public async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req, ILogger log)
        {
            await _clearConferenceChatHistoryService.ClearChatHistoryForClosedConferences();
            log.LogInformation("Cleared chat history for closed conferences");
        }
    }
}