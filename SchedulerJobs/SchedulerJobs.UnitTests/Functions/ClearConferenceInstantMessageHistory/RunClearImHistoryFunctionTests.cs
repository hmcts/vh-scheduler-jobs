using System;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;
using SchedulerJobs.Services.VideoApi.Contracts;
using Testing.Common;

namespace SchedulerJobs.UnitTests.Functions.ClearConferenceInstantMessageHistory
{
    public class RunClearImHistoryFunctionTests
    {
        private Mock<IVideoApiService> VideoApiServiceMock { get; set; }
        private TimerInfo _timerInfo;

        [SetUp]
        public void Setup()
        {
            VideoApiServiceMock = new Mock<IVideoApiService>();
            _timerInfo = new TimerInfo(new ScheduleStub(), new ScheduleStatus(), true);
        }

        [Test]
        public async Task Timer_should_log_message()
        {
            var conferences = Builder<ClosedConferencesResponse>.CreateListOfSize(10).All()
                .With(x => x.Id = Guid.NewGuid()).Build().ToList();
            VideoApiServiceMock.Setup(x => x.GetClosedConferencesToClearInstantMessageHistory()).ReturnsAsync(conferences);
            var ids = conferences.Select(x => x.Id).ToList();
            var logger = (LoggerFake)TestFactory.CreateLogger(LoggerTypes.List);
            
            await SchedulerJobs.Functions.ClearConferenceInstantMessageHistory.RunAsync(_timerInfo, logger, new ClearConferenceChatHistoryService(VideoApiServiceMock.Object));
            logger.GetLoggedMessages().First().Should().Be("Timer is running late!");
            logger.GetLoggedMessages().Last().Should().Be("Cleared chat history for closed conferences");

            VideoApiServiceMock.Verify(x => x.ClearConferenceChatHistory(It.IsIn<Guid>(ids)), Times.Exactly(ids.Count));
        }
    }
}
