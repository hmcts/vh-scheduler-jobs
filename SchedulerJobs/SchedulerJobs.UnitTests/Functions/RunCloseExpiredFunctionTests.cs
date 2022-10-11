using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Functions;
using SchedulerJobs.Services;

namespace SchedulerJobs.UnitTests.Functions
{
    public class RunCloseExpiredFunctionTests : AzureFunctionTestBaseSetup<ClearHearingsFunction>
    {
        [Test]
        public async Task Timer_should_log_message()
        {
            // Act
            await _sut.Run(_timerInfo, _logger);

            // Assert
            _mocker.Mock<ICloseConferenceService>().Verify(x => x.CloseConferencesAsync(false), Times.Once);
            _logger.GetLoggedMessages().Last().Should().StartWith("Close hearings function executed");
        }
    }
}
