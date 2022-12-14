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
            var entryPrefix = "job_running_status_";
            var jobName = "TestJob";
            var key = $"{entryPrefix}{jobName}";
            var expectedIsRunningValue = true;

            var serialized = JsonConvert.SerializeObject(expectedIsRunningValue, SerializerSettings);
            var rawData = Encoding.UTF8.GetBytes(serialized);
            _distributedCacheMock.Setup(x => x.GetAsync(key, CancellationToken.None)).ReturnsAsync(rawData);

            await _distributedJobRunningStatusRunningStatusCache.UpdateJobRunningStatus(expectedIsRunningValue,
                jobName);            
            var result = await _distributedJobRunningStatusRunningStatusCache.IsJobRunning(jobName);

            result.Should().Be(expectedIsRunningValue);
        }
        
        private static JsonSerializerSettings SerializerSettings => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects, Formatting = Formatting.None
        };
    }
}
