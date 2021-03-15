using RestSharp;

namespace SchedulerJobs.AcceptanceTests.Helpers
{
    public class RequestHandler
    {
        public string BaseUrl { get; set; }

        public RequestHandler(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        public RestClient Client()
        {
            var client = new RestClient(BaseUrl);
            client.AddDefaultHeader("Accept", "application/json");
            return client;
        }

        public RestRequest Get(string path) => new RestRequest(path, Method.GET);
    }
}
