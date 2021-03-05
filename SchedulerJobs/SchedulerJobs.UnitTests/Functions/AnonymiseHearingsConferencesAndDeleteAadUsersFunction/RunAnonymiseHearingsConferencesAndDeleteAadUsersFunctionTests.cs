using BookingsApi.Client;
using BookingsApi.Contract.Responses;
using FluentAssertions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Testing.Common;
using UserApi.Client;
using VideoApi.Client;

namespace SchedulerJobs.UnitTests.Functions.AnonymiseHearingsConferencesAndDeleteAadUsersFunction
{
    public class RunAnonymiseHearingsConferencesAndDeleteAadUsersFunctionTests
    {
        private Mock<IVideoApiClient> _videoApiClientMock;
        private Mock<IBookingsApiClient> _bookingApiClientMock;
        private Mock<IUserApiClient> _userApiClientMock;
        private readonly TimerInfo _timerInfo = new TimerInfo(new ScheduleStub(), new ScheduleStatus(), true);

        [SetUp]
        public void Setup()
        {
            _videoApiClientMock = new Mock<IVideoApiClient>();
            _bookingApiClientMock = new Mock<IBookingsApiClient>();
            _userApiClientMock = new Mock<IUserApiClient>();

            var usernames = new UserWithClosedConferencesResponse();
            usernames.Usernames = new List<string>();
            usernames.Usernames.Add("username1@email.com");
            usernames.Usernames.Add("username2@email.com");
            usernames.Usernames.Add("username3@email.com");

            _bookingApiClientMock.Setup(x => x.GetPersonByClosedHearingsAsync()).ReturnsAsync(usernames);
        }

        [Test]
        public async Task Timer_should_log_message_all_older_conferences_were_updated()
        {
            var logger = (LoggerFake)TestFactory.CreateLogger(LoggerTypes.List);
            await SchedulerJobs.Functions.AnonymiseHearingsConferencesAndDeleteAadUsersFunction.Run(_timerInfo, logger,
                new AnonymiseHearingsConferencesDataService(_videoApiClientMock.Object, _bookingApiClientMock.Object, _userApiClientMock.Object));
            logger.GetLoggedMessages().Last().Should()
                .StartWith("Data anonymised for hearings, conferences older than 3 months.");
        }
    }
}
