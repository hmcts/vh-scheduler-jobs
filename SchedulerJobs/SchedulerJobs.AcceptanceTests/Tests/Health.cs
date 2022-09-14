using System;
using System.Net;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using SchedulerJobs.AcceptanceTests.Helpers;

namespace SchedulerJobs.AcceptanceTests.Tests
{
    public class HealthTests : TestsBase
    {
        [Test]
        public void GetHealth()
        {
            var request = RequestHandler.Get(ApiUriFactory.HealthCheckEndpoints.CheckServiceHealth);
            var response = SendRequest(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.IsSuccessful.Should().BeTrue();
        }
    }
}
