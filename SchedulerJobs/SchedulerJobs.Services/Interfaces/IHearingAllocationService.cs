using System.Threading.Tasks;

namespace SchedulerJobs.Services.Interfaces
{
    public interface IHearingAllocationService
    {
        Task AllocateHearingsAsync();
    }
}
