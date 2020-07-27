using Microsoft.Extensions.Logging;
using SchedulerJobs.Common.ApiHelper;
using SchedulerJobs.Common.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SchedulerJobs.Services
{
    public interface IUserApiService
    {
        /// <summary>
        /// Deletes a user from Aad.
        /// </summary>
        /// <param name="username"></param>
        Task DeleteUserAsync(string username);
    }
    public class UserApiService : IUserApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _log;
        private readonly ApiUriFactory _apiUriFactory;
        public UserApiService(HttpClient httpClient, HearingServicesConfiguration hearingServicesConfiguration,
            ILoggerFactory factory)
        {
            _httpClient = httpClient;
            _log = factory.CreateLogger<BookingsApiService>();
            _httpClient.BaseAddress = new Uri(hearingServicesConfiguration.UserApiUrl);
            _apiUriFactory = new ApiUriFactory();
        }
        public async Task DeleteUserAsync(string username)
        {
            _log.LogInformation($"UserApiService: Executing DeleteUserAsync for { username } at: {DateTime.UtcNow}");
            _log.LogTrace($"Delete AD user with username {username}");
            var uriString = _apiUriFactory.UserEndpoints.DeleteUser(username);
            var response = await _httpClient.DeleteAsync(uriString);
            response.EnsureSuccessStatusCode();
        }
    }
}
