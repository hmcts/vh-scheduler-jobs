using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Sds.Jobs;
using SchedulerJobs.Services;

namespace SchedulerJobs.Sds.UnitTests.Jobs
{
    public class DeleteAudioRecordingApplicationsJobTests : JobTestBaseSetup<DeleteAudioRecordingApplicationsJob>
    {
        private DeleteAudioRecordingApplicationsJob _sut;
        private Mock<ICloseConferenceService> _closeConferenceService;
        
        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            _closeConferenceService = new Mock<ICloseConferenceService>();
            services.AddScoped(s => _closeConferenceService.Object);

            var serviceProvider = services.BuildServiceProvider();

            _sut = new DeleteAudioRecordingApplicationsJob(Logger, Lifetime.Object, serviceProvider);
        }
        
        [Test]
        public async Task Timer_should_log_message_all_audio_app_were_deleted()
        {
            // Arrange
            _closeConferenceService.Setup(x => x.DeleteAudiorecordingApplicationsAsync()).ReturnsAsync(3);
            
            // Act
            await _sut.DoWorkAsync();

            // Assert
            Logger.GetLoggedMessages().Last().Should().StartWith("Delete audio recording applications job executed for 3 conferences");
        }
    }
}