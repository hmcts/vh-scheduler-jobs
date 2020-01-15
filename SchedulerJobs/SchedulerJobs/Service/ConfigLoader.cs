using System;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace SchedulerJobs.Service
{
    public class ConfigLoader
    {
        public readonly IConfiguration Configuration;
        public ConfigLoader()
        {
            var configRootBuilder = new ConfigurationBuilder()
                   .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables()
                   .AddUserSecrets<Startup>();

            Configuration = configRootBuilder.Build();

            KeyVaultClient keyVaultClient = null;

            bool isDevelopment = Environment.GetEnvironmentVariable("Development", EnvironmentVariableTarget.Process) == null || bool.Parse(Environment.GetEnvironmentVariable("Development", EnvironmentVariableTarget.Process));

            if (!isDevelopment)
            {
                AzureServiceTokenProvider azpv = new AzureServiceTokenProvider($"RunAs = App; AppId ={Configuration["KeyVault:ClientId"]}");
                keyVaultClient = new KeyVaultClient(
                        new KeyVaultClient.AuthenticationCallback(azpv.KeyVaultTokenCallback));
            }
            else
            {
                keyVaultClient = new KeyVaultClient(async (authority, resource, scope) =>
                    {
                        var adCredential = new ClientCredential(Configuration["KeyVault:ClientId"], Configuration["KeyVault:ClientSecret"]);
                        var authenticationContext = new AuthenticationContext(authority, null);
                        return (await authenticationContext.AcquireTokenAsync(resource, adCredential)).AccessToken;
                    });
            }

            configRootBuilder.AddAzureAppConfiguration(options =>
            {

                options.Connect(Configuration["ConnectionStrings:AppConfig"])
                .Select(KeyFilter.Any)
                .Select(KeyFilter.Any, labelFilter: "scheduler-jobs")
                .UseAzureKeyVault(keyVaultClient);
            });

            Configuration = configRootBuilder.Build();
        }
    }
}
