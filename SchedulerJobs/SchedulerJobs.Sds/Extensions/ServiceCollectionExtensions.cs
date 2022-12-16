using System.Diagnostics.CodeAnalysis;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using SchedulerJobs.Sds.Caching;
using StackExchange.Redis;

namespace SchedulerJobs.Sds.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisInfrastructure(this IServiceCollection serviceCollection,
            string connectionString)
        {
            serviceCollection.AddStackExchangeRedisCache(options => { options.Configuration = connectionString; });
            serviceCollection.AddSingleton<IDistributedJobRunningStatusCache, DistributedJobRunningStatusCache>();
            
            serviceCollection.AddSingleton<IRedisContextAccessor>(_ =>
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
