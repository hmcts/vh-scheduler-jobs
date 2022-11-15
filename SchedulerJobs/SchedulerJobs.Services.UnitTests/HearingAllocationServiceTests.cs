using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookingsApi.Client;
using BookingsApi.Contract.Responses;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Services.Interfaces;

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
            AssertMessageLogged("AllocateHearingsAsync - Feature WorkAllocation is turned off!", LogLevel.Information);
        }

        [Test]
        public async Task AllocateHearingsAsync_Should_Call_Bookings_Api_For_Each_Unallocated_Hearing()
        {
            // Arrange
            var hearing1Id = Guid.NewGuid();
            var hearing2Id = Guid.NewGuid();
            var hearing3Id = Guid.NewGuid();
            var unallocatedHearings = new List<HearingDetailsResponse>
            {
                new()
                {
                    Id = hearing1Id
                },
                new()
                {
                    Id = hearing2Id
                },
                new()
                {
                    Id = hearing3Id
                }
            };
            
            _bookingsApiClient.Setup(x => x.GetUnallocatedHearingsAsync()).ReturnsAsync(unallocatedHearings);

            // Act
            await _service.AllocateHearingsAsync();

            // Assert
            _bookingsApiClient.Verify(x => x.GetUnallocatedHearingsAsync(), Times.Once);
            _bookingsApiClient.Verify(x => x.AllocateHearingAutomaticallyAsync(It.IsAny<Guid>()), Times.Exactly(unallocatedHearings.Count));
            unallocatedHearings.ForEach(h =>
            {
                _bookingsApiClient.Verify(x => x.AllocateHearingAutomaticallyAsync(h.Id), Times.Once);
            });
        }

        [Test]
        public async Task AllocateHearingsAsync_Should_Continue_To_Process_Other_Hearings_When_Exception_Thrown_Allocating_A_Hearing()
        {
            // TODO
            
            // Arrange
            
            // Act

            // Assert
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
    }
}
