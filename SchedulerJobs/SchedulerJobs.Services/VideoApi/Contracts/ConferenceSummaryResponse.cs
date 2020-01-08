using System;
using System.Collections.Generic;

namespace SchedulerJobs.Services.VideoApi.Contracts
{
    public class ConferenceSummaryResponse
    {
        public Guid Id { get; set; }
        public DateTime ScheduledDateTime { get; set; }
        public DateTime? ClosedDateTime { get; set; }
        public string CaseType { get; set; }
        public string CaseNumber { get; set; }
        public string CaseName { get; set; }
        public int ScheduledDuration { get; set; }
        public ConferenceState Status { get; set; }
        public List<ParticipantSummaryResponse> Participants { get; set; }
        public int PendingTasks { get; set; }
        public Guid HearingRefId { get; set; }
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
