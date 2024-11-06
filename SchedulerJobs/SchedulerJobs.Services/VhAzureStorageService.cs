using Azure.Storage.Blobs;
using SchedulerJobs.Services.Configuration;

namespace SchedulerJobs.Services
{
    public class VhAzureStorageService : AzureStorageServiceBase, IAzureStorageService
    {
        public VhAzureStorageService(BlobServiceClient serviceClient, AzureConfiguration azureConfig, bool useUserDelegation, IBlobClientExtension blobClientExtension)
        : base(serviceClient, azureConfig, blobClientExtension, useUserDelegation )
        {
        }

        public AzureStorageServiceType AzureStorageServiceType { get; } = AzureStorageServiceType.Vh;
    }
}
