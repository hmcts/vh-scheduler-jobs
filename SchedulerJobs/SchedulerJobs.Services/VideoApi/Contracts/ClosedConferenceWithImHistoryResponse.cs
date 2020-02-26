using System;

namespace SchedulerJobs.Services.VideoApi.Contracts
{
    /// <summary>
    /// A closed conference with chat history that needs to be cleared
    /// </summary>
    public class ClosedConferenceWithImHistoryResponse
    {
        /// <summary>
        /// The conference's UUID
        /// </summary>
        public Guid Id { get; set; }
    }
}