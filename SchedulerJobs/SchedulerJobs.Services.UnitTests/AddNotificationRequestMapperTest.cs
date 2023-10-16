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
        
        var participantId = Guid.NewGuid();
        var firstName = "firstname";
        var lastName = "lastname";
        var userName = "username";
        var emailAddress = "contact@email.com";
        var hearingDetails = new HearingDetailsResponse();
        hearingDetails.Participants = new List<ParticipantResponse>();
        ParticipantResponse participant1 = new ParticipantResponse();
        participant1.Id = participantId;
        participant1.FirstName = firstName;
        participant1.LastName = lastName;
        participant1.ContactEmail = emailAddress;
        participant1.Username = userName;
        participant1.UserRoleName = UserRoleNames.Individual;
        hearingDetails.Participants.Add(participant1);
        
        var hearingId = Guid.NewGuid();    
        var caseName = "random case name";
        var caseNumber = "random case number";
        

        hearingDetails.Cases = new List<CaseResponse>();
        var caseResponse = new CaseResponse();
        caseResponse.Name = caseName;
        caseResponse.Number = caseNumber;
        hearingDetails.Cases.Add(caseResponse);

        hearingDetails.Id = hearingId;
        hearingDetails.ScheduledDateTime = DateTime.UtcNow;
        

        var hearing = new HearingNotificationResponse();
        hearing.Hearing = hearingDetails;
        hearing.TotalDays = 1;

        var parameters = new Dictionary<string, string>
        {
            {"name", $"{firstName} {lastName}"},
            {"case name", caseName},
            {"case number", caseNumber},
            {"day month year", hearingDetails.ScheduledDateTime.ToEmailDateGbLocale() },
            {"day month year_CY", hearingDetails.ScheduledDateTime.ToEmailDateCyLocale() },
            {"start time", hearingDetails.ScheduledDateTime.ToEmailTimeGbLocale() },
            {"time", hearingDetails.ScheduledDateTime.ToEmailTimeGbLocale() },
            {"username", userName},
            {"email address", emailAddress}
        };
        
        var result = AddNotificationRequestMapper.MapToHearingReminderNotification(hearing, participant1, true);

        result.Should().NotBeNull();
        result.HearingId.Should().Be(hearingId);
        result.ParticipantId.Should().Be(participantId);
        result.ContactEmail.Should().Be(participant1.ContactEmail);
        result.NotificationType.Should().Be(NotificationType.NewHearingReminderLipSingleDay);
        result.MessageType.Should().Be(MessageType.Email);
        result.Parameters.Should().BeEquivalentTo(parameters);
    }
    
    [Test]
    public void should_map_properties_for_notification_reminder_request_for_multiday_day_post_2023_toggle_true()
    {
        
        var participantId = Guid.NewGuid();
        var firstName = "firstname";
        var lastName = "lastname";
        var userName = "username";
        var emailAddress = "contact@email.com";
        var hearingDetails = new HearingDetailsResponse();
        hearingDetails.Participants = new List<ParticipantResponse>();
        ParticipantResponse participant1 = new ParticipantResponse();
        participant1.Id = participantId;
        participant1.FirstName = firstName;
        participant1.LastName = lastName;
        participant1.ContactEmail = emailAddress;
        participant1.Username = userName;
        participant1.UserRoleName = UserRoleNames.Individual;
        hearingDetails.Participants.Add(participant1);
        
        var hearingId = Guid.NewGuid();    
        var caseName = "random case name";
        var caseNumber = "random case number";
        

        hearingDetails.Cases = new List<CaseResponse>();
        var caseResponse = new CaseResponse();
        caseResponse.Name = caseName;
        caseResponse.Number = caseNumber;
        hearingDetails.Cases.Add(caseResponse);

        hearingDetails.Id = hearingId;
        hearingDetails.ScheduledDateTime = DateTime.UtcNow;
        

        var hearing = new HearingNotificationResponse();
        hearing.Hearing = hearingDetails;
        hearing.TotalDays = 3;

        var parameters = new Dictionary<string, string>
        {
            {"name", $"{firstName} {lastName}"},
            {"case name", caseName},
            {"case number", caseNumber},
            {"day month year", hearingDetails.ScheduledDateTime.ToEmailDateGbLocale() },
            {"day month year_CY", hearingDetails.ScheduledDateTime.ToEmailDateCyLocale() },
            {"start time", hearingDetails.ScheduledDateTime.ToEmailTimeGbLocale() },
            {"time", hearingDetails.ScheduledDateTime.ToEmailTimeGbLocale() },
            {"username", userName},
            {"number of days", "3"},
            {"email address", emailAddress}
        };
        
        var result = AddNotificationRequestMapper.MapToHearingReminderNotification(hearing, participant1, true);

        result.Should().NotBeNull();
        result.HearingId.Should().Be(hearingId);
        result.ParticipantId.Should().Be(participantId);
        result.ContactEmail.Should().Be(participant1.ContactEmail);
        result.NotificationType.Should().Be(NotificationType.NewHearingReminderLipMultiDay);
        result.MessageType.Should().Be(MessageType.Email);
        result.Parameters.Should().BeEquivalentTo(parameters);
    }
    
    [Test]
    public void should_map_properties_for_notification_reminder_request_for_single_day_post_2023_toggle_false()
    {
        
        var participantId = Guid.NewGuid();
        var firstName = "firstname";
        var lastName = "lastname";
        var userName = "username";
        var emailAddress = "contact@email.com";
        var hearingDetails = new HearingDetailsResponse();
        hearingDetails.Participants = new List<ParticipantResponse>();
        ParticipantResponse participant1 = new ParticipantResponse();
        participant1.Id = participantId;
        participant1.FirstName = firstName;
        participant1.LastName = lastName;
        participant1.ContactEmail = emailAddress;
        participant1.Username = userName;
        participant1.UserRoleName = UserRoleNames.Individual;
        hearingDetails.Participants.Add(participant1);
        
        var hearingId = Guid.NewGuid();    
        var caseName = "random case name";
        var caseNumber = "random case number";
        

        hearingDetails.Cases = new List<CaseResponse>();
        var caseResponse = new CaseResponse();
        caseResponse.Name = caseName;
        caseResponse.Number = caseNumber;
        hearingDetails.Cases.Add(caseResponse);

        hearingDetails.Id = hearingId;
        hearingDetails.ScheduledDateTime = DateTime.UtcNow;
        

        var hearing = new HearingNotificationResponse();
        hearing.Hearing = hearingDetails;
        hearing.TotalDays = 1;

        var parameters = new Dictionary<string, string>
        {
            {"name", $"{firstName} {lastName}"},
            {"case name", caseName},
            {"case number", caseNumber},
            {"day month year", hearingDetails.ScheduledDateTime.ToEmailDateGbLocale() },
            {"time", hearingDetails.ScheduledDateTime.ToEmailTimeGbLocale() },
            {"username", userName},
            {"email address", emailAddress}
        };
        
        var result = AddNotificationRequestMapper.MapToHearingReminderNotification(hearing, participant1, false);

        result.Should().NotBeNull();
        result.HearingId.Should().Be(hearingId);
        result.ParticipantId.Should().Be(participantId);
        result.ContactEmail.Should().Be(participant1.ContactEmail);
        result.NotificationType.Should().Be(NotificationType.NewHearingReminderLIP);
        result.MessageType.Should().Be(MessageType.Email);
        result.Parameters.Should().BeEquivalentTo(parameters);
    }
}