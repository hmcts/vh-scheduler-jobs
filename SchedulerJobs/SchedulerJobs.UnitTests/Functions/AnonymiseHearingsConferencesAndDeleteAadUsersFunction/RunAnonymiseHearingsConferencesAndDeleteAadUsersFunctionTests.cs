using FluentAssertions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;
using SchedulerJobs.Services.BookingApi.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Testing.Common;

namespace SchedulerJobs.UnitTests.Functions.AnonymiseHearingsConferencesAndDeleteAadUsersFunction
{
    public class RunAnonymiseHearingsConferencesAndDeleteAadUsersFunctionTests
    {
        private Mock<IVideoApiService> _videoApiServiceMock;
        private Mock<IBookingsApiService> _bookingApiServiceMock;
        private Mock<IUserApiService> _userApiServiceMock;
        private Mock<ILogger<AnonymiseHearingsConferencesDataService>> _loggerMock;
        private readonly TimerInfo _timerInfo = new TimerInfo(new ScheduleStub(), new ScheduleStatus(), true);

        [SetUp]
        public void Setup()
        {
            _videoApiServiceMock = new Mock<IVideoApiService>();
            _bookingApiServiceMock = new Mock<IBookingsApiService>();
            _userApiServiceMock = new Mock<IUserApiService>();
            _loggerMock = new Mock<ILogger<AnonymiseHearingsConferencesDataService>>();

            var usernames = new UserWithClosedConferencesResponse();
            usernames.Usernames = new List<string>();
            usernames.Usernames.Add("username1@email.com");
            usernames.Usernames.Add("username2@email.com");
            usernames.Usernames.Add("username3@email.com");

            _bookingApiServiceMock.Setup(x => x.GetUsersWithClosedConferencesAsync()).ReturnsAsync(usernames);
        }

        [Test]
        public async Task Timer_should_log_message_all_older_conferences_were_updated()
        {
            var logger = (LoggerFake)TestFactory.CreateLogger(LoggerTypes.List);
            await SchedulerJobs.Functions.AnonymiseHearingsConferencesAndDeleteAadUsersFunction.Run(_timerInfo, logger,
                new AnonymiseHearingsConferencesDataService(_videoApiServiceMock.Object, _bookingApiServiceMock.Object, _userApiServiceMock.Object, _loggerMock.Object));
            logger.GetLoggedMessages().Last().Should()
                .StartWith("Data anonymised for hearings, conferences older than 3 months.");
        }
    }
}
