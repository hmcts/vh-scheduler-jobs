using SchedulerJobs.Services;

namespace SchedulerJobs.CronJobs.Jobs
{
    public class ClearConferenceInstantMessageHistoryJob : BaseJob
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public ClearConferenceInstantMessageHistoryJob(
            ILogger<ClearConferenceInstantMessageHistoryJob> logger,
            IHostApplicationLifetime lifetime,
            IServiceProvider serviceProvider) : base(lifetime)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var clearConferenceChatHistoryService = scope.ServiceProvider.GetRequiredService<IClearConferenceChatHistoryService>();

            await clearConferenceChatHistoryService.ClearChatHistoryForClosedConferences();
            _logger.LogInformation("Cleared chat history for closed conferences");
        }
    }
}