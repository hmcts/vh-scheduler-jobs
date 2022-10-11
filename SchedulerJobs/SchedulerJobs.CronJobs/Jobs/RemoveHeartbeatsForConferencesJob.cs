using SchedulerJobs.Services;

namespace SchedulerJobs.CronJobs.Jobs
{
    public class RemoveHeartbeatsForConferencesJob : BaseJob
    {
        private readonly ILogger<RemoveHeartbeatsForConferencesJob> _logger;
        private readonly IServiceProvider _serviceProvider;

        public RemoveHeartbeatsForConferencesJob(
            IHostApplicationLifetime lifetime,
            ILogger<RemoveHeartbeatsForConferencesJob> logger,
            IServiceProvider serviceProvider) : base(lifetime, logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var removeHeartbeatsForConferencesService = scope.ServiceProvider.GetRequiredService<IRemoveHeartbeatsForConferencesService>();
            
            await removeHeartbeatsForConferencesService.RemoveHeartbeatsForConferencesAsync().ConfigureAwait(false);
            _logger.LogInformation("Removed heartbeats for conferences older than 14 days.");
        }
    }
}