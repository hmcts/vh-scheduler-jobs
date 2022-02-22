using System.Threading.Tasks;
using BookingsApi.Client;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Common.Constants;
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

    public class
        AnonymiseHearingsConferencesDataService : IAnonymiseHearingsConferencesDataService
    {
        private readonly IBookingsApiClient _bookingsApiClient;
        private readonly ILogger<AnonymiseHearingsConferencesDataService> _logger;
        private readonly IUserApiClient _userApiClient;
        private readonly IVideoApiClient _videoApiClient;

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

            await _videoApiClient.AnonymiseConferenceWithHearingIdsAsync(new AnonymiseConferenceWithHearingIdsRequest
                { HearingIds = anonymisationData.HearingIds });
            await _videoApiClient.AnonymiseQuickLinkParticipantWithHearingIdsAsync(
                new AnonymiseQuickLinkParticipantWithHearingIdsRequest { HearingIds = anonymisationData.HearingIds });

            await _bookingsApiClient.AnonymiseParticipantAndCaseByHearingIdAsync("hearingIds",
                anonymisationData.HearingIds);

            if (anonymisationData.Usernames != null)
                foreach (var username in anonymisationData.Usernames)
                {
                    var userProfile = await _userApiClient.GetUserByAdUserNameAsync(username);

                    if (RemoveUserFromAd(userProfile))
                        try
                        {
                            await _userApiClient.DeleteUserAsync(username);
                        }
                        catch (UserApiException exception)
                        {
                            _logger.LogError(exception, $"unknown exception when attempting to delete user {username}",
                                username);
                        }

                    try
                    {
                        await _videoApiClient.AnonymiseParticipantWithUsernameAsync(username);
                    }
                    catch (VideoApiException exception)
                    {
                        _logger.LogError(exception, $"unknown exception when attempting to delete user {username}",
                            username);
                    }

                    try
                    {
                        await _bookingsApiClient.AnonymisePersonWithUsernameForExpiredHearingsAsync(username);
                    }
                    catch (BookingsApiException exception)
                    {
                        _logger.LogError(exception, $"unknown exception when attempting to delete user {username}",
                            username);
                    }
                }
        }

        private bool RemoveUserFromAd(UserProfile userProfile)
        {
            return userProfile.UserRole != AzureAdUserRoles.Judge &&
                   userProfile.UserRole != AzureAdUserRoles.VhOfficer &&
                   userProfile.UserRole != AzureAdUserRoles.StaffMember &&
                   !userProfile.IsUserAdmin;
        }
    }
}