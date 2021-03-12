using System;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Common.Security;

namespace SchedulerJobs.UnitTests.Common.Security
{
    public class AzureTokenProviderTests
    {
        [Test]
        public void Should_get_access_token()
        {
            var azureTokenProvider = new AzureTokenProvider(
                Options.Create(new AzureAdConfiguration
                {
                    TenantId = "teanantid"
                }));

            Assert.Throws<AggregateException>(() =>
            azureTokenProvider.GetClientAccessToken("1234", "1234", "1234"));
        }
    }
}
