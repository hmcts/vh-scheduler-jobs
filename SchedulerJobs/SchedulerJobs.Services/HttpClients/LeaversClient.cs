using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SchedulerJobs.Common.ApiHelper;
using SchedulerJobs.Common.Models;

namespace SchedulerJobs.Services.HttpClients
{
    public class LeaversClient : ILeaversClient
    {
        private readonly HttpClient _httpClient;

        public LeaversClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string BaseUrl { get; set; }

        public async Task<IEnumerable<JudiciaryPersonModel>> GetLeaversAsync(DateTime updatedSince, int page = 1, int perPage = 100)
        {
            var response = await _httpClient.GetAsync
            (
                $"{BaseUrl}/leavers?left_since={updatedSince:yyyy-MM-dd}&page={page}&per_page={perPage}"
            );

            await ResponseHandler.HandleUnsuccessfulResponse(response);

            return ApiRequestHelper.Deserialise<List<JudiciaryPersonModel>>(await response.Content.ReadAsStringAsync());
        }
    }
}