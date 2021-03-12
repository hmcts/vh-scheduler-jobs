using FluentAssertions;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Functions;
using SchedulerJobs.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerJobs.UnitTests.Functions
{
    public class RunDeleteAudiorecordingApplicationsFunctionTests : AzureFunctionTestBaseSetup<DeleteAudiorecordingApplicationsFunction>
    {
        [SetUp]
        public void Setup()
        {
            _mocker.Mock<ICloseConferenceService>().Setup(x => x.DeleteAudiorecordingApplicationsAsync()).ReturnsAsync(3);
        }

        [Test]
        public async Task Timer_should_log_message_all_audio_app_were_deleted()
        {
            // Act
            await _sut.Run(_timerInfo, _logger);

            // Assert
            _logger.GetLoggedMessages().Last().Should().StartWith("Delete audiorecording applications function executed for 3 conferences");
        }
    }
}
