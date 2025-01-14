using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using Bogus;
using BookingsApi.Client;
using BookingsApi.Contract.V2.Responses;
using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationApi.Client;
using NotificationApi.Contract.Requests;
using NUnit.Framework;

namespace SchedulerJobs.Services.UnitTests;

public class HearingNotificationServiceProcessMultiDaysTests
{
    private AutoMock _mocker;
    private HearingNotificationService _sut;
    private static readonly Faker Faker = new();
        
    [SetUp]
    public void Setup()
    {
        _mocker = AutoMock.GetLoose();
        _sut = _mocker.Create<HearingNotificationService>();
    }

    [Test]
    public async Task should_send_multi_day_reminder_request_for_first_day_of_multi_day_hearing_for_supported_roles()
    {
        // arrange
        var participants = Builder<ParticipantResponseV2>.CreateListOfSize(4)
            .All().With(x => x.Id = Guid.NewGuid())
            .TheFirst(1).With(x => x.UserRoleName = RoleNames.Judge)
            .TheNext(1).With(x => x.UserRoleName = RoleNames.Individual)
            .TheNext(1).With(x => x.UserRoleName = RoleNames.Representative)
            .TheNext(1).With(x => x.UserRoleName = RoleNames.JudicialOfficeHolder)
            .Build().ToList();

        var judge = participants.Find(x => x.UserRoleName == RoleNames.Judge);
        var individual = participants.Find(x => x.UserRoleName == RoleNames.Individual);
        var representative = participants.Find(x => x.UserRoleName == RoleNames.Representative);
        var judicialOfficeHolder = participants.Find(x => x.UserRoleName == RoleNames.JudicialOfficeHolder);
        
        var hearing = Builder<HearingDetailsResponseV2>.CreateNew()
            .With(x => x.Id = Guid.NewGuid())
            .With(x => x.Cases = Builder<CaseResponseV2>.CreateListOfSize(1).Build().ToList())
            .With(x => x.Participants = participants).Build();

        var hearingsForNotification = new List<HearingNotificationResponseV2>
        {
            new()
            {
                TotalDays = 2,
                Hearing = hearing,
                SourceHearing = hearing
            }
        };
        _mocker.Mock<IBookingsApiClient>().Setup(x => x.GetHearingsForNotificationAsync())
            .ReturnsAsync(hearingsForNotification);
            
        // act
        await _sut.SendNotificationsAsync();
            
        // assert
        _mocker.Mock<INotificationApiClient>()
            .Verify(
                x => x.SendMultiDayHearingReminderEmailAsync(
                    It.Is<MultiDayHearingReminderRequest>(y => y.ParticipantId == individual.Id)), Times.Once);
        
        _mocker.Mock<INotificationApiClient>()
            .Verify(
                x => x.SendMultiDayHearingReminderEmailAsync(
                    It.Is<MultiDayHearingReminderRequest>(y => y.ParticipantId == representative.Id)), Times.Once);
        
        _mocker.Mock<INotificationApiClient>()
            .Verify(
                x => x.SendMultiDayHearingReminderEmailAsync(
                    It.Is<MultiDayHearingReminderRequest>(y => y.ParticipantId == judicialOfficeHolder.Id)), Times.Once);
        
        _mocker.Mock<INotificationApiClient>()
            .Verify(
                x => x.SendMultiDayHearingReminderEmailAsync(
                    It.Is<MultiDayHearingReminderRequest>(y => y.ParticipantId == judge.Id)), Times.Once);
    }

