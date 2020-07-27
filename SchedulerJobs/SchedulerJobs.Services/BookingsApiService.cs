using Microsoft.Extensions.Logging;
using SchedulerJobs.Common.ApiHelper;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Services.BookingApi.Contracts;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SchedulerJobs.Services
{
    public interface IBookingsApiService
    {
        Task<UserWithClosedConferencesResponse> GetUsersWithClosedConferencesAsync();
        /// <summary>
        /// Anonymises the hearing, case, person, organisation and participant data for hearings older than 3 months.
        /// </summary>
        Task AnonymiseHearingsAsync();
    }
    public class BookingsApiService : IBookingsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiUriFactory _apiUriFactory;
        private readonly ILogger<BookingsApiService> _logger;
        public BookingsApiService(HttpClient httpClient, HearingServicesConfiguration hearingServicesConfiguration,
            ILogger<BookingsApiService> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(hearingServicesConfiguration.BookingsApiUrl);
            _apiUriFactory = new ApiUriFactory();
            _logger = logger;
        }
        public async Task AnonymiseHearingsAsync()
        {
            _logger.LogInformation($"BookingsApiService: Executing AnonymiseHearings at: {DateTime.UtcNow}");
            _logger.LogTrace($"Scheduler: Anonymise old hearings");
            var uriString = _apiUriFactory.HearingsEndpoints.AnonymiseHearings();
            var response = await _httpClient.PatchAsync(uriString, null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<UserWithClosedConferencesResponse> GetUsersWithClosedConferencesAsync()
        {
            _logger.LogInformation($"BookingsApiService: Executing GetUsersWithClosedconferences at: {DateTime.UtcNow}");
            _logger.LogTrace($"Scheduler: Retrieve list of users with closed hearings and no future hearings.");
            var uriString = _apiUriFactory.PersonsEndpoints.GetUserWithClosedHearings();

            var response = await _httpClient.GetAsync(uriString);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return ApiRequestHelper.DeserialiseSnakeCaseJsonToResponse<UserWithClosedConferencesResponse>(content);
        }
    }
}
