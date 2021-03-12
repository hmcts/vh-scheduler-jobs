﻿using Autofac.Extras.Moq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using NUnit.Framework;
using Testing.Common;

namespace SchedulerJobs.UnitTests.Functions
{
    public class AzureFunctionTestBaseSetup<T>
    {
        protected readonly TimerInfo _timerInfo = new TimerInfo(new ScheduleStub(), new ScheduleStatus(), true);
        protected AutoMock _mocker;
        protected T _sut;
        protected LoggerFake _logger;

        [SetUp]
        protected void MockerSetup()
        {
            _mocker = AutoMock.GetLoose();
            _sut = _mocker.Create<T>();
            _logger = (LoggerFake)TestFactory.CreateLogger(LoggerTypes.List);
        }
    }
}
