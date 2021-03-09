using FluentAssertions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;
using System.Linq;
using System.Threading.Tasks;
using Testing.Common;
using VideoApi.Client;

namespace SchedulerJobs.UnitTests.Functions.RemoveHeartbeatsForConferencesFunction
{
    public class RemoveHeartbeatsForConferencesFunctionTests
    {
        private Mock<IVideoApiClient> _videoApiClientMock;
        private readonly TimerInfo _timerInfo = new TimerInfo(new ScheduleStub(), new ScheduleStatus(), true);

        [SetUp]
        public void Setup()
        {
            _videoApiClientMock = new Mock<IVideoApiClient>();
        }

        [Test]
        public async Task Timer_should_log_message_heartbeat_for_older_conferences_were_deleted()
        {
            var logger = (LoggerFake)TestFactory.CreateLogger(LoggerTypes.List);
            await SchedulerJobs.Functions.RemoveHeartbeatsForConferencesFunction.Run(_timerInfo, logger,
                new RemoveHeartbeatsForConferencesService(_videoApiClientMock.Object));
            logger.GetLoggedMessages().Last().Should()
                .StartWith("Removed heartbeats for conferences older than 14 days.");
        }
    }
}
