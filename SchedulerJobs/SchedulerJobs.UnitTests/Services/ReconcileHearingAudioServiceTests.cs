using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoApi.Client;
using VideoApi.Contract.Responses;

namespace SchedulerJobs.UnitTests.Services
{
    public class ReconcileHearingAudioServiceTests
    {
        private Mock<IVideoApiClient> _videoApiClient;
        private IReconcileHearingAudioService _reconcileAudioHearingService;
        private Mock<ILogger<ReconcileHearingAudioService>> _logger;
        private List<ConferenceHearingRoomsResponse> conferenceRoomHearingResponses;

        [SetUp]
        public void Setup()
        {
            conferenceRoomHearingResponses = new List<ConferenceHearingRoomsResponse>();
            conferenceRoomHearingResponses.Add(new ConferenceHearingRoomsResponse() { HearingId = Guid.NewGuid().ToString(), FileNamePrefix = string.Empty, Label = string.Empty });

            _videoApiClient = new Mock<IVideoApiClient>();
            _logger = new Mock<ILogger<ReconcileHearingAudioService>>();

            _reconcileAudioHearingService = new ReconcileHearingAudioService(_videoApiClient.Object, _logger.Object);
        }

        [Test]
        public async Task Should_Reconcile_successfully_for_the_filename_with_count()
        {
            _videoApiClient.Setup(x => x.GetConferencesHearingRoomsAsync(It.IsAny<string>())).ReturnsAsync(conferenceRoomHearingResponses);
            _videoApiClient.Setup(x => x.ReconcileAudioFilesInStorageAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(true);

            await _reconcileAudioHearingService.ReconcileAudiorecordingsWithConferencesAsync();

            _videoApiClient.Verify(x => x.ReconcileAudioFilesInStorageAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            _videoApiClient.Verify(x => x.GetConferencesHearingRoomsAsync(It.IsAny<string>()), Times.Once);
        }


        [Test]
        public async Task Should_Reconcile_successfully_for_Multiple_filename_with_count()
        {
            var conferenceRoomHearingResponses1 = new List<ConferenceHearingRoomsResponse>();

            conferenceRoomHearingResponses1.Add(new ConferenceHearingRoomsResponse() { HearingId = Guid.NewGuid().ToString(), FileNamePrefix = string.Empty, Label = string.Empty });
            conferenceRoomHearingResponses1.Add(new ConferenceHearingRoomsResponse() { HearingId = Guid.NewGuid().ToString(), FileNamePrefix = string.Empty, Label = string.Empty });

            _videoApiClient.Setup(x => x.GetConferencesHearingRoomsAsync(It.IsAny<string>())).ReturnsAsync(conferenceRoomHearingResponses1);
            _videoApiClient.SetupSequence(x => x.ReconcileAudioFilesInStorageAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(true).ReturnsAsync(true);

            await _reconcileAudioHearingService.ReconcileAudiorecordingsWithConferencesAsync();

            _videoApiClient.Verify(x => x.ReconcileAudioFilesInStorageAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(2));
            _videoApiClient.Verify(x => x.GetConferencesHearingRoomsAsync(It.IsAny<string>()), Times.Exactly(1));
        }


        [Test]
        public void Should_Reconcile_Catch_Exception_when_Video_Api_GetConferencesHearingRoomsAsync_Throw_Exception()
        {
            _videoApiClient.Setup(x => x.GetConferencesHearingRoomsAsync(It.IsAny<string>())).Throws(new Exception("Error"));

            _videoApiClient.Setup(x => x.ReconcileAudioFilesInStorageAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(true);
                        
            Assert.That(async () => await _reconcileAudioHearingService.ReconcileAudiorecordingsWithConferencesAsync(), Throws.TypeOf<Exception>().With.Message.EqualTo("Error"));
            
            _videoApiClient.Verify(x => x.GetConferencesHearingRoomsAsync(It.IsAny<string>()), Times.Once);
            _videoApiClient.Verify(x => x.ReconcileAudioFilesInStorageAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);

        }

        [Test]
        public async Task Should_Reconcile_Catch_Exception_when_Video_Api_ReconcileAudioFilesInStorageAsync_Throw_Exception()
        {
            _videoApiClient.Setup(x => x.GetConferencesHearingRoomsAsync(It.IsAny<string>())).ReturnsAsync(conferenceRoomHearingResponses);
            _videoApiClient.Setup(x => x.ReconcileAudioFilesInStorageAsync(It.IsAny<string>(), It.IsAny<int>())).Throws(new Exception("Error"));

            await _reconcileAudioHearingService.ReconcileAudiorecordingsWithConferencesAsync();

            _videoApiClient.Verify(x => x.GetConferencesHearingRoomsAsync(It.IsAny<string>()), Times.Once);
            _videoApiClient.Verify(x => x.ReconcileAudioFilesInStorageAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once);

        }
    }
}
