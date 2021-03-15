using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Testing.Common
{
    public class LoggerFake: ILogger
    {
        private readonly IList<string> _logs;

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
            _logs = new List<string>();
        }

        public void Log<TState>(LogLevel logLevel,
                                EventId eventId,
                                TState state,
                                Exception exception,
                                Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            _logs.Add(message);
        }

        public IList<string> GetLoggedMessages() => _logs;
    }

    public enum LoggerTypes
    {
        Null,
        List
    }
}
