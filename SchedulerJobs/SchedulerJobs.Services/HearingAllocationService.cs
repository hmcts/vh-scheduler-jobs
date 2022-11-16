using System.Linq;
using System.Threading.Tasks;
using BookingsApi.Client;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Common.Configuration;

namespace SchedulerJobs.Services
{
    public interface IHearingAllocationService
    {
        Task AllocateHearingsAsync();
    }
    
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
                _logger.LogInformation("AllocateHearings: Feature WorkAllocation is turned off!");
                return;
            }
            
            _logger.LogInformation("AllocateHearings: Starting to allocate hearings");
            
            var hearings = await _bookingsApiClient.GetUnallocatedHearingsAsync();

            var hearingsAllocated = 0;
            foreach (var hearingId in hearings.Select(h => h.Id).ToList())
            {
                try
                {
                    var allocatedUser = await _bookingsApiClient.AllocateHearingAutomaticallyAsync(hearingId);
                    _logger.LogInformation("AllocateHearings: Allocated user {allocatedUsername} to hearing {hearingId}", 
                        allocatedUser.Username, 
                        hearingId);
                    hearingsAllocated++;
                }
                catch (BookingsApiException e)
                {
                    _logger.LogError(e, "AllocateHearings: Error allocating hearing {hearingId}", hearingId);
                }
            }
            
            _logger.LogInformation("AllocateHearings: Completed allocation of hearings, {hearingsAllocated} of {hearingsToAllocate} hearings allocated",
                hearingsAllocated,
                hearings.Count);
        }
    }
}
