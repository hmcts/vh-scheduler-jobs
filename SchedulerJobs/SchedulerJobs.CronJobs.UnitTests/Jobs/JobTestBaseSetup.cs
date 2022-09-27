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
            Logger = TestFactory.CreateFakeLogger<T>(LoggerTypes.List);
            Lifetime = new Mock<IHostApplicationLifetime>();
        }
    }
}