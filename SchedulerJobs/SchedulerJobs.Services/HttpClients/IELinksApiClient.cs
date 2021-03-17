using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchedulerJobs.Common.Models;

namespace SchedulerJobs.Services.HttpClients
{
    public interface IELinksApiClient
    {
        Task<IEnumerable<JudiciaryPersonModel>> GetPeopleAsync(DateTime updatedSince, int perPage = 100, int page = 1);
    }
}