using System;
using System.Linq;
using System.Threading.Tasks;
using BookingsApi.Client;

namespace SchedulerJobs.Services
{
    public interface IJobHistoryService
    {
        public Task UpdateJobHistory(string callingJob, bool jobSucceeded);
        public Task<DateTime?> GetMostRecentSuccessfulRunDate(string callingJob);
    }
    public class JobHistoryService : IJobHistoryService 
    {
        private readonly IBookingsApiClient _bookingsApiClient;

        public JobHistoryService(IBookingsApiClient bookingsApiClient)
        {
            _bookingsApiClient = bookingsApiClient;
        }

        public async Task UpdateJobHistory(string callingJob, bool jobSucceeded)
        {
            await _bookingsApiClient.UpdateJobHistoryAsync(callingJob, jobSucceeded);
        }
        
        public async Task<DateTime?> GetMostRecentSuccessfulRunDate(string callingJob)
        {
            var jobDates= await _bookingsApiClient.GetJobHistoryAsync(callingJob);
            return jobDates.Where(e => e.IsSuccessful)
                           .OrderByDescending(e => e.LastRunDate)
                           .FirstOrDefault()?
                           .LastRunDate;
        }
    }
}