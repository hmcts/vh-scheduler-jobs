﻿using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Common.Security;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SchedulerJobs.Common.ApiHelper
{
    public class BookingsServiceTokenHandler : DelegatingHandler
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IAzureTokenProvider _azureTokenProvider;
        private readonly AzureAdConfiguration _azureAdConfiguration;
        private readonly ServicesConfiguration _servicesConfiguration;

        private const string TokenCacheKey = "BookingsApiServiceToken";

        public BookingsServiceTokenHandler(
            IOptions<AzureAdConfiguration> azureAdConfigurationOptions,
            IOptions<ServicesConfiguration> servicesConfigurationOptions,
            IMemoryCache memoryCache,
            IAzureTokenProvider azureTokenProvider)
        {
            _azureAdConfiguration = azureAdConfigurationOptions.Value;
            _servicesConfiguration = servicesConfigurationOptions.Value;
            _memoryCache = memoryCache;
            _azureTokenProvider = azureTokenProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _memoryCache.Get<string>(TokenCacheKey);
            if (string.IsNullOrEmpty(token))
            {
                var authenticationResult = _azureTokenProvider.GetAuthorisationResult(_azureAdConfiguration.ClientId,
                    _azureAdConfiguration.ClientSecret, _servicesConfiguration.BookingsApiResourceId);
                token = authenticationResult.AccessToken;
                var tokenExpireDateTime = authenticationResult.ExpiresOn.DateTime.AddMinutes(-1);
                _memoryCache.Set(TokenCacheKey, token, tokenExpireDateTime);
            }

            request.Headers.Add("Authorization", $"Bearer {token}");
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
