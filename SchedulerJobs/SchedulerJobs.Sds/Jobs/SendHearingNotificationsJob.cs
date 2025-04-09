using SchedulerJobs.Sds.Caching;
using SchedulerJobs.Services.Interfaces;
using SchedulerJobs.Common.Logging;

namespace SchedulerJobs.Sds.Jobs
{
    public class SendHearingNotificationsJob : BaseJob
    {
        private readonly ILogger<SendHearingNotificationsJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        
        public SendHearingNotificationsJob(
            IHostApplicationLifetime lifetime,
            ILogger<SendHearingNotificationsJob> logger,
            IServiceProvider serviceProvider,
            IDistributedJobRunningStatusCache distributedJobRunningStatusCache) : base(lifetime, logger, distributedJobRunningStatusCache)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var hearingNotificationService = scope.ServiceProvider.GetRequiredService<IHearingNotificationService>();
            
            await hearingNotificationService.SendNotificationsAsync();

            _logger.LogInformationSendHearingNotifications(DateTime.UtcNow);
        }
    }
}
