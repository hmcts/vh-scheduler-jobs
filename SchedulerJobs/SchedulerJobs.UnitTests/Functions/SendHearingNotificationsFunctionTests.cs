using FluentAssertions;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Functions;
using SchedulerJobs.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerJobs.UnitTests.Functions
{
    public class SendHearingNotificationsFunctionTests : AzureFunctionTestBaseSetup<SendHearingNotificationsFunction>
    {
        [Test]
        public async Task Should_Call_HearingNotificationServiceAsync_Once()
        {
            _mocker.Mock<IHearingNotificationService>().Setup(x => x.SendNotificationsAsync());

            await _sut.RunAsync(_timerInfo, _logger);

            _mocker.Mock<IHearingNotificationService>().Verify(x => x.SendNotificationsAsync(), Times.Once);
        }

        [Test]
        public async Task Timer_Should_Log_Finish_Message_()
        {
            await _sut.RunAsync(_timerInfo, _logger);

            _logger.GetLoggedMessages().Last().Should().StartWith("Send hearing notifications - Completed at");
        }
    }
}
