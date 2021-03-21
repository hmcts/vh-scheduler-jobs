using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Functions;
using SchedulerJobs.Services;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SchedulerJobs.Common.Configuration;

namespace SchedulerJobs.UnitTests.Functions
{
    public class GetJudiciaryUsersFunctionTests : AzureFunctionTestBaseSetup<GetJudiciaryUsersFunction>
    {
        [SetUp]
        public void Setup()
        {
            _mocker.Mock<IELinksService>().Setup(x => x.ImportJudiciaryPeopleAsync(It.IsAny<DateTime>()));
            _mocker.Mock<IOptions<ServicesConfiguration>>().Setup(x => x.Value).Returns(new ServicesConfiguration
            {
                ELinksApiGetPeopleUpdatedSinceDays = 10
            });
        }

        [Test]
        public async Task Timer_should_log_message_all_audio_app_were_deleted()
        {
            await _sut.RunAsync(_timerInfo, _logger);

            _logger.GetLoggedMessages().Last().Should().StartWith("Finished GetJudiciaryUsersFunction at");
        }
    }
}
