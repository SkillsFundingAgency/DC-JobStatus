namespace ESFA.DC.JobStatus.Dto
{
    public class JobStatusWithValidationDataDto
    {
        public JobStatusWithValidationDataDto()
        {
        }

        public JobStatusWithValidationDataDto(long jobId, int jobStatus, long numberOfLearners)
        {
            JobId = jobId;
            JobStatus = jobStatus;
            NumberOfLearners = numberOfLearners;
        }

        public long JobId { get; set; }

        public int JobStatus { get; set; }

        public long NumberOfLearners { get; set; }
    }
}
