using System.Threading.Tasks;

namespace SchedulerJobs.Services.Interfaces
{
    public interface IReconcileHearingAudioService
    {
        Task ReconcileAudiorecordingsWithConferencesAsync();
    }
}
