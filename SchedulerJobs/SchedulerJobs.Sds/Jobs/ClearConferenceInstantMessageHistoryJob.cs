using SchedulerJobs.Sds.Caching;
using SchedulerJobs.Services;
using SchedulerJobs.Common.Logging;

namespace SchedulerJobs.Sds.Jobs
{
    public class ClearConferenceInstantMessageHistoryJob : BaseJob
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public ClearConferenceInstantMessageHistoryJob(
            ILogger<ClearConferenceInstantMessageHistoryJob> logger,
            IHostApplicationLifetime lifetime,
            IServiceProvider serviceProvider,
            IDistributedJobRunningStatusCache distributedJobRunningStatusCache) : base(lifetime, logger, distributedJobRunningStatusCache)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var clearConferenceChatHistoryService = scope.ServiceProvider.GetRequiredService<IClearConferenceChatHistoryService>();

            await clearConferenceChatHistoryService.ClearChatHistoryForClosedConferences();
            _logger.LogInformationClearedChatHistoryForClosedConferences();
        }
    }
}