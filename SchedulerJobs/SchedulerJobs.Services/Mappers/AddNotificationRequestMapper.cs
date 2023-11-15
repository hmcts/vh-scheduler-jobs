using System.Collections.Generic;
using System.Linq;
using BookingsApi.Contract.V1.Responses;
using NotificationApi.Contract;
using NotificationApi.Contract.Requests;
using SchedulerJobs.Common.Enums;
using SchedulerJobs.Common.Extensions;

namespace SchedulerJobs.Services.Mappers
{
    public static class AddNotificationRequestMapper
    {
        private static readonly string Judiciary = "judiciary";
        public static AddNotificationRequest MapToHearingReminderNotification(HearingNotificationResponse hearing,
            ParticipantResponse participant, bool featureTogglePost2023 = false)
        {
            var parameters = InitCommonParameters(hearing.Hearing, featureTogglePost2023);

            var isMultiDay = hearing.TotalDays > 1;
            if (isMultiDay)
            {
                parameters.Add("number of days", hearing.TotalDays.ToString());
            }

            AddParticipantParameters(participant, parameters);

            return new AddNotificationRequest
            {
                HearingId = hearing.Hearing.Id,
                MessageType = MessageType.Email,
                ContactEmail = participant.ContactEmail,
                NotificationType = GetNotificationType(participant, featureTogglePost2023, isMultiDay),
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

        private static NotificationType GetNotificationType(ParticipantResponse participant, bool featureTogglePost2023 = false, bool isMultiDay = false)
        {
            NotificationType notificationType;

            switch (participant.UserRoleName)
            {
                case UserRoleNames.Individual:
                    notificationType = GetReminderNotificationTypeForIndividual(isMultiDay, featureTogglePost2023);
                    break;
                case UserRoleNames.Representative:
                    notificationType = NotificationType.NewHearingReminderRepresentative;
                    break;
                default:       //JudicialOfficeHolder
                    if (IsEjudge(participant))
                    {
                        notificationType = NotificationType.NewHearingReminderEJudJoh;
                    }
                    else
                    {
                        notificationType = NotificationType.NewHearingReminderJOH;
                    }
                    
                    break;
            }

            return notificationType;
        }
        
        public static bool IsEjudge(ParticipantResponse participant)
        {
            return (participant.ContactEmail.ToLower() == participant.Username.ToLower() && participant.Username.ToLower().Contains(Judiciary));
        }

        private static Dictionary<string, string> InitCommonParameters(HearingDetailsResponse hearing, bool featureTogglePost2023 = false)
        {
            var @case = hearing.Cases[0];
            
            var parameters = new Dictionary<string, string>
            {
                {"case name", @case.Name},
                {"case number", @case.Number},
                {"time", hearing.ScheduledDateTime.ToEmailTimeGbLocale()},
                {"day month year", hearing.ScheduledDateTime.ToEmailDateGbLocale()}
            };

            if (featureTogglePost2023)
            {
                parameters.Add("day month year_CY", hearing.ScheduledDateTime.ToEmailDateCyLocale());
                parameters.Add("start time", hearing.ScheduledDateTime.ToEmailTimeGbLocale());
            }

            return parameters;
        }
        private static NotificationType GetReminderNotificationTypeForIndividual(bool isMultiDay, bool featureTogglePost2023)
        {
            NotificationType notificationType;
            if (featureTogglePost2023)
            {
                notificationType = (isMultiDay) ? NotificationType.NewHearingReminderLipMultiDay : NotificationType.NewHearingReminderLipSingleDay;
            }
            else
            {
                notificationType = NotificationType.NewHearingReminderLIP;
            }

            return notificationType;
        }
    }
}
