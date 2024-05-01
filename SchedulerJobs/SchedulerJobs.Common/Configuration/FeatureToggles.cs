using System;
using System.Diagnostics.CodeAnalysis;
using LaunchDarkly.Sdk;
using LaunchDarkly.Sdk.Server;
using LaunchDarkly.Sdk.Server.Interfaces;

namespace SchedulerJobs.Common.Configuration
{
    public interface IFeatureToggles
    {
        public bool StorePeopleIngestion();
        public bool WorkAllocationToggle();
        public bool ImportAllJudiciaryUsersToggle();
        public bool UsePostMay2023Template();
        public bool EjudFeatureToggle();
        public bool ELinksV2Api();
    }
    
    [ExcludeFromCodeCoverage]
    public class FeatureToggles : IFeatureToggles
    {
        private readonly ILdClient _ldClient;
        private readonly Context _context;
        private const string LdUser = "vh-scheduler-jobs";
        private const string StorePeopleIngestionToggleKey = "store-people-ingestion";
        private const string WorkAllocationToggleKey = "vho-work-allocation";
        private const string ImportAllJudiciaryUsersToggleKey = "import-all-judiciary-users";
        private const string NewNotifyTemplatesToggleKey = "notify-post-may-2023-templates";
        private const string EjudFeatureToggleKey = "ejud-feature";
        public FeatureToggles(string sdkKey, string environmentName)
        {
            var config = LaunchDarkly.Sdk.Server.Configuration.Builder(sdkKey).Build();
            
            _context = Context.Builder(LdUser).Name(environmentName).Build();
            _ldClient = new LdClient(config);
        }

        public bool StorePeopleIngestion() => GetBoolToggle(StorePeopleIngestionToggleKey);

        public bool WorkAllocationToggle() => GetBoolToggle(WorkAllocationToggleKey);

        public bool ImportAllJudiciaryUsersToggle() => GetBoolToggle(ImportAllJudiciaryUsersToggleKey);
        
        public bool UsePostMay2023Template() => GetBoolToggle(NewNotifyTemplatesToggleKey);
        
        public bool EjudFeatureToggle() => GetBoolToggle(EjudFeatureToggleKey);
        public bool ELinksV2Api()
        {
            // TODO: create a new toggle in LaunchDarkly and return the value here
            return false;
        }

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
