using BookingsApi.Client;
using BookingsApi.Contract.Responses;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationApi.Client;
using NUnit.Framework;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Services;
using SchedulerJobs.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchedulerJobs.UnitTests.Services
{
    public class HearingNotificationServiceTests
    {
        private Mock<IBookingsApiClient> _bookingApiClient;
        private Mock<INotificationApiClient> _notificationApiClient;
        private Mock<IFeatureToggles> _featureToggles;
        private IHearingNotificationService _hearingNotificationService;
        private Mock<ILogger<HearingNotificationService>> _logger;
        private List<HearingDetailsResponse> hearings;


        [SetUp]
        public void Setup()
        {
            

            _bookingApiClient = new Mock<IBookingsApiClient>();
            _notificationApiClient = new Mock<INotificationApiClient>();
            _featureToggles = new Mock<IFeatureToggles>();

            _logger = new Mock<ILogger<HearingNotificationService>>();

            _hearingNotificationService = new HearingNotificationService(_bookingApiClient.Object, _featureToggles.Object, _notificationApiClient.Object, _logger.Object);
        }

        [Test]
        public async Task should_not_call_bookingapi_GetHearingsForNotificationAsync_when_BookAndConfirm_toggle_is_off()
        {
            _featureToggles.Setup(x => x.BookAndConfirmToggle()).Returns(false);
            
            await _hearingNotificationService.SendNotificationsAsync();

            _bookingApiClient.Verify(x => x.GetHearingsForNotificationAsync(), Times.Never);
        }


        [Test]
        public async Task should_not_call_notificationApi_when_no_bookings_are_retruned()
        {
            hearings = new List<HearingDetailsResponse>();

            _featureToggles.Setup(x => x.BookAndConfirmToggle()).Returns(true);
            _bookingApiClient.Setup(x => x.GetHearingsForNotificationAsync()).ReturnsAsync(hearings);

            await _hearingNotificationService.SendNotificationsAsync();

            _bookingApiClient.Verify(x => x.GetHearingsForNotificationAsync(), Times.Once);
            _notificationApiClient.Verify(x => x.CreateNewNotificationAsync(new NotificationApi.Contract.Requests.AddNotificationRequest()), Times.Never);
        }

    }
}
