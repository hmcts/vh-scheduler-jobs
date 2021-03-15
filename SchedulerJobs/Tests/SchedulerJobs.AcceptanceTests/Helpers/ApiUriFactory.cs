namespace SchedulerJobs.AcceptanceTests.Helpers
{
    public static class ApiUriFactory
    {
        public static class HealthCheckEndpoints
        {
            private const string ApiRoot = "/health";
            public static string CheckServiceHealth => $"{ApiRoot}/liveness";
        }
    }
}
