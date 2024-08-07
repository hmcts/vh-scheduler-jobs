using BookingsApi.Contract.V1.Requests;
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
                WorkPhone = source.WorkPhone,
                Fullname = source.Fullname,
                Surname = source.Surname,
                Title = source.Title,
                KnownAs = source.KnownAs,
                PersonalCode = source.PersonalCode,
                PostNominals = source.PostNominals,
                HasLeft = source.HasLeft,
                Leaver = source.Leaver,
                LeftOn = source.LeftOn,
                Deleted = source.Deleted,
                DeletedOn = source.DeletedOn
            };
        }
    }
}