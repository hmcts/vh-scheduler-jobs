using Moq;
using NUnit.Framework;
using VideoApi.Client;

namespace SchedulerJobs.Services.UnitTests
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
