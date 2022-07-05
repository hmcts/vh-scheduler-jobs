using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using SchedulerJobs.Common.ApiHelper;
using SchedulerJobs.Common.Models;
using SchedulerJobs.Services.Configuration;

namespace SchedulerJobs.Services.HttpClients
{
    public class PeoplesClient : IPeoplesClient
    {
        private readonly HttpClient _httpClient;
        private readonly VhAzureStorageService _service;

        public PeoplesClient(HttpClient httpClient, VhAzureStorageService service)
        {
            _httpClient = httpClient;
            _service = service;
        }

        public string BaseUrl { get; set; }

        public async Task<PeopleResponse> GetPeopleAsync(DateTime updatedSince, int page = 1, int perPage = 100)
        {
            var response = await _httpClient.GetAsync
            (
                $"{BaseUrl}/people?updated_since={updatedSince:yyyy-MM-dd}&page={page}&per_page={perPage}&include_previous_appointments=true"
            );

            string fileName = $"people-{updatedSince:yyyy-MM-dd}-page{page}.json";
            await ResponseHandler.HandleUnsuccessfulResponse(response);

            var clientResponse = await response.Content.ReadAsStringAsync();
            
            byte[] fileToBytes = Encoding.ASCII.GetBytes(clientResponse);
            
            await _service.UploadFile(fileName, fileToBytes);
            
            return ApiRequestHelper.Deserialise<PeopleResponse>(clientResponse);
        }
        
    }
}