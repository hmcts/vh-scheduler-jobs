using BookingsApi.Client;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using NotificationApi.Client;
using BookingsApi.Contract.V2.Responses;
using NotificationApi.Contract.Requests;

namespace SchedulerJobs.Services
{
    public class HearingNotificationService(
        IBookingsApiClient bookingsApiClient,
        INotificationApiClient notificationApiClient,
        ILogger<HearingNotificationService> logger)
        : IHearingNotificationService
    {
        public async Task SendNotificationsAsync()
        {
            logger.LogInformation("SendNotificationsAsync - Started");

            var hearings = await GetHearings();

            if (hearings.Count < 1)
            {
                logger.LogInformation("SendNotificationsAsync - No hearings to send notifications");
                return;
            }

            await ProcessHearings(hearings);
        }

        private async Task ProcessHearings(List<HearingNotificationResponseV2> hearings)
        {
            foreach (var item in hearings)
            {
                await SendNotificationsForParticipantsInHearing(item);
            }
        }

        private async Task SendNotificationsForParticipantsInHearing(HearingNotificationResponseV2 item)
        {
            foreach (var participant in item.Hearing.Participants)
            {
                if (IsSubsequentDayOfMultiDayHearing(item))
                {
                    await ProcessSubsequentDayOfMultiDayHearing(item, participant);
                    continue;
                }
         
                switch (item.TotalDays)
                {
                    case > 1:
                        await ProcessMultiDayHearing(item, participant);
                        break;
                    case 1:
                        await ProcessSingleDayHearing(item, participant);
                        break;
                    default:
                        LogUnsupportedParticipantForNotification(item, participant);
                        continue;
                }
            }
        }

        private async Task ProcessMultiDayHearing(HearingNotificationResponseV2 item, ParticipantResponseV2 participant)
        {
            try
            {
                await notificationApiClient.SendMultiDayHearingReminderEmailAsync(new MultiDayHearingReminderRequest()
                {
                    TotalDays = item.TotalDays,
                    ScheduledDateTime = item.Hearing.ScheduledDateTime,
                    ContactEmail = participant.ContactEmail,
                    Name = $"{participant.FirstName} {participant.LastName}",
                    Username = participant.Username,
                    ParticipantId = participant.Id,
                    HearingId = item.Hearing.Id,
                    CaseName = item.Hearing.Cases[0].Name,
                    CaseNumber = item.Hearing.Cases[0].Number,
                    RoleName = participant.UserRoleName
                });
            }
            catch (NotificationApiException ex)
            {
                logger.LogError(ex,
                    "Error sending multi day hearing reminder email for hearing {HearingId} and case number {CaseNumber} to participant {ParticipantId}",
                    item.Hearing.Id, item.Hearing.Cases[0].Number, participant.Id);
            }
        }

        private async Task ProcessSingleDayHearing(HearingNotificationResponseV2 item, ParticipantResponseV2 participant)
        {
            try
            {
                await notificationApiClient.SendSingleDayHearingReminderEmailAsync(
                    new SingleDayHearingReminderRequest()
                    {
                        ScheduledDateTime = item.Hearing.ScheduledDateTime,
                        ContactEmail = participant.ContactEmail,
                        Name = $"{participant.FirstName} {participant.LastName}",
                        Username = participant.Username,
                        ParticipantId = participant.Id,
                        HearingId = item.Hearing.Id,
                        CaseName = item.Hearing.Cases[0].Name,
                        CaseNumber = item.Hearing.Cases[0].Number,
                        RoleName = participant.UserRoleName,
                        Representee = participant.Representee
                    });
            }
            catch (NotificationApiException ex)
            {
                logger.LogError(ex,
                    "Error sending single day hearing reminder email for hearing {HearingId} and case number {CaseNumber} to participant {ParticipantId}",
                    item.Hearing.Id, item.Hearing.Cases[0].Number, participant.Id);
            }
        }

        private async Task ProcessSubsequentDayOfMultiDayHearing(HearingNotificationResponseV2 item, ParticipantResponseV2 participant)
        {
            if (!item.SourceHearing.Participants.Exists(x => x.ContactEmail == participant.ContactEmail))
            {
                await ProcessSingleDayHearing(item, participant);
            }
        }

        private async Task<List<HearingNotificationResponseV2>> GetHearings()
        {
            var response = await bookingsApiClient.GetHearingsForNotificationAsync();

            var hearings = response.ToList();
            return hearings;
        }

        private static bool IsSubsequentDayOfMultiDayHearing(HearingNotificationResponseV2 item) => 
            item.TotalDays > 1 && item.Hearing.Id != item.SourceHearing.Id;

        private void LogUnsupportedParticipantForNotification(HearingNotificationResponseV2 item, ParticipantResponseV2 participant)
        {
            logger.LogInformation(
                "SendNotificationsAsync - Ignored Participant: {ParticipantId} has role {RoleName} which is not supported for notification in the hearing {HearingId}",
                participant.Id, participant.UserRoleName, item.Hearing.Id);
        }
    }
}
