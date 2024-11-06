using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Moq;
using NUnit.Framework;

namespace SchedulerJobs.Services.UnitTests.AzureStorageServiceBase;

public class UploadFileTests : AzureStorageServiceBaseTests
{
    [Test]
    public async Task UploadsFileToBlob()
    {
        // Arrange
        const string fileName = "test.json";
        var fileContent = new byte[] { 1, 2, 3 };
        var blobClientMock = new Mock<BlobClient>();
        
        ContainerClientMock.Setup(c => c.GetBlobClient(It.IsAny<string>())).Returns(blobClientMock.Object);

        // Act
        await AzureStorageServiceBase.UploadFile(fileName, fileContent);

        // Assert
        blobClientMock.Verify(b => b.UploadAsync(It.IsAny<Stream>()), Times.Once);
    }
}