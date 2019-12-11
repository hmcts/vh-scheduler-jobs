using Microsoft.Extensions.Configuration;

namespace SchedulerJobs.Common.Configuration
{
    public class ConfigLoader
    {
        public readonly IConfiguration Configuration;
        public ConfigLoader()
        {
            var configRootBuilder = new ConfigurationBuilder()
                   .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables();
            Configuration = configRootBuilder.Build();
        }
    }
}
