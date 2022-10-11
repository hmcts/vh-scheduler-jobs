using SchedulerJobs.Services;

namespace SchedulerJobs.Sds.Jobs
{
    public class AnonymiseHearingsConferencesAndDeleteAadUsersJob : BaseJob
    {
        public const string LogInformationMessage = "Data anonymised for hearings, conferences older than 3 months.";
        private readonly ILogger<AnonymiseHearingsConferencesAndDeleteAadUsersJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        private bool _jobSucceeded;
    
        public AnonymiseHearingsConferencesAndDeleteAadUsersJob(
            ILogger<AnonymiseHearingsConferencesAndDeleteAadUsersJob> logger,
            IHostApplicationLifetime lifetime,
            IServiceProvider serviceProvider) : base(lifetime, logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var anonymiseHearingsConferencesDataService = scope.ServiceProvider.GetRequiredService<IAnonymiseHearingsConferencesDataService>();
            var jobHistoryService = scope.ServiceProvider.GetRequiredService<IJobHistoryService>();
                
            try
            {
                await anonymiseHearingsConferencesDataService.AnonymiseHearingsConferencesDataAsync()
                    .ConfigureAwait(false);
                _jobSucceeded = true;
                _logger.LogInformation(LogInformationMessage);
            }
            catch (Exception)
            {
                _jobSucceeded = false;
                throw;
            }
            finally
            {
                await jobHistoryService.UpdateJobHistory(GetType().Name, _jobSucceeded);
            }
        }
    }
   
}