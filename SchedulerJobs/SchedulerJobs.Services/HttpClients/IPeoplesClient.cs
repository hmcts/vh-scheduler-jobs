using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchedulerJobs.Common.Models;

namespace SchedulerJobs.Services.HttpClients
{
    public interface IPeoplesClient
    {
        Task<String> GetPeopleJsonAsync(DateTime updatedSince, int page = 1, int perPage = 100);
    }
}