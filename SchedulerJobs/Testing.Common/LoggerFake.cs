using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Testing.Common
{
    public class LoggerFake: ILogger
    {
        public readonly IList<string> Logs;

        /// <summary>
        /// Points to the static instance on the NullScope class to allow the test to function
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => false;

        public LoggerFake()
        {
            Logs = new List<string>();
        }

        public void Log<TState>(LogLevel logLevel,
                                EventId eventId,
                                TState state,
                                Exception exception,
                                Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            Logs.Add(message);
        }
    }

    public enum LoggerTypes
    {
        Null,
        List
    }
}
