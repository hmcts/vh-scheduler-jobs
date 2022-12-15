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
    public class ClearConferenceInstantMessageHistoryJobTests : JobTestBaseSetup<ClearConferenceInstantMessageHistoryJob>
    {
        private ClearConferenceInstantMessageHistoryJob _sut;
        private Mock<IClearConferenceChatHistoryService> _clearConferenceChatHistoryService;
        
        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            _clearConferenceChatHistoryService = new Mock<IClearConferenceChatHistoryService>();
            services.AddScoped(s => _clearConferenceChatHistoryService.Object);

            var serviceProvider = services.BuildServiceProvider();

            _sut = new ClearConferenceInstantMessageHistoryJob(Logger, Lifetime.Object, serviceProvider, DistributedJobRunningStatusCache.Object, ConnectionStrings.Object);
        }
        
        [Test]
        public async Task Timer_should_log_message()
        {
            // Act
            await _sut.DoWorkAsync();

            // Assert
            Logger.GetLoggedMessages().Last().Should().Be("Cleared chat history for closed conferences");
            _clearConferenceChatHistoryService.Verify(x => x.ClearChatHistoryForClosedConferences(), Times.Once);
        }
    }
}
