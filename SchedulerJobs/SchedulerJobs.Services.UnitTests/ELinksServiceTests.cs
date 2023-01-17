using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingsApi.Client;
using BookingsApi.Contract.Requests;
using BookingsApi.Contract.Responses;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Common.Models;
using SchedulerJobs.Services.HttpClients;

namespace SchedulerJobs.Services.UnitTests
{
    [TestFixture]
    public class ELinksServiceTests
    {
        private Mock<IPeoplesClient> _peoplesClient;
        private Mock<ILeaversClient> _leaversClient;
        private Mock<IBookingsApiClient> _bookingsApiClient;
        private Mock<ILogger<ELinksService>> _logger;
        private Mock<IAzureStorageService> _service;
        private Mock<IFeatureToggles> _featureToggles;
        private Mock<IJobHistoryService> _jobHistoryService;
        private ELinksService _eLinksService;
        

        [SetUp]
        public void Setup()
        {
            _peoplesClient = new Mock<IPeoplesClient>();
            _leaversClient = new Mock<ILeaversClient>();
            _bookingsApiClient = new Mock<IBookingsApiClient>();
            _logger = new Mock<ILogger<ELinksService>>();
            _service = new Mock<IAzureStorageService>();
            _featureToggles = new Mock<IFeatureToggles>();
            _jobHistoryService = new Mock<IJobHistoryService>();

            _eLinksService = new ELinksService(_peoplesClient.Object, _leaversClient.Object, _bookingsApiClient.Object,
                _logger.Object, _service.Object, _featureToggles.Object, _jobHistoryService.Object);
            
        }

        [Test]
        public async Task ImportJudiciaryPeopleAsync_Clears_Existing_Judiciary_Person_Staging_Records()
        {
            var expectedPeopleResponse = new PeopleResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = false
                },
                Results = new List<JudiciaryPersonModel>()
            };

            var clientResponse = JsonConvert.SerializeObject(expectedPeopleResponse);

            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(clientResponse);

            await _eLinksService.ImportJudiciaryPeopleAsync(new DateTime());

