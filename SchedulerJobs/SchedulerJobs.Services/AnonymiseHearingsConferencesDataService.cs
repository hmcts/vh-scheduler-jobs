using BookingsApi.Client;
using System;
using System.Net;
using System.Threading.Tasks;
using UserApi.Client;
using VideoApi.Client;

namespace SchedulerJobs.Services
{
    public interface IAnonymiseHearingsConferencesDataService
    {
        Task AnonymiseHearingsConferencesDataAsync();
    }

    public class AnonymiseHearingsConferencesDataService : IAnonymiseHearingsConferencesDataService
    {
        private readonly IVideoApiClient _videoApiClient;
        private readonly IBookingsApiClient _bookingsApiClient;
        private readonly IUserApiClient _userApiClient;
        public AnonymiseHearingsConferencesDataService(IVideoApiClient videoApiClient, IBookingsApiClient bookingsApiClient,
            IUserApiClient userApiClient)
        {
            _videoApiClient = videoApiClient;
            _bookingsApiClient = bookingsApiClient;
            _userApiClient = userApiClient;
        }
        public async Task AnonymiseHearingsConferencesDataAsync()
        {
            await DeleteUserAsync();
            await _videoApiClient.AnonymiseConferencesAsync();
            await _bookingsApiClient.AnonymiseHearingsAsync();
        }

        private async Task DeleteUserAsync()
        {
            // delete users from AAD.
            // get users that do not have hearings in the future and have had hearing more than 3 months in the past. 
            // (exclude judges, vhos, test users, performance test users.
            var usersToDelete = await _bookingsApiClient.GetPersonByClosedHearingsAsync();

            if (usersToDelete != null && usersToDelete.Usernames != null)
            {
                foreach (var username in usersToDelete.Usernames)
                {
                    try
                    {
                        await _userApiClient.DeleteUserAsync(username);
                    }
                    catch (UserApiException exception)
                    {
                        if (exception.StatusCode != (int)HttpStatusCode.NotFound)
                        {
                            throw;
                        }
                    }
                }
            }
        }
    }
}
