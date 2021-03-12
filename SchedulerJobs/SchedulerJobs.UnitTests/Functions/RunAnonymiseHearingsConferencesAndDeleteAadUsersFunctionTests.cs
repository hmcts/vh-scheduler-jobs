using BookingsApi.Client;
using BookingsApi.Contract.Responses;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SchedulerJobs.Functions;

namespace SchedulerJobs.UnitTests.Functions
{

    public class RunAnonymiseHearingsConferencesAndDeleteAadUsersFunctionTests : AzureFunctionTestBaseSetup<AnonymiseHearingsConferencesAndDeleteAadUsersFunction>
    {
        [Test]
        public async Task Timer_should_log_message_all_older_conferences_were_updated()
        {
            // Arrange
            var usernames = new UserWithClosedConferencesResponse();
            usernames.Usernames = new List<string>
            {
                "username1@email.com",
                "username2@email.com",
                "username3@email.com"
            };
            _mocker.Mock<IBookingsApiClient>().Setup(x => x.GetPersonByClosedHearingsAsync()).ReturnsAsync(usernames);

            // Act
            await _sut.Run(_timerInfo, _logger);

            // Assert
            _logger.GetLoggedMessages().Last().Should().StartWith("Data anonymised for hearings, conferences older than 3 months.");
        }
    }
}
