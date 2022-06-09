using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using BookingsApi.Client;
using BookingsApi.Contract.Requests;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;

namespace SchedulerJobs.UnitTests.Services
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
    }
}