using Azure.Storage.Blobs;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Services.Configuration;

namespace SchedulerJobs.Services.UnitTests;

public class VhAzureStorageServiceTests
{
    private Mock<BlobServiceClient> _serviceClientMock;
    private Mock<AzureConfiguration> _azureConfigMock;
    private Mock<IBlobClientExtension> _blobClientExtensionMock;
    private VhAzureStorageService _vhAzureStorageService;

    [SetUp]
    public void SetUp()
    {
        _serviceClientMock = new Mock<BlobServiceClient>();
        _azureConfigMock = new Mock<AzureConfiguration>();
        _blobClientExtensionMock = new Mock<IBlobClientExtension>();

        _vhAzureStorageService = new VhAzureStorageService(
            _serviceClientMock.Object,
            _azureConfigMock.Object,
            useUserDelegation: false,
            _blobClientExtensionMock.Object);
    }

    [Test]
    public void Constructor_InitializesProperties()
    {
        // Arrange & Act
        var service = new VhAzureStorageService(
            _serviceClientMock.Object,
            _azureConfigMock.Object,
            useUserDelegation: false,
            _blobClientExtensionMock.Object);

        // Assert
        Assert.IsNotNull(service);
        Assert.AreEqual(AzureStorageServiceType.Vh, service.AzureStorageServiceType);
    }

    [Test]
    public void AzureStorageServiceType_ReturnsVh()
    {
        // Act
        var result = _vhAzureStorageService.AzureStorageServiceType;

        // Assert
        Assert.AreEqual(AzureStorageServiceType.Vh, result);
    }
}