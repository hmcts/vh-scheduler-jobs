using System;
using System.Collections.Generic;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Services.Configuration;

namespace SchedulerJobs.Services.UnitTests.AzureStorageServiceBase;

public abstract class AzureStorageServiceBaseTests
{
    protected Mock<BlobServiceClient> ServiceClientMock;
    protected Mock<BlobContainerClient> ContainerClientMock;
    protected Services.AzureStorageServiceBase AzureStorageServiceBase;
    protected const string ContainerName = "test-container";
    protected const string Endpoint = "https://test.com/";
    private const string AccountName = "account-name";
    private Mock<IBlobStorageConfiguration> _blobStorageConfigurationMock;
    private Mock<IBlobClientExtension> _blobClientExtensionMock;

    [SetUp]
    public void SetUp()
    {
        ServiceClientMock = new Mock<BlobServiceClient>();
        _blobStorageConfigurationMock = new Mock<IBlobStorageConfiguration>();
        _blobClientExtensionMock = new Mock<IBlobClientExtension>();
        ContainerClientMock = new Mock<BlobContainerClient>();
        
        ServiceClientMock.Setup(s => s.GetBlobContainerClient(It.IsAny<string>())).Returns(ContainerClientMock.Object);

        _blobStorageConfigurationMock.SetupGet(c => c.StorageContainerName).Returns(ContainerName);
        _blobStorageConfigurationMock.SetupGet(c => c.StorageEndpoint).Returns(Endpoint);
        _blobStorageConfigurationMock.SetupGet(c => c.StorageAccountName).Returns(AccountName);
        _blobStorageConfigurationMock.SetupGet(c => c.StorageAccountKey).Returns(Convert.ToBase64String("account-key"u8.ToArray()));

        AzureStorageServiceBase = new Services.AzureStorageServiceBase(
            ServiceClientMock.Object,
            _blobStorageConfigurationMock.Object,
            _blobClientExtensionMock.Object,
            useUserDelegation: false);
    }

    protected void SetUpBlobs(List<string> names, int contentLength = 100)
    {
        var blobs = new List<BlobItem>();
        
        foreach (var name in names)
        {
            var blobClientMock = new Mock<BlobClient>();
            ContainerClientMock.Setup(c => c.GetBlobClient(name)).Returns(blobClientMock.Object);
            
            var blobProperties = BlobsModelFactory.BlobProperties(contentLength: contentLength);
            var blobPropertiesResponse = Response.FromValue(blobProperties, new Mock<Response>().Object);
            _blobClientExtensionMock.Setup(e => e.GetPropertiesAsync(blobClientMock.Object))
                .ReturnsAsync(blobPropertiesResponse);
        
            blobClientMock.SetupGet(b => b.Name).Returns(name);
            
            blobs.Add(BlobsModelFactory.BlobItem(name: name));
        }

        var asyncPageable = AsyncPageable<BlobItem>.FromPages(new[] { Page<BlobItem>.FromValues(blobs, null, new Mock<Response>().Object) });
        ContainerClientMock
            .Setup(c => c.GetBlobsAsync(default, default, It.IsAny<string>(), default))
            .Returns(asyncPageable);
    }
}