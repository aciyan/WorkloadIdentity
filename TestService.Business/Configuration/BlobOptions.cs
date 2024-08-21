using Azure.Storage.Blobs;

namespace TestService.Business.Configuration
{
    /// <summary>
    /// Represents the CosmosDB configuration options
    /// </summary>
    public class BlobOptions
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets connection string for the Blob storage
        /// </summary>
        /// <value>The Connection String</value>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the blob storage Uri.
        /// </summary>
        /// <value>
        /// The blob storage connection URI.</value>
        public string ConnectionUri { get; set; }

        /// <summary>
        /// Gets the retry.
        /// </summary>
        public BlobClientOptions ClientOptions { get; set; } = new BlobClientOptions();

        #endregion Public Properties
    }
}
