using Microsoft.Extensions.Logging;

namespace Testing.Common
{
    public class LoggerFakeGeneric<T>: LoggerFake, ILogger<T>
    {
    }
}
