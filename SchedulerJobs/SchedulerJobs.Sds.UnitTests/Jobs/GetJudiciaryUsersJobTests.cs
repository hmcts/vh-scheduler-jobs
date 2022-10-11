using System;
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
    public class GetJudiciaryUsersJobTests : JobTestBaseSetup<GetJudiciaryUsersJob>
    {
        private GetJudiciaryUsersJob _sut;
        private Mock<IJobHistoryService> _jobHistoryService;
        private Mock<IELinksService> _eLinksService;
        
        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            _jobHistoryService = new Mock<IJobHistoryService>();
            _eLinksService = new Mock<IELinksService>();
            services.AddScoped(s => _jobHistoryService.Object);
            services.AddScoped(s => _eLinksService.Object);
            
            var serviceProvider = services.BuildServiceProvider();

            _sut = new GetJudiciaryUsersJob(Lifetime.Object, Logger, serviceProvider);
        }
        
        [Test]
        public async Task Timer_should_log_message_all_audio_app_were_deleted()
        {
            await _sut.DoWorkAsync();

            Logger.GetLoggedMessages().Last().Should().StartWith("Finished GetJudiciaryUsers job at");
        }

        [Test]
        public async Task Should_Call_ImportJudiciaryPeopleAsync_And_ImportLeaversJudiciaryPeopleAsync_Once()
        {
            await _sut.DoWorkAsync();
            _eLinksService.Verify(x => x.ImportJudiciaryPeopleAsync(It.IsAny<DateTime>()), Times.Once);
            _eLinksService.Verify(x => x.ImportLeaversJudiciaryPeopleAsync(It.IsAny<DateTime>()), Times.Once);
        }

        [Test]
        public async Task Should_Call_And_ImportJudiciaryPeopleAsync_On_Specific_Date()
        {
            var mockDate = DateTime.UtcNow;
            
            var dateRangeStart = mockDate.AddMinutes(-1);
            var dateRangeEnd = mockDate.AddMinutes(1);
            _jobHistoryService.Setup(x => x.GetMostRecentSuccessfulRunDate(It.IsAny<string>()))
                .ReturnsAsync(mockDate);
            
            await _sut.DoWorkAsync();
            _eLinksService.Verify(x => x.ImportJudiciaryPeopleAsync(It.IsInRange(dateRangeStart, dateRangeEnd, Moq.Range.Inclusive)), Times.Once);
        }

        [Test]
        public async Task Should_Call_And_ImportLeaversJudiciaryPeopleAsync_On_Specific_Date()
        {
            var mockDate = DateTime.UtcNow;
            
            var dateRangeStart = mockDate.AddMinutes(-1);
            var dateRangeEnd = mockDate.AddMinutes(1);
            _jobHistoryService.Setup(x => x.GetMostRecentSuccessfulRunDate(It.IsAny<string>()))
                .ReturnsAsync(mockDate);

            
            await _sut.DoWorkAsync();
            _eLinksService.Verify(x => x.ImportLeaversJudiciaryPeopleAsync(It.IsInRange(dateRangeStart, dateRangeEnd, Moq.Range.Inclusive)), Times.Once);
        }
        
        [Test]
        public async Task Should_call_job_history_with_false_when_error_thrown()
        {

            _eLinksService
                .Setup(e => e.ImportJudiciaryPeopleAsync(It.IsAny<DateTime>())).Throws<Exception>();
            try
            {
                // Act
                await _sut.DoWorkAsync();
            }
            catch(Exception)
            {
                // Assert
                _jobHistoryService.Verify(x => x.UpdateJobHistory(It.IsAny<string>(), false), Times.Once);
            }
        }
    }   
}