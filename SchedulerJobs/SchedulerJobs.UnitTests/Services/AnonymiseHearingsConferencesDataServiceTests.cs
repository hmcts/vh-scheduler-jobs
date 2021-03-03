using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;
using SchedulerJobs.Services.BookingApi.Contracts;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace SchedulerJobs.UnitTests.Services
{
    public class AnonymiseHearingsConferencesDataServiceTests
    {
        private Mock<IVideoApiService> _videoApiService;
        private Mock<IBookingsApiService> _bookingApiService;
        private Mock<IUserApiService> _userApiService;
        private Mock<ILogger<AnonymiseHearingsConferencesDataService>> _loggerMock;
        private IAnonymiseHearingsConferencesDataService _anonymiseHearingsConferencesDataService;

        [SetUp]
        public void Setup()
        {
            _userApiService = new Mock<IUserApiService>();
            _bookingApiService = new Mock<IBookingsApiService>();
            _videoApiService = new Mock<IVideoApiService>();
            _loggerMock = new Mock<ILogger<AnonymiseHearingsConferencesDataService>>();
            _anonymiseHearingsConferencesDataService = new AnonymiseHearingsConferencesDataService(_videoApiService.Object,
                _bookingApiService.Object, _userApiService.Object, _loggerMock.Object);
        }

        [Test]
        public void Should_anonymise_the_old_hearings_and_conferences()
        {
            var usernames = new UserWithClosedConferencesResponse();
            usernames.Usernames = new List<string>();
            usernames.Usernames.Add("username1@email.com");
            usernames.Usernames.Add("username2@email.com");
            usernames.Usernames.Add("username3@email.com");
            _bookingApiService.Setup(x => x.GetUsersWithClosedConferencesAsync()).ReturnsAsync(usernames);

            _anonymiseHearingsConferencesDataService.AnonymiseHearingsConferencesDataAsync();

            _userApiService.Verify(x => x.DeleteUserAsync(It.IsAny<string>()), Times.Exactly(3));
            _videoApiService.Verify(x => x.AnonymiseConferencesAsync(), Times.Once);
            _bookingApiService.Verify(x => x.AnonymiseHearingsAsync(), Times.Once);
        }
    }
}
