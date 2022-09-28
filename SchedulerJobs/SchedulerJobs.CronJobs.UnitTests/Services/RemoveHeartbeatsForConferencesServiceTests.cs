﻿using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;
using VideoApi.Client;

namespace SchedulerJobs.CronJobs.UnitTests.Services
{
    [TestFixture]
    public class RemoveHeartbeatsForConferencesServiceTests
    {
        private Mock<IVideoApiClient> _videoApiClient;
        private IRemoveHeartbeatsForConferencesService _removeHeartbeatsForConferencesService;

        [SetUp]
        public void Setup()
        {
            _videoApiClient = new Mock<IVideoApiClient>();
            _removeHeartbeatsForConferencesService = new RemoveHeartbeatsForConferencesService(_videoApiClient.Object);
        }

        [Test]
        public void Should_remove_heartbeats_for_old_conferences()
        {
            _removeHeartbeatsForConferencesService.RemoveHeartbeatsForConferencesAsync();
            _videoApiClient.Verify(x => x.RemoveHeartbeatsForConferencesAsync(), Times.Once);
        }
    }
}
