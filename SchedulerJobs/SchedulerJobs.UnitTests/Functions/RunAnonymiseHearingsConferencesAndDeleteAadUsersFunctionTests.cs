﻿using FluentAssertions;
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
        public async Task Timer_should_log_message_all_older_conferences_were_updated()
        {
            // Act
            await _sut.Run(_timerInfo, _logger);

            // Assert
            _mocker.Mock<IAnonymiseHearingsConferencesDataService>().Verify(x => x.AnonymiseHearingsConferencesDataAsync(), Times.Once);
            _logger.GetLoggedMessages().Last().Should().StartWith("Data anonymised for hearings, conferences older than 3 months.");
        }
    }
}
