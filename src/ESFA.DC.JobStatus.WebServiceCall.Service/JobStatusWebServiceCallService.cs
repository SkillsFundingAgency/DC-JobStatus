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

        public void Subscribe()
        {
            _queueSubscriptionService.Subscribe((dto, token) => ProcessMessageAsync((T)dto, token));
        }

        private async Task<bool> ProcessMessageAsync(T jobStatusDto, CancellationToken cancellationToken)
        {
            try
            {
                await client.PostAsync($"{_endPointUrl}/Job/{jobStatusDto.JobId}/{jobStatusDto.JobStatus}", new StringContent(jobStatusDto.JobStatus.ToString(), Encoding.UTF8), cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to post job status message", ex);
                return false;
            }
        }
    }
}
