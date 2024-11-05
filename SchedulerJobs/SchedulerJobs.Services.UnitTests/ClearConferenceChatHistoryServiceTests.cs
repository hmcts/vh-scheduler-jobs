using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VideoApi.Client;
using VideoApi.Contract.Responses;

namespace SchedulerJobs.Services.UnitTests;

public class ClearConferenceChatHistoryServiceTests
{
    private Mock<IVideoApiClient> _videoApiClientMock;
    private ClearConferenceChatHistoryService _service;
    
    [SetUp]
    public void SetUp()
    {
        _videoApiClientMock = new Mock<IVideoApiClient>();
        _service = new ClearConferenceChatHistoryService(_videoApiClientMock.Object);
    }

    [Test]
    public async Task ClearChatHistoryForClosedConferences_CallsRemoveInstantMessagesAsync_ForEachClosedConference()
    {
        // Arrange
        var closedConferences = new List<ClosedConferencesResponse>
        {
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() }
        };

        _videoApiClientMock.Setup(x => x.GetClosedConferencesWithInstantMessagesAsync())
            .ReturnsAsync(closedConferences);

        // Act
        await _service.ClearChatHistoryForClosedConferences();

        // Assert
        foreach (var conference in closedConferences)
        {
            _videoApiClientMock.Verify(x => x.RemoveInstantMessagesAsync(conference.Id), Times.Once);
        }
    }

    [Test]
    public async Task ClearChatHistoryForClosedConferences_DoesNotCallRemoveInstantMessagesAsync_WhenNoClosedConferences()
    {
        // Arrange
        _videoApiClientMock.Setup(x => x.GetClosedConferencesWithInstantMessagesAsync())
            .ReturnsAsync(new List<ClosedConferencesResponse>());

        // Act
        await _service.ClearChatHistoryForClosedConferences();

        // Assert
        _videoApiClientMock.Verify(x => x.RemoveInstantMessagesAsync(It.IsAny<Guid>()), Times.Never);
    }
}