﻿using System.Threading.Tasks;
using ESFA.DC.JobStatus.Dto;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Queueing.Interface;

namespace ESFA.DC.JobStatus
{
    public sealed class JobStatus : IJobStatus
    {
        private readonly IQueuePublishService<JobStatusDto> _queuePublishService;

        public JobStatus(IQueuePublishService<JobStatusDto> queuePublishService)
        {
            _queuePublishService = queuePublishService;
        }

        public async Task JobStartedAsync(long jobId)
        {
            await _queuePublishService.PublishAsync(new JobStatusDto(jobId, (int)Interface.JobStatusType.Processing));
        }

        public async Task JobFinishedAsync(long jobId)
        {
            await _queuePublishService.PublishAsync(new JobStatusDto(jobId, (int)Interface.JobStatusType.Completed));
        }

        public async Task JobFailedIrrecoverablyAsync(long jobId)
        {
            await _queuePublishService.PublishAsync(new JobStatusDto(jobId, (int)Interface.JobStatusType.Failed));
        }

        public async Task JobFailedRecoverablyAsync(long jobId)
        {
            await _queuePublishService.PublishAsync(new JobStatusDto(jobId, (int)Interface.JobStatusType.FailedRetry));
        }
    }
}
