using BookingsApi.Client;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchedulerJobs.Common.ApiHelper;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Common.Security;
using SchedulerJobs.Services;
using System;
using System.IO;
using Microsoft.FeatureManagement;
using SchedulerJobs.Services.HttpClients;
using UserApi.Client;
using VH.Core.Configuration;
using VideoApi.Client;
using SchedulerJobs.Services.Interfaces;

[assembly: FunctionsStartup(typeof(SchedulerJobs.Startup))]
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
            services.AddFeatureManagement();
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
            services.AddScoped<IReconcileHearingAudioService, ReconcileHearingAudioService>();

            bool.TryParse(configuration["UseELinksStub"], out var useELinksStub);

            if (useELinksStub)
            {
                services.AddScoped<IELinksService, ELinksServiceStub>();
            }
            else
            {
                services.AddScoped<IELinksService, ELinksService>();
                services.AddTransient<ELinksApiDelegatingHandler>();
                services.AddHttpClient<IPeoplesClient, PeoplesClient>()
                    .AddHttpMessageHandler<ELinksApiDelegatingHandler>()
                    .AddTypedClient(httpClient =>
                    {
                        var peoplesClient = new PeoplesClient(httpClient)
                        {
                            BaseUrl = serviceConfiguration.ELinksPeoplesBaseUrl
                        };
                        return (IPeoplesClient)peoplesClient;
                    });
                services.AddHttpClient<ILeaversClient, LeaversClient>()
                    .AddHttpMessageHandler<ELinksApiDelegatingHandler>()
                    .AddTypedClient(httpClient =>
                    {
                        var leaversClient = new LeaversClient(httpClient)
                        {
                            BaseUrl = serviceConfiguration.ELinksLeaversBaseUrl
                        };
                        return (ILeaversClient)leaversClient;
                    });
            }

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
        }
    }
}
