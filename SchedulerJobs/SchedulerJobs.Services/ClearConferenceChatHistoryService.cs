using System.Linq;
using System.Threading.Tasks;
using VideoApi.Client;

namespace SchedulerJobs.Services
{
    public interface IClearConferenceChatHistoryService
    {
        Task ClearChatHistoryForClosedConferences();
    }
    
    public class ClearConferenceChatHistoryService : IClearConferenceChatHistoryService
    {
        private readonly IVideoApiClient _videoApiClient;

        public ClearConferenceChatHistoryService(IVideoApiClient videoApiClient)
        {
            _videoApiClient = videoApiClient;
        }

        public async Task ClearChatHistoryForClosedConferences()
        {
            var conferences = await _videoApiClient.GetClosedConferencesWithInstantMessagesAsync();
            var tasks = conferences.Select(c => _videoApiClient.RemoveInstantMessagesAsync(c.Id)).ToArray();
            Task.WaitAll(tasks);
        }
    }
}