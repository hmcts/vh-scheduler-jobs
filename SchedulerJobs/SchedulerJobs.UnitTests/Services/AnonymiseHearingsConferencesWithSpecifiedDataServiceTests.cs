using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingsApi.Client;
using BookingsApi.Contract.Responses;
using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Common.Constants;
using SchedulerJobs.Services;
using UserApi.Client;
using UserApi.Contract.Responses;
using VideoApi.Client;
using VideoApi.Contract.Requests;

namespace SchedulerJobs.UnitTests.Services
{
    public class AnonymiseHearingsConferencesWithSpecifiedDataServiceTests
    {
        private Mock<IVideoApiClient> _videoApiClient;
        private Mock<IBookingsApiClient> _bookingApiClient;
        private Mock<IUserApiClient> _userApiClient;
        private IAnonymiseHearingsConferencesWithSpecifiedDataService _anonymiseHearingsConferencesWithSpecifiedDataService;
        private Mock<ILogger<AnonymiseHearingsConferencesWithSpecifiedDataService>> _logger;
        
        [SetUp]
        public void Setup()
        {
            _userApiClient = new Mock<IUserApiClient>();
            _bookingApiClient = new Mock<IBookingsApiClient>();
            _videoApiClient = new Mock<IVideoApiClient>();
            _logger = new Mock<ILogger<AnonymiseHearingsConferencesWithSpecifiedDataService>>();

            var userProfile = new UserProfile { UserRole = "Individual" };
            _userApiClient.Setup(x => x.GetUserByAdUserNameAsync(It.IsAny<string>())).ReturnsAsync(userProfile);

            _anonymiseHearingsConferencesWithSpecifiedDataService = new AnonymiseHearingsConferencesWithSpecifiedDataService(_videoApiClient.Object,
                _bookingApiClient.Object, _userApiClient.Object, _logger.Object);
        }
        
        [Test]
        public async Task Anonymises_And_Deletes_Specified_Usernames()
        {
            var anonymisationData = new AnonymisationDataResponse
            {
                Usernames = new List<string> {"username1@hmcts.net", "username2@hmcts.net" }
            };
            
            _bookingApiClient.Setup(x => x.GetAnonymisationDataAsync()).ReturnsAsync(anonymisationData);

            await _anonymiseHearingsConferencesWithSpecifiedDataService.AnonymiseHearingsConferencesWithSpecifiedData();
            
            
            _userApiClient.Verify(userApi => userApi
                .DeleteUserAsync(It.Is<string>(username => anonymisationData.Usernames.Contains(username))), Times.Exactly(2));
            
            _bookingApiClient.Verify(bookingsApi => bookingsApi
                .AnonymisePersonWithUsernameForExpiredHearingsAsync(It.Is<string>(username => anonymisationData.Usernames.Contains(username))), Times.Exactly(2));
            
            _videoApiClient.Verify(videoApi => videoApi
                .AnonymiseParticipantWithUsernameAsync(It.Is<string>(username => anonymisationData.Usernames.Contains(username))), Times.Exactly(2));
        }

        [Test]
        public async Task Anonymises_Conferences_And_Hearings_Data_With_Specified_Hearing_Ids()
        {
            var anonymisationData = new AnonymisationDataResponse
            {
                HearingIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            };
            
            _bookingApiClient.Setup(x => x.GetAnonymisationDataAsync()).ReturnsAsync(anonymisationData);
            
            await _anonymiseHearingsConferencesWithSpecifiedDataService.AnonymiseHearingsConferencesWithSpecifiedData();
            
            _videoApiClient.Verify(videoApi => videoApi.AnonymiseConferenceWithHearingIdsAsync(It.Is<AnonymiseConferenceWithHearingIdsRequest>(request => request.HearingIds == anonymisationData.HearingIds)), Times.Once);
            _videoApiClient.Verify(videoApi => videoApi.AnonymiseQuickLinkParticipantWithHearingIdsAsync(It.Is<AnonymiseQuickLinkParticipantWithHearingIdsRequest>(request => request.HearingIds == anonymisationData.HearingIds)), Times.Once);
            
            _bookingApiClient.Verify(bookingsApi => bookingsApi.AnonymiseParticipantAndCaseByHearingIdAsync(It.IsAny<string>(), It.Is<List<Guid>>(body => body == anonymisationData.HearingIds)), Times.Once);
        }

