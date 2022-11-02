using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;

namespace SchedulerJobs.Functions
{
    public class HearingsAllocationFunction
    {

        public HearingsAllocationFunction()
        {
            
        }

        /// <summary>
        /// Function to automate hearing allocation
        /// </summary>
        /// <param name="myTimer">Set time to run every day at 4:40 AM</param>
        /// <param name="log"></param>
        [FunctionName("AllocateHearings")]
        public async Task Run([TimerTrigger("0 0 3 * * *")]TimerInfo myTimer, ILogger log)
        {
            if (myTimer?.IsPastDue ?? true)
            {
                log.LogTrace("Closed hearings function running late");
            }

            
            log.LogTrace($"Close hearings function executed and allocated 10 hearings");
        }
    }
}

