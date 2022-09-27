using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SchedulerJobs.CronJobs.Jobs;
using SchedulerJobs.Services.Interfaces;

namespace SchedulerJobs.CronJobs.UnitTests.Jobs
{
    public class ReconcileHearingAudioWithStorageJobTests : JobTestBaseSetup<ReconcileHearingAudioWithStorageJob>
    {
        private ReconcileHearingAudioWithStorageJob _sut;
        private Mock<IReconcileHearingAudioService> _reconcileHearingAudioService;
        
        [SetUp]
        public void Setup()
        {
            // TODO move to single base setup class so we don't need to repeat for each job
            var services = new ServiceCollection();
            _reconcileHearingAudioService = new Mock<IReconcileHearingAudioService>();
            services.AddScoped(s => _reconcileHearingAudioService.Object);

            var serviceProvider = services.BuildServiceProvider();

            _sut = new ReconcileHearingAudioWithStorageJob(Lifetime.Object, Logger, serviceProvider);
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
            await _sut.DoWorkAsync();

            Logger.GetLoggedMessages().Last().Should().StartWith("Reconcile audio recording files with number of conferences for the day - Done");
        }
    }
}