using SchedulerJobs.Common.Configuration;

namespace SchedulerJobs.AcceptanceTests.Configuration
{
    public class Config
    {
        public AzureAdConfiguration AzureAdConfiguration { get; set; }
        public ServicesConfiguration Services { get; set; }
    }
}
