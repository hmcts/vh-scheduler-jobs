using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;
using NUnit.Framework;

namespace SchedulerJobs.Services.UnitTests;

public class BlobClientExtensionTests
{
    private Mock<BlobClient> _blobClientMock;
    private BlobClientExtension _blobClientExtension;

    [SetUp]
    public void SetUp()
    {
        _blobClientMock = new Mock<BlobClient>();
        _blobClientExtension = new BlobClientExtension();
    }

    [Test]
    public async Task GetPropertiesAsync_ReturnsBlobProperties()
    {
        // Arrange
        var expectedProperties = BlobsModelFactory.BlobProperties();
        var responseMock = new Mock<Response<BlobProperties>>();
        responseMock.Setup(r => r.Value).Returns(expectedProperties);

        _blobClientMock.Setup(b => b.GetPropertiesAsync(null, default))
            .ReturnsAsync(responseMock.Object);

        // Act
        var result = await _blobClientExtension.GetPropertiesAsync(_blobClientMock.Object);

        // Assert
        Assert.That(result, Is.EqualTo(expectedProperties));
    }
}