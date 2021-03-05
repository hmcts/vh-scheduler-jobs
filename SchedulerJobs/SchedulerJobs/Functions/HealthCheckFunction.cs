using SchedulerJobs.Contract.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;
using VideoApi.Client;
using UserApi.Client;
using BookingsApi.Client;

namespace BookingQueueSubscriber
{
    public class HealthCheckFunction
    {
        [FunctionName("HealthCheck")]
        public async Task<IActionResult> HealthCheck(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health/liveness")] HttpRequest req, ILogger log, [Inject] IVideoApiClient videoApiClient, [Inject] IUserApiClient userApiClient, [Inject] IBookingsApiClient bookingsApiClient)
        {
            var response = new HealthCheckResponse
            {
                VideoApiHealth = { Successful = true },
                BookingsApiHealth = { Successful = true },
                UserApiHealth = { Successful = true },
                AppVersion = GetApplicationVersion()
            };

            try
            {
                await videoApiClient.GetExpiredOpenConferencesAsync();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unable to retrieve expired open conferences");
                response.VideoApiHealth = HandleVideoApiCallException(ex);
            }

            try
            {
                await bookingsApiClient.CheckServiceHealthAsync();
            }
            catch (Exception ex)
            {
                response.BookingsApiHealth.Successful = false;
                response.BookingsApiHealth.ErrorMessage = ex.Message;
                response.BookingsApiHealth.Data = ex.Data;
            }

            try
            {
                await userApiClient.CheckServiceHealthAsync();
            }
            catch (Exception ex)
            {
                response.UserApiHealth.Successful = false;
                response.UserApiHealth.Data = ex.Data;
                response.UserApiHealth.ErrorMessage = ex.Message;
            }

            return new OkObjectResult(response);
        }

        private HealthCheck HandleVideoApiCallException(Exception ex)
        {
            var isApiException = ex is VideoApiException;
            var healthCheck = new HealthCheck { Successful = true };
            if (isApiException && ((VideoApiException)ex).StatusCode != (int)HttpStatusCode.InternalServerError)
            {
                return healthCheck;
            }

            healthCheck.Successful = false;
            healthCheck.ErrorMessage = ex.Message;
            healthCheck.Data = ex.Data;

            return healthCheck;
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
