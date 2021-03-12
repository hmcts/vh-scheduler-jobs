using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Functions;
using SchedulerJobs.Services;

namespace SchedulerJobs.UnitTests.Functions
{
    public class ClearConferenceInstantMessageHistoryTests : AzureFunctionTestBaseSetup<ClearConferenceInstantMessageHistory>
    {
        [Test]
        public async Task Timer_should_log_message()
        {
            // Act
            await _sut.RunAsync(_timerInfo, _logger);

            // Assert
            _logger.GetLoggedMessages().First().Should().Be("Timer is running late!");
            _logger.GetLoggedMessages().Last().Should().Be("Cleared chat history for closed conferences");
            _mocker.Mock<IClearConferenceChatHistoryService>().Verify(x => x.ClearChatHistoryForClosedConferences(), Times.Once);
        }
    }
}
