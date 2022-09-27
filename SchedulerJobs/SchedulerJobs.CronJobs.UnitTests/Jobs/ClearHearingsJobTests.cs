using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SchedulerJobs.CronJobs.Jobs;
using SchedulerJobs.Services;

namespace SchedulerJobs.CronJobs.UnitTests.Jobs
{
    public class ClearHearingsJobTests : JobTestBaseSetup<ClearHearingsJob>
    {
        private ClearHearingsJob _sut;
        private Mock<ICloseConferenceService> _closeConferenceService;
        
        [SetUp]
        public void Setup()
        {
            // TODO move to single base setup class so we don't need to repeat for each job
            var services = new ServiceCollection();
            _closeConferenceService = new Mock<ICloseConferenceService>();
            services.AddScoped(s => _closeConferenceService.Object);

            var serviceProvider = services.BuildServiceProvider();

            _sut = new ClearHearingsJob(Lifetime.Object, Logger, serviceProvider);
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
