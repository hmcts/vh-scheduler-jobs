using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Common.Security;
using SchedulerJobs;
using SchedulerJobs.Services;
using SchedulerJobs.Services.VideoApi.Contracts;
using System.Threading.Tasks;

namespace SchedulerJobs.UnitTests
{
    [TestFixture]
    public class CloseConferenceServiceTest
    {
        private  Mock<IVideoApiService> _videoApiService;
        private  ICloseConferenceService _closeConferenceService;
       
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

            var conferences = new List<ConferenceSummaryResponse>();
            _videoApiService.Setup(x => x.GetOpenConferencesByScheduledDate(It.IsAny<DateTime>())).Returns(Task.FromResult(conferences));

            _closeConferenceService.CloseConferencesAsync(DateTime.Now);
            _videoApiService.Verify(x => x.CloseConference(It.IsAny<Guid>()), Times.Never);
            _videoApiService.Verify(x => x.RemoveVirtualCourtRoom(It.IsAny<Guid>()), Times.Never);

        }

        [Test]
        public void Conferences_is_null_and_nothing_done()
        {
            _videoApiService = new Mock<IVideoApiService>();
            _closeConferenceService = new CloseConferenceService(_videoApiService.Object);

            List<ConferenceSummaryResponse> conferences = null;
            _videoApiService.Setup(x => x.GetOpenConferencesByScheduledDate(It.IsAny<DateTime>())).Returns(Task.FromResult(conferences));

            _closeConferenceService.CloseConferencesAsync(DateTime.Now);
            _videoApiService.Verify(x => x.CloseConference(It.IsAny<Guid>()), Times.Never);
            _videoApiService.Verify(x => x.RemoveVirtualCourtRoom(It.IsAny<Guid>()), Times.Never);

        }

        [Test]
        public void Close_conferences_and_remove_virtual_court_rooms()
        {
            var response = new ConferenceSummaryResponse
            {
                HearingRefId = new Guid("45857c71-b47e-48a4-9434-006cc49d8a3b"),
                Id = new Guid("a02dea09-4442-424d-bcaa-033d703e5cb7"),
            };
            var conferences = new List<ConferenceSummaryResponse> { response };
            _videoApiService.Setup(x => x.GetOpenConferencesByScheduledDate(It.IsAny<DateTime>())).Returns(Task.FromResult(conferences));

            _closeConferenceService.CloseConferencesAsync(DateTime.Now);
            _videoApiService.Verify(x => x.CloseConference(It.IsAny<Guid>()), Times.AtLeastOnce);
            _videoApiService.Verify(x => x.RemoveVirtualCourtRoom(It.IsAny<Guid>()), Times.AtLeastOnce);

        }
    }
}
