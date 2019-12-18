using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Common.Security;
using SchedulerJobs;

namespace SchedulerJobs.UnitTests
{
    public class VideoServiceTokenHandlerTests
    {
        [Test]
        public void Should_add_authorization_header()
        {
            var memoryCache = new Mock<IMemoryCache>().Object;
            var tokenProviderMock = new Mock<IAzureTokenProvider>();
            var azureTokenProvider = tokenProviderMock.Object;
            new VideoServiceTokenHandler(
                new AzureAdConfiguration
                {
                    Authority = "auth",
                    ClientId = "id",
                    ClientSecret = "secret",
                    TenantId = "tenant",
                   
                }, memoryCache, azureTokenProvider,
                new HearingServicesConfiguration
                {
                    VideoApiResourceId = "resourceid"
                });

            tokenProviderMock.Setup(x => x.GetAuthorisationResult(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        }
    }
}
