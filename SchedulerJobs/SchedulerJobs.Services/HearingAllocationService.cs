using System.Linq;
using System.Threading.Tasks;
using BookingsApi.Client;
using Microsoft.Extensions.Logging;

namespace SchedulerJobs.Services
{
    public interface IHearingAllocationService
    {
        Task AllocateHearingsAsync();
    }
    
    public class HearingAllocationService : IHearingAllocationService
    {
        private readonly IBookingsApiClient _bookingsApiClient;
        private readonly ILogger<HearingAllocationService> _logger;
        
        public HearingAllocationService(IBookingsApiClient bookingsApiClient,
            ILogger<HearingAllocationService> logger)
        {
            _bookingsApiClient = bookingsApiClient;
            _logger = logger;
        }
        
        public async Task AllocateHearingsAsync()
        {
            _logger.LogInformation("AllocateHearings: Starting to allocate hearings");
            
            var hearings = await _bookingsApiClient.GetUnallocatedHearingsV2Async();

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
                    if (e.StatusCode == (int) System.Net.HttpStatusCode.BadRequest)
                    {
                        _logger.LogWarning(e, "AllocateHearings: Error allocating hearing {hearingId}", hearingId);
                        continue;
                    }
                    _logger.LogError(e, "AllocateHearings: Error allocating hearing {hearingId}", hearingId);
                }
            }
            
            _logger.LogInformation("AllocateHearings: Completed allocation of hearings, {hearingsAllocated} of {hearingsToAllocate} hearings allocated",
                hearingsAllocated,
                hearings.Count);
        }
    }
}
