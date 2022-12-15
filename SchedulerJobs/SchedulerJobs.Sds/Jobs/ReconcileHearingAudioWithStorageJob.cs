using Microsoft.Extensions.Options;
using SchedulerJobs.Common.Caching;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Services.Interfaces;

namespace SchedulerJobs.Sds.Jobs
{
    public class ReconcileHearingAudioWithStorageJob : BaseJob
    {
        private readonly ILogger<ReconcileHearingAudioWithStorageJob> _logger;
        private readonly IServiceProvider _serviceProvider;
    
        public ReconcileHearingAudioWithStorageJob(
            IHostApplicationLifetime lifetime,
            ILogger<ReconcileHearingAudioWithStorageJob> logger,
            IServiceProvider serviceProvider,
            IDistributedJobRunningStatusCache distributedJobRunningStatusCache,
            IOptions<ConnectionStrings> connectionStrings) : base(lifetime, logger, distributedJobRunningStatusCache, connectionStrings)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var reconcileHearingAudioService = scope.ServiceProvider.GetRequiredService<IReconcileHearingAudioService>();
        
            await reconcileHearingAudioService.ReconcileAudiorecordingsWithConferencesAsync();

            _logger.LogInformation("Reconcile audio recording files with number of conferences for the day - Done");
        }
    }
   
}