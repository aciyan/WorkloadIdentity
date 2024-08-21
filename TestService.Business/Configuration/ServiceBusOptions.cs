
namespace TestService.Business.Configuration
{
    public class ServiceBusOptions
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the queue name.
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// Gets or sets the servicebus namespace connection string.
        /// </summary>
        public string NamespaceConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified namespace.
        /// </summary>
        public string FullyQualifiedNamespace { get; set; }

        #endregion Public Properties
    }
}
