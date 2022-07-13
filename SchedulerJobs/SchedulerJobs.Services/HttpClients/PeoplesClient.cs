using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

        public async Task<PeopleResponse> GetPeopleAsync(DateTime updatedSince, int page = 1, int perPage = 100)
        {
            var response = await _httpClient.GetAsync
            (
                $"{BaseUrl}/people?updated_since={updatedSince:yyyy-MM-dd}&page={page}&per_page={perPage}&include_previous_appointments=true"
            );

            await ResponseHandler.HandleUnsuccessfulResponse(response);

            var clientResponse = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<PeopleResponse>(clientResponse);
        }
        
        public async Task<String> GetPeopleJsonAsync(DateTime updatedSince, int page = 1, int perPage = 100)
        {
            var response = await _httpClient.GetAsync
            (
                $"{BaseUrl}/people?updated_since={updatedSince:yyyy-MM-dd}&page={page}&per_page={perPage}&include_previous_appointments=true"
            );
            
            await ResponseHandler.HandleUnsuccessfulResponse(response);
            
            return await response.Content.ReadAsStringAsync();
        }
    }
}