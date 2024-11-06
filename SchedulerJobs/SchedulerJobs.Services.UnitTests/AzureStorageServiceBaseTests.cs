using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Common.Exceptions;
using SchedulerJobs.Services.Configuration;

namespace SchedulerJobs.Services.UnitTests;

public class AzureStorageServiceBaseTests
{
    private Mock<BlobServiceClient> _serviceClientMock;
    private Mock<IBlobStorageConfiguration> _blobStorageConfigurationMock;
    private Mock<IBlobClientExtension> _blobClientExtensionMock;
    private Mock<BlobContainerClient> _containerClientMock;
    private AzureStorageServiceBase _azureStorageServiceBase;
    private const string ContainerName = "test-container";
    private const string Endpoint = "https://test.com/";
    private const string AccountName = "account-name";

    [SetUp]
    public void SetUp()
    {
        _serviceClientMock = new Mock<BlobServiceClient>();
        _blobStorageConfigurationMock = new Mock<IBlobStorageConfiguration>();
        _blobClientExtensionMock = new Mock<IBlobClientExtension>();
        _containerClientMock = new Mock<BlobContainerClient>();
        
        _serviceClientMock.Setup(s => s.GetBlobContainerClient(It.IsAny<string>())).Returns(_containerClientMock.Object);

        _blobStorageConfigurationMock.SetupGet(c => c.StorageContainerName).Returns(ContainerName);
        _blobStorageConfigurationMock.SetupGet(c => c.StorageEndpoint).Returns(Endpoint);
        _blobStorageConfigurationMock.SetupGet(c => c.StorageAccountName).Returns(AccountName);
        _blobStorageConfigurationMock.SetupGet(c => c.StorageAccountKey).Returns(Convert.ToBase64String("account-key"u8.ToArray()));

        _azureStorageServiceBase = new AzureStorageServiceBase(
            _serviceClientMock.Object,
            _blobStorageConfigurationMock.Object,
            _blobClientExtensionMock.Object,
            useUserDelegation: false);
    }

    [Test]
    public async Task UploadFile_UploadsFileToBlob()
    {
        // Arrange
        const string fileName = "test.json";
        var fileContent = new byte[] { 1, 2, 3 };
        var blobClientMock = new Mock<BlobClient>();
        
        _containerClientMock.Setup(c => c.GetBlobClient(It.IsAny<string>())).Returns(blobClientMock.Object);

        // Act
        await _azureStorageServiceBase.UploadFile(fileName, fileContent);

        // Assert
        blobClientMock.Verify(b => b.UploadAsync(It.IsAny<Stream>()), Times.Once);
    }

    [Test]
    public async Task ClearBlobs_DeletesAllBlobs()
    {
        // Arrange
        var blobClientMock = new Mock<BlobClient>();
        var blobs = new List<BlobItem> { BlobsModelFactory.BlobItem("blob1"), BlobsModelFactory.BlobItem("blob2") };

        var mockPageable = new Mock<Pageable<BlobItem>>();
        mockPageable.Setup(pageable => pageable.GetEnumerator())
            .Returns(() => blobs.GetEnumerator());
        _containerClientMock
            .Setup(c => c.GetBlobs(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), default))
            .Returns(mockPageable.Object);
        
        _containerClientMock.Setup(c => c.GetBlobClient(It.IsAny<string>())).Returns(blobClientMock.Object);

        // Act
        await _azureStorageServiceBase.ClearBlobs();

