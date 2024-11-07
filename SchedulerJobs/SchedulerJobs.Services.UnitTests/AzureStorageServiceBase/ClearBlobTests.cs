using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;
using NUnit.Framework;

namespace SchedulerJobs.Services.UnitTests.AzureStorageServiceBase;

public class ClearBlobTests : AzureStorageServiceBaseTests
{
    [Test]
    public async Task DeletesAllBlobs()
    {
        // Arrange
        var blobClientMock = new Mock<BlobClient>();
        var blobs = new List<BlobItem> { BlobsModelFactory.BlobItem("blob1"), BlobsModelFactory.BlobItem("blob2") };

        var mockPageable = new Mock<Pageable<BlobItem>>();
        mockPageable.Setup(pageable => pageable.GetEnumerator())
            .Returns(() => blobs.GetEnumerator());
        ContainerClientMock
            .Setup(c => c.GetBlobs(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), default))
            .Returns(mockPageable.Object);
        
        ContainerClientMock.Setup(c => c.GetBlobClient(It.IsAny<string>())).Returns(blobClientMock.Object);

        // Act
        await AzureStorageServiceBase.ClearBlobs();

        // Assert
        blobClientMock.Verify(b => b.DeleteAsync(default, default, default), Times.Exactly(blobs.Count));
    }
}