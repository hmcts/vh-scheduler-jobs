using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using SchedulerJobs.Common.Caching;
using StackExchange.Redis;

namespace SchedulerJobs.Common.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisInfrastructure(this IServiceCollection serviceCollection,
            string connectionString)
        {
            serviceCollection.AddStackExchangeRedisCache(options => { options.Configuration = connectionString; });
            serviceCollection.AddSingleton<IDistributedJobRunningStatusCache, DistributedJobRunningStatusCache>();
            
            serviceCollection.AddSingleton<IRedisContextAcccessor>(_ =>
            {
                var muxer = ConnectionMultiplexer.Connect(connectionString);
                var connectionMultiplexers = new List<RedLockMultiplexer> { new(muxer) };
                var redLockFactory = RedLockFactory.Create(connectionMultiplexers);

                return new RedisContextAccessor(redLockFactory);
            });

            return serviceCollection;
        }
    }
}
