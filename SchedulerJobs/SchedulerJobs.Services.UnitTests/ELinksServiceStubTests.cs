using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using BookingsApi.Client;
using BookingsApi.Contract.V1.Requests;
using Moq;
using NUnit.Framework;

namespace SchedulerJobs.Services.UnitTests
{
    public class ELinksServiceStubTests
    {
        private AutoMock _autoMock;
        private ELinksServiceStub _sut;

        [SetUp]
        public void Setup()
        {
            _autoMock = AutoMock.GetLoose();
            _sut = _autoMock.Create<ELinksServiceStub>();
        }

        [Test]
        public async Task should_import_fixed_automation_and_manual_judiciary_accounts()
        {
            await _sut.ImportJudiciaryPeopleAsync(DateTime.UtcNow);
            
            _autoMock.Mock<IBookingsApiClient>()
                .Verify(x=> 
                    x.BulkJudiciaryPersonsAsync(It.Is<IEnumerable<JudiciaryPersonRequest>>(r => 
                        r.Count() == 38)), Times.Once);
        }

        [Test]
        public async Task should_not_call_bookings_api_for_leavers()
        {
            await _sut.ImportLeaversJudiciaryPeopleAsync(DateTime.UtcNow);
            _autoMock.Mock<IBookingsApiClient>()
                .Verify(x=> 
                    x.BulkJudiciaryPersonsAsync(It.IsAny<IEnumerable<JudiciaryPersonRequest>>()), Times.Never);
        }

        [Test]
        public async Task should_return_default_value_for_updated_since()
        {
            var updatedSince = await _sut.GetUpdatedSince();
            Assert.That(updatedSince.Date, Is.EqualTo(DateTime.UtcNow.AddDays(-1).Date));
        }
    }
}