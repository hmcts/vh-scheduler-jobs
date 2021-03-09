using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;
using Testing.Common;
using VideoApi.Client;
using VideoApi.Contract.Responses;

namespace SchedulerJobs.UnitTests.Functions.ClearHearingsFunction
{
    public class RunCloseExpiredFunctionTests
    {
        private Mock<IVideoApiClient> _videoApiClientMock;
        private readonly TimerInfo _timerInfo = new TimerInfo(new ScheduleStub(), new ScheduleStatus(), true);

        [SetUp]
        public void Setup()
        {
            _videoApiClientMock = new Mock<IVideoApiClient>();
            var result = new List<ExpiredConferencesResponse>();
            _videoApiClientMock.Setup(x => x.GetExpiredOpenConferencesAsync()).ReturnsAsync(result);
        }

        [Test]
        public async Task Timer_should_log_message()
        {
            var logger = (LoggerFake)TestFactory.CreateLogger(LoggerTypes.List);
            await SchedulerJobs.Functions.ClearHearingsFunction.Run(_timerInfo, logger, new CloseConferenceService(_videoApiClientMock.Object));
            logger.GetLoggedMessages().Last().Should().StartWith("Close hearings function executed");
        }
    }
}
