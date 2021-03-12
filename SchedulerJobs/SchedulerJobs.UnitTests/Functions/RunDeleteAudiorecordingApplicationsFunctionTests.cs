using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Functions;
using System;
using System.Linq;
using System.Threading.Tasks;
using VideoApi.Client;
using VideoApi.Contract.Responses;

namespace SchedulerJobs.UnitTests.Functions
{
    public class RunDeleteAudiorecordingApplicationsFunctionTests : AzureFunctionTestBaseSetup<DeleteAudiorecordingApplicationsFunction>
    {
        [SetUp]
        public void Setup()
        {
            var conferences = Builder<ExpiredConferencesResponse>.CreateListOfSize(3).All()
               .With(x => x.HearingId = Guid.NewGuid()).Build().ToList();
            _mocker.Mock<IVideoApiClient>().Setup(x => x.GetExpiredAudiorecordingConferencesAsync()).ReturnsAsync(conferences);
            _mocker.Mock<IVideoApiClient>().Setup(x => x.DeleteAudioApplicationAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Timer_should_log_message_all_audio_app_were_deleted()
        {
            // Act
            await _sut.Run(_timerInfo, _logger);

            // Assert
            _logger.GetLoggedMessages().Last().Should().StartWith("Delete audiorecording applications function executed for 3 conferences");
        }

        [Test]
        public async Task Timer_should_log_message_audio_app_were_not_deleted()
        {
            _mocker.Mock<IVideoApiClient>().Setup(x => x.DeleteAudioApplicationAsync(It.IsAny<Guid>())).Throws(new Exception());

            // Act
            await _sut.Run(_timerInfo, _logger);
            
            // Assert
            _logger.GetLoggedMessages().Last().Should().StartWith("Delete audiorecording applications function executed for 0 conferences");
        }
    }
}
