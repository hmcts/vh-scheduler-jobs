using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;

namespace SchedulerJobs.Functions
{
    public class AnonymiseHearingsConferencesAndDeleteAadUsersFunction
    {
        public const string LogInformationMessage = "Data anonymised for hearings, conferences older than 3 months.";
        private readonly IAnonymiseHearingsConferencesDataService _anonymiseHearingsConferencesDataService;

        public AnonymiseHearingsConferencesAndDeleteAadUsersFunction(
            IAnonymiseHearingsConferencesDataService anonymiseHearingsConferencesDataService)
        {
            _anonymiseHearingsConferencesDataService = anonymiseHearingsConferencesDataService;
        }

        /// <summary>
        ///     Function to anonymise hearing and conference data older than three months
        /// </summary>
        /// <param name="myTimer">Set time to run every day at 5:30 AM</param>
        /// <param name="log"></param>
        [FunctionName("AnonymiseHearingsConferencesAndDeleteAadUsersFunction")]
        public async Task Run([TimerTrigger("0 30 5 * * *")] TimerInfo myTimer, ILogger log)
        {
            if (myTimer?.IsPastDue ?? true)
            {
                log.LogTrace("Anonymise data function running late");
            }

            await _anonymiseHearingsConferencesDataService.AnonymiseHearingsConferencesDataAsync(GetType().Name)
                .ConfigureAwait(false);

            log.LogInformation(LogInformationMessage);
        }
    }
}