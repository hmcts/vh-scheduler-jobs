using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Common.ApiHelper;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Common.Security;

namespace SchedulerJobs.UnitTests.Common.ApiHelper
{
    public class UserServiceTokenHandlerTests
    {
        [Test]
        public void Should_add_authorization_header()
        {
            var memoryCache = new Mock<IMemoryCache>().Object;
            var tokenProviderMock = new Mock<IAzureTokenProvider>();
            var azureTokenProvider = tokenProviderMock.Object;
            new UserServiceTokenHandler(new AzureAdConfiguration
            {
                ClientId = "id",
                ClientSecret = "secret",
                TenantId = "tenant",
            }, memoryCache, azureTokenProvider,
            new HearingServicesConfiguration { BookingsApiResourceId = "resourceid" });
            tokenProviderMock.Setup(x => x.GetAuthorisationResult(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        }
    }
}
