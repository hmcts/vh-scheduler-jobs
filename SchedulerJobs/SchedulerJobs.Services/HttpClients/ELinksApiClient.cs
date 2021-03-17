using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SchedulerJobs.Common.Exceptions;
using SchedulerJobs.Common.Models;

namespace SchedulerJobs.Services.HttpClients
{
    public class ELinksApiClient : IELinksApiClient
    {
        private readonly HttpClient _httpClient;

        public ELinksApiClient(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<IEnumerable<JudiciaryPersonModel>> GetPeopleAsync(DateTime updatedSince, int perPage = 100, int page = 1)
        {
            var response = await _httpClient.GetAsync
            (
                $"api/people?updated_since={updatedSince:yyyy-MM-dd}&page={page}&per_page={perPage}"
            );

            await HandleUnsuccessfulResponse(response);

            return JsonConvert.DeserializeObject<List<JudiciaryPersonModel>>(await response.Content.ReadAsStringAsync());       
        }
        
        private static async Task HandleUnsuccessfulResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();

                throw new ELinksApiException(errorMessage, response.StatusCode);
            }
        }
    }
}