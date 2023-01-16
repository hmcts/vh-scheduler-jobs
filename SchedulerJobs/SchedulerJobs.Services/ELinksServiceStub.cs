using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingsApi.Client;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Common.Models;
using SchedulerJobs.Services.Extensions;
using SchedulerJobs.Services.Mappers;

namespace SchedulerJobs.Services
{
    public class ELinksServiceStub : IELinksService
    {
        private readonly IBookingsApiClient _bookingsApiClient;
        private readonly ILogger<ELinksServiceStub> _logger;

        public ELinksServiceStub(IBookingsApiClient bookingsApiClient, ILogger<ELinksServiceStub> logger)
        {
            _bookingsApiClient = bookingsApiClient;
            _logger = logger;
        }

        public async Task ImportJudiciaryPeopleAsync(DateTime fromDate)
        {
            _logger.LogInformation("ImportJudiciaryPeople: using stub");
            _logger.LogInformation("ImportJudiciaryPeople: Removing all records from JudiciaryPersonsStaging");
            await _bookingsApiClient.RemoveAllJudiciaryPersonsStagingAsync();
            var peopleResult = RetrieveManualAccounts().Concat(RetrieveAutomationAccounts()).ToList();
            peopleResult.AddRange(RetrieveLeaverAccounts());

            _logger.LogInformation("ImportJudiciaryPeople: Calling bookings API with {PeopleResultCount} people",
                peopleResult.Count);
            var mappedForJudiciaryPersonsStaging = peopleResult.Select(JudiciaryPersonStagingRequestMapper.MapTo);
            var mappedForJudiciaryPersons = peopleResult.Select(JudiciaryPersonRequestMapper.MapTo);
            
            await _bookingsApiClient.BulkJudiciaryPersonsStagingAsync(mappedForJudiciaryPersonsStaging);
            
            var response = await _bookingsApiClient.BulkJudiciaryPersonsAsync(mappedForJudiciaryPersons); 
            response?.ErroredRequests.ForEach(x =>
                _logger.LogError("ImportJudiciaryPeople: {ErrorResponseMessage}", x.Message));
        }

        public Task ImportLeaversJudiciaryPeopleAsync(DateTime fromDate)
        {
            _logger.LogInformation("No judiciary persons leaving: using stub");
            return Task.CompletedTask;
        }

        public Task<DateTime> GetUpdatedSince() => Task.FromResult(DateTime.UtcNow.AddDays(-1));

        private static List<JudiciaryPersonModel> RetrieveManualAccounts()
        {
            var accounts = new List<JudiciaryPersonModel>();
            var manualIds = GetManualIds();
            for (var i = 0; i < 10; i++)
            {
                var id = manualIds[i];
                var number = i + 1;
                accounts.Add(InitPersonModel("Manual", number, id));
            }

            return accounts;
        }
        
        private static List<JudiciaryPersonModel> RetrieveAutomationAccounts()
        {
            var accounts = new List<JudiciaryPersonModel>();
            var manualIds = GetAutomationIds();
            for (var i = 0; i < 25; i++)
            {
                var id = manualIds[i];
                var number = i + 1;
                accounts.Add(InitPersonModel("Auto", number, id));
            }

            return accounts;
        }
        
        private static List<JudiciaryPersonModel> RetrieveLeaverAccounts()
        {
            var accounts = new List<JudiciaryPersonModel>();
            var leaverIds = GetLeaverIds();
            for (var i = 0; i < 3; i++)
            {
                var id = leaverIds[i];
                accounts.Add(InitLeaverPersonModel(id));
            }

            return accounts;
        }

        private static JudiciaryPersonModel InitPersonModel(string prefix, int number, string id)
        {
            var ticks = DateTime.UtcNow.Ticks.ToString();
            return new JudiciaryPersonModel
            {
                Email = $"{prefix.ToLower()}_judge_{number}@judiciarystaging.onmicrosoft.com",
                Fullname = $"{prefix.Capitalise()} Judge",
                KnownAs = $"{prefix} {number}",
                Surname = $"Judge {number}",
                Id = id,
                HasLeft = false,
                PersonalCode = ticks.Substring(ticks.Length-5),
                PostNominals = null,
                Title = "Honour",
                Leaver = false,
                LeftOn = ""
            };
        }
        
        private static JudiciaryPersonModel InitLeaverPersonModel(string id)
        {
            return new JudiciaryPersonModel
            {
                Id = id,
                PersonalCode = null,
                Leaver = true,
                LeftOn = "2021-02-26"
            };
        }

