using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace SchedulerJobs.Functions
{
    public static class ClearConferenceInstantMessageHistory
    {
        [FunctionName("ClearConferenceInstantMessageHistory")]
        public static async Task RunAsync([TimerTrigger("0 0 * * * *"
#if DEBUG
                , RunOnStartup=true
#endif
            ),] 
            TimerInfo myTimer, 
            ILogger log,
            [Inject]IClearConferenceChatHistoryService clearConferenceChatHistoryService
        )
        {
            if (myTimer.IsPastDue)
            {
                log.LogInformation("Timer is running late!");
            }
            
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");
            await clearConferenceChatHistoryService.ClearChatHistoryForClosedConferences();
            log.LogInformation("Cleared chat history for closed conferences");

        }
    }
}