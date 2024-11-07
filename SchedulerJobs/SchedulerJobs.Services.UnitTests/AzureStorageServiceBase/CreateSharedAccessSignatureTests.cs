using System;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Moq;
using NUnit.Framework;

namespace SchedulerJobs.Services.UnitTests.AzureStorageServiceBase;

public class CreateSharedAccessSignatureTests : AzureStorageServiceBaseTests
{
    [Test]
    public async Task ReturnsValidSignature()
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
        ServiceClientMock.Setup(s => s.GetUserDelegationKeyAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), default))
            .ReturnsAsync(Response.FromValue(userDelegationKeyMock.Object, new Mock<Response>().Object));

        // Act
        var result = await AzureStorageServiceBase.CreateSharedAccessSignature(filePath, validUntil);

        // Assert
        const string expectedSignature = $"{Endpoint}{ContainerName}/{filePath}?";
        Assert.That(result.Contains(expectedSignature));
    }
}