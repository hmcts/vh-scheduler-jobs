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
    public class RemoveHeartbeatsForConferencesJobTests : JobTestBaseSetup<RemoveHeartbeatsForConferencesJob>
    {
        private RemoveHeartbeatsForConferencesJob _sut;
        private Mock<IRemoveHeartbeatsForConferencesService> _removeHeartbeatsForConferencesService;
        
        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            _removeHeartbeatsForConferencesService = new Mock<IRemoveHeartbeatsForConferencesService>();
            services.AddScoped(s => _removeHeartbeatsForConferencesService.Object);

            var serviceProvider = services.BuildServiceProvider();

            _sut = new RemoveHeartbeatsForConferencesJob(Lifetime.Object, Logger, serviceProvider, DistributedJobCache.Object);
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