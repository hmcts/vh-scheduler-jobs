using SchedulerJobs.Services.Interfaces;

namespace SchedulerJobs.CronJobs.Jobs
{
    public class ReconcileHearingAudioWithStorageJob : BaseJob
    {
        private readonly ILogger<ReconcileHearingAudioWithStorageJob> _logger;
        private readonly IServiceProvider _serviceProvider;
    
        public ReconcileHearingAudioWithStorageJob(
            IHostApplicationLifetime lifetime,
            ILogger<ReconcileHearingAudioWithStorageJob> logger,
            IServiceProvider serviceProvider) : base(lifetime)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
    
        protected override async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var reconcileHearingAudioService = scope.ServiceProvider.GetRequiredService<IReconcileHearingAudioService>();
        
            await reconcileHearingAudioService.ReconcileAudiorecordingsWithConferencesAsync();

            _logger.LogTrace("Reconcile audio recording files with number of conferences for the day - Done");
        }
    }
   
}