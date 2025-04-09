using System.Threading.Tasks;
using BookingsApi.Client;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Common.Constants;
using SchedulerJobs.Common.Logging;
using UserApi.Client;
using UserApi.Contract.Responses;
using VideoApi.Client;
using VideoApi.Contract.Requests;

namespace SchedulerJobs.Services
{
    public interface IAnonymiseHearingsConferencesDataService
    {
        Task AnonymiseHearingsConferencesDataAsync();
    }

    public class AnonymiseHearingsConferencesDataService : IAnonymiseHearingsConferencesDataService
    {
        private readonly IBookingsApiClient _bookingsApiClient;
        private readonly ILogger<AnonymiseHearingsConferencesDataService> _logger;
        private readonly IUserApiClient _userApiClient;
        private readonly IVideoApiClient _videoApiClient;
        private const string ProcessingUsernameExceptionMessage = "unknown exception when processing {username}";


        public AnonymiseHearingsConferencesDataService(IVideoApiClient videoApiClient,
            IBookingsApiClient bookingsApiClient,
            IUserApiClient userApiClient,
            ILogger<AnonymiseHearingsConferencesDataService> logger)
        {
            _videoApiClient = videoApiClient;
            _bookingsApiClient = bookingsApiClient;
            _userApiClient = userApiClient;
            _logger = logger;
        }

        public async Task AnonymiseHearingsConferencesDataAsync()
        {
            var anonymisationData = await _bookingsApiClient.GetAnonymisationDataAsync();

            if (anonymisationData.HearingIds.Count != 0)
            {
                _logger.LogInformationHearingIds(anonymisationData.HearingIds);

                await _videoApiClient.AnonymiseConferenceWithHearingIdsAsync(
                    new AnonymiseConferenceWithHearingIdsRequest
                        {HearingIds = anonymisationData.HearingIds});

                await _videoApiClient.AnonymiseQuickLinkParticipantWithHearingIdsAsync(
                    new AnonymiseQuickLinkParticipantWithHearingIdsRequest
                        {HearingIds = anonymisationData.HearingIds});

                await _bookingsApiClient.AnonymiseParticipantAndCaseByHearingIdAsync(anonymisationData.HearingIds);
            }


            if (anonymisationData.Usernames.Count == 0) return;

            foreach (var username in anonymisationData.Usernames)
            {
                try
                {
                    var userProfile = await _userApiClient.GetUserByAdUserNameAsync(username);

                    if (ShouldRemoveUserFromAd(userProfile))
                    {
                        await _userApiClient.DeleteUserAsync(username);
                    }
                }
                catch (UserApiException exception)
                {
                    _logger.LogErrorUnknownExceptionForUser(exception,
                        username);
                }

                try
                {
                    await _videoApiClient.AnonymiseParticipantWithUsernameAsync(username);
                }
                catch (VideoApiException exception)
                {
                    _logger.LogErrorUnknownExceptionForUser(exception,
                        username);
                }

                try
                {
                    await _bookingsApiClient.AnonymisePersonWithUsernameForExpiredHearingsAsync(username);
                }
                catch (BookingsApiException exception)
                {
                    _logger.LogErrorUnknownExceptionForUser(exception, username);
                }
            }
        }

        private static bool ShouldRemoveUserFromAd(UserProfile userProfile)
        {
            return userProfile.UserRole != AzureAdUserRoles.Judge &&
                   userProfile.UserRole != AzureAdUserRoles.VhOfficer &&
                   userProfile.UserRole != AzureAdUserRoles.StaffMember &&
                   !userProfile.IsUserAdmin;
        }
    }
}