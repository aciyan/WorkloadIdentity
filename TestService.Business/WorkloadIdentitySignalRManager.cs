using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Logging;
using TestService.Business.Abstraction;

namespace TestService.Business
{
    public class WorkloadIdentitySignalRManager : IWorkloadIdentitySignalRManager
    {
        #region Private Members

        private readonly ServiceHubContext _serviceHubContext;
        private readonly ILogger _logger = Logger.CreateLogger(nameof(WorkloadIdentitySignalRManager));

        #endregion

        #region Constructor
        public WorkloadIdentitySignalRManager(ServiceHubContext serviceHubContext)
        { 
            _serviceHubContext = serviceHubContext;
        }
        #endregion

        #region Implementing Public Methods
        public async Task SendAsync(string message)
        {
            try
            {
                _logger.LogDebug("TestService: SignalR - SendAsyc Started");
                await _serviceHubContext.Clients.All.SendAsync("ReceiveMessage: ", message);
                _logger.LogDebug("TestService: SignalR - SendAsyc Ended");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestService: SignalR - SendAsyc threw exception");
                throw;
            }
        }
        #endregion
    }
}
