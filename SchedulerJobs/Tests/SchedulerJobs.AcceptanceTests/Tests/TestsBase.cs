using NUnit.Framework;
using RestSharp;
using SchedulerJobs.AcceptanceTests.Configuration;
using SchedulerJobs.AcceptanceTests.Helpers;
using TestContext = SchedulerJobs.AcceptanceTests.Helpers.TestContext;

namespace SchedulerJobs.AcceptanceTests.Tests
{
    public class TestsBase
    {
        protected TestContext Context;
        protected RequestHandler RequestHandler;

        [SetUp]
        public void SetUpConfig()
        {
            Context = new TestContext { Config = new Config() };
            Context = new Setup().RegisterSecrets(Context);
            RequestHandler = new RequestHandler(Context.Config.Services.SchedulerJobsUrl);
        }

        protected IRestResponse SendRequest(IRestRequest request)
        {
            return RequestHandler.Client().Execute(request);
        }
    }
}
