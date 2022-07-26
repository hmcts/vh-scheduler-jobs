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
using Azure.Storage;
using Azure.Storage.Blobs;
using SchedulerJobs.Services.HttpClients;
using UserApi.Client;
using VH.Core.Configuration;
using VideoApi.Client;
using SchedulerJobs.Services.Interfaces;
using NotificationApi.Client;
using SchedulerJobs.Services.Configuration;

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

            const List<string> keyVaults=new List<string> (){
                "vh-infra-core",
                "vh-scheduler-jobs",
                "vh-bookings-api",
                "vh-video-api",
                "vh-notification-api",
                "vh-user-api"
            };

            var context = builder.GetContext();
            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.json"), true)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), true)
                .AddUserSecrets("518CD6B6-4F2B-4431-94C8-4D0F4137295F")
                .AddEnvironmentVariables();
            
            foreach (var keyVault in keyVaults)
            {
                builder.ConfigurationBuilder.AddAksKeyVaultSecretProvider(keyVault);
            }
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
            
            services.Configure<AzureConfiguration>(options =>
            {
                configuration.GetSection("WowzaConfiguration").Bind(options);
            });

            var serviceConfiguration = new ServicesConfiguration();
            configuration.GetSection("VhServices").Bind(serviceConfiguration);
            
            var azureConfiguration = new AzureConfiguration();
            configuration.GetSection("AzureConfiguration").Bind(azureConfiguration);
            
            
            services.AddSingleton(configuration.GetSection("AzureConfiguration").Get<AzureConfiguration>());
            
            
            var vhBlobServiceClient = new BlobServiceClient(new Uri(azureConfiguration.StorageEndpoint),
                new StorageSharedKeyCredential(azureConfiguration.StorageAccountName, azureConfiguration.StorageAccountKey));
            var blobClientExtension = new BlobClientExtension();
            var azureStorage =
                new VhAzureStorageService(vhBlobServiceClient, azureConfiguration, false, blobClientExtension);
            services.AddSingleton<IAzureStorageService>(x => azureStorage);
            
            
            services.AddScoped<IAzureTokenProvider, AzureTokenProvider>();

            services.AddLogging(builder =>
              builder.AddApplicationInsights(configuration["ApplicationInsights:InstrumentationKey"])
            );

            services.AddScoped<ICloseConferenceService, CloseConferenceService>();
            services.AddScoped<IClearConferenceChatHistoryService, ClearConferenceChatHistoryService>();
            services.AddScoped<IAnonymiseHearingsConferencesDataService, AnonymiseHearingsConferencesDataService>();
            services.AddScoped<IRemoveHeartbeatsForConferencesService, RemoveHeartbeatsForConferencesService>();
            services.AddScoped<IReconcileHearingAudioService, ReconcileHearingAudioService>();
            services.AddScoped<IHearingNotificationService, HearingNotificationService>();

            bool.TryParse(configuration["UseELinksStub"], out var useELinksStub);

            var featureToggle = new FeatureToggles(configuration.GetSection("FeatureToggles:SDK-Key").Value);
            
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
            services.AddTransient<NotificationServiceTokenHandler>();

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

            services.AddHttpClient<INotificationApiClient, NotificationApiClient>()
                .AddHttpMessageHandler<NotificationServiceTokenHandler>()
                .AddTypedClient(httpClient =>
                {
                    var client = NotificationApiClient.GetClient(httpClient);
                    client.BaseUrl = serviceConfiguration.NotificationApiUrl;
                    client.ReadResponseAsString = true;
                    return (INotificationApiClient)client;
                });

            services.AddSingleton<IFeatureToggles>(featureToggle);
        }
    }
}
