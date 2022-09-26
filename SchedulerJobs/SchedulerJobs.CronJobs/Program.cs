using Azure.Storage;
using Azure.Storage.Blobs;
using BookingsApi.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NotificationApi.Client;
using SchedulerJobs.Common.ApiHelper;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Common.Security;
using SchedulerJobs.CronJobs.Jobs;
using SchedulerJobs.Services;
using SchedulerJobs.Services.Configuration;
using SchedulerJobs.Services.HttpClients;
using SchedulerJobs.Services.Interfaces;
using UserApi.Client;
using VideoApi.Client;

var hostBuilder = Host.CreateDefaultBuilder(args);

hostBuilder.ConfigureServices((hostContext, services) =>
{
    var configuration = hostContext.Configuration;

    RegisterJobs(services, args);
    RegisterServices(services, configuration);
});

var host = hostBuilder.Build();

await host.RunAsync();

void RegisterJobs(IServiceCollection services, IEnumerable<string> args)
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

    if (args.Contains(nameof(DeleteAudiorecordingApplicationsJob)))
    {
        services.AddHostedService<DeleteAudiorecordingApplicationsJob>();
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
}

void RegisterServices(IServiceCollection services, IConfiguration configuration)
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
      builder.AddApplicationInsights(configuration["ApplicationInsights:InstrumentationKey"])
    );

    services.AddScoped<ICloseConferenceService, CloseConferenceService>();
    services.AddScoped<IClearConferenceChatHistoryService, ClearConferenceChatHistoryService>();
    services.AddScoped<IAnonymiseHearingsConferencesDataService, AnonymiseHearingsConferencesDataService>();
    services.AddScoped<IRemoveHeartbeatsForConferencesService, RemoveHeartbeatsForConferencesService>();
    services.AddScoped<IReconcileHearingAudioService, ReconcileHearingAudioService>();
    services.AddScoped<IHearingNotificationService, HearingNotificationService>();
    services.AddScoped<IJobHistoryService, JobHistoryService>();

    bool.TryParse(configuration["UseELinksStub"], out var useELinksStub);

    var featureToggle = new FeatureToggles(configuration.GetSection("FeatureToggles:SdkKey").Value);
    
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