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
        
        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();

            var serviceProvider = services.BuildServiceProvider();

            _sut = new HearingsAllocationJob(Lifetime.Object, Logger, serviceProvider);
        }
        
        [Test]
        public async Task Timer_should_log_message()
        {
            // Act
            await _sut.DoWorkAsync();

            // Assert
            
            Logger.GetLoggedMessages().Last().Should().StartWith("Close hearings function executed and allocated");
        }
    }   
}
