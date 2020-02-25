using System;

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
        public string GetExpiredOpenConferences() => $"{ApiRoot}/expired";
        public string CloseConference(Guid conferenceId) => $"{ApiRoot}/{conferenceId}/close";
        public string ClearConferenceChatHistory(Guid conferenceId) => $"{ApiRoot}/{conferenceId}/messages";
        public string GetClosedConferencesWithInstantMessageHistory() => $"{ApiRoot}/ExpiredIM";
    }
}