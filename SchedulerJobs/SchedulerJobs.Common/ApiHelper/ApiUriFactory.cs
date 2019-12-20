using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerJobs.Common.ApiHelper
{
    public class ApiUriFactory
    {
        public ConferenceEndpoints ConferenceEndpoints { get; }

        public ApiUriFactory()
        {
            ConferenceEndpoints = new ConferenceEndpoints();
        }
    }

    public class ConferenceEndpoints
    {
        private string ApiRoot => "conferences";
        public string UpdateConference => $"{ApiRoot}";

        public string GetOpenConferencesByScheduledDate(string scheduledDate) => $"{ApiRoot}/fromdate?scheduledDate={scheduledDate}";

        public string CloseConference(Guid conferenceId) => $"{ApiRoot}/{conferenceId}/close";
    }
}
