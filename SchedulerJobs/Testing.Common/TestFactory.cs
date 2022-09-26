using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Testing.Common
{
    public static class TestFactory
    {
        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            return type == LoggerTypes.List ? new LoggerFake() : NullLoggerFactory.Instance.CreateLogger("Null Logger");
        }
        
        public static LoggerFakeGeneric<T> CreateFakeLogger<T>(LoggerTypes type = LoggerTypes.Null)
        {
            return type == LoggerTypes.List ? new LoggerFakeGeneric<T>() : null;
        }
    }
}
