using Microsoft.Azure.Cosmos;

namespace TestService.Business.Configuration
{
    /// <summary>
    /// Represents the CosmosDB configuration options
    /// </summary>
    public class CosmosDbOptions
    {
        #region Public Properties

        /// <summary>
        /// Set the Unique Connection string for Cosmos DB
        /// </summary>
        /// <value></value>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the cosmos database connection Uri.
        /// </summary>
        /// <value>
        /// The cosmos connection URI.
        /// </value>
        public string ConnectionUri { get; set; }

        /// <summary>
        /// Gets or Sets the Cosmos Client Options
        /// </summary>
        /// <value>The client options</value>
        public CosmosClientOptions ClientOptions { get; set; } = new CosmosClientOptions();

        /// <summary>
        /// Gets or sets the cosmos database name.
        /// </summary>
        /// <value>
        /// The database name.</value>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets the cosmos container name.
        /// </summary>
        /// <value>
        /// The container.</value>
        public string Container { get; set; }

        #endregion Public Properties
    }
}
