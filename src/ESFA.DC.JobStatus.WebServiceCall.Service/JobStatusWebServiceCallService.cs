using System;
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
        private readonly IQueueSubscriptionService<T> _queueSubscriptionService;

        private readonly ILogger _logger;

        private readonly string _endPointUrl;

        private readonly HttpClient client = new HttpClient();

        public JobStatusWebServiceCallService(
            IJobStatusWebServiceCallServiceConfig jobStatusWebServiceCallServiceConfig, IQueueSubscriptionService<T> queueSubscriptionService, ILogger logger)
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
            _queueSubscriptionService.Subscribe((dto, props, token) => ProcessMessageAsync(dto, token));
        }

        private async Task<IQueueCallbackResult> ProcessMessageAsync(T jobStatusDto, CancellationToken cancellationToken)
        {
            try
            {
                if (jobStatusDto.NumberOfLearners == -1)
                {
                    await client.PostAsync(
                        $"{_endPointUrl}/Job/{jobStatusDto.JobId}/{jobStatusDto.JobStatus}",
                        new StringContent(jobStatusDto.JobStatus.ToString(), Encoding.UTF8),
                        cancellationToken);
                }

                // Todo: Post the status + number of learners

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
