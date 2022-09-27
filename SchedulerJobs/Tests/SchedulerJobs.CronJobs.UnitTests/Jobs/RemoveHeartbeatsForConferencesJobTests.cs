using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SchedulerJobs.CronJobs.Jobs;
using SchedulerJobs.Services;

namespace SchedulerJobs.CronJobs.UnitTests.Jobs
{
    public class RemoveHeartbeatsForConferencesJobTests : JobTestBaseSetup<RemoveHeartbeatsForConferencesJob>
    {
        private RemoveHeartbeatsForConferencesJob _sut;
        private Mock<IRemoveHeartbeatsForConferencesService> _removeHeartbeatsForConferencesService;
        
        [SetUp]
        public void Setup()
        {
            // TODO move to single base setup class so we don't need to repeat for each job
            var services = new ServiceCollection();
            _removeHeartbeatsForConferencesService = new Mock<IRemoveHeartbeatsForConferencesService>();
            services.AddScoped(s => _removeHeartbeatsForConferencesService.Object);

            var serviceProvider = services.BuildServiceProvider();

            _sut = new RemoveHeartbeatsForConferencesJob(Lifetime.Object, Logger, serviceProvider);
        }
        
        [Test]
        public async Task Timer_should_log_message_heartbeat_for_older_conferences_were_deleted()
        {
            // Act
            await _sut.DoWorkAsync();
            
            // Assert
            Logger.GetLoggedMessages().Last().Should().StartWith("Removed heartbeats for conferences older than 14 days.");
        }
    }
}