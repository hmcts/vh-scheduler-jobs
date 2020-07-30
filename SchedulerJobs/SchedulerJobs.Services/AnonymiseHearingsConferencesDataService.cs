using System.Threading.Tasks;

namespace SchedulerJobs.Services
{
    public interface IAnonymiseHearingsConferencesDataService
    {
        Task AnonymiseHearingsConferencesDataAsync();
    }

    public class AnonymiseHearingsConferencesDataService : IAnonymiseHearingsConferencesDataService
    {
        private readonly IVideoApiService _videoApiService;
        private readonly IBookingsApiService _bookingsApiService;
        private readonly IUserApiService _userApiService;
        public AnonymiseHearingsConferencesDataService(IVideoApiService videoApiService, IBookingsApiService bookingsApiService,
            IUserApiService userApiService)
        {
            _videoApiService = videoApiService;
            _bookingsApiService = bookingsApiService;
            _userApiService = userApiService;
        }
        public async Task AnonymiseHearingsConferencesDataAsync()
        {
            await DeleteUserAsync();

            await _videoApiService.AnonymiseConferencesAsync();

            await _bookingsApiService.AnonymiseHearingsAsync();
        }

        private async Task DeleteUserAsync()
        {
            // delete users from AAD.
            // get users that do not have hearings in the future and have had hearing more than 3 months in the past. 
            // (exclude judges, vhos, test users, performance test users.
            var usersToDelete = await _bookingsApiService.GetUsersWithClosedConferencesAsync();
            if (usersToDelete != null && usersToDelete.Usernames != null)
            {
                foreach (var username in usersToDelete.Usernames)
                {
                    await _userApiService.DeleteUserAsync(username);
                }
            }
        }
    }
}
