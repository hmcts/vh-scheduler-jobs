using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingsApi.Client;
using BookingsApi.Contract.Requests;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Common.Models;
using SchedulerJobs.Services;
using SchedulerJobs.Services.HttpClients;

namespace SchedulerJobs.UnitTests.Services
{
    [TestFixture]
    public class ELinksServiceTests
    {
        private Mock<IELinksApiClient> _eLinksApiClient;
        private Mock<IBookingsApiClient> _bookingsApiClient;
        private Mock<ILogger<ELinksService>> _logger;

        private ELinksService _eLinksService;
        
        [SetUp]
        public void Setup()
        {
            _eLinksApiClient = new Mock<IELinksApiClient>();
            _bookingsApiClient = new Mock<IBookingsApiClient>();
            _logger = new Mock<ILogger<ELinksService>>();

            _eLinksService = new ELinksService(_eLinksApiClient.Object, _bookingsApiClient.Object, _logger.Object);
        }

        [Test]
        public void Should_throw_exception()
        {
            _eLinksApiClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("some exception"));

            Assert.ThrowsAsync<Exception>(() => _eLinksService.ImportJudiciaryPeopleAsync(new DateTime()));
            
            _eLinksApiClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            _bookingsApiClient.Verify(x => x.BulkJudiciaryPersonsAsync(It.IsAny<IEnumerable<JudiciaryPersonRequest>>()), Times.Never);
        }
        
        [Test]
        public async Task Should_not_call_booking_api_client_when_no_results()
        {
            _eLinksApiClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<JudiciaryPersonModel>());

            await _eLinksService.ImportJudiciaryPeopleAsync(new DateTime());
            
            _eLinksApiClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            _bookingsApiClient.Verify(x => x.BulkJudiciaryPersonsAsync(It.IsAny<IEnumerable<JudiciaryPersonRequest>>()), Times.Never);
        }
        
        [Test]
        public async Task Should_call_booking_api_client_with_one_page_of_results()
        {
            var judiciaryPersonModels = new List<JudiciaryPersonModel>
            {
                new JudiciaryPersonModel{Id = Guid.NewGuid(), Email = "one"},    
                new JudiciaryPersonModel{Id = Guid.NewGuid(), Email = "two"},    
                new JudiciaryPersonModel{Id = Guid.NewGuid(), Email = "three"}    
            };
            
            _eLinksApiClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>())).ReturnsAsync(judiciaryPersonModels);
            _eLinksApiClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>())).ReturnsAsync(new List<JudiciaryPersonModel>());

            await _eLinksService.ImportJudiciaryPeopleAsync(new DateTime());
            
            _eLinksApiClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()), Times.Once);
            _eLinksApiClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()), Times.Once);
            _bookingsApiClient.Verify(x => x.BulkJudiciaryPersonsAsync(It.Is<IEnumerable<JudiciaryPersonRequest>>
                (
                    x => 
                    x.ElementAt(0).Email == judiciaryPersonModels.ElementAt(0).Email &&
                    x.ElementAt(1).Email == judiciaryPersonModels.ElementAt(1).Email &&
                    x.ElementAt(2).Email == judiciaryPersonModels.ElementAt(2).Email
                )), Times.Once);
        }
        
        [Test]
        public async Task Should_call_booking_api_client_with_many_pages_of_results()
        {
            var judiciaryPersonModels = new List<JudiciaryPersonModel>
            {
                new JudiciaryPersonModel{Id = Guid.NewGuid(), Email = "one"},    
                new JudiciaryPersonModel{Id = Guid.NewGuid(), Email = "two"},    
                new JudiciaryPersonModel{Id = Guid.NewGuid(), Email = "three"}    
            };
            
            _eLinksApiClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>())).ReturnsAsync(judiciaryPersonModels);
            _eLinksApiClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>())).ReturnsAsync(judiciaryPersonModels);
            _eLinksApiClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 3, It.IsAny<int>())).ReturnsAsync(judiciaryPersonModels);
            _eLinksApiClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 4, It.IsAny<int>())).ReturnsAsync(judiciaryPersonModels);
            _eLinksApiClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 5, It.IsAny<int>())).ReturnsAsync(new List<JudiciaryPersonModel>());

            await _eLinksService.ImportJudiciaryPeopleAsync(new DateTime());
            
            _eLinksApiClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()), Times.Once);
            _eLinksApiClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()), Times.Once);
            _eLinksApiClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 3, It.IsAny<int>()), Times.Once);
            _eLinksApiClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 4, It.IsAny<int>()), Times.Once);
            _eLinksApiClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 5, It.IsAny<int>()), Times.Once);
            
            _bookingsApiClient.Verify(x => x.BulkJudiciaryPersonsAsync(It.IsAny<IEnumerable<JudiciaryPersonRequest>>()), Times.Exactly(4));
        }
        
        [Test]
        public async Task Should_call_booking_api_client_which_returns_some_error_responses()
        {
            var judiciaryPersonModels = new List<JudiciaryPersonModel>
            {
                new JudiciaryPersonModel{Id = Guid.NewGuid(), Email = "one"},    
                new JudiciaryPersonModel{Id = Guid.NewGuid(), Email = "two"},    
                new JudiciaryPersonModel{Id = Guid.NewGuid(), Email = "three"}    
            };
            
            _eLinksApiClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>())).ReturnsAsync(judiciaryPersonModels);
            _eLinksApiClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>())).ReturnsAsync(new List<JudiciaryPersonModel>());

            _bookingsApiClient.Setup(x => x.BulkJudiciaryPersonsAsync(It.IsAny<IEnumerable<JudiciaryPersonRequest>>()))
                .ReturnsAsync(new BulkJudiciaryPersonResponse
                {
                    ErroredRequests = new List<JudiciaryPersonErrorResponse>{new JudiciaryPersonErrorResponse{Message = "some error"}}
                });
            
            await _eLinksService.ImportJudiciaryPeopleAsync(new DateTime());
            
            _eLinksApiClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()), Times.Once);
            _eLinksApiClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()), Times.Once);
            _bookingsApiClient.Verify(x => x.BulkJudiciaryPersonsAsync(It.IsAny<IEnumerable<JudiciaryPersonRequest>>()), Times.Once);
        }
    }
}