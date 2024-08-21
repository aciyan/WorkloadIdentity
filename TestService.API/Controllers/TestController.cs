using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;
using TestService.Business.Abstraction;
using TestService.Business.Models;

namespace TestService.API.Controllers
{
    [ApiController]
    [Route("test")]
    public class TestController : ControllerBase
    {
        #region Private Members

        private readonly IManagedIdentityCosmosManager _managedIdentityCosmosManager;
        private readonly IManagedIdentityBlobManager _managedIdentityBlobManager;
        private readonly IManagedIdentityQueueManager _managedIdentityQueueManager;
        private readonly IManagedIdentityApimManager _managedIdentityAPIMManager;
        private readonly IManagedIdentityAdxManager _managedIdentityAdxManager;
        private readonly IApplicationInsightsWIManager _applicationInsightsWIManager;
        private readonly IWorkloadIdentityRedisManager _workloadIdentityRedisManager;
        private readonly IWorkloadIdentitySignalRManager _workloadIdentitySignalRManager;
        private readonly ILogger<TestController> _logger = Logger.CreateLogger(nameof(TestController));

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TestController"/> class.
        /// </summary>
        /// <param name="logger"></param>
        public TestController(ILogger<TestController> logger,
            IManagedIdentityCosmosManager managedIdentityTableManager,
            IManagedIdentityBlobManager managedIdentityBlobManager,
            IManagedIdentityQueueManager managedIdentityQueueManager,
            IManagedIdentityApimManager managedIdentityAPIMManager,
            IManagedIdentityAdxManager managedIdentityAdxManager,
            IApplicationInsightsWIManager applicationInsightsWIManager,
            IWorkloadIdentityRedisManager workloadIdentityRedisManager,
            IWorkloadIdentitySignalRManager workloadIdentitySignalRManager)
        {
            _managedIdentityCosmosManager = managedIdentityTableManager;
            _managedIdentityBlobManager = managedIdentityBlobManager;
            _managedIdentityQueueManager = managedIdentityQueueManager;
            _managedIdentityAPIMManager = managedIdentityAPIMManager;
            _managedIdentityAdxManager = managedIdentityAdxManager;
            _applicationInsightsWIManager = applicationInsightsWIManager;
            _workloadIdentityRedisManager = workloadIdentityRedisManager;
            _workloadIdentitySignalRManager = workloadIdentitySignalRManager;
        }

        #endregion Public Constructors

        #region Public Methods

        [HttpGet("ping")]
        public ActionResult Pong()
        {
            return Ok("Pong..");
        }

        /// <summary>
        /// Tests the Cosmos asynchronous.
        /// </summary>
        /// <param name="returnId">The returnId.</param>
        /// <returns></returns>
        [HttpGet("CosmosRead")]
        public async Task<List<TestDataModel>> CosmosRead([FromQuery]string returnId)
        {
            return await _managedIdentityCosmosManager.CosmosRead(returnId);
        }

        /// <summary>
        /// Tests the Cosmos asynchronous.
        /// </summary>
        /// <param name="taxDocument">The tax document.</param>
        /// <returns></returns>
        [HttpPost("CosmosWrite")]
        public async Task<string> CosmosWrite([FromBody] string taxDocument)
        {
            return await _managedIdentityCosmosManager.CosmosWrite(taxDocument);
        }

        /// <summary>
        /// Tests the Blob download asynchronously.
        /// </summary>
        /// <param name="blobName">The blob name.</param>
        /// <param name="containerName">The container name.</param>
        /// <returns></returns>
        [HttpGet("BlobDownload")]
        public async Task<byte[]> TestBlobDownload([FromQuery]string blobName, [FromQuery]string containerName)
        {
            return await _managedIdentityBlobManager.DownloadBlobAsync(blobName, containerName);
        }

        /// <summary>
        /// Tests the Blob upload asynchronously.
        /// </summary>
        /// <param name="blobName">The blob name.</param>
        /// <param name="containerName">The container name.</param>
        /// <returns></returns>
        [HttpPost("BlobUpload")]
        public async Task<bool> BlobUpload([FromQuery] string blobName,[FromQuery] string containerName,
            [FromQuery] string contentType = null,
            [FromQuery] IDictionary<string, string> metadata = null,
            [FromQuery] IDictionary<string, string> tags = null)
        {
            var mockData = System.IO.File.ReadAllText(@"SampleData.json");
            var contentByte = Encoding.ASCII.GetBytes(mockData);
            bool returnResult = await _managedIdentityBlobManager.UploadBlobAsync(blobName, containerName, contentByte, metadata, tags, contentType);
            return returnResult;
        }

        /// <summary>
        /// Tests the ASB queue connection asynchronously.
        /// </summary>
        /// <returns></returns>
        [HttpGet("PushToQueue")]
        public async Task<bool> PushToQueue()
        {
             await _managedIdentityQueueManager.SendMessageToQueue();
            return true;
        }

        /// <summary>
        /// Gets the client credentials.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAPIMToken")]
        public async Task<APIMResponse> GetApimToken()
        {
            return await _managedIdentityAPIMManager.AquireAccessTokenAsync();
        }

        /// <summary>
        /// Tests Adx connection
        /// </summary>
        /// <returns></returns>
        [HttpGet("TestAdx")]
        public async Task<List<string[]>> TestAdx()
        {
            return await _managedIdentityAdxManager.TestAdxConnection();
        }

        /// <summary>
        /// Log with Workload Identity
        /// </summary>
        /// <returns></returns>
        [HttpGet("TestLog")]
        public void TestLog()
        {
            _applicationInsightsWIManager.LogMessages();
        }

        /// <summary>
        /// Set Cache with Workload Identity
        /// </summary>
        /// <returns></returns>
        [HttpGet("SetCache")]
        public async Task<bool> SetCache(string key, string value)
        {
            return await _workloadIdentityRedisManager.SetCacheAsync(key, value);
        }

        /// <summary>
        /// Get Cache with Workload Identity
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetCache")]
        public async Task<string> GetCache(string key)
        {
            return await _workloadIdentityRedisManager.GetCacheAsync(key);
        }

        /// <summary>
        /// Test SignalR with Workload Identity
        /// </summary>
        /// <returns></returns>
        [HttpGet("TestSignalR")]
        public async Task TestSignalR(string message)
        {
            await _workloadIdentitySignalRManager.SendAsync(message);
        }

        #endregion Public Methods

        #region Private Methods

        #endregion Private Methods
    }
}