using BookingsApi.Client;
using BookingsApi.Contract.Requests;
using BookingsApi.Contract.Responses;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SchedulerJobs.Common.Models;
using SchedulerJobs.Services;
using SchedulerJobs.Services.HttpClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerJobs.UnitTests.Services
{
    [TestFixture]
    public class ELinksServiceTests
    {
        private Mock<IPeoplesClient> _peoplesClient;
        private Mock<ILeaversClient> _leaversClient;
        private Mock<IBookingsApiClient> _bookingsApiClient;
        private Mock<ILogger<ELinksService>> _logger;

        private ELinksService _eLinksService;

        [SetUp]
        public void Setup()
        {
            _peoplesClient = new Mock<IPeoplesClient>();
            _leaversClient = new Mock<ILeaversClient>();
            _bookingsApiClient = new Mock<IBookingsApiClient>();
            _logger = new Mock<ILogger<ELinksService>>();

            _eLinksService = new ELinksService(_peoplesClient.Object, _leaversClient.Object, _bookingsApiClient.Object,
                _logger.Object);
        }

        [Test]
        public async Task Should_not_call_booking_api_client_when_no_results()
        {
            var expectedPeopleResponse = new PeopleResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = false
                },
                Results = new List<JudiciaryPersonModel>()
            };
            var expectedLeaverResponse = new LeaversResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = false
                },
                Results = new List<JudiciaryLeaverModel>()
            };
            _peoplesClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(expectedPeopleResponse);
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(expectedLeaverResponse);

            await _eLinksService.ImportJudiciaryPeopleAsync(new DateTime());
            await _eLinksService.ImportLeaversJudiciaryPeopleAsync(new DateTime());

            _peoplesClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Once);
            _leaversClient.Verify(x => x.GetLeaversAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Once);
            _bookingsApiClient.Verify(x => x.BulkJudiciaryPersonsAsync(It.IsAny<IEnumerable<JudiciaryPersonRequest>>()),
                Times.Never);
        }

        [Test]
        public async Task Should_call_booking_api_client_with_one_page_of_results()
        {
            var person1 = Guid.NewGuid().ToString();
            var person2 = Guid.NewGuid().ToString();
            var person3 = Guid.NewGuid().ToString();

            var judiciaryPersonModels = new List<JudiciaryPersonModel>
            {
                new JudiciaryPersonModel {Id = person1, Email = "one"},
                new JudiciaryPersonModel {Id = person2, Email = "two"},
                new JudiciaryPersonModel {Id = person3, Email = "three"}
            };

            var judiciaryLeaverModels = new List<JudiciaryLeaverModel>
            {
                new JudiciaryLeaverModel {Id = person1, Leaver = true},
                new JudiciaryLeaverModel {Id = person2, Leaver = true}
            };

            var personPage1Response = new PeopleResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = true
                },
                Results = judiciaryPersonModels
            };
            var personPage2Response = new PeopleResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = false
                },
                Results = new List<JudiciaryPersonModel>()
            };
            var leaverPage1Response = new LeaversResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = true
                },
                Results = judiciaryLeaverModels
            };
            var leaverPage2Response = new LeaversResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = false
                },
                Results = new List<JudiciaryLeaverModel>()
            };

            _peoplesClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()))
                .ReturnsAsync(personPage1Response);
            _peoplesClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()))
                .ReturnsAsync(personPage2Response);
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()))
                .ReturnsAsync(leaverPage1Response);
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()))
                .ReturnsAsync(leaverPage2Response);

            await _eLinksService.ImportJudiciaryPeopleAsync(new DateTime());
            await _eLinksService.ImportLeaversJudiciaryPeopleAsync(new DateTime());

            _peoplesClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()), Times.Once);
            _peoplesClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()), Times.Once);
            _leaversClient.Verify(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()), Times.Once);
            _leaversClient.Verify(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()), Times.Once);
            _bookingsApiClient.Verify(x => x.BulkJudiciaryPersonsAsync(It.Is<IEnumerable<JudiciaryPersonRequest>>
            (
                x =>
                    x.ElementAt(0).Id == person1 &&
                    x.ElementAt(1).Id == person2 &&
                    x.ElementAt(2).Id == person3
            )), Times.Exactly(1));
            _bookingsApiClient.Verify(x => x.BulkJudiciaryLeaversAsync(It.Is<IEnumerable<JudiciaryLeaverRequest>>
            (
                x =>
                    x.ElementAt(0).Id == person1
            )), Times.Exactly(1));
        }

        [Test]
        public async Task Should_call_booking_api_client_with_many_pages_of_leavers_results()
        {
            var person1 = Guid.NewGuid().ToString();
            var judiciaryLeaverModels = new List<JudiciaryLeaverModel>
            {
                new JudiciaryLeaverModel {Id = person1, Leaver = true, LeftOn = new DateTime().ToString()}
            };
            CommonTestSetUp();
            _peoplesClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>())).ReturnsAsync(
                new PeopleResponse()
                {
                    Pagination = new Pagination
                    {
                        MorePages = true
                    },
                    Results = new List<JudiciaryPersonModel>()
                    {
                        new JudiciaryPersonModel {Id = person1, Email = "one"}
                    }
                });
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 5, It.IsAny<int>()))
                .ReturnsAsync(new LeaversResponse()
                {
                    Pagination = new Pagination
                    {
                        MorePages = false
                    },
                    Results = judiciaryLeaverModels
                });

            await CommonActAndAssert(4, 5);
        }
        
        [Test]
        public async Task Should_call_booking_api_client_with_one_of_many_pages_of_leavers_results_has_exception()
        {
            var person1 = Guid.NewGuid();
            CommonTestSetUp();
            var personPage1Response = new PeopleResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = true
                },
                Results = new List<JudiciaryPersonModel>()
                {
                    new JudiciaryPersonModel{Id = Guid.NewGuid().ToString(), Email = "one"},
                }
            };
            _peoplesClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>())).ReturnsAsync(personPage1Response);
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 5, It.IsAny<int>())).ThrowsAsync(new JsonSerializationException("Error converting value 1ad85664b - aab9 - 4a8d - 8e76 - 0affdcb5b90f"));
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 6, It.IsAny<int>())).ReturnsAsync(new LeaversResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = false
                },
                Results = new List<JudiciaryLeaverModel>
                {
                    new JudiciaryLeaverModel{Id = person1.ToString(), Leaver = true, LeftOn = new DateTime().ToString()}
                }
            });
            await CommonActAndAssert(4, 5);
        }

        [Test]
        public async Task Should_call_booking_api_client_with_many_pages_of_results()
        {
            CommonTestSetUp();
            var personPage1Response = new PeopleResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = true
                },
                Results = new List<JudiciaryPersonModel>()
                {
                    new JudiciaryPersonModel{Id = Guid.NewGuid().ToString(), Email = "one"},
                }
            };
            _peoplesClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>())).ReturnsAsync(personPage1Response);
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 5, It.IsAny<int>())).ReturnsAsync(new LeaversResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = false
                },
                Results = new List<JudiciaryLeaverModel>()
            });

            await CommonActAndAssert(4, 4);
        }

        [Test]
        public async Task Should_call_booking_api_client_with_one_of_many_pages_of_results_has_exception()
        {
            CommonTestSetUp();
            _peoplesClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()))
                .ThrowsAsync(new JsonSerializationException("Error converting value 1ad85664b - aab9 - 4a8d - 8e76 - 0affdcb5b90f"));
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 5, It.IsAny<int>())).ReturnsAsync(new LeaversResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = false
                },
                Results = new List<JudiciaryLeaverModel>()
            });

            await CommonActAndAssert(3, 4);
        }

        [Test]
        public async Task Should_call_booking_api_client_which_returns_some_error_responses()
        {
            var person1 = Guid.NewGuid().ToString();
            var person2 = Guid.NewGuid().ToString();
            var person3 = Guid.NewGuid().ToString();

            var judiciaryPersonModels = new List<JudiciaryPersonModel>
            {
                new JudiciaryPersonModel {Id = person1, Email = "one"},
                new JudiciaryPersonModel {Id = person2, Email = "two"},
                new JudiciaryPersonModel {Id = person3, Email = "three"}
            };

            var judiciaryLeaverModels = new List<JudiciaryLeaverModel>
            {
                new JudiciaryLeaverModel {Id = person1, Leaver = true},
                new JudiciaryLeaverModel {Id = person2, Leaver = true}
            };

            var personPage1Response = new PeopleResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = true
                },
                Results = judiciaryPersonModels
            };
            var personPage2Response = new PeopleResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = false
                },
                Results = new List<JudiciaryPersonModel>()
            };
            var leaverPage1Response = new LeaversResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = true
                },
                Results = judiciaryLeaverModels
            };
            var leaverPage2Response = new LeaversResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = false
                },
                Results = new List<JudiciaryLeaverModel>()
            };

            _peoplesClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>())).ReturnsAsync(personPage1Response);
            _peoplesClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>())).ReturnsAsync(personPage2Response);
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>())).ReturnsAsync(leaverPage1Response);
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>())).ReturnsAsync(leaverPage2Response);

            _bookingsApiClient.Setup(x => x.BulkJudiciaryPersonsAsync(It.IsAny<IEnumerable<JudiciaryPersonRequest>>()))
                .ReturnsAsync(new BulkJudiciaryPersonResponse
                {
                    ErroredRequests = new List<JudiciaryPersonErrorResponse> { new JudiciaryPersonErrorResponse { Message = "some error" } }
                });

            await _eLinksService.ImportJudiciaryPeopleAsync(DateTime.Now.AddDays(-100));
            await _eLinksService.ImportLeaversJudiciaryPeopleAsync(DateTime.Now.AddDays(-100));

            _peoplesClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()), Times.Once);
            _peoplesClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()), Times.Once);
            _leaversClient.Verify(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()), Times.Once);
            _bookingsApiClient.Verify(x => x.BulkJudiciaryPersonsAsync(It.IsAny<IEnumerable<JudiciaryPersonRequest>>()), Times.Once);
            _bookingsApiClient.Verify(x => x.BulkJudiciaryLeaversAsync(It.IsAny<IEnumerable<JudiciaryLeaverRequest>>()), Times.Once);
        }
        
        private void CommonTestSetUp()
        {
            var person1 = Guid.NewGuid().ToString();
            var person2 = Guid.NewGuid().ToString();
            var person3 = Guid.NewGuid().ToString();

            var judiciaryPersonModels = new List<JudiciaryPersonModel>
            {
                new JudiciaryPersonModel {Id = person1, Email = "one"},
                new JudiciaryPersonModel {Id = person2, Email = "two"},
                new JudiciaryPersonModel {Id = person3, Email = "three"}
            };

            var judiciaryLeaverModels = new List<JudiciaryLeaverModel>
            {
                new JudiciaryLeaverModel {Id = person1, Leaver = true, LeftOn = new DateTime().ToString()}
            };

            var expectedPeopleResponse = new PeopleResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = true
                },
                Results = judiciaryPersonModels
            };
            var expectedLeaverResponse = new LeaversResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = true
                },
                Results = judiciaryLeaverModels
            };

            _peoplesClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()))
                .ReturnsAsync(expectedPeopleResponse);
            _peoplesClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 3, It.IsAny<int>()))
                .ReturnsAsync(expectedPeopleResponse);
            _peoplesClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 4, It.IsAny<int>()))
                .ReturnsAsync(expectedPeopleResponse);
            _peoplesClient.Setup(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 5, It.IsAny<int>())).ReturnsAsync(
                new PeopleResponse()
                {
                    Pagination = new Pagination
                    {
                        MorePages = false
                    },
                    Results = new List<JudiciaryPersonModel>()
                });
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()))
                .ReturnsAsync(expectedLeaverResponse);
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()))
                .ReturnsAsync(expectedLeaverResponse);
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 3, It.IsAny<int>()))
                .ReturnsAsync(expectedLeaverResponse);
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 4, It.IsAny<int>()))
                .ReturnsAsync(expectedLeaverResponse);
        }

        private async Task CommonActAndAssert(int peopleTimes, int leaverTimes)
        {
            await _eLinksService.ImportJudiciaryPeopleAsync(new DateTime());
            await _eLinksService.ImportLeaversJudiciaryPeopleAsync(new DateTime());

            _peoplesClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()), Times.Once);
            _peoplesClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()), Times.Once);
            _peoplesClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 3, It.IsAny<int>()), Times.Once);
            _peoplesClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 4, It.IsAny<int>()), Times.Once);
            _peoplesClient.Verify(x => x.GetPeopleAsync(It.IsAny<DateTime>(), 5, It.IsAny<int>()), Times.Once);
            _leaversClient.Verify(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()), Times.Once);
            _leaversClient.Verify(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()), Times.Once);
            _leaversClient.Verify(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 3, It.IsAny<int>()), Times.Once);
            _leaversClient.Verify(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 4, It.IsAny<int>()), Times.Once);
            _leaversClient.Verify(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 5, It.IsAny<int>()), Times.Once);

            _bookingsApiClient.Verify(x => x.BulkJudiciaryLeaversAsync(It.IsAny<IEnumerable<JudiciaryLeaverRequest>>()),
                Times.Exactly(leaverTimes));
        }
    }
}