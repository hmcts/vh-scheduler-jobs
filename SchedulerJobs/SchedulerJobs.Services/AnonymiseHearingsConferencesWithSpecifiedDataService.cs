using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BookingsApi.Client;
using BookingsApi.Contract.Responses;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Common.Constants;
using UserApi.Client;
using UserApi.Contract.Responses;
using VideoApi.Client;
using VideoApi.Contract.Requests;

namespace SchedulerJobs.Services
{
    public interface IAnonymiseHearingsConferencesWithSpecifiedDataService
    {
        Task AnonymiseHearingsConferencesWithSpecifiedData();
    }

    public class
        AnonymiseHearingsConferencesWithSpecifiedDataService : IAnonymiseHearingsConferencesWithSpecifiedDataService
    {
        private readonly IVideoApiClient _videoApiClient;
        private readonly IBookingsApiClient _bookingsApiClient;
        private readonly IUserApiClient _userApiClient;
        private readonly ILogger<AnonymiseHearingsConferencesWithSpecifiedDataService> _logger;

        public AnonymiseHearingsConferencesWithSpecifiedDataService(IVideoApiClient videoApiClient,
            IBookingsApiClient bookingsApiClient,
            IUserApiClient userApiClient,
            ILogger<AnonymiseHearingsConferencesWithSpecifiedDataService> logger)
        {
            _videoApiClient = videoApiClient;
            _bookingsApiClient = bookingsApiClient;
            _userApiClient = userApiClient;
            _logger = logger;
        }

        public async Task AnonymiseHearingsConferencesWithSpecifiedData()
        {
            var anonymisationData = await _bookingsApiClient.GetAnonymisationDataAsync();

            await _videoApiClient.AnonymiseConferenceWithHearingIdsAsync(new AnonymiseConferenceWithHearingIdsRequest
                { HearingIds = anonymisationData.HearingIds });
            await _videoApiClient.AnonymiseQuickLinkParticipantWithHearingIdsAsync(
                new AnonymiseQuickLinkParticipantWithHearingIdsRequest { HearingIds = anonymisationData.HearingIds });

            await _bookingsApiClient.AnonymiseParticipantAndCaseByHearingIdAsync("hearingIds",
                anonymisationData.HearingIds);

            if (anonymisationData.Usernames != null)
            {
                foreach (var username in anonymisationData.Usernames)
                {
                    var userProfile = await _userApiClient.GetUserByAdUserNameAsync(username);

                    if (RemoveUserFromAd(userProfile))
                    {
                        try
                        {
                            await _userApiClient.DeleteUserAsync(username);
                        }
                        catch (UserApiException exception)
                        {
                            _logger.LogError(exception, $"unknown exception when attempting to delete user {username}",
                                username);
                        }
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
        }

        private bool RemoveUserFromAd(UserProfile userProfile) =>
            userProfile.UserRole != AzureAdUserRoles.Judge &&
            userProfile.UserRole != AzureAdUserRoles.VhOfficer &&
            userProfile.UserRole != AzureAdUserRoles.StaffMember &&
            !userProfile.IsUserAdmin;
    }
}