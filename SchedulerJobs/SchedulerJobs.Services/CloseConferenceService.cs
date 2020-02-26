using System.Linq;
using System.Threading.Tasks;

namespace SchedulerJobs.Services
{
    public interface ICloseConferenceService
    {
        Task<int> CloseConferencesAsync();
    }

    public class CloseConferenceService : ICloseConferenceService
    {
        private readonly IVideoApiService _videoApiService;
        public CloseConferenceService(IVideoApiService videoApiService)
        {
            _videoApiService = videoApiService;
        }

        public async Task<int> CloseConferencesAsync()
        {
            var conferences = await _videoApiService.GetExpiredOpenConferences();
            var conferenceCount = 0;
            if (conferences != null && conferences.Any())
            {
                conferenceCount = conferences.Count;
                conferences.ForEach(async s =>
                {
                    await _videoApiService.CloseConference(s.Id);
                });
            }

            return conferenceCount;
        }
    }
}

