﻿namespace SchedulerJobs.Common.Configuration
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
        public string ELinksPeoplesBaseUrl { get; set; } = "https://localhost:9999/";
        public string ELinksLeaversBaseUrl { get; set; } = "https://localhost:9988/";
        public string ELinksApiKey { get; set; }
        public string ELinksPeoplesBaseUrlV2 { get; set; } = "https://localhost:9999/";
        public string ELinksLeaversBaseUrlV2 { get; set; } = "https://localhost:9988/";
        public string ELinksApiKeyV2 { get; set; }
        public int ELinksApiGetPeopleUpdatedSinceDays { get; set; }
        public string NotificationApiResourceId { get; set; }
        public string NotificationApiUrl { get; set; } = "https://localhost:59390/";
    }
}
