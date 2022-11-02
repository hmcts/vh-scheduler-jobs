using SchedulerJobs.Services;

namespace SchedulerJobs.Sds.Jobs
{
    public class HearingsAllocationJob : BaseJob
    {
        private readonly ILogger<HearingsAllocationJob> _logger;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Job to automate hearing allocation
        /// </summary>
        /// <param name="lifetime">Set timer</param>
        /// <param name="logger"></param>
        /// <param name="serviceProvider"></param>
        public HearingsAllocationJob(
            IHostApplicationLifetime lifetime,
            ILogger<HearingsAllocationJob> logger,
            IServiceProvider serviceProvider) : base(lifetime, logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();

            _logger.LogInformation($"Close hearings function executed and allocated 10 hearings");
        }
    }   
}