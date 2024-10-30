using Azure.Storage;
using Azure.Storage.Blobs;
using BookingsApi.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NotificationApi.Client;
using SchedulerJobs.Common.ApiHelper;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Common.Security;
using SchedulerJobs.Sds.Jobs;
using SchedulerJobs.Services;
using SchedulerJobs.Services.Configuration;
using SchedulerJobs.Services.HttpClients;
using SchedulerJobs.Services.Interfaces;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration.KeyPerFile;
using Microsoft.Extensions.FileProviders;
using SchedulerJobs.Sds.Configuration;
using SchedulerJobs.Sds.Extensions;
using UserApi.Client;
using VideoApi.Client;

var hostBuilder = Host.CreateDefaultBuilder(args);

hostBuilder.ConfigureServices((hostContext, services) =>
{
    var configuration = hostContext.Configuration;

    RegisterJobs(services, args);
    RegisterServices(services, configuration);
});

hostBuilder.ConfigureAppConfiguration(ConfigureAppConfiguration);

var host = hostBuilder.Build();

await host.RunAsync();

[ExcludeFromCodeCoverage]
public static partial class Program
{
    private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
        
        var keyVaults=new List<string> (){
            "vh-infra-core",
            "vh-scheduler-jobs",
            "vh-bookings-api",
            "vh-video-api",
            "vh-notification-api",
            "vh-user-api"
        };

        builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true)
            .AddUserSecrets("eb01af44-05dd-4541-90b1-6fed69c33e3a")
            .AddEnvironmentVariables();
        
        foreach (var keyVault in keyVaults)
        {
            var filePath = $"/mnt/secrets/{keyVault}";
            if (Directory.Exists(filePath))
            {
                builder.Add(GetKeyPerFileSource(filePath));    
            }
        }
    }
    
    private static KeyPerFileConfigurationSource GetKeyPerFileSource(string filePath)
    {
        return new KeyPerFileConfigurationSource
        {
            FileProvider = new PhysicalFileProvider(filePath),
            Optional = true,
            ReloadOnChange = true,
            SectionDelimiter = "--" // Set your custom delimiter here
        };
    }

    private static void RegisterJobs(IServiceCollection services, IEnumerable<string> args)
    {
        if (args.Contains(nameof(AnonymiseHearingsConferencesAndDeleteAadUsersJob))) 
        { 
            services.AddHostedService<AnonymiseHearingsConferencesAndDeleteAadUsersJob>(); 
        }
        
        if (args.Contains(nameof(ClearConferenceInstantMessageHistoryJob))) 
        { 
            services.AddHostedService<ClearConferenceInstantMessageHistoryJob>(); 
        }

        if (args.Contains(nameof(ClearHearingsJob)))
        {
            services.AddHostedService<ClearHearingsJob>();
        }

        if (args.Contains(nameof(GetJudiciaryUsersJob)))
        {
            services.AddHostedService<GetJudiciaryUsersJob>();   
        }

        if (args.Contains(nameof(ReconcileHearingAudioWithStorageJob)))
        {
            services.AddHostedService<ReconcileHearingAudioWithStorageJob>();
        }

        if (args.Contains(nameof(RemoveHeartbeatsForConferencesJob)))
        {
            services.AddHostedService<RemoveHeartbeatsForConferencesJob>();
        }

        if (args.Contains(nameof(SendHearingNotificationsJob)))
        {
            services.AddHostedService<SendHearingNotificationsJob>();
        }
        
        if (args.Contains(nameof(HearingsAllocationJob)))
        {
            services.AddHostedService<HearingsAllocationJob>();
        }
    }

    private static void RegisterServices(IServiceCollection services, IConfiguration configuration)
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
        
        Console.WriteLine("VH Scheduler jobs: RegisterServices : resolving dependency for azure config with singleton middleware");
        services.AddSingleton(configuration.GetSection("AzureConfiguration").Get<AzureConfiguration>());
        
        var vhBlobServiceClient = new BlobServiceClient(new Uri(azureConfiguration.StorageEndpoint),
            new StorageSharedKeyCredential(azureConfiguration.StorageAccountName, azureConfiguration.StorageAccountKey));
        var blobClientExtension = new BlobClientExtension();
        var azureStorage =
            new VhAzureStorageService(vhBlobServiceClient, azureConfiguration, false, blobClientExtension);
        services.AddSingleton<IAzureStorageService>(x => azureStorage);
        
        services.AddScoped<IAzureTokenProvider, AzureTokenProvider>();
        
        services.AddLogging(builder =>
            builder.AddApplicationInsights(
                configureTelemetryConfiguration: config => config.ConnectionString = configuration["ApplicationInsights:ConnectionString"],
                configureApplicationInsightsLoggerOptions: _ => {}
            ));

        services.AddScoped<ICloseConferenceService, CloseConferenceService>();
        services.AddScoped<IClearConferenceChatHistoryService, ClearConferenceChatHistoryService>();
        services.AddScoped<IAnonymiseHearingsConferencesDataService, AnonymiseHearingsConferencesDataService>();
        services.AddScoped<IRemoveHeartbeatsForConferencesService, RemoveHeartbeatsForConferencesService>();
        services.AddScoped<IReconcileHearingAudioService, ReconcileHearingAudioService>();
        services.AddScoped<IHearingNotificationService, HearingNotificationService>();
        services.AddScoped<IJobHistoryService, JobHistoryService>();
        services.AddScoped<IHearingAllocationService, HearingAllocationService>();

        var useELinksStub = false;
        if (bool.TryParse(configuration["UseELinksStub"], out var parsedUseELinksStub))
        {
            useELinksStub = parsedUseELinksStub;
        }

        var envName = configuration["VhServices:BookingsApiResourceId"]; // any service url will do here since we only care about the env name
        var featureToggle = new FeatureToggles(configuration.GetSection("FeatureToggles:SdkKey").Value, envName);
        
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
        
        var connectionStrings = new ConnectionStrings();
        configuration.GetSection("ConnectionStrings").Bind(connectionStrings);
        services.AddRedisInfrastructure(connectionStrings.RedisCache);
    }
}