using System;
using System.Threading.Tasks;

namespace SchedulerJobs.Services.HttpClients
{
    public interface IPeoplesClient
    {
        Task<String> GetPeopleJsonAsync(DateTime updatedSince, int page = 1, int perPage = 100);
    }
}