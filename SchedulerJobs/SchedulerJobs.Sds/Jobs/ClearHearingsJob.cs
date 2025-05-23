using SchedulerJobs.Sds.Caching;
using SchedulerJobs.Services;
using SchedulerJobs.Common.Logging;

namespace SchedulerJobs.Sds.Jobs
{
    public class ClearHearingsJob : BaseJob
    {
        private readonly ILogger<ClearHearingsJob> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ClearHearingsJob(
            IHostApplicationLifetime lifetime,
            ILogger<ClearHearingsJob> logger,
            IServiceProvider serviceProvider,
            IDistributedJobRunningStatusCache distributedJobRunningStatusCache) : base(lifetime, logger, distributedJobRunningStatusCache)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var closeConferenceService = scope.ServiceProvider.GetRequiredService<ICloseConferenceService>();

            var conferencesCount = await closeConferenceService.CloseConferencesAsync();
            _logger.LogInformationCloseHearingsJobExecutedHearinsClosed(conferencesCount);
        }
    }   
}
