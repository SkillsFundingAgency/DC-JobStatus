using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.JobStatus.Dto;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing.Interface;

namespace ESFA.DC.JobStatus.WebServiceCall.Service
{
    public sealed class JobStatusWebServiceCallService<T> : IJobStatusWebServiceCallService<T>
        where T : JobStatusDto, new()
    {
        private readonly IQueueSubscriptionService<JobStatusDto> _queueSubscriptionService;

        private readonly ILogger _logger;

        private readonly string _endPointUrl;

        private readonly HttpClient client = new HttpClient();

        public JobStatusWebServiceCallService(
            IJobStatusWebServiceCallServiceConfig jobStatusWebServiceCallServiceConfig, IQueueSubscriptionService<JobStatusDto> queueSubscriptionService, ILogger logger)
        {
            _queueSubscriptionService = queueSubscriptionService;
            _logger = logger;
            _endPointUrl = jobStatusWebServiceCallServiceConfig.EndPointUrl;
            if (!_endPointUrl.EndsWith("/"))
            {
                _endPointUrl = _endPointUrl + "/";
            }
        }

        // Todo: Remove me
        public void Subscribe()
        {
            throw new NotImplementedException();
        }

        public void Subscribe(bool deadLetter)
        {
            if (deadLetter)
            {
                _queueSubscriptionService.Subscribe((dto, props, token) => ProcessDeadLetterMessageAsync((T)dto, props, token));
            }
            else
            {
                _queueSubscriptionService.Subscribe((dto, props, token) => ProcessMessageAsync((T)dto, token));
            }
        }

        private async Task<IQueueCallbackResult> ProcessDeadLetterMessageAsync(T jobStatusDto, IDictionary<string, object> messageProperties, CancellationToken cancellationToken)
        {
            //if (thrownException is TimeoutException)
            //{
            //    await _jobStatus.JobFailedRecoverablyAsync(jobContextMessage.JobId);
            //}
            //else
            //{
            //    await _jobStatus.JobFailedIrrecoverablyAsync(jobContextMessage.JobId);
            //}
            try
            {
                return new QueueCallbackResult(true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to post dead letter job status message", ex);
                return new QueueCallbackResult(false, ex);
            }
        }

        private async Task<IQueueCallbackResult> ProcessMessageAsync(T jobStatusDto, CancellationToken cancellationToken)
        {
            try
            {
                await client.PostAsync($"{_endPointUrl}/Job/{jobStatusDto.JobId}/{jobStatusDto.JobStatus}", new StringContent(jobStatusDto.JobStatus.ToString(), Encoding.UTF8), cancellationToken);
                return new QueueCallbackResult(true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to post job status message", ex);
                return new QueueCallbackResult(false, ex);
            }
        }
    }
}
