using Microsoft.Extensions.Caching.Memory;
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
        private readonly HearingServicesConfiguration _hearingServicesConfiguration;

        private const string TokenCacheKey = "BookingsApiServiceToken";

        public BookingsServiceTokenHandler(AzureAdConfiguration azureAdConfiguration, IMemoryCache memoryCache,
            IAzureTokenProvider azureTokenProvider, HearingServicesConfiguration hearingServicesConfiguration)
        {
            _azureAdConfiguration = azureAdConfiguration;
            _memoryCache = memoryCache;
            _azureTokenProvider = azureTokenProvider;
            _hearingServicesConfiguration = hearingServicesConfiguration;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _memoryCache.Get<string>(TokenCacheKey);
            if (string.IsNullOrEmpty(token))
            {
                var authenticationResult = _azureTokenProvider.GetAuthorisationResult(_azureAdConfiguration.ClientId,
                    _azureAdConfiguration.ClientSecret, _hearingServicesConfiguration.BookingsApiResourceId);
                token = authenticationResult.AccessToken;
                var tokenExpireDateTime = authenticationResult.ExpiresOn.DateTime.AddMinutes(-1);
                _memoryCache.Set(TokenCacheKey, token, tokenExpireDateTime);
            }

            request.Headers.Add("Authorization", $"Bearer {token}");
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
