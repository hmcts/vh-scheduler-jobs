namespace SchedulerJobs.Services.VideoApi.Contracts
{
    public class ParticipantSummaryResponse
    {
        /// <summary>
        /// The participant username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The participant display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The current participant status
        /// </summary>
        public ParticipantState Status { get; set; }

        /// <summary>
        /// The participant role in conference
        /// </summary>
        public UserRole UserRole { get; set; }

        /// <summary>
        /// The representee (if participant is a representative)
        /// </summary>
        public string Representee { get; set; }

        /// <summary>
        /// The group a participant belongs to
        /// </summary>
        public string CaseGroup { get; set; }
    }

    public enum ParticipantState
    {
        None = 0,
        NotSignedIn = 1,
        UnableToJoin = 2,
        Joining = 3,
        Available = 4,
        InHearing = 5,
        InConsultation = 6,
        Disconnected = 7
    }
    public enum UserRole
    {
        None = 0,
        CaseAdmin = 1,
        VideoHearingsOfficer = 2,
        HearingFacilitationSupport = 3,
        Judge = 4,
        Individual = 5,
        Representative = 6
    }
}
