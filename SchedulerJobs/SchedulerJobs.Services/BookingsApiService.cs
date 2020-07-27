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
        Task<UserWithClosedConferencesResponse> GetUsersWithClosedConferences();
        /// <summary>
        /// Anonymises the hearing, case, person, organisation and participant data for hearings older than 3 months.
        /// </summary>
        Task AnonymiseHearings();
    }
    public class BookingsApiService : IBookingsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _log;
        private readonly ApiUriFactory _apiUriFactory;
        public BookingsApiService(HttpClient httpClient, HearingServicesConfiguration hearingServicesConfiguration,
            ILoggerFactory factory)
        {
            _httpClient = httpClient;
            _log = factory.CreateLogger<BookingsApiService>();
            _httpClient.BaseAddress = new Uri(hearingServicesConfiguration.BookingsApiUrl);
            _apiUriFactory = new ApiUriFactory();
        }
        public async Task AnonymiseHearings()
        {
            _log.LogInformation($"BookingsApiService: Executing AnonymiseHearings at: {DateTime.UtcNow}");
            _log.LogTrace($"Scheduler: Anonymise old hearings");
            var uriString = _apiUriFactory.HearingsEndpoints.AnonymiseHearings();
            var response = await _httpClient.PatchAsync(uriString, null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<UserWithClosedConferencesResponse> GetUsersWithClosedConferences()
        {
            _log.LogInformation($"BookingsApiService: Executing GetUsersWithClosedconferences at: {DateTime.UtcNow}");
            _log.LogTrace($"Scheduler: Retrieve list of users with closed hearings and no future hearings.");
            var uriString = _apiUriFactory.PersonsEndpoints.GetUserWithClosedHearings();

            var response = await _httpClient.GetAsync(uriString);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return ApiRequestHelper.DeserialiseSnakeCaseJsonToResponse<UserWithClosedConferencesResponse>(content);
        }
    }
}
