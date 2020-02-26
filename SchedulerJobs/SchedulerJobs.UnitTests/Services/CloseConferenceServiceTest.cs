using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;
using SchedulerJobs.Services.VideoApi.Contracts;

namespace SchedulerJobs.UnitTests.Services
{
    [TestFixture]
    public class CloseConferenceServiceTest
    {
        private Mock<IVideoApiService> _videoApiService;
        private ICloseConferenceService _closeConferenceService;

        [SetUp]
        public void Setup()
        {
            _videoApiService = new Mock<IVideoApiService>();
            _closeConferenceService = new CloseConferenceService(_videoApiService.Object);
        }

        [Test]
        public void Empty_list_of_conferences_and_nothing_done()
        {
            _videoApiService = new Mock<IVideoApiService>();
            _closeConferenceService = new CloseConferenceService(_videoApiService.Object);

            var conferences = new List<ExpiredConferencesResponse>();
            _videoApiService.Setup(x => x.GetExpiredOpenConferences()).Returns(Task.FromResult(conferences));

            _closeConferenceService.CloseConferencesAsync();
            _videoApiService.Verify(x => x.CloseConference(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Conferences_is_null_and_nothing_done()
        {
            _videoApiService = new Mock<IVideoApiService>();
            _closeConferenceService = new CloseConferenceService(_videoApiService.Object);

            _videoApiService.Setup(x => x.GetExpiredOpenConferences()).Returns(Task.FromResult((List<ExpiredConferencesResponse>) null));

            _closeConferenceService.CloseConferencesAsync();
            _videoApiService.Verify(x => x.CloseConference(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Close_conferences_and_remove_virtual_court_rooms()
        {
            var response = new ExpiredConferencesResponse
            {
                Id = new Guid("a02dea09-4442-424d-bcaa-033d703e5cb7"),
            };
           
            var conferences = new List<ExpiredConferencesResponse> { response };
            _videoApiService.Setup(x => x.GetExpiredOpenConferences()).Returns(Task.FromResult(conferences));

            _closeConferenceService.CloseConferencesAsync();
            _videoApiService.Verify(x => x.CloseConference(It.IsAny<Guid>()), Times.AtLeastOnce);
        }
    }
}
