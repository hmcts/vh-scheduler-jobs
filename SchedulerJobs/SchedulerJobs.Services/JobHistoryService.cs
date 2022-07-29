using System.Threading.Tasks;
using BookingsApi.Client;

namespace SchedulerJobs.Services
{
    public interface IJobHistoryService
    {
        public Task UpdateJobHistory(string callingJob, bool jobSucceeded);
    }
    public class JobHistoryService : IJobHistoryService 
    {
        private IBookingsApiClient _bookingsApiClient;

        public JobHistoryService(IBookingsApiClient bookingsApiClient)
        {
            _bookingsApiClient = bookingsApiClient;
        }

        public async Task UpdateJobHistory(string callingJob, bool jobSucceeded)
        {
            await _bookingsApiClient.UpdateJobHistoryAsync(callingJob, jobSucceeded);
        }
    }
}