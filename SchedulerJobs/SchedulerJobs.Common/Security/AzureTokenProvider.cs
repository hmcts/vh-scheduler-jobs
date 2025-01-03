﻿using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SchedulerJobs.Common.Configuration;

namespace SchedulerJobs.Common.Security
{
    public interface IAzureTokenProvider
    {
        string GetClientAccessToken(string clientId, string clientSecret, string clientResource);
        AuthenticationResult GetAuthorisationResult(string clientId, string clientSecret, string clientResource);
    }

    [ExcludeFromCodeCoverage]
    public class AzureTokenProvider : IAzureTokenProvider
    {
        private readonly AzureAdConfiguration _azureAdConfiguration;

        public AzureTokenProvider(IOptions<AzureAdConfiguration> azureAdConfigurationOptions)
        {
            _azureAdConfiguration = azureAdConfigurationOptions.Value;
        }

        public string GetClientAccessToken(string clientId, string clientSecret, string clientResource)
        {
            var result = GetAuthorisationResult(clientId, clientSecret, clientResource);
            return result.AccessToken;
        }

        public AuthenticationResult GetAuthorisationResult(string clientId, string clientSecret, string clientResource)
        {
            AuthenticationResult result;
            var credential = new ClientCredential(clientId, clientSecret);
            var authContext =
                new AuthenticationContext($"{AzureAdConfiguration.Authority}{_azureAdConfiguration.TenantId}");

            try
            {
                result = authContext.AcquireTokenAsync(clientResource, credential).Result;
            }
            catch (AdalException)
            {
                throw new UnauthorizedAccessException();
            }

            return result;
        }
    }
}