            _bookingsApiClient.Verify(x => x.RemoveAllJudiciaryPersonsStagingAsync(),
                Times.Exactly(1));
        }

        [Test]
        public async Task ImportJudiciaryPeopleAsync_Clears_Existing_Json_Blobs()
        {
            var expectedPeopleResponse = new PeopleResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = false,
                    CurrentPage = 1,
                    Pages = 1
                },
                Results = new List<JudiciaryPersonModel>()
            };
            
            var clientResponse = JsonConvert.SerializeObject(expectedPeopleResponse);
            

            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(clientResponse);
            
            _featureToggles.Setup(x => x.StorePeopleIngestion())
                .Returns(true);

            await _eLinksService.ImportJudiciaryPeopleAsync(new DateTime());

            _service.Verify(x => x.ClearBlobs(),
                Times.Exactly(1));
        }
        
        [Test]
        public async Task ImportJudiciaryPeopleAsync_Not_Clears_Existing_Json_Blobs()
        {
            var expectedPeopleResponse = new PeopleResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = false,
                    CurrentPage = 2,
                    Pages = 2
                },
                Results = new List<JudiciaryPersonModel>()
            };

            var clientResponse = JsonConvert.SerializeObject(expectedPeopleResponse);
            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()))
                .ReturnsAsync(clientResponse);
            
            _featureToggles.Setup(x => x.StorePeopleIngestion())
                .Returns(true);

            await _eLinksService.ImportJudiciaryPeopleAsync(new DateTime());

            _service.Verify(x => x.ClearBlobs(),
                Times.Exactly(0));
        }
        
        [Test]
        public async Task ImportJudiciaryPeopleAsync_Create_Combined_File()
        {
            var person1 = Guid.NewGuid().ToString();
            var person2 = Guid.NewGuid().ToString();
            var judiciaryPersonModels = new List<JudiciaryPersonModel>
              {
                  new JudiciaryPersonModel {Id = person1, Email = "one"},
                  new JudiciaryPersonModel {Id = person2, Email = "two"},
                  new JudiciaryPersonModel {Id = null, Email = "three"}
              };
            var expectedPeopleResponse = new PeopleResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = false,
                    CurrentPage = 2,
                    Pages = 2
                },
                Results = judiciaryPersonModels
            };
            var clientResponse = JsonConvert.SerializeObject(expectedPeopleResponse);
            
            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(clientResponse);

            _featureToggles.Setup(x => x.StorePeopleIngestion())
                .Returns(true);

            await _eLinksService.ImportJudiciaryPeopleAsync(new DateTime());

            _service.Verify(x => x.UploadFile("combined.json", It.IsAny<byte[]>()),
                Times.Exactly(1));
        }
        
        [Test]
        public async Task ImportJudiciaryPeopleAsync_Not_Upload_With_FeautureFlag_Off()
        {
            var expectedPeopleResponse = new PeopleResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = false,
                    CurrentPage = 2,
                    Pages = 2
                },
                Results = new List<JudiciaryPersonModel>()
            };
            var clientResponse = JsonConvert.SerializeObject(expectedPeopleResponse);

            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(clientResponse);
            
            _featureToggles.Setup(x => x.StorePeopleIngestion())
                .Returns(false);

            await _eLinksService.ImportJudiciaryPeopleAsync(new DateTime());

            _service.Verify(x => x.ClearBlobs(),
                Times.Exactly(0));
            
            _peoplesClient.Verify(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()),
                Times.Exactly(0));

        }

        [Test]
        public async Task ImportJudiciaryPeopleAsync_Adds_Invalid_Accounts()
        {
            var person1 = Guid.NewGuid().ToString();
            var person2 = Guid.NewGuid().ToString();

            var judiciaryPersonModels = new List<JudiciaryPersonModel>
            {
                new JudiciaryPersonModel {Id = person1, Email = "one"},
                new JudiciaryPersonModel {Id = person2, Email = "two"},
                new JudiciaryPersonModel {Id = null, Email = "three"}
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
            var clientResponse1 = JsonConvert.SerializeObject(personPage1Response);
            var clientResponse2 = JsonConvert.SerializeObject(personPage2Response);

            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()))
                .ReturnsAsync(clientResponse1);
            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()))
                .ReturnsAsync(clientResponse2);

            await _eLinksService.ImportJudiciaryPeopleAsync(new DateTime());

            _bookingsApiClient.Verify(x => x.BulkJudiciaryPersonsStagingAsync(
                It.Is<IEnumerable<JudiciaryPersonStagingRequest>>
                (
                    x =>
                        x.ElementAt(0).Id == person1 &&
                        x.ElementAt(1).Id == person2 &&
                        x.ElementAt(2).Id == null
                )), Times.Exactly(1));
        }

        [Test]
        public async Task ImportJudiciaryPeopleAsync_Requests_To_Add_JudiciaryPersonStaging_Records()
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
            var clientResponse1 = JsonConvert.SerializeObject(personPage1Response);
            var clientResponse2 = JsonConvert.SerializeObject(personPage2Response);
            
            
            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()))
                .ReturnsAsync(clientResponse1);
            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()))
                .ReturnsAsync(clientResponse2);

            await _eLinksService.ImportJudiciaryPeopleAsync(new DateTime());

            _bookingsApiClient.Verify(x => x.BulkJudiciaryPersonsStagingAsync(
                It.Is<IEnumerable<JudiciaryPersonStagingRequest>>
                (
                    x =>
                        x.ElementAt(0).Id == person1 &&
                        x.ElementAt(1).Id == person2 &&
                        x.ElementAt(2).Id == person3
                )), Times.Exactly(1));
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
            
            var clientResponse = JsonConvert.SerializeObject(expectedPeopleResponse);

            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(expectedLeaverResponse);
            
            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(clientResponse);

            await _eLinksService.ImportJudiciaryPeopleAsync(new DateTime());
            await _eLinksService.ImportLeaversJudiciaryPeopleAsync(new DateTime());

            _peoplesClient.Verify(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()),
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

            var personPage1Response = new
            {
                pagination = new
                {
                    more_pages = true
                },
                results = judiciaryPersonModels
            };
            var personPage2Response = new
            {
                pagination = new
                {
                    more_pages = false
                },
                results = new List<JudiciaryPersonModel>()
            };
            var personPage1 = JsonConvert.SerializeObject(personPage1Response);
            var personPage2 = JsonConvert.SerializeObject(personPage2Response);
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

            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()))
                .ReturnsAsync(leaverPage1Response);
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()))
                .ReturnsAsync(leaverPage2Response);
            
            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()))
                .ReturnsAsync(personPage1);
            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()))
                .ReturnsAsync(personPage2);

            await _eLinksService.ImportJudiciaryPeopleAsync(new DateTime());
            await _eLinksService.ImportLeaversJudiciaryPeopleAsync(new DateTime());

            _peoplesClient.Verify(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()), Times.Once);
            _peoplesClient.Verify(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()), Times.Once);
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
            var peopleResponse = new PeopleResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = true
                },
                Results = new List<JudiciaryPersonModel>()
                {
                    new JudiciaryPersonModel {Id = person1, Email = "one"}
                }
            };
            var clientResponse = JsonConvert.SerializeObject(peopleResponse);

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
                    new JudiciaryPersonModel {Id = Guid.NewGuid().ToString(), Email = "one"},
                }
            };
            var clientResponse = JsonConvert.SerializeObject(personPage1Response);
            
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 5, It.IsAny<int>())).ThrowsAsync(
                new JsonSerializationException("Error converting value 1ad85664b - aab9 - 4a8d - 8e76 - 0affdcb5b90f"));
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 6, It.IsAny<int>())).ReturnsAsync(
                new LeaversResponse()
                {
                    Pagination = new Pagination
                    {
                        MorePages = false
                    },
                    Results = new List<JudiciaryLeaverModel>
                    {
                        new JudiciaryLeaverModel
                            {Id = person1.ToString(), Leaver = true, LeftOn = new DateTime().ToString()}
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
                    new JudiciaryPersonModel {Id = Guid.NewGuid().ToString(), Email = "one"},
                }
            };
            var clientResponse = JsonConvert.SerializeObject(personPage1Response);

            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 5, It.IsAny<int>())).ReturnsAsync(
                new LeaversResponse()
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

            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 5, It.IsAny<int>())).ReturnsAsync(
                new LeaversResponse()
                {
                    Pagination = new Pagination
                    {
                        MorePages = false
                    },
                    Results = new List<JudiciaryLeaverModel>()
                });
            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()))
                .ThrowsAsync(
                    new JsonSerializationException(
                        "Error converting value 1ad85664b - aab9 - 4a8d - 8e76 - 0affdcb5b90f"));
            
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

            var personPage1Response = new
            {
                pagination = new
                {
                    more_pages = true
                },
                results = judiciaryPersonModels
            };
            var personPage2Response = new
            {
                pagination = new
                {
                    more_pages = false
                },
                results = new List<JudiciaryPersonModel>()
            };
            
            var clientResponse1 = JsonConvert.SerializeObject(personPage1Response);
            var clientResponse2 = JsonConvert.SerializeObject(personPage2Response);
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
            
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()))
                .ReturnsAsync(leaverPage1Response);
            _leaversClient.Setup(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()))
                .ReturnsAsync(leaverPage2Response);
            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()))
                .ReturnsAsync(clientResponse1);
            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()))
                .ReturnsAsync(clientResponse2);
            
            _bookingsApiClient.Setup(x => x.BulkJudiciaryPersonsAsync(It.IsAny<IEnumerable<JudiciaryPersonRequest>>()))
                .ReturnsAsync(new BulkJudiciaryPersonResponse
                {
                    ErroredRequests = new List<JudiciaryPersonErrorResponse>
                        {new JudiciaryPersonErrorResponse {Message = "some error"}}
                });

            await _eLinksService.ImportJudiciaryPeopleAsync(DateTime.Now.AddDays(-100));
            await _eLinksService.ImportLeaversJudiciaryPeopleAsync(DateTime.Now.AddDays(-100));

            _peoplesClient.Verify(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()), Times.Once);
            _peoplesClient.Verify(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()), Times.Once);
            _leaversClient.Verify(x => x.GetLeaversAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()), Times.Once);
            _bookingsApiClient.Verify(x => x.BulkJudiciaryPersonsAsync(It.IsAny<IEnumerable<JudiciaryPersonRequest>>()),
                Times.Once);
            _bookingsApiClient.Verify(x => x.BulkJudiciaryLeaversAsync(It.IsAny<IEnumerable<JudiciaryLeaverRequest>>()),
                Times.Once);
        }

        [Test]
        public async Task GetUpdatedSince_With_ImportAllJudiciaryUsers_Toggled_On_Returns_Minimum_DateTime()
        {
            // Arrange
            _featureToggles.Setup(x => x.ImportAllJudiciaryUsersToggle()).Returns(true);
       
            // Act
            var updatedSince = await _eLinksService.GetUpdatedSince();

            // Assert
            Assert.AreEqual(DateTime.Parse("0001-01-01"), updatedSince);
        }

        [Test]
        public async Task GetUpdatedSince_With_ImportAllJudiciaryUsers_Toggled_Off_And_Previous_Successful_Run_Returns_Previous_Successful_Run_DateTime()
        {
            // Arrange
            _featureToggles.Setup(x => x.ImportAllJudiciaryUsersToggle()).Returns(false);
            _jobHistoryService.Setup(x => x.GetMostRecentSuccessfulRunDate(It.IsAny<string>()))
                .ReturnsAsync(DateTime.Parse("2022-01-01"));
            
            // Act
            var updatedSince = await _eLinksService.GetUpdatedSince();
            
            // Assert
            Assert.AreEqual(DateTime.Parse("2022-01-01"), updatedSince);
        }
        
        [Test]
        public async Task GetUpdatedSince_With_ImportAllJudiciaryUsers_Toggled_Off_And_Previous_Successful_Run_Returns_Yesterdays_DateTime()
        {
            // Arrange
            _featureToggles.Setup(x => x.ImportAllJudiciaryUsersToggle()).Returns(false);
            _jobHistoryService.Setup(x => x.GetMostRecentSuccessfulRunDate(It.IsAny<string>()))
                .ReturnsAsync((DateTime?)null);
            
            // Act
            var updatedSince = await _eLinksService.GetUpdatedSince();
            
            // Assert
            Assert.AreEqual(DateTime.UtcNow.AddDays(-1).Date, updatedSince.Date);
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

            var expectedPeopleResponse = new
            {
                pagination = new
                {
                    more_pages = true
                },
                results = judiciaryPersonModels
            };
            var clientResponse1 = JsonConvert.SerializeObject(expectedPeopleResponse);

            var clientNoMorePage = new
            {
                pagination = new
                {
                    more_pages = false
                },
                results = new List<JudiciaryPersonModel>()
            };
            
            var clientResponse2 = JsonConvert.SerializeObject(clientNoMorePage);
            
            var expectedLeaverResponse = new LeaversResponse()
            {
                Pagination = new Pagination
                {
                    MorePages = true
                },
                Results = judiciaryLeaverModels
            };

            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()))
                .ReturnsAsync(clientResponse1);
            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 3, It.IsAny<int>()))
                .ReturnsAsync(clientResponse1);
            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 4, It.IsAny<int>()))
                .ReturnsAsync(clientResponse1);
            _peoplesClient.Setup(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 5, It.IsAny<int>())).ReturnsAsync(clientResponse2);
            
            
            
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

            _peoplesClient.Verify(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 1, It.IsAny<int>()), Times.Once);
            _peoplesClient.Verify(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 2, It.IsAny<int>()), Times.Once);
            _peoplesClient.Verify(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 3, It.IsAny<int>()), Times.Once);
            _peoplesClient.Verify(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 4, It.IsAny<int>()), Times.Once);
            _peoplesClient.Verify(x => x.GetPeopleJsonAsync(It.IsAny<DateTime>(), 5, It.IsAny<int>()), Times.Once);
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