using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.JobContext;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Queueing.Interface;

namespace ESFA.DC.JobStatus.WebServiceCall.Service
{
    public sealed class FailedJobsWebServiceCallService : IJobStatusWebServiceCallService<JobContextDto>
    {
        private readonly IQueueSubscriptionService<JobContextDto> _queueSubscriptionService;

        private readonly ILogger _logger;

        private readonly string _endPointUrl;

        private readonly HttpClient client = new HttpClient();

        public FailedJobsWebServiceCallService(
            IJobStatusWebServiceCallServiceConfig jobStatusWebServiceCallServiceConfig,
            IQueueSubscriptionService<JobContextDto> queueSubscriptionService,
            ILogger logger)
        {
            _queueSubscriptionService = queueSubscriptionService;
            _logger = logger;
            _endPointUrl = jobStatusWebServiceCallServiceConfig.EndPointUrl;
            if (!_endPointUrl.EndsWith("/"))
            {
                _endPointUrl = _endPointUrl + "/";
            }
        }

        public void Subscribe()
        {
            _queueSubscriptionService.Subscribe(ProcessDeadLetterMessageAsync);
        }

        private async Task<IQueueCallbackResult> ProcessDeadLetterMessageAsync(JobContextDto jobContextDto, IDictionary<string, object> messageProperties, CancellationToken cancellationToken)
        {
            bool irrecoverable = false;
            if (messageProperties.TryGetValue("Exceptions", out object exceptions))
            {
                string[] exceptionList = exceptions.ToString().Split(':');
                if (exceptionList.Contains("NullReferenceException"))
                {
                    irrecoverable = true;
                }
            }

            if (irrecoverable)
            {
                await client.PostAsync(
                    $"{_endPointUrl}/Job/{jobContextDto.JobId}/{JobStatusType.Failed}",
                    new StringContent(JobStatusType.Failed.ToString(), Encoding.UTF8),
                    cancellationToken);
            }
            else
            {
                await client.PostAsync(
                    $"{_endPointUrl}/Job/{jobContextDto.JobId}/{JobStatusType.FailedRetry}",
                    new StringContent(JobStatusType.FailedRetry.ToString(), Encoding.UTF8),
                    cancellationToken);
            }

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
    }
}
