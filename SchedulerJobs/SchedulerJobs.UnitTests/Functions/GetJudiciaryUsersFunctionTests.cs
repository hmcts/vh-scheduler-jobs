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
        public async Task Timer_should_log_message_all_audio_app_were_deleted()
        {
            await _sut.RunAsync(_timerInfo, _logger);

            _logger.GetLoggedMessages().Last().Should().StartWith("Finished GetJudiciaryUsersFunction at");
        }

        [Test]
        public async Task Should_Call_ImportJudiciaryPeopleAsync_And_ImportLeaversJudiciaryPeopleAsync_Once()
        {
            await _sut.RunAsync(_timerInfo, _logger);
            _elinksService.Verify(x => x.ImportJudiciaryPeopleAsync(It.IsAny<DateTime>()), Times.Once);
            _elinksService.Verify(x => x.ImportLeaversJudiciaryPeopleAsync(It.IsAny<DateTime>()), Times.Once);
        }

        [Test]
        public async Task Should_Call_And_ImportJudiciaryPeopleAsync_On_Specific_Date()
        {
            var dateRangeStart = DateTime.UtcNow.AddDays(-_servicesConfiguration.Object.Value.ELinksApiGetPeopleUpdatedSinceDays).AddMinutes(-1);
            var dateRangeEnd = DateTime.UtcNow.AddDays(-_servicesConfiguration.Object.Value.ELinksApiGetPeopleUpdatedSinceDays).AddMinutes(1);
            await _sut.RunAsync(_timerInfo, _logger);
            _elinksService.Verify(x => x.ImportJudiciaryPeopleAsync(It.IsInRange(dateRangeStart, dateRangeEnd, Moq.Range.Inclusive)), Times.Once);
        }

        [Test]
        public async Task Should_Call_And_ImportLeaversJudiciaryPeopleAsync_On_Specific_Date()
        {
            var dateRangeStart = DateTime.UtcNow.AddDays(-_servicesConfiguration.Object.Value.ELinksApiGetPeopleUpdatedSinceDays).AddMinutes(-1);
            var dateRangeEnd = DateTime.UtcNow.AddDays(-_servicesConfiguration.Object.Value.ELinksApiGetPeopleUpdatedSinceDays).AddMinutes(1);
            await _sut.RunAsync(_timerInfo, _logger);
            _elinksService.Verify(x => x.ImportLeaversJudiciaryPeopleAsync(It.IsInRange(dateRangeStart, dateRangeEnd, Moq.Range.Inclusive)), Times.Once);
        }

    }
}
