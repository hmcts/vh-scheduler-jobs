using System;

namespace SchedulerJobs.Services.VideoApi.Contracts
{
    public class ExpiredConferencesResponse
    {
        /// <summary>
        /// The conference's UUID
        /// </summary>
        public Guid Id { get; set; }
    }

    public enum ConferenceState
    {
        NotStarted = 0,
        InSession = 1,
        Paused = 2,
        Suspended = 3,
        Closed = 4
    }
}
