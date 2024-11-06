using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Moq;
using NUnit.Framework;

namespace SchedulerJobs.Services.UnitTests.AzureStorageServiceBase;

public class FileExistsTests : AzureStorageServiceBaseTests
{
    [Test]
    public async Task ReturnsTrue_WhenFileExists()
    {
        // Arrange
        const string filePath = "test.json";
        var blobClientMock = new Mock<BlobClient>();
        
        ContainerClientMock.Setup(c => c.GetBlobClient(It.IsAny<string>())).Returns(blobClientMock.Object);
        blobClientMock.Setup(b => b.ExistsAsync(default)).ReturnsAsync(Response.FromValue(true, null!));

        // Act
        var result = await AzureStorageServiceBase.FileExistsAsync(filePath);

        // Assert
        Assert.IsTrue(result);
    }
}