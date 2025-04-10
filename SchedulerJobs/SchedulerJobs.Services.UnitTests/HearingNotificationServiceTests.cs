using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingsApi.Client;
using BookingsApi.Contract.V2.Enums;
using BookingsApi.Contract.V2.Responses;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationApi.Client;
using NotificationApi.Contract.Requests;
using NUnit.Framework;

namespace SchedulerJobs.Services.UnitTests
{
    public class HearingNotificationServiceTests
    {
        private Mock<IBookingsApiClient> _bookingApiClient;
        private Mock<INotificationApiClient> _notificationApiClient;
        private HearingNotificationService _hearingNotificationService;
        private Mock<ILogger<HearingNotificationService>> _logger;
        private List<HearingNotificationResponseV2> _noHearings;
        private List<HearingNotificationResponseV2> _hearings;
        private List<HearingNotificationResponseV2> _hearingsEjud;
        private List<HearingNotificationResponseV2> _hearingsMultiple;

        [SetUp]
        public void Setup()
        {
            _bookingApiClient = new Mock<IBookingsApiClient>();
            _notificationApiClient = new Mock<INotificationApiClient>();
            _logger = new Mock<ILogger<HearingNotificationService>>();
            _logger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
            _logger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
            _logger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
            _hearingNotificationService = new HearingNotificationService(_bookingApiClient.Object, _notificationApiClient.Object, _logger.Object);
            _hearings = [CreateHearing()];
            _hearingsEjud = [CreateHearingWithEjud()];
            _hearingsMultiple = [CreateHearing(), CreateHearing()];
        }

        [Test]
        public async Task should_not_call_notificationApi_when_no_bookings_are_returned()
        {
            _noHearings = [];

            _bookingApiClient.Setup(x => x.GetHearingsForNotificationAsync()).ReturnsAsync(_noHearings);

            await _hearingNotificationService.SendNotificationsAsync();

            _bookingApiClient.Verify(x => x.GetHearingsForNotificationAsync(), Times.Once);
            _notificationApiClient.Verify(x => x.SendSingleDayHearingReminderEmailAsync(new SingleDayHearingReminderRequest()), Times.Never);
        }

        [Test]
        public async Task should__call_notificationApi_when_bookings_are_returned()
        {
            var expectedCount = _hearings.SelectMany(x => x.Hearing.Participants).Count();
            _bookingApiClient.Setup(x => x.GetHearingsForNotificationAsync()).ReturnsAsync(_hearings);
            _notificationApiClient
                .Setup(x => x.SendSingleDayHearingReminderEmailAsync(It.IsAny<SingleDayHearingReminderRequest>()))
                .Returns(Task.CompletedTask);

            await _hearingNotificationService.SendNotificationsAsync();

            _bookingApiClient.Verify(x => x.GetHearingsForNotificationAsync(), Times.Once);
            _notificationApiClient.Verify(x => x.SendSingleDayHearingReminderEmailAsync(It.IsAny<SingleDayHearingReminderRequest>()), Times.Exactly(expectedCount));
        }

        [Test]
        public async Task should__call_notificationApi_when_Ejud_bookings_are_returned()
        {
            var expectedCount = _hearings.SelectMany(x => x.Hearing.Participants).Count();
            _bookingApiClient.Setup(x => x.GetHearingsForNotificationAsync()).ReturnsAsync(_hearingsEjud);
            _notificationApiClient.Setup(x => x.SendSingleDayHearingReminderEmailAsync(It.IsAny<SingleDayHearingReminderRequest>()))
                .Returns(Task.CompletedTask);

            await _hearingNotificationService.SendNotificationsAsync();

            _bookingApiClient.Verify(x => x.GetHearingsForNotificationAsync(), Times.Once);
            _notificationApiClient.Verify(x => x.SendSingleDayHearingReminderEmailAsync(It.IsAny<SingleDayHearingReminderRequest>()), Times.Exactly(expectedCount));
        }

        [Test]
        public async Task should__call_notificationApi_when_multiple_bookings_are_returned()
        {
            var expectedCount = _hearingsMultiple.SelectMany(x => x.Hearing.Participants).Count();
            _bookingApiClient.Setup(x => x.GetHearingsForNotificationAsync()).ReturnsAsync(_hearingsMultiple);
            _notificationApiClient.Setup(x => x.SendSingleDayHearingReminderEmailAsync(It.IsAny<SingleDayHearingReminderRequest>()))
                .Returns(Task.CompletedTask);

            await _hearingNotificationService.SendNotificationsAsync();

            _bookingApiClient.Verify(x => x.GetHearingsForNotificationAsync(), Times.Once);
            _notificationApiClient.Verify(x => x.SendSingleDayHearingReminderEmailAsync(It.IsAny<SingleDayHearingReminderRequest>()), Times.Exactly(expectedCount));
        }

