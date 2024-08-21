using System;

namespace Azure.AppConfiguration
{
    public class ConnectionInfo
    {
        /// <summary>
        /// Gets or sets the URI of the Azure App Configuration.
        /// </summary>
        public Uri Uri
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a flag indicating whether managed identity is used for authentication.
        /// </summary>
        public bool IsManagedIdentity
        {

            get; set;
        }

        /// <summary>
        /// Gets or sets the connection string for the Azure App Configuration.
        /// </summary>
        public string? ConnectionString
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the tenant ID for managed identity authentication.
        /// </summary>
        public string? TenantId
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the client ID for managed identity authentication.
        /// </summary>
        public string? ClientId
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the client secret for managed identity authentication.
        /// </summary>
        public string? ClientSecret
        {
            get; set;
        }
    }
}
