using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using Testing.Common;

namespace SchedulerJobs.CronJobs.UnitTests.Jobs
{
    public class JobTestBaseSetup<T>
    {
        protected LoggerFakeGeneric<T> Logger;
        protected Mock<IHostApplicationLifetime> Lifetime;

        [SetUp]
        protected void MockerSetup()
        {
            Logger = new LoggerFakeGeneric<T>();
            Lifetime = new Mock<IHostApplicationLifetime>();
        }
    }
}