using System;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerJobs.Services
{
    public interface ICloseConferenceService
    {
        Task CloseConferencesAsync(DateTime date);
    }

    public class CloseConferenceService : ICloseConferenceService
    {
        private readonly IVideoApiService _videoApiService;

        public CloseConferenceService(IVideoApiService videoApiService)
        {
            _videoApiService = videoApiService;
        }

        public async Task CloseConferencesAsync(DateTime date)
        {
            var conferences = await _videoApiService.GetOpenConferencesByScheduledDate(date);

            if (conferences != null && conferences.Any())
            {
                conferences.ForEach(async s => {

                    await _videoApiService.CloseConference(s.Id);
                    await _videoApiService.RemoveVirtualCourtRoom(s.HearingRefId);
                    }
                );
            }
        }
    }
}

