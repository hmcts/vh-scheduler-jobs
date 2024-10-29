using System;
using System.Linq;
using System.Threading.Tasks;
using VideoApi.Client;

namespace SchedulerJobs.Services
{
    public interface ICloseConferenceService
    {
        Task<int> CloseConferencesAsync();
    }

    public class CloseConferenceService : ICloseConferenceService
    {
        private readonly IVideoApiClient _videoApiClient;

        public CloseConferenceService(IVideoApiClient videoApiClient)
        {
            _videoApiClient = videoApiClient;
        }

        public async Task<int> CloseConferencesAsync()
        {
            var conferences = await _videoApiClient.GetExpiredOpenConferencesAsync();
            var conferenceCount = 0;
            if (conferences != null && conferences.Any())
            {
                conferenceCount = conferences.Count;
                foreach (var conference in conferences)
                {
                    await _videoApiClient.CloseConferenceAsync(conference.Id);
                }
            }

            return conferenceCount;
        }
    }
}

