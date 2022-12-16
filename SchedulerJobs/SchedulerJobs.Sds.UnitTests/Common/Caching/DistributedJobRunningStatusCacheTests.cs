using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using RedLockNet;
using SchedulerJobs.Sds.Caching;

namespace SchedulerJobs.Sds.UnitTests.Common.Caching
{
    public class DistributedJobRunningStatusCacheTests
    {
        private Mock<IDistributedCache> _distributedCacheMock;
        private DistributedJobRunningStatusCache _distributedJobRunningStatusCache;
        private Mock<IRedisContextAccessor> _redisContextAccessor;
        
        [SetUp]
        public void Setup()
        {
            _distributedCacheMock = new Mock<IDistributedCache>();
            _redisContextAccessor = new Mock<IRedisContextAccessor>();
            _distributedJobRunningStatusCache = new DistributedJobRunningStatusCache(_distributedCacheMock.Object, _redisContextAccessor.Object);
        }
        
        [Test]
        public async Task UpdateJobRunningStatus_Updates_Cache_Value_When_Job_Is_Running()
        {
            // Arrange
            var entryPrefix = "job_running_status_";
            var jobName = "TestJob";
            var key = $"{entryPrefix}{jobName}";
            var expectedIsRunningValue = true;
            var serialized = JsonConvert.SerializeObject(expectedIsRunningValue, SerializerSettings);
            var rawData = Encoding.UTF8.GetBytes(serialized);
            _distributedCacheMock.Setup(x => x.GetAsync(key, CancellationToken.None)).ReturnsAsync(rawData);

            // Act
            await _distributedJobRunningStatusCache.UpdateJobRunningStatus(expectedIsRunningValue,
                jobName);
            
            // Assert
            var result = await _distributedJobRunningStatusCache.IsJobRunning(jobName);
            result.Should().Be(expectedIsRunningValue);
        }

        [Test]
        public async Task UpdateJobRunningStatus_Removes_Cache_Value_When_Job_Is_No_Longer_Running()
        {
            // Arrange
            var entryPrefix = "job_running_status_";
            var jobName = "TestJob";
            var key = $"{entryPrefix}{jobName}";
            var expectedIsRunningValue = false;
            var serialized = JsonConvert.SerializeObject(expectedIsRunningValue, SerializerSettings);
            var rawData = Encoding.UTF8.GetBytes(serialized);
            _distributedCacheMock.Setup(x => x.GetAsync(key, CancellationToken.None)).ReturnsAsync(rawData);

            // Act
            await _distributedJobRunningStatusCache.UpdateJobRunningStatus(expectedIsRunningValue,
                jobName);
            
            // Assert
            var result = await _distributedJobRunningStatusCache.IsJobRunning(jobName);
            result.Should().Be(expectedIsRunningValue);
        }

        [Test]
        public void WriteToCache_Throws_Exception_When_CacheEntryOptions_Not_Set()
        {
            // Arrange
            var redisContextAccessor = new Mock<IRedisContextAccessor>();
            var cache = new DistributedJobRunningStatusCache(_distributedCacheMock.Object, null, redisContextAccessor.Object);
            var entryPrefix = "job_running_status_";
            var jobName = "TestJob";
            var key = $"{entryPrefix}{jobName}";

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await cache.WriteToCache(key, It.IsAny<bool>()));
        }

        [Test]
        public async Task ReadFromCache_Returns_False_When_Exception_Thrown()
        {
            // Arrange
            var jobName = "TestJob";
            _distributedCacheMock.Setup(x => x.GetAsync(jobName, default))
                .Throws<Exception>();

            // Act
            var result = await _distributedJobRunningStatusCache.IsJobRunning(jobName);

            // Assert
            result.Should().Be(false);
        }

        [Test]
        public async Task CreateLock_Creates_Lock()
        {
            // Arrange
            var entryPrefix = "job_running_status_";
            var jobName = "TestJob";
            var key = $"{entryPrefix}{jobName}";
            var expectedIsRunningValue = false;
            var serialized = JsonConvert.SerializeObject(expectedIsRunningValue, SerializerSettings);
            var rawData = Encoding.UTF8.GetBytes(serialized);
            _redisContextAccessor.Setup(x => x.CreateLockAsync(key, It.IsAny<TimeSpan>()))
                .ReturnsAsync(() => new Mock<IRedLock>().Object);
            _distributedCacheMock.Setup(x => x.GetAsync(key, CancellationToken.None)).ReturnsAsync(rawData);

            // Act
            var result = await _distributedJobRunningStatusCache.CreateLockAsync(jobName);
            
            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public async Task DisposeCache_Disposes()
        {
            // Arrange
            var redisContextAccessor = new Mock<IRedisContextAccessor>();
            var cache = new DistributedJobRunningStatusCache(_distributedCacheMock.Object, redisContextAccessor.Object);

            // Act
            cache.DisposeCache();

            // Assert
            redisContextAccessor.Verify(x => x.DisposeContext(), Times.Once);
        }
        
        private static JsonSerializerSettings SerializerSettings => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects, Formatting = Formatting.None
        };
    }
}
