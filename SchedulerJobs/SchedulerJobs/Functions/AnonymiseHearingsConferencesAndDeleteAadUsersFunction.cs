using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;
using System;
using System.Threading.Tasks;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace SchedulerJobs.Functions
{
    public static class AnonymiseHearingsConferencesAndDeleteAadUsersFunction
    {
        /// <summary>
        /// Function to anonymise hearing and conference data older than three months
        /// </summary>
        /// <param name="myTimer">Set time to run every day at 1:10 AM</param>
        /// <param name="log"></param>
        /// <param name="anonymiseHearingsConferencesDataService"></param>
        [FunctionName("AnonymiseHearingsConferencesAndDeleteAadUsersFunction")]
        public static async Task Run([TimerTrigger("* */5 * * * *")]TimerInfo myTimer, 
            ILogger log, 
            [Inject] IAnonymiseHearingsConferencesDataService anonymiseHearingsConferencesDataService)
        {
            if (myTimer != null && myTimer.IsPastDue)
            {
                log.LogTrace("Anonymise data function running late");
            }
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");
            await anonymiseHearingsConferencesDataService.AnonymiseHearingsConferencesDataAsync().ConfigureAwait(false);
            log.LogInformation("Data anonymised for hearings, conferences older than 3 months.");
        }
    }
}
