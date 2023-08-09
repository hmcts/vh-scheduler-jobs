using System;
using LaunchDarkly.Sdk;
using LaunchDarkly.Sdk.Server;
using LaunchDarkly.Sdk.Server.Interfaces;

namespace SchedulerJobs.Common.Configuration
{
    public interface IFeatureToggles
    {
        public bool BookAndConfirmToggle();
        public bool StorePeopleIngestion();
        public bool WorkAllocationToggle();
        public bool ImportAllJudiciaryUsersToggle();
    }
    
    public class FeatureToggles : IFeatureToggles
    {
        private readonly ILdClient _ldClient;
        private readonly Context _context;
        private const string LdUser = "vh-scheduler-jobs";
        private const string BookAndConfirmToggleKey = "Book_and_Confirm";
        private const string StorePeopleIngestionToggleKey = "store-people-ingestion";
        private const string WorkAllocationToggleKey = "vho-work-allocation";
        private const string ImportAllJudiciaryUsersToggleKey = "import-all-judiciary-users";
        
        public FeatureToggles(string sdkKey, string environmentName)
        {
            var config = LaunchDarkly.Sdk.Server.Configuration.Builder(sdkKey).Build();
            
            _context = Context.Builder(LdUser).Name(environmentName).Build();
            _ldClient = new LdClient(config);
        }

        public bool BookAndConfirmToggle() => GetBoolToggle(BookAndConfirmToggleKey);
        
        public bool StorePeopleIngestion() => GetBoolToggle(StorePeopleIngestionToggleKey);

        public bool WorkAllocationToggle() => GetBoolToggle(WorkAllocationToggleKey);

        public bool ImportAllJudiciaryUsersToggle() => GetBoolToggle(ImportAllJudiciaryUsersToggleKey);
        
        private bool GetBoolToggle(string key)
        {
            if (!_ldClient.Initialized)
            {
                throw new InvalidOperationException("LaunchDarkly client not initialized");
            }
            return _ldClient.BoolVariation(key, _context);
        }
    }
}