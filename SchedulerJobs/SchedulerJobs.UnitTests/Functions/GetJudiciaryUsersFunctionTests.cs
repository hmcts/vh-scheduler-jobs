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

        protected Mock<IELinksService> _elinksService;
        protected Mock<IOptions<ServicesConfiguration>> _servicesConfiguration;

        [SetUp]
        public void Setup()
        {
            _elinksService = _mocker.Mock<IELinksService>();
            _elinksService.Setup(x => x.ImportJudiciaryPeopleAsync(It.IsAny<DateTime>()));
            _elinksService.Setup(x => x.ImportLeaversJudiciaryPeopleAsync(It.IsAny<DateTime>()));
        }
        protected override void MockerAdditionalSetupBeforeSutCreation()
        {
            _servicesConfiguration = _mocker.Mock<IOptions<ServicesConfiguration>>();
            _servicesConfiguration.Setup(x => x.Value).Returns(new ServicesConfiguration()
            {
                ELinksApiGetPeopleUpdatedSinceDays = 10
            });
        }

        [Test]
        public void should_return_completed_task()
        {
            _sut.RunAsync(_timerInfo, _logger).Should().Be(Task.CompletedTask);
        }
    }
}
