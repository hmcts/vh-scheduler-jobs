using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SchedulerJobs;
using SchedulerJobs.Services;
using SchedulerJobs.Services.VideoApi.Contracts;

namespace SchedulerJobs.UnitTests.ClearHearingsFunction
{
    public class RunTests
    {
        protected Mock<IVideoApiService> VideoApiServiceMock { get; set; }

        [SetUp]
        public void Setup()
        {
            VideoApiServiceMock = new Mock<IVideoApiService>();
            var result = Task.FromResult(new List<ConferenceSummaryResponse>());
            VideoApiServiceMock.Setup(x => x.GetOpenConferencesByScheduledDate(DateTime.UtcNow)).Returns(result);
        }

        [Test]
        public async Task Timer_should_log_message()
        {
            var logger = (LoggerFake)TestFactory.CreateLogger(LoggerTypes.List);
            await SchedulerJobs.ClearHearingsFunction.Run(null, logger, new CloseConferenceService(VideoApiServiceMock.Object));
            var msg = logger.Logs[0];
            Assert.IsTrue(msg.Contains("Close hearings function executed at"));
        }
    }
}
