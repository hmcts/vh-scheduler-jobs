using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Sds.Jobs;
using SchedulerJobs.Services.Interfaces;

namespace SchedulerJobs.Sds.UnitTests.Jobs
{
    public class ReconcileHearingAudioWithStorageJobTests : JobTestBaseSetup<ReconcileHearingAudioWithStorageJob>
    {
        private ReconcileHearingAudioWithStorageJob _sut;
        private Mock<IReconcileHearingAudioService> _reconcileHearingAudioService;
        
        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            _reconcileHearingAudioService = new Mock<IReconcileHearingAudioService>();
            services.AddScoped(s => _reconcileHearingAudioService.Object);

            var serviceProvider = services.BuildServiceProvider();

            _sut = new ReconcileHearingAudioWithStorageJob(Lifetime.Object, Logger, serviceProvider, DistributedJobRunningStatusCache.Object);
        }
        
        [Test]
        public async Task Should_Call_ReconcileAudiorecordingsWithConferencesAsync_Once()
        {
            _reconcileHearingAudioService.Setup(x => x.ReconcileAudiorecordingsWithConferencesAsync());

            await _sut.DoWorkAsync();

            _reconcileHearingAudioService.Verify(x => x.ReconcileAudiorecordingsWithConferencesAsync(), Times.Once);
        }

        [Test]
        public async Task Timer_Should_Log_Finish_Message_()
        {
            // Arrange
            Logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information);
            
            await _sut.DoWorkAsync();

            Logger.GetLoggedMessages().Last().Should().StartWith("Reconcile audio recording files with number of conferences for the day - Done");
        }
    }
}