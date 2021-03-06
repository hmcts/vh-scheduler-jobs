using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchedulerJobs.Common.ApiHelper;
using SchedulerJobs.Common.Models;

namespace SchedulerJobs.Services.HttpClients
{
    public class PeoplesClient : IPeoplesClient
    {
        private readonly HttpClient _httpClient;

        public PeoplesClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string BaseUrl { get; set; }

        public async Task<IEnumerable<JudiciaryPersonModel>> GetPeopleAsync(DateTime updatedSince, int page = 1, int perPage = 100)
        {
            var response = await _httpClient.GetAsync
            (
                $"{BaseUrl}/people?updated_since={updatedSince:yyyy-MM-dd}&page={page}&per_page={perPage}"
            );

            await ResponseHandler.HandleUnsuccessfulResponse(response);

            var model = ApiRequestHelper.Deserialise<PeopleResults>(await response.Content.ReadAsStringAsync());
            return model.Results;
        }
    }
}