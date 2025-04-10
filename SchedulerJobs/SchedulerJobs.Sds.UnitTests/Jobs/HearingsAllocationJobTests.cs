using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Sds.Jobs;
using SchedulerJobs.Services;

namespace SchedulerJobs.Sds.UnitTests.Jobs
{
    public class HearingsAllocationJobTests : JobTestBaseSetup<HearingsAllocationJob>
    {
        private HearingsAllocationJob _sut;
        private Mock<IHearingAllocationService> _hearingAllocationService;
        
        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            _hearingAllocationService = new Mock<IHearingAllocationService>();
            services.AddScoped(_ => _hearingAllocationService.Object);
            
            var serviceProvider = services.BuildServiceProvider();

            _sut = new HearingsAllocationJob(Lifetime.Object, Logger, serviceProvider, DistributedJobRunningStatusCache.Object);
        }
        
        [Test]
        public async Task Timer_should_log_message()
        {
            await _sut.DoWorkAsync();

            // Assert
            Logger.GetLoggedMessages().Last().Should().StartWith("Close hearings function executed");
            _hearingAllocationService.Verify(x => x.AllocateHearingsAsync(), Times.Once);
        }
    }   
}
