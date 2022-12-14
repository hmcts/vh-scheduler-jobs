using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SchedulerJobs.Common.Caching;

namespace SchedulerJobs.UnitTests.Common.Caching
{
    public class DistributedJobRunningStatusCacheTests
    {
        private Mock<IDistributedCache> _distributedCacheMock;
        private DistributedJobRunningStatusRunningStatusCache _distributedJobRunningStatusRunningStatusCache;
        
        [SetUp]
        public void Setup()
        {
            _distributedCacheMock = new Mock<IDistributedCache>();
            _distributedJobRunningStatusRunningStatusCache = new DistributedJobRunningStatusRunningStatusCache(_distributedCacheMock.Object);
        }
        
        [Test]
        public async Task Updates_Cache_Value()
        {
            var keyName = "KeyName";
            var expectedIsRunningValue = true;

            var serialized = JsonConvert.SerializeObject(expectedIsRunningValue, SerializerSettings);
            var rawData = Encoding.UTF8.GetBytes(serialized);
            _distributedCacheMock.Setup(x => x.GetAsync(keyName, CancellationToken.None)).ReturnsAsync(rawData);

            await _distributedJobRunningStatusRunningStatusCache.UpdateJobRunningStatus(expectedIsRunningValue,
                keyName);            
            var result = await _distributedJobRunningStatusRunningStatusCache.IsJobRunning(keyName);

            result.Should().Be(expectedIsRunningValue);
        }
        
        private static JsonSerializerSettings SerializerSettings => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects, Formatting = Formatting.None
        };
    }
}
