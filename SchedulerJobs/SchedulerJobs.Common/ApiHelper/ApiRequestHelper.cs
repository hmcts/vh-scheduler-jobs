using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SchedulerJobs.Common.ApiHelper
{
    public static class ApiRequestHelper
    {
        public static T Deserialise<T>(string response)
        {
            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
            
            return JsonConvert.DeserializeObject<T>(response, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });
        }

        public static string SerialiseRequestToSnakeCaseJson(object request)
        {
            return JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            });
        }
    }
}