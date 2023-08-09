using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingsApi.Client;
using BookingsApi.Contract.Responses;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationApi.Client;
using NUnit.Framework;
using SchedulerJobs.Services.Interfaces;
using SchedulerJobs.Services.Mappers;

namespace SchedulerJobs.Services.UnitTests
{
    public class HearingNotificationServiceTests
    {
        private Mock<IBookingsApiClient> _bookingApiClient;
        private Mock<INotificationApiClient> _notificationApiClient;
        private IHearingNotificationService _hearingNotificationService;
        private Mock<ILogger<HearingNotificationService>> _logger;
        private List<HearingDetailsResponse> _nohearings;
        private List<HearingDetailsResponse> _hearings;
        private List<HearingDetailsResponse> _hearingsEjud;
        private List<HearingDetailsResponse> _hearingsWithNoNotificationUserroles;
        private List<HearingDetailsResponse> _hearingsMultiple;

        [SetUp]
        public void Setup()
        {
            _bookingApiClient = new Mock<IBookingsApiClient>();
            _notificationApiClient = new Mock<INotificationApiClient>();
            _logger = new Mock<ILogger<HearingNotificationService>>();
            _hearingNotificationService = new HearingNotificationService(_bookingApiClient.Object, _notificationApiClient.Object, _logger.Object);
            _hearings = new List<HearingDetailsResponse>() { CreateHearing() };
            _hearingsEjud = new List<HearingDetailsResponse>() { CreateHearingWithEjud() };
            _hearingsMultiple = new List<HearingDetailsResponse>() { CreateHearing(), CreateHearing() };
            _hearingsWithNoNotificationUserroles = new List<HearingDetailsResponse>() { CreateHearingWithNoRoles() };
        }

        [Test]
        public async Task should_not_call_bookingapi_GetHearingsForNotificationAsync_when_BookAndConfirm_toggle_is_off()
        {
            await _hearingNotificationService.SendNotificationsAsync();

            _bookingApiClient.Verify(x => x.GetHearingsForNotificationAsync(), Times.Never);
        }

        [Test]
        public async Task should_not_call_notificationApi_when_no_bookings_are_retruned()
        {
            _nohearings = new List<HearingDetailsResponse>();
            
            _bookingApiClient.Setup(x => x.GetHearingsForNotificationAsync()).ReturnsAsync(_nohearings);

            await _hearingNotificationService.SendNotificationsAsync();

            _bookingApiClient.Verify(x => x.GetHearingsForNotificationAsync(), Times.Once);
            _notificationApiClient.Verify(x => x.CreateNewNotificationAsync(new NotificationApi.Contract.Requests.AddNotificationRequest()), Times.Never);
        }

        [Test]
        public async Task should__call_notificationApi_when_bookings_are_retruned()
        {
            _bookingApiClient.Setup(x => x.GetHearingsForNotificationAsync()).ReturnsAsync(_hearings);
            _notificationApiClient.SetupSequence(x => x.CreateNewNotificationAsync(It.IsAny<NotificationApi.Contract.Requests.AddNotificationRequest>()))
                .Returns(Task.CompletedTask)
                .Returns(Task.CompletedTask)
                .Returns(Task.CompletedTask) ;

            await _hearingNotificationService.SendNotificationsAsync();

            Assert.AreEqual(false, AddNotificationRequestMapper.IsEjudge(_hearings[0].Participants.First(x => x.UserRoleName == "Judicial Office Holder")));

            _bookingApiClient.Verify(x => x.GetHearingsForNotificationAsync(), Times.Once);
            _notificationApiClient.Verify(x => x.CreateNewNotificationAsync(It.IsAny<NotificationApi.Contract.Requests.AddNotificationRequest>()), Times.Exactly(3));
        }

        [Test]
        public async Task should__call_notificationApi_when_Ejud_bookings_are_retruned()
        {
            _bookingApiClient.Setup(x => x.GetHearingsForNotificationAsync()).ReturnsAsync(_hearingsEjud);
            _notificationApiClient.SetupSequence(x => x.CreateNewNotificationAsync(It.IsAny<NotificationApi.Contract.Requests.AddNotificationRequest>()))
                .Returns(Task.CompletedTask)
                .Returns(Task.CompletedTask)
                .Returns(Task.CompletedTask);

            await _hearingNotificationService.SendNotificationsAsync();

            Assert.AreEqual(true, AddNotificationRequestMapper.IsEjudge(_hearingsEjud[0].Participants.First(x => x.UserRoleName == "Judicial Office Holder")));

            _bookingApiClient.Verify(x => x.GetHearingsForNotificationAsync(), Times.Once);
            _notificationApiClient.Verify(x => x.CreateNewNotificationAsync(It.IsAny<NotificationApi.Contract.Requests.AddNotificationRequest>()), Times.Exactly(3));
        }

        [Test]
        public async Task should__call_notificationApi_when_multiple_bookings_are_retruned()
        {
            _bookingApiClient.Setup(x => x.GetHearingsForNotificationAsync()).ReturnsAsync(_hearingsMultiple);
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
            
            _bookingApiClient.Setup(x => x.GetHearingsForNotificationAsync()).ReturnsAsync(_hearingsWithNoNotificationUserroles);

            await _hearingNotificationService.SendNotificationsAsync();

            _bookingApiClient.Verify(x => x.GetHearingsForNotificationAsync(), Times.Once);
            _notificationApiClient.Verify(x => x.CreateNewNotificationAsync(new NotificationApi.Contract.Requests.AddNotificationRequest()), Times.Never);
        }


        private static HearingDetailsResponse CreateHearing()
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

        private static HearingDetailsResponse CreateHearingWithEjud()
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
                        ContactEmail = "manual.representative_15@judiciarystaging.onmicrosoft.com",
                        TelephoneNumber =  " +44(0)71234567891",
                        Username = "manual.representative_15@judiciarystaging.onmicrosoft.com",
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
        private static HearingDetailsResponse CreateHearingWithNoRoles()
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
