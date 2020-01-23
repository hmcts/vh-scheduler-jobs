using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SchedulerJobs.Services.VideoApi.Contracts;

namespace SchedulerJobs.Services
{
    public class VideoApiServiceFake : IVideoApiService
    {
        public Task CloseConference(Guid conferenceId)
        {
            return Task.FromResult(HttpStatusCode.OK);
        }

        public Task<List<ExpiredConferencesResponse>> GetExpiredOpenConferences()
        {
            return Task.FromResult(new List<ExpiredConferencesResponse>
            {
                new ExpiredConferencesResponse
                {
                    Id = Guid.NewGuid()
                }
            });
        }
    }
}
