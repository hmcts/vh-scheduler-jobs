using System.Linq;
using System.Threading.Tasks;

namespace SchedulerJobs.Services
{
    public interface IClearConferenceChatHistoryService
    {
        Task ClearChatHistoryForClosedConferences();
    }
    
    public class ClearConferenceChatHistoryService : IClearConferenceChatHistoryService
    {
        private readonly IVideoApiService _videoApiService;

        public ClearConferenceChatHistoryService(IVideoApiService videoApiService)
        {
            _videoApiService = videoApiService;
        }

        public async Task ClearChatHistoryForClosedConferences()
        {
            var conferences = await _videoApiService.GetClosedConferencesToClearInstantMessageHistory();
            var tasks = conferences.Select(c => _videoApiService.ClearConferenceChatHistory(c.Id)).ToArray();
            Task.WaitAll(tasks);
        }
    }
}