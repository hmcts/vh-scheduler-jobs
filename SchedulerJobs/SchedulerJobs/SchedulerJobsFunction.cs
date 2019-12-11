using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace SchedulerJobs
{
    public static class SchedulerJobsFunction
    {
        [FunctionName("SchedulerJobsFunction")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
