﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using SchedulerJobs.Service;

namespace SchedulerJobs.UnitTests
{
    /// <summary>
    /// Tests for logger class
    /// </summary>
    /// <remarks>
    /// Though difficult to reliably test that the TelemetryClient does what is expected.
    /// At the very least this class will test any possibly failure within the logic of the methods.
    /// </remarks>
    public class ApplicationLoggerTest
    {
        [Test]
        public void Should_trace_without_failure()
        {
            ApplicationLogger.Trace("Category", "Title", "Information");
        }

        [Test]
        public void Should_trace_without_or_without_properties()
        {
            ApplicationLogger.TraceWithProperties("Category", "Title", null);
            ApplicationLogger.TraceWithProperties("Category", "Title", new Dictionary<string, string> { { "property", "value" } });
        }

        [Test]
        public void Should_trace_without_or_without_object()
        {
            ApplicationLogger.TraceWithObject("Category", "Title",  null);
            dynamic someObject = new
            {
                Property = "value"
            };
            ApplicationLogger.TraceWithObject("Category", "Title",  someObject);
        }

        [Test]
        public void Should_throw_exception_if_trying_to_trace_null_exception()
        {
            Assert.Throws<ArgumentNullException>(
                () => ApplicationLogger.TraceException("Category", "Title", null, null));
        }

        [Test]
        public void Should_trace_exception()
        {
            var exception = new Exception("Test");
            var properties = new Dictionary<string, string> { { "property", "value" } };
            ApplicationLogger.TraceException("Category", "Title", exception,  null);
            ApplicationLogger.TraceException("Category", "Title", exception,  null);
            ApplicationLogger.TraceException("Category", "Title", exception,  properties);
        }

        [Test]
        public void Should_trace_event()
        {
            var properties = new Dictionary<string, string> { { "property", "value" } };
            ApplicationLogger.TraceEvent("Title", properties);
            ApplicationLogger.TraceEvent("Title", null);
        }

        [Test]
        public void Should_trace_result()
        {
            ApplicationLogger.TraceRequest("Operation", DateTimeOffset.Now, TimeSpan.FromSeconds(2), "200", true);
        }
    }
}