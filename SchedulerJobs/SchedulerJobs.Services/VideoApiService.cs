using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Common.ApiHelper;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Services.VideoApi.Contracts;

namespace SchedulerJobs.Services
{
    public interface IVideoApiService
    {
        Task<List<ExpiredConferencesResponse>> GetExpiredOpenConferences();

        Task CloseConference(Guid conferenceId);
    }
    public class VideoApiService : IVideoApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _log;
        private readonly ApiUriFactory _apiUriFactory;
        public VideoApiService(HttpClient httpClient,
            HearingServicesConfiguration hearingServicesConfiguration,
            ILoggerFactory factory
         )
        {
            _httpClient = httpClient;
            _log = factory.CreateLogger<VideoApiService>();
            _httpClient.BaseAddress = new Uri(hearingServicesConfiguration.VideoApiUrl);
            _apiUriFactory = new ApiUriFactory();
        }

        public async Task CloseConference(Guid conferenceId)
        {
            _log.LogTrace($"Close conference by Id {conferenceId}");
            var uriString = _apiUriFactory.ConferenceEndpoints.CloseConference(conferenceId);
            var response = await _httpClient.PutAsync(uriString, null).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<ExpiredConferencesResponse>> GetExpiredOpenConferences()
        {
            _log.LogTrace($"Getting expired open conferences");

            var uriString = _apiUriFactory.ConferenceEndpoints.GetExpiredOpenConferences();
            var response = await _httpClient.GetAsync(uriString).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return ApiRequestHelper.DeserialiseSnakeCaseJsonToResponse<List<ExpiredConferencesResponse>>(content);
        }
    }
}
