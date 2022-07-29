using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using SchedulerJobs.Functions;
using SchedulerJobs.Services;

namespace SchedulerJobs.UnitTests.Functions
{

    public class RunAnonymiseHearingsConferencesAndDeleteAadUsersFunctionTests : AzureFunctionTestBaseSetup<AnonymiseHearingsConferencesAndDeleteAadUsersFunction>
    {
        [Test]
        public async Task Timer_should_log_message_all_older_conferences_were_updated_and_update_job_history()
        {
            // Act
            await _sut.Run(_timerInfo, _logger);

            // Assert
            _mocker.Mock<IAnonymiseHearingsConferencesDataService>().Verify(x => x.AnonymiseHearingsConferencesDataAsync(), Times.Once);
            _mocker.Mock<IJobHistoryService>().Verify(x => x.UpdateJobHistory(It.IsAny<string>(), true), Times.Once);
            _logger.GetLoggedMessages().Last().Should().StartWith(AnonymiseHearingsConferencesAndDeleteAadUsersFunction.LogInformationMessage);
        }

        [Test]
        public async Task Should_call_job_history_with_false_when_error_thrown()
        {

            _mocker.Mock<IAnonymiseHearingsConferencesDataService>()
                .Setup(e => e.AnonymiseHearingsConferencesDataAsync()).Throws<Exception>();
            try
            {
                // Act
                await _sut.Run(_timerInfo, _logger);
            }
            catch(Exception)
            {
                // Assert
                _mocker.Mock<IJobHistoryService>().Verify(x => x.UpdateJobHistory(It.IsAny<string>(), false), Times.Once);
            }
        }
    }
}
