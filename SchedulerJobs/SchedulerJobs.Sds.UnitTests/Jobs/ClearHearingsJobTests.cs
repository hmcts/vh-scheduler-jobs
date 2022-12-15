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
    public class ClearHearingsJobTests : JobTestBaseSetup<ClearHearingsJob>
    {
        private ClearHearingsJob _sut;
        private Mock<ICloseConferenceService> _closeConferenceService;
        
        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            _closeConferenceService = new Mock<ICloseConferenceService>();
            services.AddScoped(s => _closeConferenceService.Object);

            var serviceProvider = services.BuildServiceProvider();

            _sut = new ClearHearingsJob(Lifetime.Object, Logger, serviceProvider, DistributedJobRunningStatusCache.Object, ConnectionStrings.Object);
        }
        
        [Test]
        public async Task Timer_should_log_message()
        {
            // Act
            await _sut.DoWorkAsync();

            // Assert
            _closeConferenceService.Verify(x => x.CloseConferencesAsync(), Times.Once);
            Logger.GetLoggedMessages().Last().Should().StartWith("Close hearings job executed");
        }
    }   
}
