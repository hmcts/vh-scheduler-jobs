using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Sds.Jobs;
using SchedulerJobs.Services.Interfaces;

namespace SchedulerJobs.Sds.UnitTests.Jobs
{
    public class SendHearingNotificationsJobTests : JobTestBaseSetup<SendHearingNotificationsJob>
    {
        private SendHearingNotificationsJob _sut;
        private Mock<IHearingNotificationService> _hearingNotificationService;
        
        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            _hearingNotificationService = new Mock<IHearingNotificationService>();
            services.AddScoped(s => _hearingNotificationService.Object);

            var serviceProvider = services.BuildServiceProvider();

            _sut = new SendHearingNotificationsJob(Lifetime.Object, Logger, serviceProvider, DistributedJobRunningStatusCache.Object);
        }
        
        [Test]
        public async Task Should_Call_HearingNotificationServiceAsync_Once()
        {
            _hearingNotificationService.Setup(x => x.SendNotificationsAsync());

            await _sut.DoWorkAsync();

            _hearingNotificationService.Verify(x => x.SendNotificationsAsync(), Times.Once);
        }

        [Test]
        public async Task Timer_Should_Log_Finish_Message_()
        {
            await _sut.DoWorkAsync();

            Logger.GetLoggedMessages().Last().Should().StartWith("Send hearing notifications - Completed at");
        }
    }   
}