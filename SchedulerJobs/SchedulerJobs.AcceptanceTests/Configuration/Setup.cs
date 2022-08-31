using AcceptanceTests.Common.Configuration;
using AcceptanceTests.Common.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SchedulerJobs.AcceptanceTests.Helpers;
using SchedulerJobs.Common.Configuration;
using ConfigurationManager = AcceptanceTests.Common.Configuration.ConfigurationManager;

namespace SchedulerJobs.AcceptanceTests.Configuration
{
    public class Setup
    {
        private IConfigurationRoot _configRoot;
        private TestContext _context;

        public TestContext RegisterSecrets(TestContext context)
        {
            _context = context;
            _configRoot = ConfigurationManager.BuildConfig("518CD6B6-4F2B-4431-94C8-4D0F4137295F", "ffc3783d-4ef2-40e2-9588-080bb740aa9a");
            RegisterAzureSecrets();
            RegisterServices();
            return context;
        }

        private void RegisterAzureSecrets()
        {
            var azureOptions = Options.Create(_configRoot.GetSection("AzureAd").Get<AzureAdConfiguration>());
            _context.Config.AzureAdConfiguration = azureOptions.Value;
            _context.Config.AzureAdConfiguration.Authority.Should().NotBeNullOrWhiteSpace();
            _context.Config.AzureAdConfiguration.ClientId.Should().NotBeNullOrWhiteSpace();
            _context.Config.AzureAdConfiguration.ClientSecret.Should().NotBeNullOrWhiteSpace();
            _context.Config.AzureAdConfiguration.TenantId.Should().NotBeNullOrWhiteSpace();
        }

        private void RegisterServices()
        {
            _context.Config.Services = GetTargetTestEnvironment() == string.Empty ? Options.Create(_configRoot.GetSection("VhServices").Get<ServicesConfiguration>()).Value
                : Options.Create(_configRoot.GetSection($"Testing.{GetTargetTestEnvironment()}.VhServices").Get<ServicesConfiguration>()).Value;
            if (_context.Config.Services == null && GetTargetTestEnvironment() != string.Empty)
            {
                throw new TestSecretsFileMissingException(GetTargetTestEnvironment());
            }

            _context.Config.Services.SchedulerJobsUrl.Should().NotBeNullOrWhiteSpace();
        }

        private static string GetTargetTestEnvironment()
        {
            return NUnit.Framework.TestContext.Parameters["TargetTestEnvironment"] ?? string.Empty;
        }
    }
}
