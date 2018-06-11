using System.Threading.Tasks;

namespace ESFA.DC.JobStatus.Interface
{
    public interface IJobStatus
    {
        Task JobStartedAsync(long jobId);

        Task JobFinishedAsync(long jobId);

        Task JobFailedIrrecoverablyAsync(long jobId);

        Task JobFailedRecoverablyAsync(long jobId);
    }
}
