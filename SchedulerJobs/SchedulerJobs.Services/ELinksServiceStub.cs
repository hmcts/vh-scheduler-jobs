using System;
using System.Collections.Generic;
using System.Globalization;
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
            var peopleResult = RetrieveManualAccounts().Concat(RetrieveAutomationAccounts()).ToList();

            _logger.LogInformation("ImportJudiciaryPeople: Calling bookings API with {PeopleResultCount} people",
                peopleResult.Count);
            var mapped = peopleResult.Select(JudiciaryPersonRequestMapper.MapTo);
            var response = await _bookingsApiClient.BulkJudiciaryPersonsAsync(mapped); 
            response?.ErroredRequests.ForEach(x =>
                _logger.LogError("ImportJudiciaryPeople: {ErrorResponseMessage}", x.Message));
        }

        public Task ImportLeaversJudiciaryPeopleAsync(DateTime fromDate)
        {
            _logger.LogInformation("No judiciary persons leaving: using stub");
            return Task.CompletedTask;
        }

        private List<JudiciaryPersonModel> RetrieveManualAccounts()
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
        
        private List<JudiciaryPersonModel> RetrieveAutomationAccounts()
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

        private JudiciaryPersonModel InitPersonModel(string prefix, int number, Guid id)
        {
            return new JudiciaryPersonModel
            {
                Email = $"{prefix.ToLower()}_judge_{number}@judiciarystaging.onmicrosoft.com",
                Fullname = $"{prefix.Capitalise()} Judge",
                KnownAs = $"{prefix} {number}",
                Surname = $"Judge {number}",
                Id = id,
                HasLeft = false,
                PersonalCode = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture).Reverse().Take(5).ToString(),
                PostNominals = null,
                Title = "Honour"
            };
        }

        private List<Guid> GetManualIds()
        {
            return new List<Guid>
            {
                Guid.Parse("76586e7a-2366-4df0-92c2-c892ae142716"),
                Guid.Parse("e02ebe68-ce60-43bc-82bd-69d768bd33b5"),
                Guid.Parse("ae0de20f-c307-42df-aa81-17b1070c9a3d"),
                Guid.Parse("36714b55-3e86-4e36-bdf7-7240c12204a4"),
                Guid.Parse("d453c07e-0f0c-48a4-ba3e-a498b85d5700"),
                
                Guid.Parse("afe3385c-cb99-4c8e-bc97-531283053111"),
                Guid.Parse("4d80157c-64e3-437e-8b19-2204ea9f0bca"),
                Guid.Parse("5395f5ef-47a3-409e-ab65-12e16d9ca470"),
                Guid.Parse("44a233a7-e094-48ec-a329-c7f1569fd8cf"), 
                Guid.Parse("4a135b6a-2e8a-4eea-a6ee-a58ae0d8b4c4")
            };
        }
        
        private List<Guid> GetAutomationIds()
        {
            return new List<Guid>
            {
                Guid.Parse("a0459b77-5ba0-4565-9743-dd2a1060c61c"),
                Guid.Parse("bb851e9d-3cf5-4648-bcdd-e8815beab019"),
                Guid.Parse("d770523d-f857-4498-af60-6d4ff1a9b8d5"),
                Guid.Parse("88a04e9c-b558-400f-806f-66d341d49b74"),
                Guid.Parse("86c1ee3d-f503-46a3-979b-f3c64b1d62e8"),
                
                Guid.Parse("7642c043-e262-45ed-b3ef-1fb0fde40710"),
                Guid.Parse("e45f6d24-21b8-49b7-8eae-5de0178c2373"),
                Guid.Parse("c9d8dc01-7b63-45f0-b8b7-eea0e5400829"),
                Guid.Parse("ae9ebe6a-a341-4570-86ce-3b729c8eb329"), 
                Guid.Parse("b643e771-a6f3-485f-a7e5-86a032bf87a1"),
                
                Guid.Parse("2f76d600-6e1b-46aa-a0ab-a0aaa7f8779c"),
                Guid.Parse("29268a26-8f52-47b1-9683-b8b57d8d92ad"),
                Guid.Parse("0fe7fe58-2232-48ec-a7bc-5169eb91dc81"),
                Guid.Parse("49ea1726-9de8-4396-ad84-7951c3519078"),
                Guid.Parse("a27ccbba-65ff-427a-9d05-3353c4a1249e"),
                
                Guid.Parse("8d7f4f85-1e2d-490f-ba5d-2438f98587f8"),
                Guid.Parse("ffcb7f99-eb90-4002-a02b-b206e50e398d"),
                Guid.Parse("d16ae21c-8388-4e31-b621-12836a436b63"),
                Guid.Parse("877f5440-b55b-460c-98f3-c06a1efd7e8b"), 
                Guid.Parse("1478dca1-73c6-4b69-90f8-62de442bf746"),
                
                Guid.Parse("cbc3b0b5-2db9-44c5-a94a-003e688069f0"),
                Guid.Parse("59285b71-057e-466f-9112-110603856b29"),
                Guid.Parse("0a4208b9-2747-40dd-96e7-2de1baebed69"),
                Guid.Parse("5db19c6b-026b-40a5-89b5-e943840d6087"),
                Guid.Parse("104274d1-32bc-46e8-a6e7-cd0bb227074a")
                
            };
        }
    }
}