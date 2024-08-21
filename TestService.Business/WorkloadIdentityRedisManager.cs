using Azure.Identity;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TestService.Business.Abstraction;
using TestService.Business.Configuration;

namespace TestService.Business
{
    public class WorkloadIdentityRedisManager : IWorkloadIdentityRedisManager
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private IDatabase _redisCache;
        private readonly WorkloadIdentitySettings _workloadIdentitySettings;
        private readonly ManagedIdentityCredential _managedIdentityCredential;
        private readonly string _redisConnectionString;
        private readonly ILogger _logger = Logger.CreateLogger(nameof(WorkloadIdentityRedisManager));

        public WorkloadIdentityRedisManager(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _redisCache = _connectionMultiplexer.GetDatabase();
        }

        public WorkloadIdentityRedisManager(WorkloadIdentitySettings workloadIdentitySettings,
            ManagedIdentityCredential managedIdentityCredential,
            string redisConnectionString)
        {
            _workloadIdentitySettings = workloadIdentitySettings;
            _managedIdentityCredential = managedIdentityCredential;
            _redisConnectionString = redisConnectionString;
        }

        public async Task<string> GetCacheAsync(string key)
        {
            _logger.LogDebug("TestService: Redis - GetCacheAsync Started");
            try
            {
                await ConnectAsync();
                var result = await _redisCache.StringGetAsync(key);
                _logger.LogInformation($"GetCacheAsync: {result}");
                _logger.LogDebug("TestService: Redis - GetCacheAsync Ended");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestService: Redis - GetCacheAsync Failed");
                return null;
            }
        }

        public async Task<bool> SetCacheAsync(string key, string data)
        {
            try
            {
                _logger.LogDebug("TestService: Redis - SetCacheAsync Started");
                await ConnectAsync();
                var result = await _redisCache.StringSetAsync(key, data);
                _logger.LogInformation($"SetCacheAsync: {result}");
                _logger.LogDebug("TestService: Redis - SetCacheAsync Ended");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestService: Redis - SetCacheAsync Failed");
                return false;
            }
        }

        private async Task ConnectAsync()
        {
            try
            {
                _logger.LogDebug("TestService: Redis - ConnectAsync Started");
                StringWriter connectionLog = new();
                var configurationOptions = await ConfigurationOptions.Parse(_redisConnectionString).ConfigureForAzureWithTokenCredentialAsync(_managedIdentityCredential);
                configurationOptions.AbortOnConnectFail = false; // Fail fast for the purposes of this sample. In production code, this should remain false to retry connections on startup
                configurationOptions.Ssl = true;
                var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(configurationOptions, connectionLog);
                _redisCache = connectionMultiplexer.GetDatabase();
                _logger.LogInformation(connectionLog.ToString());
                _logger.LogInformation($"Connection to Redis Cache established: {_redisCache.Database}");
                _logger.LogDebug("TestService: Redis - ConnectAsync Ended");

            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "TestService: Redis - ConnectAsync Failed");
            }
        }
    }
}
