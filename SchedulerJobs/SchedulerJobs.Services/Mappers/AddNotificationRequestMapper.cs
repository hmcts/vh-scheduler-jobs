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
            var parameters = InitConfirmReminderParams(hearing);

            parameters.Add("name", participant.Username.ToLower());

            NotificationType notificationType;

            /*
            if (participant.UserRoleName.Contains(RoleNames.JudicialOfficeHolder,
                StringComparison.InvariantCultureIgnoreCase))
            {
                notificationType = hearing.IsParticipantAEJudJudicialOfficeHolder(participant.Id) ? NotificationType.HearingReminderEJudJoh : NotificationType.HearingReminderJoh;
                parameters.Add(NotifyParams.JudicialOfficeHolder, $"{participant.FirstName} {participant.LastName}");
            }
            else if (participant.UserRoleName.Contains(RoleNames.Representative, StringComparison.InvariantCultureIgnoreCase))
            {
                notificationType = NotificationType.HearingReminderRepresentative;
                parameters.Add(NotifyParams.ClientName, participant.Representee);
                parameters.Add(NotifyParams.SolicitorName, $"{participant.FirstName} {participant.LastName}");
            }
            else
            {
                notificationType = NotificationType.HearingReminderLip;
                parameters.Add(NotifyParams.Name, $"{participant.FirstName} {participant.LastName}");
            }*/

            notificationType = NotificationType.HearingReminderLip;

            return new AddNotificationRequest
            {
                HearingId = hearing.Id,
                MessageType = MessageType.Email,
                ContactEmail = participant.ContactEmail,
                NotificationType = notificationType,
                ParticipantId = participant.Id,
                PhoneNumber = participant.TelephoneNumber,
                Parameters = parameters
            };
        }

        private static Dictionary<string, string> InitConfirmReminderParams(HearingDetailsResponse hearing)
        {
            var @case = hearing.Cases.First();

            return new Dictionary<string, string>
            {
                {"case name", @case.Name},
                {"case number", @case.Number},
                {"time", hearing.ScheduledDateTime.ToEmailTimeGbLocale()},
                {"day month year", hearing.ScheduledDateTime.ToEmailDateGbLocale()}
            };
        }
    }
}