using Microsoft.Extensions.Options;
using SchedulerJobs.Common.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SchedulerJobs.Common.ApiHelper
{
    public class ELinksApiDelegatingHandler : DelegatingHandler
    {
        private readonly ServicesConfiguration _servicesConfiguration;

        public ELinksApiDelegatingHandler(IOptions<ServicesConfiguration> servicesConfigurationOptions)
        {
            _servicesConfiguration = servicesConfigurationOptions.Value;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Authorization", $"Bearer {_servicesConfiguration.ELinksApiKey}");
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
