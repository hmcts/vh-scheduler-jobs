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
        private readonly IFeatureToggles _featureToggles;
        private readonly ILogger<HearingNotificationService> _logger;
        private readonly List<string> _userRoleList = new List<string>() { "Individual", "Representative", "Judicial Office Holder" };

        public HearingNotificationService(IBookingsApiClient bookingsApiClient, IFeatureToggles featureToggles, INotificationApiClient notificationApiClient,  ILogger<HearingNotificationService> logger)
        {
            _bookingsApiClient = bookingsApiClient;
            _notificationApiClient = notificationApiClient;
            _featureToggles = featureToggles;
            _logger = logger;
        }

        public async Task SendNotificationsAsync()
        {
            _logger.LogInformation("SendNotificationsAsync - Started");

            if (!_featureToggles.BookAndConfirmToggle())
            {
                _logger.LogInformation("SendNotificationsAsync - Feature BookAndConfirm is turned off!");
                return;
            }

            var hearings = await getHearings();

            if (hearings.Count < 0)
            {
                _logger.LogInformation("SendNotificationsAsync - No hearings to send notifications");
                return;
            }

            await processHearings(hearings);

        }

        private async Task processHearings(List<HearingDetailsResponse> hearings)
        {
            foreach (var item in hearings)
            {
                if (!item.Participants.Any(x => _userRoleList.Contains(x.UserRoleName)))
                {
                    _logger.LogInformation($"SendNotificationsAsync - Ignored hearing: {item.Id} case:{item.Cases[0].Name} as no participant with role required for notification.");
                    continue;
                }

                foreach (var participant in item.Participants)
                {
                    if (!_userRoleList.Contains(participant.UserRoleName))
                    {
                        continue;
                    }

                    await processParticipantForNotification(item, participant);
                }
            }
        }

        private async Task processParticipantForNotification(HearingDetailsResponse item, ParticipantResponse participant)
        {
            switch (participant.UserRoleName)
            {
                case UserRoleNames.Individual:
                case UserRoleNames.Representative:
                case UserRoleNames.JudicialOfficeHolder:
                    await sendHearingNotification(item, participant);
                    break;
                default:
                    break;
            }
        }

        private async Task sendHearingNotification(HearingDetailsResponse item, ParticipantResponse participant)
        {
            var request = AddNotificationRequestMapper.MapToHearingReminderNotification(item, participant);
            await _notificationApiClient.CreateNewNotificationAsync(request);
        }

        private async Task<List<HearingDetailsResponse>> getHearings()
        {
            var response = await _bookingsApiClient.GetHearingsForNotificationAsync();

            var hearings = response.ToList();
            return hearings;
        }
    }
}
