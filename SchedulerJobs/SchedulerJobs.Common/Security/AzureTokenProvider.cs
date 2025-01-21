using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using SchedulerJobs.Common.Configuration;

namespace SchedulerJobs.Common.Security
{
    public interface IAzureTokenProvider
    {
        Task<string> GetClientAccessToken(string clientId, string clientSecret, string clientResource);
        Task<AuthenticationResult> GetAuthorisationResult(string clientId, string clientSecret, string clientResource);
    }

    [ExcludeFromCodeCoverage]
    public class AzureTokenProvider : IAzureTokenProvider
    {
        private readonly AzureAdConfiguration _azureAdConfiguration;

        public AzureTokenProvider(IOptions<AzureAdConfiguration> azureAdConfigurationOptions)
        {
            _azureAdConfiguration = azureAdConfigurationOptions.Value;
        }

        public async Task<string> GetClientAccessToken(string clientId, string clientSecret, string clientResource)
        {
            var result = await GetAuthorisationResult(clientId, clientSecret, clientResource);
            return result.AccessToken;
        }

        public async Task<AuthenticationResult> GetAuthorisationResult(string clientId, string clientSecret,
            string clientResource)
        {
            AuthenticationResult result;
            var authority = $"{AzureAdConfiguration.Authority}{_azureAdConfiguration.TenantId}";
            var app = ConfidentialClientApplicationBuilder.Create(clientId).WithClientSecret(clientSecret)
                .WithAuthority(authority).Build();

            try
            {
                result = await app.AcquireTokenForClient(new[] { $"{clientResource}/.default" }).ExecuteAsync();
            }
            catch (MsalServiceException)
            {
                throw new UnauthorizedAccessException();
            }

            return result;
        }
    }
}
