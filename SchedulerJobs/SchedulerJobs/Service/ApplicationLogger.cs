using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;

namespace SchedulerJobs.Service
{
    /// <summary>
    /// The application logger class send telemetry to Application Insights.
    /// </summary>
    public static class ApplicationLogger
    {
        private static readonly TelemetryClient TelemetryClient = new TelemetryClient();

        public static void Trace(string traceCategory, string eventTitle, string information)
        {
            var telemetry = new TraceTelemetry(traceCategory, SeverityLevel.Information);
            telemetry.Properties.Add("Information", information);
            telemetry.Properties.Add("Event", eventTitle);
            TelemetryClient.TrackTrace(telemetry);
        }

        public static void TraceWithProperties(string traceCategory, string eventTitle, IDictionary<string, string> properties)
        {
            var telemetry = new TraceTelemetry(traceCategory, SeverityLevel.Information);

            telemetry.Properties.Add("Event", eventTitle);

            if (properties != null)
            {
                foreach (KeyValuePair<string, string> entry in properties)
                {
                    telemetry.Properties.Add(entry.Key, entry.Value);
                }
            }

            TelemetryClient.TrackTrace(telemetry);

        }

        public static void TraceWithObject(string traceCategory, string eventTitle, object valueToSerialized)
        {
            var telemetry = new TraceTelemetry(traceCategory, SeverityLevel.Information);

            telemetry.Properties.Add("Event", eventTitle);

            if (valueToSerialized != null)
            {
                telemetry.Properties.Add(valueToSerialized.GetType().Name, JsonConvert.SerializeObject(valueToSerialized, Formatting.None));
            }

            TelemetryClient.TrackTrace(telemetry);
        }

        public static void TraceException(string traceCategory, string eventTitle, Exception exception, IDictionary<string, string> properties)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var exceptionTelemetry = new ExceptionTelemetry(exception);

            exceptionTelemetry.Properties.Add("Event", traceCategory + " " + eventTitle);

            if (properties != null)
            {
                foreach (var (key, value) in properties)
                {
                    exceptionTelemetry.Properties.Add(key, value);
                }
            }

            TelemetryClient.TrackException(exceptionTelemetry);
        }

        public static void TraceException(string traceCategory, string eventTitle, Exception exception)
        {
            TraceException(traceCategory, eventTitle, exception, null);
        }

        public static void TraceEvent(string eventTitle, IDictionary<string, string> properties)
        {
            var telemetryEvent = new EventTelemetry(eventTitle);

            if (properties != null)
            {
                foreach (KeyValuePair<string, string> entry in properties)
                {
                    telemetryEvent.Properties.Add(entry.Key, entry.Value);
                }
            }

            TelemetryClient.TrackEvent(telemetryEvent);
        }

        public static void TraceRequest(string operationName, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
        {
            var telemetryOperation = new RequestTelemetry(operationName, startTime, duration, responseCode, success);
            TelemetryClient.TrackRequest(telemetryOperation);
        }
    }
}
