using BookingsApi.Contract.Requests;
using SchedulerJobs.Common.Models;

namespace SchedulerJobs.Services.Mappers
{
    public static class JudiciaryPersonRequestMapper
    {
        public static JudiciaryPersonRequest MapTo(JudiciaryPersonModel source)
        {
            if (string.IsNullOrEmpty(source.PersonalCode))
            {
                return null;
            }
            
            return new JudiciaryPersonRequest
            {
                Id = source.Id,
                Email = source.Email,
                Fullname = source.Fullname,
                Surname = source.Surname,
                Title = source.Title,
                KnownAs = source.KnownAs,
                PersonalCode = source.PersonalCode,
                PostNominals = source.PostNominals,
                HasLeft = source.HasLeft,
                Leaver = source.Leaver,
                LeftOn = source.LeftOn
            };
        }
    }
}