        [Test]
        public async Task Does_Not_Delete_User_From_Ad_When_User_Is_Admin()
        {
            var usernameNotToDeleteFromAd = "username1@hmcts.net";
            var anonymisationData = new AnonymisationDataResponse
            {
                Usernames = new List<string> {usernameNotToDeleteFromAd, "username2@hmcts.net" }
            };
            
            _bookingApiClient.Setup(x => x.GetAnonymisationDataAsync()).ReturnsAsync(anonymisationData);
            
            _userApiClient.Setup(x => x.GetUserByAdUserNameAsync(It.Is<string>(username => username == usernameNotToDeleteFromAd)))
                .ReturnsAsync(() => Builder<UserProfile>
                    .CreateNew().With(r => r.IsUserAdmin = true)
                    .Build());
            
            await _anonymiseHearingsConferencesWithSpecifiedDataService.AnonymiseHearingsConferencesWithSpecifiedData();
            
            _userApiClient.Verify(userApi => userApi.DeleteUserAsync(It.Is<string>(username => username == usernameNotToDeleteFromAd)), Times.Never);
            _userApiClient.Verify(userApi => userApi.DeleteUserAsync(It.Is<string>(username => username == anonymisationData.Usernames.Last())), Times.Once);
        }
        
        [Test]
        [TestCase(AzureAdUserRoles.Judge)]
        [TestCase(AzureAdUserRoles.VhOfficer)]
        [TestCase(AzureAdUserRoles.StaffMember)]
        public async Task Does_Not_Delete_User_From_Ad_For_User_Role(string userRole)
        {
            var usernameNotToDeleteFromAd = "username1@hmcts.net";
            var anonymisationData = new AnonymisationDataResponse
            {
                Usernames = new List<string> {usernameNotToDeleteFromAd, "username2@hmcts.net" }
            };
            
            _bookingApiClient.Setup(x => x.GetAnonymisationDataAsync()).ReturnsAsync(anonymisationData);
            
            _userApiClient.Setup(x => x.GetUserByAdUserNameAsync(It.Is<string>(username => username == usernameNotToDeleteFromAd)))
                .ReturnsAsync(() => Builder<UserProfile>
                    .CreateNew().With(r => r.UserRole = userRole)
                    .Build());
            
            await _anonymiseHearingsConferencesWithSpecifiedDataService.AnonymiseHearingsConferencesWithSpecifiedData();
            
            _userApiClient.Verify(userApi => userApi.DeleteUserAsync(It.Is<string>(username => username == usernameNotToDeleteFromAd)), Times.Never);
            _userApiClient.Verify(userApi => userApi.DeleteUserAsync(It.Is<string>(username => username == anonymisationData.Usernames.Last())), Times.Once);
        }

