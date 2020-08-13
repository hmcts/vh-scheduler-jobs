using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;

namespace SchedulerJobs.UnitTests.Services
{
    [TestFixture]
    public class RemoveHeartbeatsForConferencesServiceTests
    {
        private Mock<IVideoApiService> _videoApiService;
        private IRemoveHeartbeatsForConferencesService _removeHeartbeatsForConferencesService;

        [SetUp]
        public void Setup()
        {
            _videoApiService = new Mock<IVideoApiService>();
            _removeHeartbeatsForConferencesService = new RemoveHeartbeatsForConferencesService(_videoApiService.Object);
        }

        [Test]
        public void Should_remove_heartbeats_for_old_conferences()
        {
            _removeHeartbeatsForConferencesService.RemoveHeartbeatsForConferencesAsync();
            _videoApiService.Verify(x => x.RemoveHeartbeatsForConferencesAsync(), Times.Once);
        }
    }
}
