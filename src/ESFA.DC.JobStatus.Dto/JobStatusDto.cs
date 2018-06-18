namespace ESFA.DC.JobStatus.Dto
{
    /// <summary>
    /// The Job Status DTO, must be serialisable.
    /// </summary>
    public class JobStatusDto
    {
        public JobStatusDto()
        {
        }

        public JobStatusDto(long jobId, int jobStatus, long numberOfLearners = -1)
        {
            JobId = jobId;
            JobStatus = jobStatus;
            NumberOfLearners = numberOfLearners;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public long JobId { get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public int JobStatus { get; set; }

        public long NumberOfLearners { get; set; }
    }
}
