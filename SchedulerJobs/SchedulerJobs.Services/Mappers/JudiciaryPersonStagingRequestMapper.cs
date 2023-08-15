﻿using BookingsApi.Contract.V1.Requests;
using SchedulerJobs.Common.Models;

namespace SchedulerJobs.Services.Mappers
{
    public class JudiciaryPersonStagingRequestMapper
    {
        public static JudiciaryPersonStagingRequest MapTo(JudiciaryPersonModel source)
        {
            return new JudiciaryPersonStagingRequest
            {
                Id = source.Id,
                Email = source.Email,
                Fullname = source.Fullname,
                Surname = source.Surname,
                Title = source.Title,
                KnownAs = source.KnownAs,
                PersonalCode = source.PersonalCode,
                PostNominals = source.PostNominals,
                Leaver = source.Leaver.ToString(),
                LeftOn = source.LeftOn
            };
        }
    }
}