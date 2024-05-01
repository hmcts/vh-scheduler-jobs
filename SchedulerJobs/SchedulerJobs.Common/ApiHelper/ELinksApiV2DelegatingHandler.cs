using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SchedulerJobs.Common.Configuration;

namespace SchedulerJobs.Common.ApiHelper;

public class ELinksApiV2DelegatingHandler : DelegatingHandler
{
    private readonly ServicesConfiguration _servicesConfiguration;

    public ELinksApiV2DelegatingHandler(IOptions<ServicesConfiguration> servicesConfigurationOptions)
    {
        _servicesConfiguration = servicesConfigurationOptions.Value;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("Authorization", $"Token {_servicesConfiguration.ELinksApiKeyV2}");
        return await base.SendAsync(request, cancellationToken);
    }
}