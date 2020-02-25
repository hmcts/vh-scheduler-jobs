using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;
using SchedulerJobs.Services.VideoApi.Contracts;
using Testing.Common;

namespace SchedulerJobs.UnitTests.Functions.ClearHearingsFunction
{
    public class RunCloseExpiredFunctionTests
    {
        protected Mock<IVideoApiService> VideoApiServiceMock { get; set; }

        [SetUp]
        public void Setup()
        {
            VideoApiServiceMock = new Mock<IVideoApiService>();
            var result = Task.FromResult(new List<ExpiredConferencesResponse>());
            VideoApiServiceMock.Setup(x => x.GetExpiredOpenConferences()).Returns(result);
        }

        [Test]
        public async Task Timer_should_log_message()
        {
            var logger = (LoggerFake)TestFactory.CreateLogger(LoggerTypes.List);
            await SchedulerJobs.Functions.ClearHearingsFunction.Run(null, logger, new CloseConferenceService(VideoApiServiceMock.Object));
            logger.Logs.Last().Should().StartWith("Close hearings function executed");
        }
    }
}
