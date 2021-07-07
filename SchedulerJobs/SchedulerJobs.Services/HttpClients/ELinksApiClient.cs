using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchedulerJobs.Common.ApiHelper;
using SchedulerJobs.Common.Exceptions;
using SchedulerJobs.Common.Models;

namespace SchedulerJobs.Services.HttpClients
{
    public class ELinksApiClient : IELinksApiClient
    {
        private readonly HttpClient _httpClient;

        public ELinksApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string BaseUrl { get; set; }

        public async Task<IEnumerable<JudiciaryPersonModel>> GetPeopleAsync(DateTime updatedSince, int page = 1, int perPage = 100)
        {
            var response = await _httpClient.GetAsync
            (
                $"{BaseUrl}api/people?updated_since={updatedSince:yyyy-MM-dd}&page={page}&per_page={perPage}"
            );

            await HandleUnsuccessfulResponse(response);

            return ApiRequestHelper.Deserialise<List<JudiciaryPersonModel>>(await response.Content.ReadAsStringAsync());       
        }

        public async Task<IEnumerable<JudiciaryPersonModel>> GetLeaversAsync(DateTime updatedSince, int page = 1, int perPage = 100)
        {
            var response = await _httpClient.GetAsync
            (
                $"{BaseUrl}api/v2/leavers?left_since={updatedSince:yyyy-MM-dd}&page={page}&per_page={perPage}"
            );

            await HandleUnsuccessfulResponse(response);

            return ApiRequestHelper.Deserialise<List<JudiciaryPersonModel>>(await response.Content.ReadAsStringAsync());
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