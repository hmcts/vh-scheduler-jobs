using System;
using System.Collections.Generic;
using BookingsApi.Contract.V1.Responses;
using FluentAssertions;
using NotificationApi.Contract;
using NUnit.Framework;
using SchedulerJobs.Common.Enums;
using SchedulerJobs.Common.Extensions;
using SchedulerJobs.Services.Mappers;

namespace SchedulerJobs.Services.UnitTests;

public class AddNotificationRequestMapperTest
{
    [Test]
    public void should_map_properties_for_notification_reminder_request_for_singleday_post_2023_toggle_true()
    {
        var participant = CreateParticipant();

        var caseResponse = CreateCaseResponse();

        var hearingDetails = CreateHearingDetails(participant, caseResponse);

        var hearing = new HearingNotificationResponse
        {
            Hearing = hearingDetails,
            TotalDays = 1
        };

        var parameters = new Dictionary<string, string>
        {
            {"name", $"{participant.FirstName} {participant.LastName}"},
            {"case name", caseResponse.Name},
            {"case number", caseResponse.Number},
            {"day month year", hearingDetails.ScheduledDateTime.ToEmailDateGbLocale() },
            {"day month year_CY", hearingDetails.ScheduledDateTime.ToEmailDateCyLocale() },
            {"start time", hearingDetails.ScheduledDateTime.ToEmailTimeGbLocale() },
            {"time", hearingDetails.ScheduledDateTime.ToEmailTimeGbLocale() },
            {"username", participant.Username},
            {"email address", participant.ContactEmail}
        };
        
        var result = AddNotificationRequestMapper.MapToHearingReminderNotification(hearing, participant, true);

        result.Should().NotBeNull();
        result.HearingId.Should().Be(hearingDetails.Id);
        result.ParticipantId.Should().Be(participant.Id);
        result.ContactEmail.Should().Be(participant.ContactEmail);
        result.NotificationType.Should().Be(NotificationType.NewHearingReminderLipSingleDay);
        result.MessageType.Should().Be(MessageType.Email);
        result.Parameters.Should().BeEquivalentTo(parameters);
    }

    [Test]
    public void should_map_properties_for_notification_reminder_request_for_multiday_day_post_2023_toggle_true()
    {
        var participant = CreateParticipant();

        var caseResponse = CreateCaseResponse();

        var hearingDetails = CreateHearingDetails(participant, caseResponse);

        var hearing = new HearingNotificationResponse
        {
            Hearing = hearingDetails,
            TotalDays = 3
        };
        
        var parameters = new Dictionary<string, string>
        {
            {"name", $"{participant.FirstName} {participant.LastName}"},
            {"case name", caseResponse.Name},
            {"case number", caseResponse.Number},
            {"day month year", hearingDetails.ScheduledDateTime.ToEmailDateGbLocale() },
            {"day month year_CY", hearingDetails.ScheduledDateTime.ToEmailDateCyLocale() },
            {"start time", hearingDetails.ScheduledDateTime.ToEmailTimeGbLocale() },
            {"time", hearingDetails.ScheduledDateTime.ToEmailTimeGbLocale() },
            {"username", participant.Username},
            {"number of days", "3"},
            {"email address", participant.ContactEmail}
        };
        
        var result = AddNotificationRequestMapper.MapToHearingReminderNotification(hearing, participant, true);

        result.Should().NotBeNull();
        result.HearingId.Should().Be(hearingDetails.Id);
        result.ParticipantId.Should().Be(participant.Id);
        result.ContactEmail.Should().Be(participant.ContactEmail);
        result.NotificationType.Should().Be(NotificationType.NewHearingReminderLipMultiDay);
        result.MessageType.Should().Be(MessageType.Email);
        result.Parameters.Should().BeEquivalentTo(parameters);
    }
    
    [Test]
    public void should_map_properties_for_notification_reminder_request_for_single_day_post_2023_toggle_false()
    {
        var participant = CreateParticipant();

        var caseResponse = CreateCaseResponse();

        var hearingDetails = CreateHearingDetails(participant, caseResponse);

        var hearing = new HearingNotificationResponse
        {
            Hearing = hearingDetails,
            TotalDays = 1
        };

        var parameters = new Dictionary<string, string>
        {
            {"name", $"{participant.FirstName} {participant.LastName}"},
            {"case name", caseResponse.Name},
            {"case number", caseResponse.Number},
            {"day month year", hearingDetails.ScheduledDateTime.ToEmailDateGbLocale() },
            {"time", hearingDetails.ScheduledDateTime.ToEmailTimeGbLocale() },
            {"username", participant.Username},
            {"email address", participant.ContactEmail}
        };
        
        var result = AddNotificationRequestMapper.MapToHearingReminderNotification(hearing, participant, false);

        result.Should().NotBeNull();
        result.HearingId.Should().Be(hearingDetails.Id);
        result.ParticipantId.Should().Be(participant.Id);
        result.ContactEmail.Should().Be(participant.ContactEmail);
        result.NotificationType.Should().Be(NotificationType.NewHearingReminderLIP);
        result.MessageType.Should().Be(MessageType.Email);
        result.Parameters.Should().BeEquivalentTo(parameters);
    }
    
    private ParticipantResponse CreateParticipant()
    {
        var participantId = Guid.NewGuid();
        var firstName = "firstname";
        var lastName = "lastname";
        var userName = "username";
        var emailAddress = "contact@email.com";
        
        return new ParticipantResponse
        {
            Id = participantId,
            FirstName = firstName,
            LastName = lastName,
            ContactEmail = emailAddress,
            Username = userName,
            UserRoleName = UserRoleNames.Individual
        };
    }
    private CaseResponse CreateCaseResponse()
    {
        var caseName = "random case name";
        var caseNumber = "random case number";
        
        return new CaseResponse
        {
            Name = caseName,
            Number = caseNumber
        };
    }
    private HearingDetailsResponse CreateHearingDetails(ParticipantResponse participant, CaseResponse caseResponse)
    {
        var hearingId = Guid.NewGuid(); 
        var hearingDetails = new HearingDetailsResponse();
        hearingDetails.Participants = new List<ParticipantResponse>();
        
        hearingDetails.Participants.Add(participant);
        
        hearingDetails.Cases = new List<CaseResponse>();
        
        hearingDetails.Cases.Add(caseResponse);

        hearingDetails.Id = hearingId;
        hearingDetails.ScheduledDateTime = DateTime.UtcNow;

        return hearingDetails;
    }
}
