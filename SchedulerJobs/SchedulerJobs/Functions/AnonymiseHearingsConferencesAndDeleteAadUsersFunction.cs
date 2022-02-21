using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services;
using System.Threading.Tasks;
using Microsoft.FeatureManagement;

namespace SchedulerJobs.Functions
{
    public class AnonymiseHearingsConferencesAndDeleteAadUsersFunction
    {
        private readonly IAnonymiseHearingsConferencesDataService _anonymiseHearingsConferencesDataService;
        private readonly IAnonymiseHearingsConferencesWithSpecifiedDataService _anonymiseHearingsConferencesWithSpecifiedDataService;
        private readonly IFeatureManager _featureManager;
        public const string LogInformationMessage = "Data anonymised for hearings, conferences older than 3 months.";

        public AnonymiseHearingsConferencesAndDeleteAadUsersFunction(IAnonymiseHearingsConferencesDataService anonymiseHearingsConferencesDataService, IAnonymiseHearingsConferencesWithSpecifiedDataService anonymiseHearingsConferencesWithSpecifiedDataService, IFeatureManager featureManager)
        {
            _anonymiseHearingsConferencesDataService = anonymiseHearingsConferencesDataService;
            _anonymiseHearingsConferencesWithSpecifiedDataService = anonymiseHearingsConferencesWithSpecifiedDataService;
            _featureManager = featureManager;
        }

        /// <summary>
        /// Function to anonymise hearing and conference data older than three months
        /// </summary>
        /// <param name="myTimer">Set time to run every day at 5:30 AM</param>
        /// <param name="log"></param>
        [FunctionName("AnonymiseHearingsConferencesAndDeleteAadUsersFunction")]
        public async Task Run([TimerTrigger("0 30 5 * * *")]TimerInfo myTimer, ILogger log)
        {
            if (myTimer?.IsPastDue ?? true)
            {
                log.LogTrace("Anonymise data function running late");
            }

            if (await _featureManager.IsEnabledAsync(FeatureFlags
                    .EnableAnonymiseHearingsConferencesWithSpecifiedDataService))
            {
                await _anonymiseHearingsConferencesWithSpecifiedDataService.AnonymiseHearingsConferencesWithSpecifiedData().ConfigureAwait(false);
            }
            else
            {
                await _anonymiseHearingsConferencesDataService.AnonymiseHearingsConferencesDataAsync().ConfigureAwait(false);
            }
            log.LogInformation(LogInformationMessage);
        }
    }
}
