using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestService.Business.Configuration
{
    /// <summary>
    /// Represents the configs required for Managed Identity
    /// </summary>
    public class ManagedIdentitySettings
    {
        #region Public Properties

        /// <summary>
        /// The Tenant ID of the user-assigned managed identity
        /// </summary>
        /// <value>The Tenant ID.</value>
        public string TenantId { get; set; }

        /// <summary>
        /// User-assigned managed identity client ID (from Azure portal)
        /// </summary>
        /// <value>The MI CLient ID.</value>
        public string ClientId { get; set; }

        /// <summary>
        /// The object ID of the user-assigned managed identity
        /// </summary>
        /// <value>The Object ID.</value>
        public string ObjectId { get; set; }

        #endregion
    }
}
