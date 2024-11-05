using System.Diagnostics.CodeAnalysis;

namespace SchedulerJobs.Sds.Configuration
{
    [ExcludeFromCodeCoverage]
    public class ConnectionStrings
    {
        public string RedisCache { get; set; } = string.Empty;
    }
}
