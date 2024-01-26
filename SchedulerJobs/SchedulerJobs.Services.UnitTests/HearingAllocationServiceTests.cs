using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookingsApi.Client;
using BookingsApi.Contract.V1.Responses;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Common.Configuration;

namespace SchedulerJobs.Services.UnitTests
{
    public class HearingAllocationServiceTests
    {
        private IHearingAllocationService _service;
        private Mock<IBookingsApiClient> _bookingsApiClient;
        private Mock<IFeatureToggles> _featureToggles;
        private Mock<ILogger<HearingAllocationService>> _logger;
        
        [SetUp]
        public void Setup()
        {
            _bookingsApiClient = new Mock<IBookingsApiClient>();
            _featureToggles = new Mock<IFeatureToggles>();
            _featureToggles.Setup(x => x.WorkAllocationToggle()).Returns(true);
            _logger = new Mock<ILogger<HearingAllocationService>>();
            _service = new HearingAllocationService(_bookingsApiClient.Object, 
                _featureToggles.Object, 
                _logger.Object);
        }

        [Test]
        public async Task AllocateHearingsAsync_Should_Not_Call_Bookings_Api_When_Work_Allocation_Toggle_Is_Off()
        {
            // Arrange
            _featureToggles.Setup(x => x.WorkAllocationToggle()).Returns(false);

            // Act
            await _service.AllocateHearingsAsync();

            // Assert
            _bookingsApiClient.Verify(x => x.GetUnallocatedHearingsAsync(), Times.Never);
            AssertMessageLogged("AllocateHearings: Feature WorkAllocation is turned off!", LogLevel.Information);
        }

        [Test]
        public async Task AllocateHearingsAsync_Should_Call_Bookings_Api_For_Each_Unallocated_Hearing()
        {
            // Arrange
            var hearing1 = new HearingDetailsResponse { Id = Guid.NewGuid() };
            var hearing2 = new HearingDetailsResponse { Id = Guid.NewGuid() };
            var hearing3 = new HearingDetailsResponse { Id = Guid.NewGuid() };
            var unallocatedHearings = new List<HearingDetailsResponse>
            {
                hearing1,
                hearing2,
                hearing3
            };
            _bookingsApiClient.Setup(x => x.GetUnallocatedHearingsAsync()).ReturnsAsync(unallocatedHearings);

            var allocatedUser1 = new JusticeUserResponse { Username = "user1@email.com" };
            var allocatedUser2 = new JusticeUserResponse { Username = "user2@email.com" };
            var allocatedUser3 = new JusticeUserResponse { Username = "user3@email.com" };
            var allocationMappings = new Dictionary<HearingDetailsResponse, JusticeUserResponse>
            {
                {
                    hearing1, allocatedUser1
                },
                {
                    hearing2, allocatedUser2
                },
                {
                    hearing3, allocatedUser3
                }
            };
            foreach (var mapping in allocationMappings)
            {
                _bookingsApiClient.Setup(x => x.AllocateHearingAutomaticallyAsync(It.Is<Guid>(
                        hearingId => hearingId == mapping.Key.Id)))
                    .ReturnsAsync(mapping.Value);
            }

            // Act
            await _service.AllocateHearingsAsync();

            // Assert
            _bookingsApiClient.Verify(x => x.GetUnallocatedHearingsAsync(), Times.Once);
            _bookingsApiClient.Verify(x => x.AllocateHearingAutomaticallyAsync(It.IsAny<Guid>()), Times.Exactly(unallocatedHearings.Count));
            foreach (var mapping in allocationMappings)
            {
                _bookingsApiClient.Verify(x => x.AllocateHearingAutomaticallyAsync(mapping.Key.Id), Times.Once);
                AssertMessageLogged($"AllocateHearings: Allocated user {mapping.Value.Username} to hearing {mapping.Key.Id}", LogLevel.Information);
            }
            AssertMessageLogged("AllocateHearings: Completed allocation of hearings, 3 of 3 hearings allocated", LogLevel.Information);
        }