        [Test]
        public async Task should__log_error_and_continue_when_notificationApi_throws_exception()
        {
            var notificationApiException = new NotificationApiException("Error", 400, "failed somewhere", null, null);
            var expectedCount = _hearings.SelectMany(x => x.Hearing.Participants).Count();
            _bookingApiClient.Setup(x => x.GetHearingsForNotificationAsync()).ReturnsAsync(_hearings);
            _notificationApiClient.Setup(x => x.SendSingleDayHearingReminderEmailAsync(It.IsAny<SingleDayHearingReminderRequest>()))
                .ThrowsAsync(notificationApiException);

            await _hearingNotificationService.SendNotificationsAsync();

            _bookingApiClient.Verify(x => x.GetHearingsForNotificationAsync(), Times.Once);
            _notificationApiClient.Verify(x => x.SendSingleDayHearingReminderEmailAsync(It.IsAny<SingleDayHearingReminderRequest>()), Times.Exactly(expectedCount));

            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error sending single day hearing reminder email")),
                    notificationApiException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Exactly(expectedCount));
        }

        private static HearingNotificationResponseV2 CreateHearing()
        {
            Guid id = Guid.NewGuid();

            var hearing = new HearingDetailsResponseV2
            {
                Id = id,
                ScheduledDateTime = DateTime.UtcNow.AddDays(2),
                ScheduledDuration = 60,
                HearingVenueName = "Basingstoke County Court and Family Court",
                ServiceId = "123456789",
                Cases = new List<CaseResponseV2>
                {
                    new CaseResponseV2 { Number = "CASE1-Test1", IsLeadCase = false, Name = "CASE1-Test1"}
                },
                Participants = new List<ParticipantResponseV2>
                {
                    new ParticipantResponseV2
                    {
                        Id = Guid.NewGuid(),
                        DisplayName ="observer",
                        HearingRoleName ="Observer",
                        UserRoleName= "Individual",
                        Title = "Mr",
                        FirstName = "Manual",
                        MiddleNames = "",
                        LastName = "Individual 11",
                        ContactEmail = "manual.individual_11@hmcts.net",
                        TelephoneNumber =  " +44(0)71234567891",
                        Username = "manual.individual_11@hearings.reform.hmcts.net",
                        Organisation = "",
                        Representee = "",
                        LinkedParticipants = null
                    },
                    new ParticipantResponseV2
                    {
                        Id = Guid.NewGuid(),
                        DisplayName ="Representative",
                        HearingRoleName ="Panel Member",
                        UserRoleName= "Judicial Office Holder",
                        Title = "Mr",
                        FirstName = "Manual",
                        MiddleNames = "",
                        LastName = "Representative 15",
                        ContactEmail = "manual.representative_15@hmcts.net",
                        TelephoneNumber =  " +44(0)71234567891",
                        Username = "manual.representative_15@hearings.reform.hmcts.net",
                        Organisation = "MoJ",
                        Representee = "",
                        LinkedParticipants = null
                    },
                    new ParticipantResponseV2
                    {
                        Id = Guid.NewGuid(),
                        DisplayName ="Secreatary of state",
                        HearingRoleName ="Representative",
                        UserRoleName= "Representative",
                        Title = "Mr",
                        FirstName = "Manual",
                        MiddleNames = "",
                        LastName = "Individual 198",
                        ContactEmail = "manual.individual_198@hmcts.net",
                        TelephoneNumber =  " +44(0)71234567891",
                        Username = "manual.individual_198@hearings.reform.hmcts.net",
                        Organisation = "my org",
                        Representee = "individual",
                        LinkedParticipants = null
                    }
                },
                HearingRoomName = "",
                OtherInformation = "",
                CreatedDate = DateTime.Today,
                CreatedBy = "",
                UpdatedBy = "",
                UpdatedDate = DateTime.Today,
                ConfirmedBy = "",
                ConfirmedDate = DateTime.Today,
                Status = BookingStatusV2.Created,
                AudioRecordingRequired = true,
                CancelReason = "",
                Endpoints = null,
                GroupId = id
            };

            var hearingResponse = new HearingNotificationResponseV2();
            hearingResponse.Hearing = hearing;
            hearingResponse.TotalDays = 1;

            return hearingResponse;
        }

