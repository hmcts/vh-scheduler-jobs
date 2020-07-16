using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;
using SchedulerJobs.Services.VideoApi.Contracts;
using System;
using System.Linq;
using System.Threading.Tasks;
using Testing.Common;

namespace SchedulerJobs.UnitTests.Functions.DeleteAudiorecordingApplicationsFunction
{
    public class RunDeleteAudiorecordingApplicationsFunctionTest
    {
        private Mock<IVideoApiService> _videoApiServiceMock;
        private readonly TimerInfo _timerInfo = new TimerInfo(new ScheduleStub(), new ScheduleStatus(), true);

        [SetUp]
        public void Setup()
        {
            _videoApiServiceMock = new Mock<IVideoApiService>();
            var conferences = Builder<ExpiredConferencesResponse>.CreateListOfSize(3).All()
               .With(x => x.HearingId = Guid.NewGuid()).Build().ToList();
            var result = Task.FromResult(conferences);
            _videoApiServiceMock.Setup(x => x.GetExpiredAudiorecordingConferences()).Returns(result);
            _videoApiServiceMock.Setup(x => x.DeleteAudiorecordingApplication(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Timer_should_log_message_all_audio_app_were_deleted()
        {
            var logger = (LoggerFake)TestFactory.CreateLogger(LoggerTypes.List);
            await SchedulerJobs.Functions.DeleteAudiorecordingApplicationsFunction.Run(_timerInfo, logger, new CloseConferenceService(_videoApiServiceMock.Object));
            logger.GetLoggedMessages().Last().Should()
                .StartWith("Delete audiorecording applications function executed for 3 conferences");
        }


        [Test]
        public async Task Timer_should_log_message_audio_app_were_not_deleted()
        {
            var logger = (LoggerFake)TestFactory.CreateLogger(LoggerTypes.List);
            _videoApiServiceMock.Setup(x => x.DeleteAudiorecordingApplication(It.IsAny<Guid>())).Throws(new Exception());

            await SchedulerJobs.Functions.DeleteAudiorecordingApplicationsFunction.Run(_timerInfo, logger, new CloseConferenceService(_videoApiServiceMock.Object));
            logger.GetLoggedMessages().Last().Should()
                .StartWith("Delete audiorecording applications function executed for 0 conferences");
        }
    }
}
