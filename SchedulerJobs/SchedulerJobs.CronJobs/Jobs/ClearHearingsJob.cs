using SchedulerJobs.Services;

namespace SchedulerJobs.CronJobs.Jobs
{
    public class ClearHearingsJob : BaseJob
    {
        private readonly ILogger<ClearHearingsJob> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ClearHearingsJob(
            IHostApplicationLifetime lifetime,
            ILogger<ClearHearingsJob> logger,
            IServiceProvider serviceProvider) : base(lifetime)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var closeConferenceService = scope.ServiceProvider.GetRequiredService<ICloseConferenceService>();
            
            var conferencesCount = await closeConferenceService.CloseConferencesAsync();
            _logger.LogInformation($"Close hearings job executed and  {conferencesCount} hearings closed");
        }
    }   
}
