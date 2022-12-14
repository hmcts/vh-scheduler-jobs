using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Common.Caching;
using Testing.Common;

namespace SchedulerJobs.Sds.UnitTests.Jobs
{
    public class JobTestBaseSetup<T>
    {
        protected LoggerFakeGeneric<T> Logger;
        protected Mock<IHostApplicationLifetime> Lifetime;
        protected Mock<IDistributedJobCache> DistributedJobCache;

        [SetUp]
        protected void MockerSetup()
        {
            Logger = new LoggerFakeGeneric<T>();
            Lifetime = new Mock<IHostApplicationLifetime>();
            DistributedJobCache = new Mock<IDistributedJobCache>();
        }
    }
}