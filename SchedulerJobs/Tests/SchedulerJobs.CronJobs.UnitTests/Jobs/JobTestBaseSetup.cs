using Autofac.Extras.Moq;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using Testing.Common;

namespace SchedulerJobs.CronJobs.UnitTests.Jobs
{
    public class JobTestBaseSetup<T>
    {
        protected AutoMock _mocker;
        protected T _sut;
        protected LoggerFakeGeneric<T> _logger;
        protected Mock<IHostApplicationLifetime> _lifetime;

        [SetUp]
        protected void MockerSetup()
        {
            _mocker = AutoMock.GetLoose();
            MockerAdditionalSetupBeforeSutCreation();
            _sut = _mocker.Create<T>();
            _logger = TestFactory.CreateFakeLogger<T>(LoggerTypes.List);
            _lifetime = new Mock<IHostApplicationLifetime>();
        }
        
        protected virtual void MockerAdditionalSetupBeforeSutCreation(){}
    }
   
}