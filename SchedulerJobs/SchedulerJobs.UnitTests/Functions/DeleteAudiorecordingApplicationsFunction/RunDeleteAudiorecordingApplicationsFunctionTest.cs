using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Testing.Common;
using VideoApi.Client;
using VideoApi.Contract.Responses;

namespace SchedulerJobs.UnitTests.Functions.DeleteAudiorecordingApplicationsFunction
{
    public class RunDeleteAudiorecordingApplicationsFunctionTest
    {
        private Mock<IVideoApiClient> _videoApiClientMock;
        private readonly TimerInfo _timerInfo = new TimerInfo(new ScheduleStub(), new ScheduleStatus(), true);

        [SetUp]
        public void Setup()
        {
            _videoApiClientMock = new Mock<IVideoApiClient>();
            var conferences = Builder<ExpiredConferencesResponse>.CreateListOfSize(3).All()
               .With(x => x.HearingId = Guid.NewGuid()).Build().ToList();
            _videoApiClientMock.Setup(x => x.GetExpiredAudiorecordingConferencesAsync()).ReturnsAsync(conferences);
            _videoApiClientMock.Setup(x => x.DeleteAudioApplicationAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Timer_should_log_message_all_audio_app_were_deleted()
        {
            var logger = (LoggerFake)TestFactory.CreateLogger(LoggerTypes.List);
            await SchedulerJobs.Functions.DeleteAudiorecordingApplicationsFunction.Run(_timerInfo, logger, new CloseConferenceService(_videoApiClientMock.Object));
            logger.GetLoggedMessages().Last().Should()
                .StartWith("Delete audiorecording applications function executed for 3 conferences");
        }

        [Test]
        public async Task Timer_should_log_message_audio_app_were_not_deleted()
        {
            var logger = (LoggerFake)TestFactory.CreateLogger(LoggerTypes.List);
            _videoApiClientMock.Setup(x => x.DeleteAudioApplicationAsync(It.IsAny<Guid>())).Throws(new Exception());

            await SchedulerJobs.Functions.DeleteAudiorecordingApplicationsFunction.Run(_timerInfo, logger, new CloseConferenceService(_videoApiClientMock.Object));
            logger.GetLoggedMessages().Last().Should()
                .StartWith("Delete audiorecording applications function executed for 0 conferences");
        }
    }
}
