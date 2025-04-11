using SchedulerJobs.Sds.Caching;
using SchedulerJobs.Services;
using SchedulerJobs.Common.Logging;

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
        /// <param name="hearingAllocationService"></param>
        /// <param name="distributedJobRunningStatusCache"></param>
        /// <param name="redisContextAccessor"></param>
        public HearingsAllocationJob(
            IHostApplicationLifetime lifetime,
            ILogger<HearingsAllocationJob> logger,
            IServiceProvider serviceProvider,
            IDistributedJobRunningStatusCache distributedJobRunningStatusCache) : base(lifetime, logger, distributedJobRunningStatusCache)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var hearingAllocationService = scope.ServiceProvider.GetRequiredService<IHearingAllocationService>();

            await hearingAllocationService.AllocateHearingsAsync();
            
            _logger.LogInformationCloseHearingsFunctionExecuted();
        }
    }   
}
