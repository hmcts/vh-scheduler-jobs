using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Common.Caching;
using SchedulerJobs.Sds.Jobs;
using SchedulerJobs.Services;

namespace SchedulerJobs.Sds.UnitTests.Jobs
{
    public class AnonymiseHearingsConferencesAndDeleteAadUsersJobTests : JobTestBaseSetup<AnonymiseHearingsConferencesAndDeleteAadUsersJob>
    {
        private AnonymiseHearingsConferencesAndDeleteAadUsersJob _sut;
        private Mock<IAnonymiseHearingsConferencesDataService> _anonymiseHearingsConferencesDataService;
        private Mock<IJobHistoryService> _jobHistoryService;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            _anonymiseHearingsConferencesDataService = new Mock<IAnonymiseHearingsConferencesDataService>();
            _jobHistoryService = new Mock<IJobHistoryService>();
            services.AddScoped(s => _anonymiseHearingsConferencesDataService.Object);
            services.AddScoped(s => _jobHistoryService.Object);

            var serviceProvider = services.BuildServiceProvider();

            _sut = new AnonymiseHearingsConferencesAndDeleteAadUsersJob(Logger, Lifetime.Object, serviceProvider, DistributedJobRunningStatusCache.Object, ConnectionStrings.Object);
        }
        
        [Test]
        public async Task Timer_should_log_message_all_older_conferences_were_updated_and_update_job_history()
        {
            // Act
            await _sut.DoWorkAsync();

            // Assert
            _anonymiseHearingsConferencesDataService.Verify(x => x.AnonymiseHearingsConferencesDataAsync(), Times.Once);
            _jobHistoryService.Verify(x => x.UpdateJobHistory(It.IsAny<string>(), true), Times.Once);
            Logger.GetLoggedMessages().Last().Should().StartWith(AnonymiseHearingsConferencesAndDeleteAadUsersJob.LogInformationMessage);
        }

        [Test]
        public async Task Should_call_job_history_with_false_when_error_thrown()
        {

            _anonymiseHearingsConferencesDataService
                .Setup(e => e.AnonymiseHearingsConferencesDataAsync()).Throws<Exception>();
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