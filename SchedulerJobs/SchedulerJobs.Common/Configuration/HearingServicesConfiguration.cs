namespace SchedulerJobs.Common.Configuration
{
    public class HearingServicesConfiguration
    {
        public string BookingApiUrl { get; set; } = "https://localhost";
        public bool EnableBookingApiStub { get; set; }
    }
}
