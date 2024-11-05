using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SchedulerJobs.Services.Configuration
{
    [ExcludeFromCodeCoverage]
    public class AzureConfiguration : IBlobStorageConfiguration
    {
        public IEnumerable<string> RestApiEndpoints { get; set; }
        public string StreamingEndpoint { get; set; }
        public string ServerName { get; set; }
        public string HostName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string StorageDirectory { get; set; }
        public string AzureStorageDirectory { get; set; }
        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }
        public string StorageContainerName { get; set; }
        public string StorageEndpoint { get; set; }
        public string ManagedIdentityClientId { get; set; }
    }
}
