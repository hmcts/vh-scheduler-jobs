using BookingsApi.Client;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using NotificationApi.Client;
using SchedulerJobs.Common.Enums;
using BookingsApi.Contract.V1.Responses;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Services.Mappers;

namespace SchedulerJobs.Services
{
    public class HearingNotificationService : IHearingNotificationService
    {

        private readonly IBookingsApiClient _bookingsApiClient;
        private readonly INotificationApiClient _notificationApiClient;
        private readonly ILogger<HearingNotificationService> _logger;
        private readonly List<string> _userRoleList = new List<string>() { "Individual", "Representative", "Judicial Office Holder" };
        private readonly IFeatureToggles _featureToggles;
        
        public HearingNotificationService(IBookingsApiClient bookingsApiClient, INotificationApiClient notificationApiClient,  ILogger<HearingNotificationService> logger, IFeatureToggles featureToggles)
        {
            _bookingsApiClient = bookingsApiClient;
            _notificationApiClient = notificationApiClient;
            _logger = logger;
            _featureToggles = featureToggles;
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
                if (!item.Hearing.Participants.Exists(x => _userRoleList.Contains(x.UserRoleName)))
                {
                    _logger.LogInformation("SendNotificationsAsync - Ignored hearing: {ItemId} case:{CaseName} as no participant with role required for notification.", item.Hearing.Id, item.Hearing.Cases[0].Name);
                    continue;
                }

                foreach (var participant in item.Hearing.Participants)
                {
                    if (!_userRoleList.Contains(participant.UserRoleName))
                    {
                        continue;
                    }

                    await ProcessParticipantForNotification(item, participant);
                }
            }
        }

        private async Task ProcessParticipantForNotification(HearingNotificationResponse item, ParticipantResponse participant)
        {
            switch (participant.UserRoleName)
            {
                case UserRoleNames.Individual:
                case UserRoleNames.Representative:
                case UserRoleNames.JudicialOfficeHolder:
                    await SendHearingNotification(item, participant);
                    break;
            }
        }

        private async Task SendHearingNotification(HearingNotificationResponse item, ParticipantResponse participant)
        {
            var request = AddNotificationRequestMapper.MapToHearingReminderNotification(item, participant, _featureToggles.UsePostMay2023Template());
            await _notificationApiClient.CreateNewNotificationAsync(request);
        }

        private async Task<List<HearingNotificationResponse>> GetHearings()
        {
            var response = await _bookingsApiClient.GetHearingsForNotificationAsync();

            var hearings = response.ToList();
            return hearings;
        }
    }
}
