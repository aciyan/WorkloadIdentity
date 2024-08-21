using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestService.Business.Abstraction;

namespace TestService.Business
{
    public  class ApplicationInsightsWIManager : IApplicationInsightsWIManager
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<ApplicationInsightsWIManager> _logger;
        public ApplicationInsightsWIManager(IServiceProvider serviceProvider)
        {
           _telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();  
           _logger = serviceProvider.GetRequiredService<ILogger<ApplicationInsightsWIManager>>();
        }

        public void LogMessages() 
        {
            try
            {
                // Log an information message
                _logger.LogInformation("Info: Application starting up.");
                _logger.LogWarning("Warning: Application starting up.");
                _logger.LogDebug("Debug: Application starting up.");
                _logger.LogCritical("Critical: Application starting up.");
                _telemetryClient.TrackEvent("Test Service Event");
                // Simulate application logic and log an error
                throw new InvalidOperationException("This is a test exception");
            }
            catch (Exception ex)
            {
                // Log the error and send the exception telemetry
                _logger.LogError(ex, "An error occurred");
                _telemetryClient.TrackException(ex);
            }
            finally 
            {
                _telemetryClient.Flush();
            }
        }
    }
}
