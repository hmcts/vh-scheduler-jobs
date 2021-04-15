using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchedulerJobs.Common.Models;

namespace SchedulerJobs.Services.HttpClients
{
    public interface IELinksApiClient
    {
        Task<IEnumerable<JudiciaryPersonModel>> GetPeopleAsync(DateTime updatedSince, int page = 1, int perPage = 100);
        Task<IEnumerable<JudiciaryPersonModel>> GetLeaversAsync(DateTime updatedSince, int page = 1, int perPage = 100);
    }
}