    [Test]
    public async Task should_log_error_for_when_multiday_reminders_request_to_notification_api_fails()
    {
        // arrange
        var participants = Builder<ParticipantResponseV2>.CreateListOfSize(4)
            .All().With(x => x.Id = Guid.NewGuid())
            .TheFirst(1).With(x => x.UserRoleName = RoleNames.Judge)
            .TheNext(1).With(x => x.UserRoleName = RoleNames.Individual)
            .TheNext(1).With(x => x.UserRoleName = RoleNames.Representative)
            .TheNext(1).With(x => x.UserRoleName = RoleNames.JudicialOfficeHolder)
            .Build().ToList();
        
        var hearing = Builder<HearingDetailsResponseV2>.CreateNew()
            .With(x => x.Id = Guid.NewGuid())
            .With(x => x.Cases = Builder<CaseResponseV2>.CreateListOfSize(1).Build().ToList())
            .With(x => x.Participants = participants).Build();

        var hearingsForNotification = new List<HearingNotificationResponseV2>
        {
            new()
            {
                TotalDays = 2,
                Hearing = hearing,
                SourceHearing = hearing
            }
        };
        _mocker.Mock<IBookingsApiClient>().Setup(x => x.GetHearingsForNotificationAsync())
            .ReturnsAsync(hearingsForNotification);
        
        
        var notificationApiException = new NotificationApiException("Error", 400, "Template not found for user", null, null);
        _mocker.Mock<INotificationApiClient>().Setup(x => x.SendMultiDayHearingReminderEmailAsync(It.IsAny<MultiDayHearingReminderRequest>()))
            .ThrowsAsync(notificationApiException);
        
        await _sut.SendNotificationsAsync();
        
        _mocker.Mock<ILogger<HearingNotificationService>>().Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error sending multi day hearing reminder email")),
                notificationApiException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Exactly(participants.Count));
        
    }
    
    [Test]
    public async Task should_send_single_day_reminder_request_to_new_participants_on_subsequent_day_of_multi_day_hearing_for_supported_roles()
    {
        // Subsequent day = day 2, day 3 etc of a multi day hearing. ie not the first day/source hearing
        
        // arrange
        
        // participants for the first day/source hearing
        var sourceHearingParticipants = Builder<ParticipantResponseV2>.CreateListOfSize(4)
            .All().With(x => x.Id = Guid.NewGuid())
            .All().With(x => x.ContactEmail = Faker.Internet.Email())
            .TheFirst(1).With(x => x.UserRoleName = RoleNames.Judge)
            .TheNext(1).With(x => x.UserRoleName = RoleNames.Individual)
            .TheNext(1).With(x => x.UserRoleName = RoleNames.Representative)
            .TheNext(1).With(x => x.UserRoleName = RoleNames.JudicialOfficeHolder)
            .Build().ToList();

        var judge = sourceHearingParticipants.Find(x => x.UserRoleName == RoleNames.Judge);
        var individual = sourceHearingParticipants.Find(x => x.UserRoleName == RoleNames.Individual);
        var representative = sourceHearingParticipants.Find(x => x.UserRoleName == RoleNames.Representative);
        var judicialOfficeHolder = sourceHearingParticipants.Find(x => x.UserRoleName == RoleNames.JudicialOfficeHolder);
        
        var sourceHearing = Builder<HearingDetailsResponseV2>.CreateNew()
            .With(x => x.Id = Guid.NewGuid())
            .With(x => x.Cases = Builder<CaseResponseV2>.CreateListOfSize(1).Build().ToList())
            .With(x => x.Participants = sourceHearingParticipants).Build();
        
        // participants for the second day hearing
        var existingParticipants = sourceHearingParticipants.ToList();
        foreach (var existingParticipant in existingParticipants)
        {
            // same users, but they will have different participant ids
            existingParticipant.Id = Guid.NewGuid();
        }
        
        var newParticipants = Builder<ParticipantResponseV2>.CreateListOfSize(4)
            .All().With(x => x.Id = Guid.NewGuid())
            .All().With(x => x.ContactEmail = Faker.Internet.Email())
            .Build().ToList();
        
        newParticipants[0].UserRoleName = RoleNames.Judge;
        newParticipants[1].UserRoleName = RoleNames.Individual;
        newParticipants[2].UserRoleName = RoleNames.Representative;
        newParticipants[3].UserRoleName = RoleNames.JudicialOfficeHolder;

        var participants = existingParticipants.Concat(newParticipants).ToList();
        
        var newJudge = newParticipants.Find(x => x.UserRoleName == RoleNames.Judge);
        var newIndividual = newParticipants.Find(x => x.UserRoleName == RoleNames.Individual);
        var newRepresentative = newParticipants.Find(x => x.UserRoleName == RoleNames.Representative);
        var newJudicialOfficeHolder = newParticipants.Find(x => x.UserRoleName == RoleNames.JudicialOfficeHolder);

        var hearing = Builder<HearingDetailsResponseV2>.CreateNew()
            .With(x => x.Id = Guid.NewGuid())
            .With(x => x.Cases = Builder<CaseResponseV2>.CreateListOfSize(1).Build().ToList())
            .With(x => x.Participants = participants).Build();

        hearing.ScheduledDateTime = sourceHearing.ScheduledDateTime.AddDays(1);
        
        var hearingsForNotification = new List<HearingNotificationResponseV2>
        {
            new()
            {
                TotalDays = 2,
                Hearing = hearing,
                SourceHearing = sourceHearing
            }
        };
        _mocker.Mock<IBookingsApiClient>().Setup(x => x.GetHearingsForNotificationAsync())
            .ReturnsAsync(hearingsForNotification);
            
        // act
        await _sut.SendNotificationsAsync();
            
        // assert
        _mocker.Mock<INotificationApiClient>()
            .Verify(
                x => x.SendSingleDayHearingReminderEmailAsync(
                    It.Is<SingleDayHearingReminderRequest>(y => y.ContactEmail == newIndividual.ContactEmail)), Times.Once);
        
        _mocker.Mock<INotificationApiClient>()
            .Verify(
                x => x.SendSingleDayHearingReminderEmailAsync(
                    It.Is<SingleDayHearingReminderRequest>(y => y.ContactEmail == newRepresentative.ContactEmail)), Times.Once);
        
        _mocker.Mock<INotificationApiClient>()
            .Verify(
                x => x.SendSingleDayHearingReminderEmailAsync(
                    It.Is<SingleDayHearingReminderRequest>(y => y.ContactEmail == newJudicialOfficeHolder.ContactEmail)), Times.Once);
        
        _mocker.Mock<INotificationApiClient>()
            .Verify(
                x => x.SendSingleDayHearingReminderEmailAsync(
                    It.Is<SingleDayHearingReminderRequest>(y => y.ContactEmail == newJudge.ContactEmail)), Times.Once);
        
        // Should not notify the existing participants
        
        _mocker.Mock<INotificationApiClient>()
            .Verify(
                x => x.SendSingleDayHearingReminderEmailAsync(
                    It.Is<SingleDayHearingReminderRequest>(y => y.ContactEmail == individual.ContactEmail)), Times.Never);
        
        _mocker.Mock<INotificationApiClient>()
            .Verify(
                x => x.SendSingleDayHearingReminderEmailAsync(
                    It.Is<SingleDayHearingReminderRequest>(y => y.ContactEmail == representative.ContactEmail)), Times.Never);
        
        _mocker.Mock<INotificationApiClient>()
            .Verify(
                x => x.SendSingleDayHearingReminderEmailAsync(
                    It.Is<SingleDayHearingReminderRequest>(y => y.ContactEmail == judicialOfficeHolder.ContactEmail)), Times.Never);
        
        _mocker.Mock<INotificationApiClient>()
            .Verify(
                x => x.SendSingleDayHearingReminderEmailAsync(
                    It.Is<SingleDayHearingReminderRequest>(y => y.ContactEmail == judge.ContactEmail)), Times.Never);
    }
    
    [Test]
    public async Task should_send_single_day_reminder_request_for_supported_roles()
    {
        // arrange
        var participants = Builder<ParticipantResponseV2>.CreateListOfSize(4)
            .All().With(x => x.Id = Guid.NewGuid())
            .TheFirst(1).With(x => x.UserRoleName = RoleNames.Judge)
            .TheNext(1).With(x => x.UserRoleName = RoleNames.Individual)
            .TheNext(1).With(x => x.UserRoleName = RoleNames.Representative)
            .TheNext(1).With(x => x.UserRoleName = RoleNames.JudicialOfficeHolder)
            .Build().ToList();

        var judge = participants.Find(x => x.UserRoleName == RoleNames.Judge);
        var individual = participants.Find(x => x.UserRoleName == RoleNames.Individual);
        var representative = participants.Find(x => x.UserRoleName == RoleNames.Representative);
        var judicialOfficeHolder = participants.Find(x => x.UserRoleName == RoleNames.JudicialOfficeHolder);
        
        var hearing = Builder<HearingDetailsResponseV2>.CreateNew()
            .With(x => x.Id = Guid.NewGuid())
            .With(x => x.Cases = Builder<CaseResponseV2>.CreateListOfSize(1).Build().ToList())
            .With(x => x.Participants = participants).Build();

        var hearingsForNotification = new List<HearingNotificationResponseV2>
        {
            new()
            {
                TotalDays = 1,
                Hearing = hearing
            }
        };
        _mocker.Mock<IBookingsApiClient>().Setup(x => x.GetHearingsForNotificationAsync())
            .ReturnsAsync(hearingsForNotification);
            
        // act
        await _sut.SendNotificationsAsync();
            
        // assert
        _mocker.Mock<INotificationApiClient>()
            .Verify(
                x => x.SendSingleDayHearingReminderEmailAsync(
                    It.Is<SingleDayHearingReminderRequest>(y => y.ParticipantId == individual.Id)), Times.Once);
        
        _mocker.Mock<INotificationApiClient>()
            .Verify(
                x => x.SendSingleDayHearingReminderEmailAsync(
                    It.Is<SingleDayHearingReminderRequest>(y => y.ParticipantId == representative.Id)), Times.Once);
        
        _mocker.Mock<INotificationApiClient>()
            .Verify(
                x => x.SendSingleDayHearingReminderEmailAsync(
                    It.Is<SingleDayHearingReminderRequest>(y => y.ParticipantId == judicialOfficeHolder.Id)), Times.Once);
        
        _mocker.Mock<INotificationApiClient>()
            .Verify(
                x => x.SendSingleDayHearingReminderEmailAsync(
                    It.Is<SingleDayHearingReminderRequest>(y => y.ParticipantId == judge.Id)), Times.Once);
    }
}