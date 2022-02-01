using FluentAssertions;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Functions;
using SchedulerJobs.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerJobs.UnitTests.Functions
{
    public class ReconcileHearingAudioWithStorageFunctionTests : AzureFunctionTestBaseSetup<ReconcileHearingAudioWithStorageFunction>
    {
        [Test]
        public async Task Should_Call_ReconcileAudiorecordingsWithConferencesAsync_Once()
        {
            _mocker.Mock<IReconcileHearingAudioService>().Setup(x => x.ReconcileAudiorecordingsWithConferencesAsync());

            await _sut.RunAsync(_timerInfo, _logger);

            _mocker.Mock<IReconcileHearingAudioService>().Verify(x => x.ReconcileAudiorecordingsWithConferencesAsync(), Times.Once);
        }

        [Test]
        public async Task Timer_Should_Log_Finish_Message_()
        {
            await _sut.RunAsync(_timerInfo, _logger);

            _logger.GetLoggedMessages().Last().Should().StartWith("Reconcile audio recording files with number of conferences for the day - Done");
        }
    }
}
