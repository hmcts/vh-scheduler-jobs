using System.Threading.Tasks;
using VideoApi.Client;

namespace SchedulerJobs.Services
{
    public interface IRemoveHeartbeatsForConferencesService
    {
        Task RemoveHeartbeatsForConferencesAsync();
    }
    public class RemoveHeartbeatsForConferencesService : IRemoveHeartbeatsForConferencesService
    {
        private readonly IVideoApiClient _videoApiClient;

        public RemoveHeartbeatsForConferencesService(IVideoApiClient videoApiClient)
        {
            _videoApiClient = videoApiClient;
        }

        public async Task RemoveHeartbeatsForConferencesAsync()
        {
            await _videoApiClient.RemoveHeartbeatsForConferencesAsync();
        }
    }
}
