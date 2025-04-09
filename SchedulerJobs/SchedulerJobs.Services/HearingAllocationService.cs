using System.Linq;
using System.Threading.Tasks;
using BookingsApi.Client;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Common.Logging;

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
            _logger.LogInformationAllocateHearingsStartToAllocate();
            
            var hearings = await _bookingsApiClient.GetUnallocatedHearingsV2Async();

            var hearingsAllocated = 0;
            foreach (var hearingId in hearings.Select(h => h.Id).ToList())
            {
                try
                {
                    var allocatedUser = await _bookingsApiClient.AllocateHearingAutomaticallyAsync(hearingId);
                    _logger.LogInformationAllocateHearingsUserAndHearing(allocatedUser.Username, hearingId);
                    hearingsAllocated++;
                }
                catch (BookingsApiException e)
                {
                    if (e.StatusCode == (int) System.Net.HttpStatusCode.BadRequest)
                    {
                        _logger.LogWarningAllocateHearings(e, hearingId);
                        continue;
                    }
                    _logger.LogErrorAllocateHearings(e, hearingId);
                }
            }
            
            _logger.LogInformationAllocateHearingCompleteAllocation(hearingsAllocated, hearings.Count);
        }
    }
}
