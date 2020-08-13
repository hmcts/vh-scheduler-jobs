using System.Threading.Tasks;

namespace SchedulerJobs.Services
{
    public interface IRemoveHeartbeatsForConferencesService
    {
        Task RemoveHeartbeatsForConferencesAsync();
    }
    public class RemoveHeartbeatsForConferencesService : IRemoveHeartbeatsForConferencesService
    {
        private readonly IVideoApiService _videoApiService;
        public RemoveHeartbeatsForConferencesService(IVideoApiService videoApiService)
        {
            _videoApiService = videoApiService;
        }
        public async Task RemoveHeartbeatsForConferencesAsync()
        {
            await _videoApiService.RemoveHeartbeatsForConferencesAsync();
        }
    }
}
