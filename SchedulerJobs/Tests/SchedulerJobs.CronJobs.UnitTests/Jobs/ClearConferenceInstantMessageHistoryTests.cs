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
        [Test]
        public async Task Timer_should_log_message()
        {
            // Arrange
            var services = new ServiceCollection();
            var clearConferenceChatHistoryService = new Mock<IClearConferenceChatHistoryService>();
            services.AddScoped(s => clearConferenceChatHistoryService.Object);

            var serviceProvider = services.BuildServiceProvider();

            var sut = new ClearConferenceInstantMessageHistoryJob(Logger, Lifetime.Object, serviceProvider);

            // Act
            await sut.DoWorkAsync();

            // Assert
            Logger.GetLoggedMessages().Last().Should().Be("Cleared chat history for closed conferences");
            clearConferenceChatHistoryService.Verify(x => x.ClearChatHistoryForClosedConferences(), Times.Once);
        }
    }
}
