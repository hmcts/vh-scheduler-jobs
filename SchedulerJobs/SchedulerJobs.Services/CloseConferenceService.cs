using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SchedulerJobs.Services
{
    public interface ICloseConferenceService
    {
        Task<int> CloseConferencesAsync(DateTime date);
    }

    public class CloseConferenceService : ICloseConferenceService
    {
        private readonly IVideoApiService _videoApiService;
        public CloseConferenceService(IVideoApiService videoApiService)
        {
            _videoApiService = videoApiService;
        }

        public async Task<int> CloseConferencesAsync(DateTime date)
        {
            var conferences = await _videoApiService.GetOpenConferencesByScheduledDate(date);
            var conferenceCount = 0;
            if (conferences != null && conferences.Any())
            {
                conferenceCount = conferences.Count;
                conferences.ForEach(async s =>
                {
                    await _videoApiService.CloseConference(s.Id);
                    await _videoApiService.RemoveVirtualCourtRoom(s.HearingRefId);
                }
                );
            }

            return conferenceCount;
        }
    }
}

