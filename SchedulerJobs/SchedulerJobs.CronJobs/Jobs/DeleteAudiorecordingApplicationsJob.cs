using SchedulerJobs.Services;

namespace SchedulerJobs.CronJobs.Jobs
{
    public class DeleteAudiorecordingApplicationsJob : BaseJob
    {
        private readonly ILogger<DeleteAudiorecordingApplicationsJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        
        public DeleteAudiorecordingApplicationsJob(
            ILogger<DeleteAudiorecordingApplicationsJob> logger,
            IHostApplicationLifetime lifetime,
            IServiceProvider serviceProvider) : base(lifetime)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var closeConferenceService = scope.ServiceProvider.GetRequiredService<ICloseConferenceService>();
            
            var audioFilesCount = await closeConferenceService.DeleteAudiorecordingApplicationsAsync();
            _logger.LogTrace($"Delete audiorecording applications job executed for {audioFilesCount} conferences");
        }
    }
}