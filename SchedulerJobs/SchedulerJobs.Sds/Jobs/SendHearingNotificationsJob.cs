using SchedulerJobs.Services.Interfaces;

namespace SchedulerJobs.Sds.Jobs
{
    public class SendHearingNotificationsJob : BaseJob
    {
        private readonly ILogger<SendHearingNotificationsJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        
        public SendHearingNotificationsJob(
            IHostApplicationLifetime lifetime,
            ILogger<SendHearingNotificationsJob> logger,
            IServiceProvider serviceProvider) : base(lifetime, logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var hearingNotificationService = scope.ServiceProvider.GetRequiredService<IHearingNotificationService>();
            
            await hearingNotificationService.SendNotificationsAsync();

            _logger.LogInformation($"Send hearing notifications - Completed at:{DateTime.Now} ");
        }
    }
}
