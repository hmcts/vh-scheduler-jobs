using System.Threading.Tasks;
using BookingsApi.Client;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Services.Interfaces;

namespace SchedulerJobs.Services
{
    public class HearingAllocationService : IHearingAllocationService
    {
        private readonly IBookingsApiClient _bookingsApiClient;
        private readonly IFeatureToggles _featureToggles;
        private readonly ILogger<HearingAllocationService> _logger;
        
        public HearingAllocationService(IBookingsApiClient bookingsApiClient, 
            IFeatureToggles featureToggles, 
            ILogger<HearingAllocationService> logger)
        {
            _bookingsApiClient = bookingsApiClient;
            _featureToggles = featureToggles;
            _logger = logger;
        }
        
        public async Task AllocateHearingsAsync()
        {
            if (!_featureToggles.WorkAllocationToggle())
            {
                _logger.LogInformation("AllocateHearingsAsync - Feature WorkAllocation is turned off!");
                return;
            }
            
            var hearings = await _bookingsApiClient.GetUnallocatedHearingsAsync();

            foreach (var hearing in hearings)
            {
                var allocatedUser = await _bookingsApiClient.AllocateHearingAutomaticallyAsync(hearing.Id);
            }
        }
    }
}
