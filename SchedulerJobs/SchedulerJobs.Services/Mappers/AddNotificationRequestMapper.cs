using System;
using System.Collections.Generic;
using System.Linq;
using BookingsApi.Contract.Responses;
using NotificationApi.Contract;
using NotificationApi.Contract.Requests;
using SchedulerJobs.Common.Enums;
using SchedulerJobs.Common.Extensions;

namespace SchedulerJobs.Services.Mappers
{
    public static class AddNotificationRequestMapper
    {
        public static AddNotificationRequest MapToHearingReminderNotification(HearingDetailsResponse hearing,
            ParticipantResponse participant)
        {
            var parameters = InitCommonParameters(hearing);

            AddParticipantParameters(participant, parameters); 

            return new AddNotificationRequest
            {
                HearingId = hearing.Id,
                MessageType = MessageType.Email,
                ContactEmail = participant.ContactEmail,
                NotificationType = GetNotificationType(participant),
                ParticipantId = participant.Id,
                PhoneNumber = participant.TelephoneNumber,
                Parameters = parameters
            };
        }

        private static void AddParticipantParameters(ParticipantResponse participant, Dictionary<string, string> parameters)
        {
            parameters.Add("username", $"{participant.Username}");
            parameters.Add("email address", $"{participant.ContactEmail}");

            switch (participant.UserRoleName)
            {
                case UserRoleNames.Individual:
                    parameters.Add("name", $"{participant.FirstName} {participant.LastName}");
                    break;
                case UserRoleNames.Representative:
                    parameters.Add("client name", participant.Representee);
                    parameters.Add("solicitor name", $"{participant.FirstName} {participant.LastName}");
                    break;
                default:       //JudicialOfficeHolder
                    parameters.Add("judicial office holder", $"{participant.FirstName} {participant.LastName}");
                    break;
            }

        }

        private static NotificationType GetNotificationType(ParticipantResponse participant)
        {
            NotificationType notificationType;

            switch (participant.UserRoleName)
            {
                case UserRoleNames.Individual:
                    notificationType = NotificationType.NewHearingReminderLIP;
                    break;
                case UserRoleNames.Representative:
                    notificationType = NotificationType.NewHearingReminderRepresentative;
                    break;
                default:       //JudicialOfficeHolder
                    notificationType = NotificationType.NewHearingReminderJOH;
                    break;
            }

            return notificationType;
        }

        private static Dictionary<string, string> InitCommonParameters(HearingDetailsResponse hearing)
        {
            var @case = hearing.Cases.First();

            return new Dictionary<string, string>
            {
                {"case name", @case.Name},
                {"case number", @case.Number},
                {"time", hearing.ScheduledDateTime.ToEmailTimeGbLocale()},
                {"Day Month Year", hearing.ScheduledDateTime.ToEmailDateGbLocale()}
            };
        }
    }
}