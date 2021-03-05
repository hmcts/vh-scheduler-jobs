using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SchedulerJobs.Contract.Responses;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BookingQueueSubscriber
{
    public class HealthCheckFunction
    {
        [FunctionName("HealthCheck")]
        public async Task<IActionResult> HealthCheck(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health/liveness")] HttpRequest req, ILogger log)
        {
            var response = new HealthCheckResponse
            {
                VideoApiHealth = { Successful = true },
                AppVersion = GetApplicationVersion()
            };

            return new OkObjectResult(response);
        }

        private ApplicationVersion GetApplicationVersion()
        {
            var applicationVersion = new ApplicationVersion();
            applicationVersion.FileVersion = GetExecutingAssemblyAttribute<AssemblyFileVersionAttribute>(a => a.Version);
            applicationVersion.InformationVersion = GetExecutingAssemblyAttribute<AssemblyInformationalVersionAttribute>(a => a.InformationalVersion);
            return applicationVersion;
        }

        private string GetExecutingAssemblyAttribute<T>(Func<T, string> value) where T : Attribute
        {
            T attribute = (T)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(T));
            return value.Invoke(attribute);
        }
    }
}