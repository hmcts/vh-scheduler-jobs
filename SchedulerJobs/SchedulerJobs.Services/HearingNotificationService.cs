using BookingsApi.Client;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using SchedulerJobs.Common.Configuration;
using BookingsApi.Contract.Responses;
using NotificationApi.Client;
using SchedulerJobs.Common.Enums;
using System;
using SchedulerJobs.Services.Mappers;

namespace SchedulerJobs.Services
{
    public class HearingNotificationService : IHearingNotificationService
    {

        private readonly IBookingsApiClient _bookingsApiClient;
        private readonly INotificationApiClient _notificationApiClient;
        private readonly ILogger<HearingNotificationService> _logger;
        private readonly List<string> _userRoleList = new List<string>() { "Individual", "Representative", "Judicial Office Holder" };

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

        private async Task ProcessHearings(List<HearingDetailsResponse> hearings)
        {
            foreach (var item in hearings)
            {
                if (!item.Participants.Exists(x => _userRoleList.Contains(x.UserRoleName)))
                {
                    _logger.LogInformation("SendNotificationsAsync - Ignored hearing: {ItemId} case:{CaseName} as no participant with role required for notification.", item.Id, item.Cases[0].Name);
                    continue;
                }

                foreach (var participant in item.Participants)
                {
                    if (!_userRoleList.Contains(participant.UserRoleName))
                    {
                        continue;
                    }

                    await ProcessParticipantForNotification(item, participant);
                }
            }
        }

        private async Task ProcessParticipantForNotification(HearingDetailsResponse item, ParticipantResponse participant)
        {
            switch (participant.UserRoleName)
            {
                case UserRoleNames.Individual:
                case UserRoleNames.Representative:
                case UserRoleNames.JudicialOfficeHolder:
                    await SendHearingNotification(item, participant);
                    break;
                default:
                    break;
            }
        }

        private async Task SendHearingNotification(HearingDetailsResponse item, ParticipantResponse participant)
        {
            var request = AddNotificationRequestMapper.MapToHearingReminderNotification(item, participant);
            await _notificationApiClient.CreateNewNotificationAsync(request);
        }

        private async Task<List<HearingDetailsResponse>> GetHearings()
        {
            var response = await _bookingsApiClient.GetHearingsForNotificationAsync();

            var hearings = response.ToList();
            return hearings;
        }
    }
}
