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
        private readonly ApiUriFactory _apiUriFactory;
        private readonly ILogger<UserApiService> _logger;
        public UserApiService(HttpClient httpClient, HearingServicesConfiguration hearingServicesConfiguration,
            ILogger<UserApiService> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(hearingServicesConfiguration.UserApiUrl);
            _apiUriFactory = new ApiUriFactory();
            _logger = logger;
        }
        public async Task DeleteUserAsync(string username)
        {
            _logger.LogInformation($"UserApiService: Executing DeleteUserAsync for { username } at: {DateTime.UtcNow}");
            _logger.LogTrace($"Delete AD user with username {username}");
            var uriString = _apiUriFactory.UserEndpoints.DeleteUser(username);
            var response = await _httpClient.DeleteAsync(uriString);
        }
    }
}
