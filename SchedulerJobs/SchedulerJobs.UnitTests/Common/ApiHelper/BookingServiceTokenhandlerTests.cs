using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Common.ApiHelper;
using SchedulerJobs.Common.Configuration;
using SchedulerJobs.Common.Security;

namespace SchedulerJobs.UnitTests.Common.ApiHelper
{
    public class BookingServiceTokenhandlerTests
    {
        [Test]
        public void Should_add_authorization_header()
        {
            var memoryCache = new Mock<IMemoryCache>().Object;
            var tokenProviderMock = new Mock<IAzureTokenProvider>();
            var azureTokenProvider = tokenProviderMock.Object;
            new BookingsServiceTokenHandler(
                Options.Create(new AzureAdConfiguration
                {
                    ClientId = "id",
                    ClientSecret = "secret",
                    TenantId = "tenant",
                }),
                Options.Create(new ServicesConfiguration { BookingsApiResourceId = "resourceId" }),
                memoryCache, azureTokenProvider);
            tokenProviderMock.Setup(x => x.GetAuthorisationResult(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        }
    }
}