        private static List<string> GetManualIds()
        {
            return new List<string>
            {
                Guid.Parse("76586e7a-2366-4df0-92c2-c892ae142716").ToString(),
                Guid.Parse("e02ebe68-ce60-43bc-82bd-69d768bd33b5").ToString(),
                Guid.Parse("ae0de20f-c307-42df-aa81-17b1070c9a3d").ToString(),
                Guid.Parse("36714b55-3e86-4e36-bdf7-7240c12204a4").ToString(),
                Guid.Parse("d453c07e-0f0c-48a4-ba3e-a498b85d5700").ToString(),
                
                Guid.Parse("afe3385c-cb99-4c8e-bc97-531283053111").ToString(),
                Guid.Parse("4d80157c-64e3-437e-8b19-2204ea9f0bca").ToString(),
                Guid.Parse("5395f5ef-47a3-409e-ab65-12e16d9ca470").ToString(),
                Guid.Parse("44a233a7-e094-48ec-a329-c7f1569fd8cf").ToString(), 
                Guid.Parse("4a135b6a-2e8a-4eea-a6ee-a58ae0d8b4c4").ToString()
            };
        }
        
        private static List<string> GetLeaverIds()
        {
            return new List<string>
            {
                Guid.Parse("4a135b6a-2e8a-4eea-a6ee-a58ae0d8b4c1").ToString(),
                Guid.Parse("4a135b6a-2e8a-4eea-a6ee-a58ae0d8b4c2").ToString(),
                Guid.Parse("4a135b6a-2e8a-4eea-a6ee-a58ae0d8b4c3").ToString()
            };
        }
        
        private static List<string> GetAutomationIds()
        {
            return new List<string>
            {
                Guid.Parse("a0459b77-5ba0-4565-9743-dd2a1060c61c").ToString(),
                Guid.Parse("bb851e9d-3cf5-4648-bcdd-e8815beab019").ToString(),
                Guid.Parse("d770523d-f857-4498-af60-6d4ff1a9b8d5").ToString(),
                Guid.Parse("88a04e9c-b558-400f-806f-66d341d49b74").ToString(),
                Guid.Parse("86c1ee3d-f503-46a3-979b-f3c64b1d62e8").ToString(),
                
                Guid.Parse("7642c043-e262-45ed-b3ef-1fb0fde40710").ToString(),
                Guid.Parse("e45f6d24-21b8-49b7-8eae-5de0178c2373").ToString(),
                Guid.Parse("c9d8dc01-7b63-45f0-b8b7-eea0e5400829").ToString(),
                Guid.Parse("ae9ebe6a-a341-4570-86ce-3b729c8eb329").ToString(), 
                Guid.Parse("b643e771-a6f3-485f-a7e5-86a032bf87a1").ToString(),
                
                Guid.Parse("2f76d600-6e1b-46aa-a0ab-a0aaa7f8779c").ToString(),
                Guid.Parse("29268a26-8f52-47b1-9683-b8b57d8d92ad").ToString(),
                Guid.Parse("0fe7fe58-2232-48ec-a7bc-5169eb91dc81").ToString(),
                Guid.Parse("49ea1726-9de8-4396-ad84-7951c3519078").ToString(),
                Guid.Parse("a27ccbba-65ff-427a-9d05-3353c4a1249e").ToString(),
                
                Guid.Parse("8d7f4f85-1e2d-490f-ba5d-2438f98587f8").ToString(),
                Guid.Parse("ffcb7f99-eb90-4002-a02b-b206e50e398d").ToString(),
                Guid.Parse("d16ae21c-8388-4e31-b621-12836a436b63").ToString(),
                Guid.Parse("877f5440-b55b-460c-98f3-c06a1efd7e8b").ToString(), 
                Guid.Parse("1478dca1-73c6-4b69-90f8-62de442bf746").ToString(),
                
                Guid.Parse("cbc3b0b5-2db9-44c5-a94a-003e688069f0").ToString(),
                Guid.Parse("59285b71-057e-466f-9112-110603856b29").ToString(),
                Guid.Parse("0a4208b9-2747-40dd-96e7-2de1baebed69").ToString(),
                Guid.Parse("5db19c6b-026b-40a5-89b5-e943840d6087").ToString(),
                Guid.Parse("104274d1-32bc-46e8-a6e7-cd0bb227074a").ToString()
                
            };
        }
    }
}