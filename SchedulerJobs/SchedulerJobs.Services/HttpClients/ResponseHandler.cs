using SchedulerJobs.Common.Exceptions;
using System.Net.Http;
using System.Threading.Tasks;

namespace SchedulerJobs.Services.HttpClients
{
    public static class ResponseHandler
    {
        public static async Task HandleUnsuccessfulResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();

                throw new ELinksApiException(errorMessage, response.StatusCode);
            }
        }
    }
}