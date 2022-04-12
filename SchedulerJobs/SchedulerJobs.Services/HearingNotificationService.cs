using BookingsApi.Client;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Services.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using SchedulerJobs.Common.Configuration;

namespace SchedulerJobs.Services
{
    public class HearingNotificationService : IHearingNotificationService
    {

        private readonly IBookingsApiClient _bookingsApiClient;
        private readonly IFeatureToggles _featureToggles;
        private readonly ILogger<HearingNotificationService> _logger;
        private readonly List<string> _userRoleList = new List<string>() { "Individual", "Representative", "Judicial Office Holder" };

        public HearingNotificationService(IBookingsApiClient bookingsApiClient, IFeatureToggles featureToggles,  ILogger<HearingNotificationService> logger)
        {
            _bookingsApiClient = bookingsApiClient;
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

            var response = await _bookingsApiClient.GetHearingsForNotificationAsync();

            var hearings = response.ToList();

            if (hearings.Count < 0)
            {
                _logger.LogInformation("SendNotificationsAsync - No hearings to send notifications");
                return;
            }

            foreach (var item in hearings)
            {
                if (item.Participants.Count(x => _userRoleList.Contains(x.UserRoleName)) < 0)
                {
                    _logger.LogInformation("SendNotificationsAsync - Ignored hearing ");
                    continue;
                }




            }



        }
    }
}
