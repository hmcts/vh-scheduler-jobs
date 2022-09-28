using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;
using VideoApi.Client;
using VideoApi.Contract.Responses;

namespace SchedulerJobs.CronJobs.UnitTests.Services
{
    [TestFixture]
    public class CloseConferenceServiceTest
    {
        private Mock<IVideoApiClient> _videoApiClient;
        private ICloseConferenceService _closeConferenceService;

        [SetUp]
        public void Setup()
        {
            _videoApiClient = new Mock<IVideoApiClient>();
            _closeConferenceService = new CloseConferenceService(_videoApiClient.Object);
        }

        [Test]
        public void Empty_list_of_conferences_and_nothing_done()
        {
            _videoApiClient = new Mock<IVideoApiClient>();
            _closeConferenceService = new CloseConferenceService(_videoApiClient.Object);

            var conferences = new List<ExpiredConferencesResponse>();
            _videoApiClient.Setup(x => x.GetExpiredOpenConferencesAsync()).ReturnsAsync(conferences);

            _closeConferenceService.CloseConferencesAsync();
            _videoApiClient.Verify(x => x.CloseConferenceAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Conferences_is_null_and_nothing_done()
        {
            _videoApiClient = new Mock<IVideoApiClient>();
            _closeConferenceService = new CloseConferenceService(_videoApiClient.Object);

            _videoApiClient.Setup(x => x.GetExpiredOpenConferencesAsync()).ReturnsAsync((List<ExpiredConferencesResponse>) null);

            _closeConferenceService.CloseConferencesAsync();
            _videoApiClient.Verify(x => x.CloseConferenceAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Close_conferences_and_remove_virtual_court_rooms()
        {
            var response = new ExpiredConferencesResponse
            {
                Id = new Guid("a02dea09-4442-424d-bcaa-033d703e5cb7"),
            };
           
            var conferences = new List<ExpiredConferencesResponse> { response };
            _videoApiClient.Setup(x => x.GetExpiredOpenConferencesAsync()).ReturnsAsync(conferences);

            _closeConferenceService.CloseConferencesAsync();
            _videoApiClient.Verify(x => x.CloseConferenceAsync(It.IsAny<Guid>()), Times.AtLeastOnce);
        }

        [Test]
        public void Should_return_empty_list_of_closed_conferences()
        {
            _videoApiClient = new Mock<IVideoApiClient>();
            _closeConferenceService = new CloseConferenceService(_videoApiClient.Object);

            var conferences = new List<ExpiredConferencesResponse>();
            _videoApiClient.Setup(x => x.GetExpiredAudiorecordingConferencesAsync()).ReturnsAsync(conferences);

            var result =_closeConferenceService.DeleteAudiorecordingApplicationsAsync();
            _videoApiClient.Verify(x => x.DeleteAudioApplicationAsync(It.IsAny<Guid>()), Times.Never);
            Assert.AreEqual(0, result.Result);
        }

        [Test]
        public void Should_return_null_for_closed_conferences_and_nothing_done()
        {
            _videoApiClient = new Mock<IVideoApiClient>();
            _closeConferenceService = new CloseConferenceService(_videoApiClient.Object);

            _videoApiClient.Setup(x => x.GetExpiredAudiorecordingConferencesAsync()).ReturnsAsync((List<ExpiredConferencesResponse>)null);

            var result = _closeConferenceService.DeleteAudiorecordingApplicationsAsync();
            _videoApiClient.Verify(x => x.DeleteAudioApplicationAsync(It.IsAny<Guid>()), Times.Never);
            Assert.AreEqual(0, result.Result);
        }

        [Test]
        public void Should_delete_audio_applications_for_closed_conferences_with_audio_recording()
        {
            var response = new ExpiredConferencesResponse
            {
                HearingId = new Guid("a02dea09-4442-424d-bcaa-033d703e5cb7"),
            };

            var conferences = new List<ExpiredConferencesResponse> { response };
            _videoApiClient.Setup(x => x.GetExpiredAudiorecordingConferencesAsync()).ReturnsAsync(conferences);

           var result = _closeConferenceService.DeleteAudiorecordingApplicationsAsync();
            _videoApiClient.Verify(x => x.DeleteAudioApplicationAsync(It.IsAny<Guid>()), Times.AtLeastOnce);
            Assert.AreEqual(1, result.Result);
        }

        [Test]
        public void Should_not_found_audio_file_and_throw_exception()
        {
            var response = new ExpiredConferencesResponse
            {
                HearingId = new Guid("a02dea09-4442-424d-bcaa-033d703e5cb7"),
            };

            var conferences = new List<ExpiredConferencesResponse> { response };
            _videoApiClient.Setup(x => x.GetExpiredAudiorecordingConferencesAsync()).ReturnsAsync(conferences);
            _videoApiClient.Setup(x => x.DeleteAudioApplicationAsync(It.IsAny<Guid>())).Throws(new Exception());
            var result = _closeConferenceService.DeleteAudiorecordingApplicationsAsync();
            _videoApiClient.Verify(x => x.DeleteAudioApplicationAsync(It.IsAny<Guid>()), Times.AtLeastOnce);
            Assert.AreEqual(0, result.Result);
        }
    }
}
