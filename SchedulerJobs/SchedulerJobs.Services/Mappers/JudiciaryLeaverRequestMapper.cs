using BookingsApi.Contract.Requests;
using SchedulerJobs.Common.Models;

namespace SchedulerJobs.Services.Mappers
{
    public static class JudiciaryLeaverRequestMapper
    {
       public static JudiciaryLeaverRequest MapTo(JudiciaryLeaverModel source)
        {
            if (!source.Id.HasValue)
            {
                return null;
            }

            return new JudiciaryLeaverRequest
            {
                Id = source.Id.Value,
                Leaver = source.Leaver
            };
        }
    }
}