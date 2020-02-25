using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;
using SchedulerJobs.Services.VideoApi.Contracts;
using Testing.Common;

namespace SchedulerJobs.UnitTests.Functions.ClearHearingsFunction
{
    public class RunCloseExpiredFunctionTests
    {
        private Mock<IVideoApiService> _videoApiServiceMock;
        private readonly TimerInfo _timerInfo = new TimerInfo(new ScheduleStub(), new ScheduleStatus(), true);

        [SetUp]
        public void Setup()
        {
            _videoApiServiceMock = new Mock<IVideoApiService>();
            var result = Task.FromResult(new List<ExpiredConferencesResponse>());
            _videoApiServiceMock.Setup(x => x.GetExpiredOpenConferences()).Returns(result);
        }

        [Test]
        public async Task Timer_should_log_message()
        {
            var logger = (LoggerFake)TestFactory.CreateLogger(LoggerTypes.List);
            await SchedulerJobs.Functions.ClearHearingsFunction.Run(_timerInfo, logger, new CloseConferenceService(_videoApiServiceMock.Object));
            logger.GetLoggedMessages().Last().Should().StartWith("Close hearings function executed");
        }
    }
}