        [Test]
        public async Task AllocateHearingsAsync_Should_Continue_To_Process_Other_Hearings_When_Exception_Thrown_Allocating_A_Hearing()
        {
            // Arrange
            var hearing1 = new HearingDetailsResponse { Id = Guid.NewGuid() };
            var hearing2 = new HearingDetailsResponse { Id = Guid.NewGuid() };
            var hearing3 = new HearingDetailsResponse { Id = Guid.NewGuid() };
            var unallocatedHearings = new List<HearingDetailsResponse>
            {
                hearing1,
                hearing2,
                hearing3
            };
            _bookingsApiClient.Setup(x => x.GetUnallocatedHearingsAsync()).ReturnsAsync(unallocatedHearings);

            var allocatedUser1 = new JusticeUserResponse { Username = "user1@email.com" };
            var allocatedUser2 = new JusticeUserResponse { Username = "user2@email.com" };
            var allocationMappings = new Dictionary<HearingDetailsResponse, JusticeUserResponse>
            {
                {
                    hearing1, allocatedUser1
                },
                {
                    hearing2, null
                },
                {
                    hearing3, allocatedUser2
                }
            };
            foreach (var mapping in allocationMappings)
            {
                _bookingsApiClient.Setup(x => x.AllocateHearingAutomaticallyAsync(It.Is<Guid>(
                        hearingId => hearingId == mapping.Key.Id)))
                    .ReturnsAsync(mapping.Value);
            }
            var hearingIdToThrowException = hearing2.Id;
            var unknownException = new BookingsApiException("", 500, "", null, null);
            _bookingsApiClient.Setup(x => x.GetUnallocatedHearingsAsync()).ReturnsAsync(unallocatedHearings);
            _bookingsApiClient.Setup(x => x.AllocateHearingAutomaticallyAsync(It.Is<Guid>(hearingId => hearingId == hearingIdToThrowException)))
                .ThrowsAsync(unknownException);
   
            // Act
            await _service.AllocateHearingsAsync();
            
            // Assert
            _bookingsApiClient.Verify(x => x.AllocateHearingAutomaticallyAsync(It.IsAny<Guid>()), Times.Exactly(unallocatedHearings.Count));
            AssertErrorLogged(unknownException);
            AssertMessageLogged("AllocateHearings: Completed allocation of hearings, 2 of 3 hearings allocated", LogLevel.Information);
        }
        
        
        [Test]
        public async Task AllocateHearingsAsync_Should_Continue_To_Process_Other_Hearings_And_Log_Trace_When_Bad_Request_Returned()
        {
            // Arrange
            var hearing1 = new HearingDetailsResponse { Id = Guid.NewGuid() };
            var hearing2 = new HearingDetailsResponse { Id = Guid.NewGuid() };
            var hearing3 = new HearingDetailsResponse { Id = Guid.NewGuid() };
            var unallocatedHearings = new List<HearingDetailsResponse>
            {
                hearing1,
                hearing2,
                hearing3
            };
            _bookingsApiClient.Setup(x => x.GetUnallocatedHearingsAsync()).ReturnsAsync(unallocatedHearings);

            var allocatedUser1 = new JusticeUserResponse { Username = "user1@email.com" };
            var allocatedUser2 = new JusticeUserResponse { Username = "user2@email.com" };
            var allocationMappings = new Dictionary<HearingDetailsResponse, JusticeUserResponse>
            {
                {
                    hearing1, allocatedUser1
                },
                {
                    hearing2, null
                },
                {
                    hearing3, allocatedUser2
                }
            };
            foreach (var mapping in allocationMappings)
            {
                _bookingsApiClient.Setup(x => x.AllocateHearingAutomaticallyAsync(It.Is<Guid>(
                        hearingId => hearingId == mapping.Key.Id)))
                    .ReturnsAsync(mapping.Value);
            }
            var hearingIdToThrowException = hearing2.Id;
            var unknownException = new BookingsApiException("", 400, "", null, null);
            _bookingsApiClient.Setup(x => x.GetUnallocatedHearingsAsync()).ReturnsAsync(unallocatedHearings);
            _bookingsApiClient.Setup(x => x.AllocateHearingAutomaticallyAsync(It.Is<Guid>(hearingId => hearingId == hearingIdToThrowException)))
                .ThrowsAsync(unknownException);
   
            // Act
            await _service.AllocateHearingsAsync();
            
            // Assert
            _bookingsApiClient.Verify(x => x.AllocateHearingAutomaticallyAsync(It.IsAny<Guid>()), Times.Exactly(unallocatedHearings.Count));
            AssertTraceLogged(unknownException);
            AssertMessageLogged("AllocateHearings: Completed allocation of hearings, 2 of 3 hearings allocated", LogLevel.Information);
        }

        private void AssertMessageLogged(string expectedMessage, LogLevel expectedLogLevel)
        {
            _logger.Verify(x => x.Log(
                It.Is<LogLevel>(log => log == expectedLogLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == expectedMessage && @type.Name == "FormattedLogValues"),
                It.Is<Exception>(x => x == null),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        private void AssertErrorLogged(Exception exception)
        {
            _logger.Verify(x => x.Log(
                It.Is<LogLevel>(log => log == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(x => x == exception),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }
        
        private void AssertTraceLogged(Exception exception)
        {
            _logger.Verify(x => x.Log(
                It.Is<LogLevel>(log => log == LogLevel.Trace),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(x => x == exception),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }
    }
}