        // Assert
        blobClientMock.Verify(b => b.DeleteAsync(default, default, default), Times.Exactly(blobs.Count));
    }

    [Test]
    public async Task FileExistsAsync_ReturnsTrue_WhenFileExists()
    {
        // Arrange
        const string filePath = "test.json";
        var blobClientMock = new Mock<BlobClient>();
        
        _containerClientMock.Setup(c => c.GetBlobClient(It.IsAny<string>())).Returns(blobClientMock.Object);
        blobClientMock.Setup(b => b.ExistsAsync(default)).ReturnsAsync(Response.FromValue(true, null!));

        // Act
        var result = await _azureStorageServiceBase.FileExistsAsync(filePath);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task ReconcileFilesInStorage_ReturnsTrue_WhenAllFilesAreValid()
    {
        // Arrange
        const string fileNamePrefix = "json";
        const string blobName = $"blob1.{fileNamePrefix}";

        SetUpBlobs([blobName]);
        
        // Act
        const int expectedCount = 1;
        var result = await _azureStorageServiceBase.ReconcileFilesInStorage(fileNamePrefix, expectedCount);
        
        // Assert
        result.Should().Be(true);
    }

    [Test]
    public void ReconcileFilesInStorage_ThrowsException_WhenFileCountIsLessThanExpected()
    {
        // Arrange
        const string fileNamePrefix = "json";
        const string blob1Name = $"blob1.{fileNamePrefix}";
        const string blob2Name = $"blob2.{fileNamePrefix}";
        var blobs = new List<BlobItem>
        {
            BlobsModelFactory.BlobItem(name: blob1Name),
            BlobsModelFactory.BlobItem(name: blob2Name)
        };
        
        SetUpBlobs([blob1Name, blob2Name]);

        // Act & Assert
        const int expectedCount = 5;
        var actualCount = blobs.Count;
        var ex = Assert.ThrowsAsync<AudioPlatformFileNotFoundException>(() =>
            _azureStorageServiceBase.ReconcileFilesInStorage(fileNamePrefix, expectedCount));
        Assert.That(ex.Message, Does.Contain($"Expected: {expectedCount} Actual: {actualCount}"));
    }

    [Test]
    public void ReconcileFilesInStorage_ThrowsException_WhenFilesAreEmpty()
    {
        // Arrange
        const string fileNamePrefix = "json";
        const string blobName = $"blob1.{fileNamePrefix}";
        var blobs = new List<BlobItem>
        {
            BlobsModelFactory.BlobItem(name: blobName)
        };
        
        SetUpBlobs([blobName], contentLength: 0);
        
        // Act & Assert
        const int expectedCount = 1;
        var actualCount = blobs.Count;
        var ex = Assert.ThrowsAsync<AudioPlatformFileNotFoundException>(() =>
            _azureStorageServiceBase.ReconcileFilesInStorage(fileNamePrefix, expectedCount));
        Assert.That(ex.Message, Does.Contain($"Expected: {expectedCount} Actual: {actualCount}"));
        Assert.That(ex.Message, Does.Contain("Empty audio file"));
    }

    [Test]
    public async Task CreateSharedAccessSignature_ReturnsValidSignature()
    {
        // Arrange
        const string filePath = "test.txt";
        var validUntil = TimeSpan.FromHours(1);

        var blobSasBuilder = new BlobSasBuilder
        {
            BlobContainerName = "test-container",
            BlobName = filePath,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddHours(-1),
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
        };
        blobSasBuilder.SetPermissions(BlobSasPermissions.Read);

        var userDelegationKeyMock = new Mock<UserDelegationKey>();
        _serviceClientMock.Setup(s => s.GetUserDelegationKeyAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default))
            .ReturnsAsync(Response.FromValue(userDelegationKeyMock.Object, new Mock<Response>().Object));

        // Act
        var result = await _azureStorageServiceBase.CreateSharedAccessSignature(filePath, validUntil);

        // Assert
        const string expectedSignature = $"{Endpoint}{ContainerName}/{filePath}?";
        Assert.That(result.Contains(expectedSignature));
    }

    private void SetUpBlobs(List<string> names, int contentLength = 100)
    {
        var blobs = new List<BlobItem>();
        
        foreach (var name in names)
        {
            var blobClientMock = new Mock<BlobClient>();
            _containerClientMock.Setup(c => c.GetBlobClient(name)).Returns(blobClientMock.Object);
            
            var blobProperties = BlobsModelFactory.BlobProperties(contentLength: contentLength);
            var blobPropertiesResponse = Response.FromValue(blobProperties, new Mock<Response>().Object);
            _blobClientExtensionMock.Setup(e => e.GetPropertiesAsync(blobClientMock.Object))
                .ReturnsAsync(blobPropertiesResponse);
        
            blobClientMock.SetupGet(b => b.Name).Returns(name);
            
            blobs.Add(BlobsModelFactory.BlobItem(name: name));
        }

        var asyncPageable = AsyncPageable<BlobItem>.FromPages(new[] { Page<BlobItem>.FromValues(blobs, null, new Mock<Response>().Object) });
        _containerClientMock
            .Setup(c => c.GetBlobsAsync(default, default, It.IsAny<string>(), default))
            .Returns(asyncPageable);
    }
}