using Microsoft.Extensions.Options;
using SchedulerJobs.Common.Caching;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Services;

namespace SchedulerJobs.Sds.Jobs
{
    public class DeleteAudioRecordingApplicationsJob : BaseJob
    {
        private readonly ILogger<DeleteAudioRecordingApplicationsJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        
        public DeleteAudioRecordingApplicationsJob(
            ILogger<DeleteAudioRecordingApplicationsJob> logger,
            IHostApplicationLifetime lifetime,
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
            var closeConferenceService = scope.ServiceProvider.GetRequiredService<ICloseConferenceService>();
            
            var audioFilesCount = await closeConferenceService.DeleteAudiorecordingApplicationsAsync();
            _logger.LogInformation($"Delete audio recording applications job executed for {audioFilesCount} conferences");
        }
    }
}