using BookingsApi.Client;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchedulerJobs;
using SchedulerJobs.Common.ApiHelper;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Common.Security;
using SchedulerJobs.Services;
using System;
using System.IO;
using SchedulerJobs.Services.HttpClients;
using UserApi.Client;
using VH.Core.Configuration;
using VideoApi.Client;

[assembly: WebJobsStartup(typeof(Startup))]
namespace SchedulerJobs
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }


            base.ConfigureAppConfiguration(builder);

            const string vhInfraCore = "/mnt/secrets/vh-infra-core";
            const string vhSchedulerJobs = "/mnt/secrets/vh-scheduler-jobs";

            var context = builder.GetContext();
            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.json"), true)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), true)
                .AddAksKeyVaultSecretProvider(vhInfraCore)
                .AddAksKeyVaultSecretProvider(vhSchedulerJobs)
                .AddUserSecrets("518CD6B6-4F2B-4431-94C8-4D0F4137295F")
                .AddEnvironmentVariables();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var context = builder.GetContext();
            RegisterServices(builder.Services, context.Configuration);
        }

        public void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            services.AddSingleton<IMemoryCache>(memoryCache);
            services.Configure<AzureAdConfiguration>(options =>
            {
                configuration.GetSection("AzureAd").Bind(options);
            });
            services.Configure<ServicesConfiguration>(options =>
            {
                configuration.GetSection("VhServices").Bind(options);
            });

            var serviceConfiguration = new ServicesConfiguration();
            configuration.GetSection("VhServices").Bind(serviceConfiguration);

            services.AddScoped<IAzureTokenProvider, AzureTokenProvider>();

            services.AddLogging(builder =>
                builder.AddApplicationInsights(configuration["ApplicationInsights:InstrumentationKey"])
            );

            services.AddScoped<ICloseConferenceService, CloseConferenceService>();
            services.AddScoped<IClearConferenceChatHistoryService, ClearConferenceChatHistoryService>();
            services.AddScoped<IAnonymiseHearingsConferencesDataService, AnonymiseHearingsConferencesDataService>();
            services.AddScoped<IRemoveHeartbeatsForConferencesService, RemoveHeartbeatsForConferencesService>();

            services.AddTransient<VideoServiceTokenHandler>();
            services.AddTransient<BookingsServiceTokenHandler>();
            services.AddTransient<UserServiceTokenHandler>();

            services.AddHttpClient<IVideoApiClient, VideoApiClient>()
                .AddHttpMessageHandler<VideoServiceTokenHandler>()
                .AddTypedClient(httpClient =>
                {
                    var client = VideoApiClient.GetClient(httpClient);
                    client.BaseUrl = serviceConfiguration.VideoApiUrl;
                    client.ReadResponseAsString = true;
                    return (IVideoApiClient)client;
                });

            services.AddHttpClient<IBookingsApiClient, BookingsApiClient>()
                .AddHttpMessageHandler<BookingsServiceTokenHandler>()
                .AddTypedClient(httpClient =>
                {
                    var client = BookingsApiClient.GetClient(httpClient);
                    client.BaseUrl = serviceConfiguration.BookingsApiUrl;
                    client.ReadResponseAsString = true;
                    return (IBookingsApiClient)client;
                });

            services.AddHttpClient<IUserApiClient, UserApiClient>()
                .AddHttpMessageHandler<UserServiceTokenHandler>()
                .AddTypedClient(httpClient =>
                {
                    var client = UserApiClient.GetClient(httpClient);
                    client.BaseUrl = serviceConfiguration.UserApiUrl;
                    client.ReadResponseAsString = true;
                    return (IUserApiClient)client;
                });
            
            services.AddHttpClient<IELinksApiClient, ELinksApiClient>()
                .AddHttpMessageHandler<ELinksApiDelegatingHandler>()
                .AddTypedClient(httpClient => new ELinksApiClient(httpClient, serviceConfiguration.ELinksApiUrl));
        }
    }
}
