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
        private List<HearingDetailsResponse> _nohearings;
        private List<HearingDetailsResponse> _hearings_1;
        private List<HearingDetailsResponse> _hearings_with_no_notification_userroles;
        private List<HearingDetailsResponse> _hearings_multiple;

        [SetUp]
        public void Setup()
        {
            _bookingApiClient = new Mock<IBookingsApiClient>();
            _notificationApiClient = new Mock<INotificationApiClient>();
            _featureToggles = new Mock<IFeatureToggles>();
            _logger = new Mock<ILogger<HearingNotificationService>>();
            _hearingNotificationService = new HearingNotificationService(_bookingApiClient.Object, _featureToggles.Object, _notificationApiClient.Object, _logger.Object);
            _hearings_1 = new List<HearingDetailsResponse>() { createHearing() };
            _hearings_multiple = new List<HearingDetailsResponse>() { createHearing(), createHearing() };
            _hearings_with_no_notification_userroles = new List<HearingDetailsResponse>() { createHearingWithNoRoles() };
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
            _nohearings = new List<HearingDetailsResponse>();

            _featureToggles.Setup(x => x.BookAndConfirmToggle()).Returns(true);
            _bookingApiClient.Setup(x => x.GetHearingsForNotificationAsync()).ReturnsAsync(_nohearings);

            await _hearingNotificationService.SendNotificationsAsync();

            _bookingApiClient.Verify(x => x.GetHearingsForNotificationAsync(), Times.Once);
            _notificationApiClient.Verify(x => x.CreateNewNotificationAsync(new NotificationApi.Contract.Requests.AddNotificationRequest()), Times.Never);
        }

        [Test]
        public async Task should__call_notificationApi_when_bookings_are_retruned()
        {
            _featureToggles.Setup(x => x.BookAndConfirmToggle()).Returns(true);
            _bookingApiClient.Setup(x => x.GetHearingsForNotificationAsync()).ReturnsAsync(_hearings_1);
            _notificationApiClient.SetupSequence(x => x.CreateNewNotificationAsync(It.IsAny<NotificationApi.Contract.Requests.AddNotificationRequest>()))
                .Returns(Task.CompletedTask)
                .Returns(Task.CompletedTask)
                .Returns(Task.CompletedTask) ;

            await _hearingNotificationService.SendNotificationsAsync();

            _bookingApiClient.Verify(x => x.GetHearingsForNotificationAsync(), Times.Once);
            _notificationApiClient.Verify(x => x.CreateNewNotificationAsync(It.IsAny<NotificationApi.Contract.Requests.AddNotificationRequest>()), Times.Exactly(3));
        }

        [Test]
        public async Task should__call_notificationApi_when_multiple_bookings_are_retruned()
        {
            _featureToggles.Setup(x => x.BookAndConfirmToggle()).Returns(true);
            _bookingApiClient.Setup(x => x.GetHearingsForNotificationAsync()).ReturnsAsync(_hearings_multiple);
            _notificationApiClient.SetupSequence(x => x.CreateNewNotificationAsync(It.IsAny<NotificationApi.Contract.Requests.AddNotificationRequest>()))
                .Returns(Task.CompletedTask)
                .Returns(Task.CompletedTask)
                .Returns(Task.CompletedTask)
                .Returns(Task.CompletedTask)
                .Returns(Task.CompletedTask)
                .Returns(Task.CompletedTask);

            await _hearingNotificationService.SendNotificationsAsync();

            _bookingApiClient.Verify(x => x.GetHearingsForNotificationAsync(), Times.Once);
            _notificationApiClient.Verify(x => x.CreateNewNotificationAsync(It.IsAny<NotificationApi.Contract.Requests.AddNotificationRequest>()), Times.Exactly(6));
        }

        [Test]
        public async Task should_not_call_notificationApi_when_bookings_with_notification_user_roles_doesnot_exists()
        {
            _nohearings = new List<HearingDetailsResponse>();

            _featureToggles.Setup(x => x.BookAndConfirmToggle()).Returns(true);
            _bookingApiClient.Setup(x => x.GetHearingsForNotificationAsync()).ReturnsAsync(_hearings_with_no_notification_userroles);

            await _hearingNotificationService.SendNotificationsAsync();

            _bookingApiClient.Verify(x => x.GetHearingsForNotificationAsync(), Times.Once);
            _notificationApiClient.Verify(x => x.CreateNewNotificationAsync(new NotificationApi.Contract.Requests.AddNotificationRequest()), Times.Never);
        }


        private HearingDetailsResponse createHearing()
        {
            Guid id = Guid.NewGuid();

            return new HearingDetailsResponse()
            {
                Id = id,
                ScheduledDateTime = DateTime.UtcNow.AddDays(2),
                ScheduledDuration = 60,
                HearingVenueName = "Basingstoke County Court and Family Court",
                CaseTypeName = "Asylum Support",
                HearingTypeName = "Case Management Review Hearing",
                Cases = new List<CaseResponse>() {
                    new CaseResponse(){ Number = "CASE1-Test1", IsLeadCase = false, Name = "CASE1-Test1"}
                },
                Participants = new List<ParticipantResponse>()
                {
                    new ParticipantResponse()
                    {
                        Id = Guid.NewGuid(),
                        DisplayName ="observer",
                        CaseRoleName = "Observer",
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
                    new ParticipantResponse()
                    {
                        Id = Guid.NewGuid(),
                        DisplayName ="Representative",
                        CaseRoleName = "Panel Member",
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
                    new ParticipantResponse()
                    {
                        Id = Guid.NewGuid(),
                        DisplayName ="Manual Judge_01",
                        CaseRoleName = "Judge",
                        HearingRoleName ="Judge",
                        UserRoleName= "Judge",
                        Title = "Mr",
                        FirstName = "Manual",
                        MiddleNames = "",
                        LastName = "Judge_01",
                        ContactEmail = "manual.judge_01@hearings.reform.hmcts.net",
                        TelephoneNumber =  " +44(0)71234567891",
                        Username = "manual.judge_01@hearings.reform.hmcts.net",
                        Organisation = "xyz",
                        Representee = "",
                        LinkedParticipants = null
                    },
                    new ParticipantResponse()
                    {
                        Id = Guid.NewGuid(),
                        DisplayName ="Secreatary of state",
                        CaseRoleName = "Secretary of State",
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
                TelephoneParticipants = null,
                HearingRoomName = "",
                OtherInformation = "",
                CreatedDate = DateTime.Today,
                CreatedBy = "",
                UpdatedBy = "",
                UpdatedDate = DateTime.Today,
                ConfirmedBy = "",
                ConfirmedDate = DateTime.Today,
                Status = BookingsApi.Contract.Enums.BookingStatus.Created,
                QuestionnaireNotRequired = false,
                AudioRecordingRequired = true,
                CancelReason = "",
                Endpoints = null,
                GroupId = id
            };
        }

        private HearingDetailsResponse createHearingWithNoRoles()
        {
            Guid id = Guid.NewGuid();

            return new HearingDetailsResponse()
            {
                Id = id,
                ScheduledDateTime = DateTime.UtcNow.AddDays(2),
                ScheduledDuration = 60,
                HearingVenueName = "Basingstoke County Court and Family Court",
                CaseTypeName = "Asylum Support",
                HearingTypeName = "Case Management Review Hearing",
                Cases = new List<CaseResponse>() {
                    new CaseResponse(){ Number = "CASE1-Test1", IsLeadCase = false, Name = "CASE1-Test1"}
                },
                Participants = new List<ParticipantResponse>()
                {
                    new ParticipantResponse()
                    {
                        Id = Guid.NewGuid(),
                        DisplayName ="Manual Judge_01",
                        CaseRoleName = "Judge",
                        HearingRoleName ="Judge",
                        UserRoleName= "Judge",
                        Title = "Mr",
                        FirstName = "Manual",
                        MiddleNames = "",
                        LastName = "Judge_01",
                        ContactEmail = "manual.judge_01@hearings.reform.hmcts.net",
                        TelephoneNumber =  " +44(0)71234567891",
                        Username = "manual.judge_01@hearings.reform.hmcts.net",
                        Organisation = "xyz",
                        Representee = "",
                        LinkedParticipants = null
                    }
                },
                TelephoneParticipants = null,
                HearingRoomName = "",
                OtherInformation = "",
                CreatedDate = DateTime.Today,
                CreatedBy = "",
                UpdatedBy = "",
                UpdatedDate = DateTime.Today,
                ConfirmedBy = "",
                ConfirmedDate = DateTime.Today,
                Status = BookingsApi.Contract.Enums.BookingStatus.Created,
                QuestionnaireNotRequired = false,
                AudioRecordingRequired = true,
                CancelReason = "",
                Endpoints = null,
                GroupId = id
            };
        }
    }
}
