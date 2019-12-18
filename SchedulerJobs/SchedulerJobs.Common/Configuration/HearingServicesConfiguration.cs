namespace SchedulerJobs.Common.Configuration
{
    public class HearingServicesConfiguration
    {
        public string VideoApiResourceId { get; set; }

        public string VideoApiUrl { get; set; } = "https://localhost";
        public bool EnableVideoApiStub { get; set; }
    }
}
