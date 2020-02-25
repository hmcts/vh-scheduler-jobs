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
        /// <summary>
        /// Get a list of conferences that are still open but expected to be closed.
        /// </summary>
        /// <returns></returns>
        Task<List<ExpiredConferencesResponse>> GetExpiredOpenConferences();

        Task CloseConference(Guid conferenceId);

        /// <summary>
        /// Gets a list of conferences that have been closed for more than 30 minutes
        /// </summary>
        /// <returns></returns>
        Task<List<ClosedConferencesResponse>> GetClosedConferencesToClear();

        Task ClearConferenceChatHistory(Guid conferenceId);
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

        public async Task<List<ClosedConferencesResponse>> GetClosedConferencesToClear()
        {
            _log.LogTrace($"Getting conferences that have been closed for more than 30 minutes");
            var uriString = _apiUriFactory.ConferenceEndpoints.GetClosedConferencesWithInstantMessageHistory();
            var response = await _httpClient.GetAsync(uriString);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return ApiRequestHelper.DeserialiseSnakeCaseJsonToResponse<List<ClosedConferencesResponse>>(content);
        }

        public async Task ClearConferenceChatHistory(Guid conferenceId)
        {
            _log.LogTrace($"Close conference by Id {conferenceId}");
            var uriString = _apiUriFactory.ConferenceEndpoints.ClearConferenceChatHistory(conferenceId);
            var response = await _httpClient.DeleteAsync(uriString).ConfigureAwait(false);
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
