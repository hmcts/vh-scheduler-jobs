using BookingsApi.Contract.Requests;
using SchedulerJobs.Common.Models;

namespace SchedulerJobs.Services.Mappers
{
    public static class JudiciaryLeaverRequestMapper
    {
       public static JudiciaryLeaverRequest MapTo(JudiciaryLeaverModel source)
        {
            return new JudiciaryLeaverRequest
            {
                Id = source.Id,
                Leaver = source.Leaver,
                LeftOn = source.LeftOn,
                PersonalCode = source.PersonalCode
            };
        }
    }
}