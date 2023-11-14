using BookingsApi.Client;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using NotificationApi.Client;
using BookingsApi.Contract.V1.Responses;
using NotificationApi.Contract.Requests;

namespace SchedulerJobs.Services
{
    public class HearingNotificationService : IHearingNotificationService
    {
        private readonly IBookingsApiClient _bookingsApiClient;
        private readonly INotificationApiClient _notificationApiClient;
        private readonly ILogger<HearingNotificationService> _logger;
        private readonly List<string> _rolesSupportingSingleDayReminders = new() { RoleNames.Individual, RoleNames.Representative, RoleNames.JudicialOfficeHolder };
        private readonly List<string> _rolesSupportingMultiDayReminders = new() { RoleNames.Individual, RoleNames.Representative };
        
        public HearingNotificationService(IBookingsApiClient bookingsApiClient, INotificationApiClient notificationApiClient,  ILogger<HearingNotificationService> logger)
        {
            _bookingsApiClient = bookingsApiClient;
            _notificationApiClient = notificationApiClient;
            _logger = logger;
        }

        public async Task SendNotificationsAsync()
        {
            _logger.LogInformation("SendNotificationsAsync - Started");

            var hearings = await GetHearings();

            if (hearings.Count < 1)
            {
                _logger.LogInformation("SendNotificationsAsync - No hearings to send notifications");
                return;
            }

            await ProcessHearings(hearings);
        }

        private async Task ProcessHearings(List<HearingNotificationResponse> hearings)
        {
            foreach (var item in hearings)
            {
                if (!item.Hearing.Participants.Exists(x => _rolesSupportingSingleDayReminders.Contains(x.UserRoleName)))
                {
                    _logger.LogInformation("SendNotificationsAsync - Ignored hearing: {ItemId} case:{CaseName} as no participant with role required for notification", item.Hearing.Id, item.Hearing.Cases[0].Name);
                    continue;
                }

                await SendNotificationsForParticipantsInHearing(item);
            }
        }

        private async Task SendNotificationsForParticipantsInHearing(HearingNotificationResponse item)
        {
            foreach (var participant in item.Hearing.Participants)
            {
                switch (item.TotalDays)
                {
                    case > 1 when _rolesSupportingMultiDayReminders.Contains(participant.UserRoleName):
                        await ProcessMultiDayHearing(item, participant);
                        break;
                    case 1 when _rolesSupportingSingleDayReminders.Contains(participant.UserRoleName):
                        await ProcessSingleDayHearing(item, participant);
                        break;
                    default:
                        _logger.LogInformation(
                            "SendNotificationsAsync - Ignored Participant: {ParticipantId} has role {RoleName} which is not supported for notification in the hearing {HearingId}",
                            participant.Id, participant.UserRoleName, item.Hearing.Id);
                        continue;
                }
            }
        }

        private async Task ProcessMultiDayHearing(HearingNotificationResponse item, ParticipantResponse participant)
        {
            await _notificationApiClient.SendMultiDayHearingReminderEmailAsync(new MultiDayHearingReminderRequest()
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

        private async Task ProcessSingleDayHearing(HearingNotificationResponse item, ParticipantResponse participant)
        {
            await _notificationApiClient.SendSingleDayHearingReminderEmailAsync(
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
                    RoleName = participant.UserRoleName
                });
        }

        private async Task<List<HearingNotificationResponse>> GetHearings()
        {
            var response = await _bookingsApiClient.GetHearingsForNotificationAsync();

            var hearings = response.ToList();
            return hearings;
        }
    }
}
