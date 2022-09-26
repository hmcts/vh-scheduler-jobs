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
    public class ClearConferenceInstantMessageHistoryTests : JobTestBaseSetup<ClearConferenceInstantMessageHistoryJob>
    {
        private ClearConferenceInstantMessageHistoryJob _sut;
        private Mock<IClearConferenceChatHistoryService> _clearConferenceChatHistoryService;
        
        [SetUp]
        public void Setup()
        {
            // TODO move to single base setup class so we don't need to repeat for each job
            var services = new ServiceCollection();
            _clearConferenceChatHistoryService = new Mock<IClearConferenceChatHistoryService>();
            services.AddScoped(s => _clearConferenceChatHistoryService.Object);

            var serviceProvider = services.BuildServiceProvider();

            _sut = new ClearConferenceInstantMessageHistoryJob(Logger, Lifetime.Object, serviceProvider);
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
