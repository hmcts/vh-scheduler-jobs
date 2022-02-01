using BookingsApi.Client;
using BookingsApi.Contract.Responses;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Common.Constants;
using SchedulerJobs.Services;
using System;
using System.Collections.Generic;
using UserApi.Client;
using UserApi.Contract.Responses;
using VideoApi.Client;

namespace SchedulerJobs.UnitTests.Services
{
    public class AnonymiseHearingsConferencesDataServiceTests
    {
        private Mock<IVideoApiClient> _videoApiClient;
        private Mock<IBookingsApiClient> _bookingApiClient;
        private Mock<IUserApiClient> _userApiClient;
        private IAnonymiseHearingsConferencesDataService _anonymiseHearingsConferencesDataService;

        [SetUp]
        public void Setup()
        {
            _userApiClient = new Mock<IUserApiClient>();
            _bookingApiClient = new Mock<IBookingsApiClient>();
            _videoApiClient = new Mock<IVideoApiClient>();

            var userProfile = new UserProfile { UserRole = "Individual" };
            _userApiClient.Setup(x => x.GetUserByAdUserNameAsync(It.IsAny<string>())).ReturnsAsync(userProfile);

            _anonymiseHearingsConferencesDataService = new AnonymiseHearingsConferencesDataService(_videoApiClient.Object,
                _bookingApiClient.Object, _userApiClient.Object);
        }

        [Test]
        public void Should_anonymise_the_old_hearings_and_conferences()
        {
            var usernames = new UserWithClosedConferencesResponse
            {
                Usernames = new List<string> {"username1@hmcts.net", "username2@hmcts.net", "username3@hmcts.net" }
            };

            _bookingApiClient.Setup(x => x.GetPersonByClosedHearingsAsync()).ReturnsAsync(usernames);

            _anonymiseHearingsConferencesDataService.AnonymiseHearingsConferencesDataAsync();

            _userApiClient.Verify(x => x.DeleteUserAsync(It.IsAny<string>()), Times.Exactly(3));
            _videoApiClient.Verify(x => x.AnonymiseConferencesAsync(), Times.Once);
            _bookingApiClient.Verify(x => x.AnonymiseHearingsAsync(), Times.Once);
        }

        [Test]
        public void Should_not_delete_vho_accounts()
        {
            var usernames = new UserWithClosedConferencesResponse
            {
                Usernames = new List<string> { "username1@hmcts.net" }
            };

            _bookingApiClient.Setup(x => x.GetPersonByClosedHearingsAsync()).ReturnsAsync(usernames);

            var userProfile = new UserProfile { UserRole = AzureAdUserRoles.VhOfficer };
            _userApiClient.Setup(x => x.GetUserByAdUserNameAsync(It.IsAny<string>())).ReturnsAsync(userProfile);

            _anonymiseHearingsConferencesDataService.AnonymiseHearingsConferencesDataAsync();

            _userApiClient.Verify(x => x.DeleteUserAsync(It.IsAny<string>()), Times.Never);
            _videoApiClient.Verify(x => x.AnonymiseConferencesAsync(), Times.Once);
            _bookingApiClient.Verify(x => x.AnonymiseHearingsAsync(), Times.Once);
        }

        [Test]
        [TestCase(404)]
        [TestCase(403)]
        public void Should_ignore_errors_when_deleting_users_with_status_code(int statusCode)
        {
            //Arrange
            var usernames = new UserWithClosedConferencesResponse
            {
                Usernames = new List<string> { "username1@hmcts.net", "username2@hmcts.net", "username3@hmcts.net" }
            };

            _bookingApiClient.Setup(x => x.GetPersonByClosedHearingsAsync()).ReturnsAsync(usernames);
            _userApiClient.Setup(x => x.DeleteUserAsync(It.IsAny<string>()))
                .ThrowsAsync(new UserApiException("", statusCode, "", null, null));

            //Act
            _anonymiseHearingsConferencesDataService.AnonymiseHearingsConferencesDataAsync();

            //Assert
            _userApiClient.Verify(x => x.DeleteUserAsync(It.IsAny<string>()), Times.Exactly(3));
            _videoApiClient.Verify(x => x.AnonymiseConferencesAsync(), Times.Once);
            _bookingApiClient.Verify(x => x.AnonymiseHearingsAsync(), Times.Once);
        }

        [Test]
        public void Should_throw_errors_when_deleting_users()
        {
            //Arrange
            var usernames = new UserWithClosedConferencesResponse
            {
                Usernames = new List<string> { "username1@hmcts.net", "username2@hmcts.net", "username3@hmcts.net" }
            };

            _bookingApiClient.Setup(x => x.GetPersonByClosedHearingsAsync()).ReturnsAsync(usernames);
            _userApiClient.Setup(x => x.DeleteUserAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception(""));

            //Act/Assert
            Assert.ThrowsAsync<Exception>(async () =>
                await _anonymiseHearingsConferencesDataService.AnonymiseHearingsConferencesDataAsync());

            _userApiClient.Verify(x => x.DeleteUserAsync(It.IsAny<string>()), Times.Exactly(1));
            _videoApiClient.Verify(x => x.AnonymiseConferencesAsync(), Times.Never);
            _bookingApiClient.Verify(x => x.AnonymiseHearingsAsync(), Times.Never);
        }
    }
}
