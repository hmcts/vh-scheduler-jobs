using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;
using System.Threading.Tasks;

namespace SchedulerJobs.Functions
{
    public class AnonymiseHearingsConferencesAndDeleteAadUsersFunction
    {
        private readonly IAnonymiseHearingsConferencesDataService _anonymiseHearingsConferencesDataService;

        public AnonymiseHearingsConferencesAndDeleteAadUsersFunction(IAnonymiseHearingsConferencesDataService anonymiseHearingsConferencesDataService)
        {
            _anonymiseHearingsConferencesDataService = anonymiseHearingsConferencesDataService;
        }

        /// <summary>
        /// Function to anonymise hearing and conference data older than three months
        /// </summary>
        /// <param name="myTimer">Set time to run every day at 5:30 AM</param>
        /// <param name="log"></param>
        [FunctionName("AnonymiseHearingsConferencesAndDeleteAadUsersFunction")]
        public async Task Run([TimerTrigger("45 51 17 * * *")]TimerInfo myTimer, ILogger log)
        {
            if (myTimer?.IsPastDue ?? true)
            {
                log.LogTrace("Anonymise data function running late");
            }

            await _anonymiseHearingsConferencesDataService.AnonymiseHearingsConferencesDataAsync().ConfigureAwait(false);
            log.LogInformation("Data anonymised for hearings, conferences older than 3 months.");
        }
    }
}
