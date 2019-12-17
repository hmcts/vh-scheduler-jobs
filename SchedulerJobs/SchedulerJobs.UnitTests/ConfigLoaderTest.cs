using FluentAssertions;
using NUnit.Framework;
using SchedulerJobs.Service;

namespace SchedulerJobs.UnitTests
{
    public class ConfigLoaderTest
    {
        [Test]
        public void Should_get_configuration_object()
        {
            var configLoader = new ConfigLoader();
            configLoader.Configuration.Should().NotBeNull();
        }
    }
}
