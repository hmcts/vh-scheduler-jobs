using System;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Functions;
using VideoApi.Client;
using VideoApi.Contract.Responses;

namespace SchedulerJobs.UnitTests.Functions
{
    public class ClearConferenceInstantMessageHistoryTests : AzureFunctionTestBaseSetup<ClearConferenceInstantMessageHistory>
    {
        [Test]
        public async Task Timer_should_log_message()
        {
            // Arrange
            var conferences = Builder<ClosedConferencesResponse>.CreateListOfSize(10).All()
                .With(x => x.Id = Guid.NewGuid()).Build().ToList();
            _mocker.Mock<IVideoApiClient>().Setup(x => x.GetClosedConferencesWithInstantMessagesAsync()).ReturnsAsync(conferences);
            var ids = conferences.Select(x => x.Id).ToList();
            
            // Act
            await _sut.RunAsync(_timerInfo, _logger);

            // Assert
            _logger.GetLoggedMessages().First().Should().Be("Timer is running late!");
            _logger.GetLoggedMessages().Last().Should().Be("Cleared chat history for closed conferences");
            _mocker.Mock<IVideoApiClient>().Verify(x => x.RemoveInstantMessagesAsync(It.IsIn<Guid>(ids)), Times.Exactly(ids.Count));
        }
    }
}
