using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<AnonymiseHearingsConferencesDataService> _logger;
        
        public AnonymiseHearingsConferencesDataService(IVideoApiService videoApiService, IBookingsApiService bookingsApiService,
            IUserApiService userApiService, ILogger<AnonymiseHearingsConferencesDataService> logger)
        {
            _videoApiService = videoApiService;
            _bookingsApiService = bookingsApiService;
            _userApiService = userApiService;
            _logger = logger;
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
            
            _logger.LogInformation($"AnonymiseHearingsConferencesDataService: Found {usersToDelete.Usernames.Count} users to delete");
            
            if (usersToDelete?.Usernames != null)
            {
                var deletedCount = 0;
                foreach (var username in usersToDelete.Usernames)
                {
                    try
                    {
                        await _userApiService.DeleteUserAsync(username);
                        deletedCount++;
                        _logger.LogInformation($"AnonymiseHearingsConferencesDataService: deleted {username}");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"AnonymiseHearingsConferencesDataService: failed to delete {username}");
                    }
                }
                
                _logger.LogInformation($"AnonymiseHearingsConferencesDataService: Deleted {deletedCount} of {usersToDelete.Usernames.Count} users");
            }
        }
    }
}
