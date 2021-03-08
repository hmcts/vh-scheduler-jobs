﻿using BookingsApi.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchedulerJobs;
using SchedulerJobs.Common.ApiHelper;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Common.Security;
using SchedulerJobs.Service;
using SchedulerJobs.Services;
using UserApi.Client;
using VideoApi.Client;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(Startup))]
namespace SchedulerJobs
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddTimers();
            builder.AddDependencyInjection(ConfigureServices);
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            var configLoader = new ConfigLoader();
            // need to check if bind works for both tests and host

            var adConfiguration = configLoader.Configuration.GetSection("AzureAd")
                .Get<AzureAdConfiguration>() ?? BuildAdConfiguration(configLoader);

            services.AddSingleton(adConfiguration);

            var hearingServicesConfiguration =
                configLoader.Configuration.GetSection("VhServices").Get<HearingServicesConfiguration>() ??
                BuildHearingServicesConfiguration(configLoader);

            services.AddSingleton(hearingServicesConfiguration);
            services.AddScoped<IAzureTokenProvider, AzureTokenProvider>();

            services.AddScoped<VideoServiceTokenHandler>();
            services.AddScoped<BookingsServiceTokenHandler>();
            services.AddScoped<UserServiceTokenHandler>();
            services.AddLogging(builder => { builder.SetMinimumLevel(LogLevel.Debug); });

            services.AddScoped<ICloseConferenceService, CloseConferenceService>();
            services.AddScoped<IClearConferenceChatHistoryService, ClearConferenceChatHistoryService>();
            services.AddScoped<IAnonymiseHearingsConferencesDataService, AnonymiseHearingsConferencesDataService>();
            services.AddScoped<IRemoveHeartbeatsForConferencesService, RemoveHeartbeatsForConferencesService>();

            services.AddHttpClient<IVideoApiClient, VideoApiClient>()
                .AddHttpMessageHandler<VideoServiceTokenHandler>()
                .AddTypedClient(httpClient =>
                {
                    var client = VideoApiClient.GetClient(httpClient);
                    client.BaseUrl = hearingServicesConfiguration.VideoApiUrl;
                    client.ReadResponseAsString = true;
                    return (IVideoApiClient) client;
                });

            services.AddHttpClient<IBookingsApiClient, BookingsApiClient>()
                .AddHttpMessageHandler<BookingsServiceTokenHandler>()
                .AddTypedClient(httpClient =>
                {
                    var client = BookingsApiClient.GetClient(httpClient);
                    client.BaseUrl = hearingServicesConfiguration.BookingsApiUrl;
                    client.ReadResponseAsString = true;
                    return (IBookingsApiClient) client;
                });

            services.AddHttpClient<IUserApiClient, UserApiClient>()
                .AddHttpMessageHandler<UserServiceTokenHandler>()
                .AddTypedClient(httpClient =>
                {
                    var client = UserApiClient.GetClient(httpClient);
                    client.BaseUrl = hearingServicesConfiguration.UserApiUrl;
                    client.ReadResponseAsString = true;
                    return (IUserApiClient) client;
                });
        }

        private static HearingServicesConfiguration BuildHearingServicesConfiguration(ConfigLoader configLoader)
        {
            var values = configLoader.Configuration.GetSection("Values");
            var hearingServicesConfiguration = new HearingServicesConfiguration();
            values.GetSection("VhServices").Bind(hearingServicesConfiguration);
            return hearingServicesConfiguration;
        }

        private static AzureAdConfiguration BuildAdConfiguration(ConfigLoader configLoader)
        {
            var values = configLoader.Configuration.GetSection("Values");
            var azureAdConfiguration = new AzureAdConfiguration();
            values.GetSection("AzureAd").Bind(azureAdConfiguration);
            return azureAdConfiguration;
        }
    }
}
