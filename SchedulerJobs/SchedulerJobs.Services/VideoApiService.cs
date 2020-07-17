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
        Task<List<ClosedConferenceWithImHistoryResponse>> GetClosedConferencesToClearInstantMessageHistory();

        Task ClearConferenceChatHistory(Guid conferenceId);

        /// <summary>
        /// Get a list of conferences 14 hours ago with audiorecording and status greater then zero
        /// </summary>
        /// <returns>The list of ExpiredConferencesResponse</returns>
        Task<List<ExpiredConferencesResponse>> GetExpiredAudiorecordingConferences();

        /// <summary>
        /// Check if audio file is created and delete audio application from wowza 
        /// </summary>
        /// <param name="hearingId"></param>
        /// <returns></returns>
        Task DeleteAudiorecordingApplication(Guid hearingId);
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

        public async Task<List<ClosedConferenceWithImHistoryResponse>> GetClosedConferencesToClearInstantMessageHistory()
        {
            _log.LogTrace($"Getting conferences that have been closed for more than 30 minutes");
            var uriString = _apiUriFactory.ConferenceEndpoints.GetClosedConferencesWithInstantMessageHistory();
            var response = await _httpClient.GetAsync(uriString);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return ApiRequestHelper.DeserialiseSnakeCaseJsonToResponse<List<ClosedConferenceWithImHistoryResponse>>(content);
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
            return await GetListExpiredConferencesResponse(uriString);
        }

        public async Task<List<ExpiredConferencesResponse>> GetExpiredAudiorecordingConferences()
        {
            _log.LogTrace($"Getting expired audiorecording conferences");

            var uriString = _apiUriFactory.ConferenceEndpoints.GetExpiredAudiorecordingConferences;
            return await GetListExpiredConferencesResponse(uriString);
        }

        private async Task<List<ExpiredConferencesResponse>> GetListExpiredConferencesResponse(string uriString)
        {
            var response = await _httpClient.GetAsync(uriString);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return ApiRequestHelper.DeserialiseSnakeCaseJsonToResponse<List<ExpiredConferencesResponse>>(content);
        }

        public async Task DeleteAudiorecordingApplication(Guid hearingId)
        {
            _log.LogTrace($"Delete audiorecording application  by hearing Id {hearingId}");
            var uriString = _apiUriFactory.ConferenceEndpoints.DeleteAudioApplication(hearingId);
            var response = await _httpClient.DeleteAsync(uriString);
            response.EnsureSuccessStatusCode();
        }
    }
}
