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
using Testing.Common;
using VideoApi.Client;
using VideoApi.Contract.Responses;

namespace SchedulerJobs.UnitTests.Functions.ClearConferenceInstantMessageHistory
{
    public class RunClearImHistoryFunctionTests
    {
        private Mock<IVideoApiClient> VideoApiClientMock { get; set; }
        private TimerInfo _timerInfo;

        [SetUp]
        public void Setup()
        {
            VideoApiClientMock = new Mock<IVideoApiClient>();
            _timerInfo = new TimerInfo(new ScheduleStub(), new ScheduleStatus(), true);
        }

        [Test]
        public async Task Timer_should_log_message()
        {
            var conferences = Builder<ClosedConferencesResponse>.CreateListOfSize(10).All()
                .With(x => x.Id = Guid.NewGuid()).Build().ToList();
            VideoApiClientMock.Setup(x => x.GetClosedConferencesWithInstantMessagesAsync()).ReturnsAsync(conferences);
            var ids = conferences.Select(x => x.Id).ToList();
            var logger = (LoggerFake)TestFactory.CreateLogger(LoggerTypes.List);
            
            await SchedulerJobs.Functions.ClearConferenceInstantMessageHistory.RunAsync(_timerInfo, logger, new ClearConferenceChatHistoryService(VideoApiClientMock.Object));
            logger.GetLoggedMessages().First().Should().Be("Timer is running late!");
            logger.GetLoggedMessages().Last().Should().Be("Cleared chat history for closed conferences");

            VideoApiClientMock.Verify(x => x.RemoveInstantMessagesAsync(It.IsIn<Guid>(ids)), Times.Exactly(ids.Count));
        }
    }
}
