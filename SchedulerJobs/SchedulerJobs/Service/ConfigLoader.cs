using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using VH.Core.Configuration;

namespace SchedulerJobs.Service
{
    public class ConfigLoader
    {
        public readonly IConfiguration Configuration;

        public ConfigLoader()
        {
            const string vhInfraCore = "/mnt/secrets/vh-infra-core";
            const string vhSchedulerJobs = "/mnt/secrets/vh-scheduler-jobs";

            var configRootBuilder = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<Startup>()
                .AddAksKeyVaultSecretProvider(vhInfraCore)
                .AddAksKeyVaultSecretProvider(vhSchedulerJobs);

            Configuration = configRootBuilder.Build();

            KeyVaultClient keyVaultClient = null;

            AzureServiceTokenProvider azpv = new AzureServiceTokenProvider();
            keyVaultClient = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(azpv.KeyVaultTokenCallback));

            configRootBuilder.AddAzureAppConfiguration(options =>
            {
                options.Connect(Configuration["ConnectionStrings:AppConfig"])
                    .Select(KeyFilter.Any)
                    .Select(KeyFilter.Any, labelFilter: "vh-scheduler-jobs")
                    .UseAzureKeyVault(keyVaultClient);
            });

            Configuration = configRootBuilder.Build();
        }
    }
}