        [Test]
        public async Task Logs_Unexpected_User_Api_Exception_And_Continues_To_Process_Other_User()
        {
            var usernameToThrowException = "username1@hmcts.net";
           
            var anonymisationData = new AnonymisationDataResponse
            {
                Usernames = new List<string> {usernameToThrowException, "username2@hmcts.net", "username3@hmcts.net" }
            };

            var unknownException = new UserApiException("", 500, "", null, null);
            
            _bookingApiClient.Setup(x => x.GetAnonymisationDataAsync()).ReturnsAsync(anonymisationData);
            _userApiClient.Setup(x => x.DeleteUserAsync(It.Is<string>(username => username == usernameToThrowException)))
                .ThrowsAsync(unknownException);
            
            await _anonymiseHearingsConferencesWithSpecifiedDataService.AnonymiseHearingsConferencesWithSpecifiedData();

            _userApiClient.Verify(userApi => userApi
                .DeleteUserAsync(It.Is<string>(username => anonymisationData.Usernames.Contains(username))), Times.Exactly(3));
            
            _bookingApiClient.Verify(bookingsApi => bookingsApi
                .AnonymisePersonWithUsernameForExpiredHearingsAsync(It.Is<string>(username => anonymisationData.Usernames.Contains(username))), Times.Exactly(3));
            
            _videoApiClient.Verify(videoApi => videoApi
                .AnonymiseParticipantWithUsernameAsync(It.Is<string>(username => anonymisationData.Usernames.Contains(username))), Times.Exactly(3));
            
            _logger.Verify(x => x.Log(
                It.Is<LogLevel>(log => log == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(x => x == unknownException),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }
        
        [Test]
        public async Task Logs_Unexpected_Video_Api_Exception__And_Continues_To_Process_Other_User()
        {
            var usernameToThrowException = "username1@hmcts.net";
           
            var anonymisationData = new AnonymisationDataResponse
            {
                Usernames = new List<string> {usernameToThrowException, "username2@hmcts.net", "username3@hmcts.net" }
            };

            var unknownException = new VideoApiException("", 500, "", null, null);
            
            _bookingApiClient.Setup(x => x.GetAnonymisationDataAsync()).ReturnsAsync(anonymisationData);
            _videoApiClient.Setup(x => x.AnonymiseParticipantWithUsernameAsync(It.Is<string>(username => username == usernameToThrowException)))
                .ThrowsAsync(unknownException);
            
            await _anonymiseHearingsConferencesWithSpecifiedDataService.AnonymiseHearingsConferencesWithSpecifiedData();

            _userApiClient.Verify(userApi => userApi
                .DeleteUserAsync(It.Is<string>(username => anonymisationData.Usernames.Contains(username))), Times.Exactly(3));
            
            _bookingApiClient.Verify(bookingsApi => bookingsApi
                .AnonymisePersonWithUsernameForExpiredHearingsAsync(It.Is<string>(username => anonymisationData.Usernames.Contains(username))), Times.Exactly(3));
            
            _videoApiClient.Verify(videoApi => videoApi
                .AnonymiseParticipantWithUsernameAsync(It.Is<string>(username => anonymisationData.Usernames.Contains(username))), Times.Exactly(3));
            
            _logger.Verify(x => x.Log(
                It.Is<LogLevel>(log => log == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(x => x == unknownException),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }
        
        [Test]
        public async Task Logs_Unexpected_Bookings_Api_Exception__And_Continues_To_Process_Other_User()
        {
            var usernameToThrowException = "username1@hmcts.net";
           
            var anonymisationData = new AnonymisationDataResponse
            {
                Usernames = new List<string> {usernameToThrowException, "username2@hmcts.net", "username3@hmcts.net" }
            };

            var unknownException = new BookingsApiException("", 500, "", null, null);
            
            _bookingApiClient.Setup(x => x.GetAnonymisationDataAsync()).ReturnsAsync(anonymisationData);
            _bookingApiClient.Setup(x => x.AnonymisePersonWithUsernameForExpiredHearingsAsync(It.Is<string>(username => username == usernameToThrowException)))
                .ThrowsAsync(unknownException);
            
            await _anonymiseHearingsConferencesWithSpecifiedDataService.AnonymiseHearingsConferencesWithSpecifiedData();

            _userApiClient.Verify(userApi => userApi
                .DeleteUserAsync(It.Is<string>(username => anonymisationData.Usernames.Contains(username))), Times.Exactly(3));
            
            _bookingApiClient.Verify(bookingsApi => bookingsApi
                .AnonymisePersonWithUsernameForExpiredHearingsAsync(It.Is<string>(username => anonymisationData.Usernames.Contains(username))), Times.Exactly(3));
            
            _videoApiClient.Verify(videoApi => videoApi
                .AnonymiseParticipantWithUsernameAsync(It.Is<string>(username => anonymisationData.Usernames.Contains(username))), Times.Exactly(3));
            
            _logger.Verify(x => x.Log(
                It.Is<LogLevel>(log => log == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(x => x == unknownException),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }
    }
}