using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using NUnit.Framework;
using SchedulerJobs.Common.Exceptions;

namespace SchedulerJobs.Services.UnitTests.AzureStorageServiceBase;

public class ReconcileFilesInStorageTests : AzureStorageServiceBaseTests
{
    [Test]
    public async Task ReturnsTrue_WhenAllFilesAreValid()
    {
        // Arrange
        const string fileNamePrefix = "json";
        const string blobName = $"blob1.{fileNamePrefix}";

        SetUpBlobs([blobName]);
        
        // Act
        const int expectedCount = 1;
        var result = await AzureStorageServiceBase.ReconcileFilesInStorage(fileNamePrefix, expectedCount);
        
        // Assert
        result.Should().Be(true);
    }

    [Test]
    public void ThrowsException_WhenFileCountIsLessThanExpected()
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
            AzureStorageServiceBase.ReconcileFilesInStorage(fileNamePrefix, expectedCount));
        Assert.That(ex.Message, Does.Contain($"Expected: {expectedCount} Actual: {actualCount}"));
    }

    [Test]
    public void ThrowsException_WhenFilesAreEmpty()
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
            AzureStorageServiceBase.ReconcileFilesInStorage(fileNamePrefix, expectedCount));
        Assert.That(ex.Message, Does.Contain($"Expected: {expectedCount} Actual: {actualCount}"));
        Assert.That(ex.Message, Does.Contain("Empty audio file"));
    }
}