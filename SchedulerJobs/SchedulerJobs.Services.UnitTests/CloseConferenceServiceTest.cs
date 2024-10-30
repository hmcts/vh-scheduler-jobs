using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using VideoApi.Client;
using VideoApi.Contract.Responses;

namespace SchedulerJobs.Services.UnitTests
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
    }
}
