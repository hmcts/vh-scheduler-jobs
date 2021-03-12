using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;

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
            if (myTimer.IsPastDue)
            {
                log.LogInformation("Timer is running late!");
            }
            
            await _clearConferenceChatHistoryService.ClearChatHistoryForClosedConferences();
            log.LogInformation("Cleared chat history for closed conferences");

        }
    }
}