        private static HearingNotificationResponseV2 CreateHearingWithEjud()
        {
            Guid id = Guid.NewGuid();

            var hearing = new HearingDetailsResponseV2
            {
                Id = id,
                ScheduledDateTime = DateTime.UtcNow.AddDays(2),
                ScheduledDuration = 60,
                HearingVenueName = "Basingstoke County Court and Family Court",
                ServiceId = "123456789",
                Cases = new List<CaseResponseV2>
                {
                    new CaseResponseV2 { Number = "CASE1-Test1", IsLeadCase = false, Name = "CASE1-Test1"}
                },
                Participants = new List<ParticipantResponseV2>
                {
                    new ParticipantResponseV2
                    {
                        Id = Guid.NewGuid(),
                        DisplayName ="observer",
                        HearingRoleName ="Observer",
                        UserRoleName= "Individual",
                        Title = "Mr",
                        FirstName = "Manual",
                        MiddleNames = "",
                        LastName = "Individual 11",
                        ContactEmail = "manual.individual_11@hmcts.net",
                        TelephoneNumber =  " +44(0)71234567891",
                        Username = "manual.individual_11@hearings.reform.hmcts.net",
                        Organisation = "",
                        Representee = "",
                        LinkedParticipants = null
                    },
                    new ParticipantResponseV2
                    {
                        Id = Guid.NewGuid(),
                        DisplayName ="Representative",
                        HearingRoleName ="Panel Member",
                        UserRoleName= "Judicial Office Holder",
                        Title = "Mr",
                        FirstName = "Manual",
                        MiddleNames = "",
                        LastName = "Representative 15",
                        ContactEmail = "manual.representative_15@judiciarystaging.onmicrosoft.com",
                        TelephoneNumber =  " +44(0)71234567891",
                        Username = "manual.representative_15@judiciarystaging.onmicrosoft.com",
                        Organisation = "MoJ",
                        Representee = "",
                        LinkedParticipants = null
                    },
                    new ParticipantResponseV2
                    {
                        Id = Guid.NewGuid(),
                        DisplayName ="Secreatary of state",
                        HearingRoleName ="Representative",
                        UserRoleName= "Representative",
                        Title = "Mr",
                        FirstName = "Manual",
                        MiddleNames = "",
                        LastName = "Individual 198",
                        ContactEmail = "manual.individual_198@hmcts.net",
                        TelephoneNumber =  " +44(0)71234567891",
                        Username = "manual.individual_198@hearings.reform.hmcts.net",
                        Organisation = "my org",
                        Representee = "individual",
                        LinkedParticipants = null
                    }
                },
                HearingRoomName = "",
                OtherInformation = "",
                CreatedDate = DateTime.Today,
                CreatedBy = "",
                UpdatedBy = "",
                UpdatedDate = DateTime.Today,
                ConfirmedBy = "",
                ConfirmedDate = DateTime.Today,
                Status = BookingStatusV2.Created,
                AudioRecordingRequired = true,
                CancelReason = "",
                Endpoints = null,
                GroupId = id
            };
            var hearingResponse = new HearingNotificationResponseV2();
            hearingResponse.Hearing = hearing;
            hearingResponse.TotalDays = 1;

            return hearingResponse;
        }
        private static HearingNotificationResponseV2 CreateHearingWithNoRoles()
        {
            Guid id = Guid.NewGuid();

            var hearing = new HearingDetailsResponseV2
            {
                Id = id,
                ScheduledDateTime = DateTime.UtcNow.AddDays(2),
                ScheduledDuration = 60,
                HearingVenueName = "Basingstoke County Court and Family Court",
                ServiceId = "123456789",
                Cases = new List<CaseResponseV2>
                {
                    new CaseResponseV2 { Number = "CASE1-Test1", IsLeadCase = false, Name = "CASE1-Test1"}
                },
                Participants = new List<ParticipantResponseV2>
                {
                    new ParticipantResponseV2
                    {
                        Id = Guid.NewGuid(),
                        DisplayName ="John Doe",
                        HearingRoleName ="Witness",
                        UserRoleName= "MadeUp",
                        Title = "Mr",
                        FirstName = "John",
                        MiddleNames = "",
                        LastName = "Doe",
                        ContactEmail = "john@doe.com",
                        TelephoneNumber =  " +44(0)71234567891",
                        Username = "johnnn.doe@hearings.reform.hmcts.net",
                        Organisation = "xyz",
                        Representee = "",
                        LinkedParticipants = null
                    }
                },
                HearingRoomName = "",
                OtherInformation = "",
                CreatedDate = DateTime.Today,
                CreatedBy = "",
                UpdatedBy = "",
                UpdatedDate = DateTime.Today,
                ConfirmedBy = "",
                ConfirmedDate = DateTime.Today,
                Status = BookingStatusV2.Created,
                AudioRecordingRequired = true,
                CancelReason = "",
                Endpoints = null,
                GroupId = id
            };

            var hearingResponse = new HearingNotificationResponseV2();
            hearingResponse.Hearing = hearing;
            hearingResponse.TotalDays = 1;

            return hearingResponse;
        }
    }
}
