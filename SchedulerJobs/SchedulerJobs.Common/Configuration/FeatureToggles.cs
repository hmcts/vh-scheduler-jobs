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
        private readonly User _user;
        private const string LdUser = "vh-scheduler-jobs";
        private const string BookAndConfirmToggleKey = "Book_and_Confirm";
        private const string StorePeopleIngestionToggleKey = "store-people-ingestion";
        private const string WorkAllocationToggleKey = "vho-work-allocation";
        private const string ImportAllJudiciaryUsersToggleKey = "import-all-judiciary-users";
        
        public FeatureToggles(string sdkKey)
        {
            _ldClient = new LdClient(sdkKey);
            _user = User.WithKey(LdUser);
        }

        public bool BookAndConfirmToggle() => _ldClient.BoolVariation(BookAndConfirmToggleKey, _user);
        
        public bool StorePeopleIngestion() => _ldClient.BoolVariation(StorePeopleIngestionToggleKey, _user);

        public bool WorkAllocationToggle() => _ldClient.BoolVariation(WorkAllocationToggleKey, _user);

        public bool ImportAllJudiciaryUsersToggle() => _ldClient.BoolVariation(ImportAllJudiciaryUsersToggleKey, _user);
    }
}