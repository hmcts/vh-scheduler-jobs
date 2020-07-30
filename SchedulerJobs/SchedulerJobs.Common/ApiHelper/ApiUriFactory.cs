using System;

namespace SchedulerJobs.Common.ApiHelper
{
    public class ApiUriFactory
    {
        public ConferenceEndpoints ConferenceEndpoints { get; }
        public HearingsEndpoints HearingsEndpoints { get; }
        public UserEndpoints UserEndpoints { get; }
        public PersonsEndpoints PersonsEndpoints { get; }

        public ApiUriFactory()
        {
            ConferenceEndpoints = new ConferenceEndpoints();
            HearingsEndpoints = new HearingsEndpoints();
            UserEndpoints = new UserEndpoints();
            PersonsEndpoints = new PersonsEndpoints();
        }
    }

    public class ConferenceEndpoints
    {
        private string ApiRoot => "conferences";
        public string UpdateConference => $"{ApiRoot}";
        public string GetExpiredOpenConferences() => $"{ApiRoot}/expired";
        public string CloseConference(Guid conferenceId) => $"{ApiRoot}/{conferenceId}/close";
        public string ClearConferenceChatHistory(Guid conferenceId) => $"{ApiRoot}/{conferenceId}/instantmessages";
        public string GetClosedConferencesWithInstantMessageHistory() => $"{ApiRoot}/ExpiredIM";
        public string GetExpiredAudiorecordingConferences => $"{ApiRoot}/audiorecording/expired";
        public string DeleteAudioApplication(Guid hearingId) => $"{ApiRoot}/audioapplications/{hearingId}";
        public string AnonymiseConferences => $"{ApiRoot}/anonymiseconferences";
    }

    public class HearingsEndpoints
    {
        private string ApiRoot => "hearings";
        public string AnonymiseHearings() => $"{ApiRoot}/anonymisehearings";
        public string GetUserWithClosedHearings() => $"{ApiRoot}/userswithclosedhearings";
    }

    public class PersonsEndpoints
    {
        private string ApiRoot => "persons";
        public string GetUserWithClosedHearings() => $"{ApiRoot}/userswithclosedhearings";
    }

    public class UserEndpoints
    {
        private string ApiRoot => "users";
        public string DeleteUser(string userName) => $"{ApiRoot}/username/{userName}";
    }
}