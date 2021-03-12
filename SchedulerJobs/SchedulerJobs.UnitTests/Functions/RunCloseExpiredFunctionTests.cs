using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Functions;
using VideoApi.Client;
using VideoApi.Contract.Responses;

namespace SchedulerJobs.UnitTests.Functions
{
    public class RunCloseExpiredFunctionTests : AzureFunctionTestBaseSetup<ClearHearingsFunction>
    {
        [Test]
        public async Task Timer_should_log_message()
        {
            // Arrange
            var result = new List<ExpiredConferencesResponse>();
            _mocker.Mock<IVideoApiClient>().Setup(x => x.GetExpiredOpenConferencesAsync()).ReturnsAsync(result);

            // Act
            await _sut.Run(_timerInfo, _logger);

            // Assert
            _logger.GetLoggedMessages().Last().Should().StartWith("Close hearings function executed");
        }
    }
}
