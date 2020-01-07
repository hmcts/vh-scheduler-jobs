using System;

namespace SchedulerJobs.Common.ApiHelper
{
    public class ApiUriFactory
    {
        public ConferenceEndpoints ConferenceEndpoints { get; }

        public VirtualCourtRoomEndpoints VirtualCourtRoomEndpoints { get; }

        public ApiUriFactory()
        {
            ConferenceEndpoints = new ConferenceEndpoints();
            VirtualCourtRoomEndpoints = new VirtualCourtRoomEndpoints();
        }
    }

    public class ConferenceEndpoints
    {
        private string ApiRoot => "conferences";
        public string UpdateConference => $"{ApiRoot}";

        public string GetOpenConferencesByScheduledDate(string scheduledDate) => $"{ApiRoot}/fromdate?scheduledDate={scheduledDate}";

        public string CloseConference(Guid conferenceId) => $"{ApiRoot}/{conferenceId}/close";
    }

    public class VirtualCourtRoomEndpoints
    {
        private string ApiRoot => "virtualCourtRooms";

        public string RemoveVirtualCourtRoom(Guid virtualCourtRoomId) => $"{ApiRoot}/{virtualCourtRoomId}/";
    }
}
