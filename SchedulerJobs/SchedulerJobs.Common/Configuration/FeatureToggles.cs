using LaunchDarkly.Sdk;
using LaunchDarkly.Sdk.Server;
using LaunchDarkly.Sdk.Server.Interfaces;

namespace SchedulerJobs.Common.Configuration
{
    public interface IFeatureToggles
    {
        public bool BookAndConfirmToggle();
    }
    
    public class FeatureToggles : IFeatureToggles
    {
        private readonly ILdClient _ldClient;
        private readonly User _user;
        private const string LdUser = "vh-scheduler-jobs";
        private const string BookAndConfirmToggleKey = "Book_and_Confirm";
        public FeatureToggles(string sdkKey)
        {
            _ldClient = new LdClient(sdkKey);
            _user = User.WithKey(LdUser);
        }

        public bool BookAndConfirmToggle() => _ldClient.BoolVariation(BookAndConfirmToggleKey, _user);
    }
}