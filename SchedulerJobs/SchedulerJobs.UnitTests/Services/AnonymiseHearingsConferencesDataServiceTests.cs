using BookingsApi.Client;
using BookingsApi.Contract.Responses;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;
using System.Collections.Generic;
using UserApi.Client;
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
    }
}
