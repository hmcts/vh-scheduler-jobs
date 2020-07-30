using System.Collections.Generic;

namespace SchedulerJobs.Services.BookingApi.Contracts
{
    /// <summary>
    /// List of users with closed conferences
    /// </summary>
    public class UserWithClosedConferencesResponse
    {
        /// <summary>
        /// The usernames to remove from Aad
        /// </summary>
        public IList<string> Usernames { get; set; }
    }
}
