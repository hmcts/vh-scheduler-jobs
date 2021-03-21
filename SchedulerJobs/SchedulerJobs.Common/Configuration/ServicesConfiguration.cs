namespace SchedulerJobs.Common.Configuration
{
    public class ServicesConfiguration
    {
        public string BookingsApiResourceId { get; set; }
        public string BookingsApiUrl { get; set; } = "https://localhost:5300/";
        public string SchedulerJobsUrl { get; set; }
        public string VideoApiResourceId { get; set; }
        public string VideoApiUrl { get; set; } = "https://localhost:59390/";
        public string UserApiResourceId { get; set; }
        public string UserApiUrl { get; set; } = "https://localhost:5200/";
        public string ELinksApiUrl { get; set; } = "https://localhost:9999/";
        public string ELinksApiKey { get; set; }
        public int ELinksApiPeopleFromDaysAgo { get; set; }
    }
}
