using FluentAssertions;
using NUnit.Framework;
using SchedulerJobs.Functions;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerJobs.UnitTests.Functions
{
    public class RemoveHeartbeatsForConferencesFunctionTests : AzureFunctionTestBaseSetup<RemoveHeartbeatsForConferencesFunction>
    {
        [Test]
        public async Task Timer_should_log_message_heartbeat_for_older_conferences_were_deleted()
        {
            // Act
            await _sut.Run(_timerInfo, _logger);
            
            // Assert
            _logger.GetLoggedMessages().Last().Should().StartWith("Removed heartbeats for conferences older than 14 days.");
        }
    }
}
