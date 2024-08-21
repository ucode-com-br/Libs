using System;

namespace Azure.AppConfiguration
{
    public class AppConfigurationOptions
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="AppConfigurationOptions"/> class with a connection string.
        /// </summary>
        /// <param name="connectionstring">The connection string.</param>
        public AppConfigurationOptions(string connectionstring)
        {
            this.Connection ??= new ConnectionInfo();

            this.Connection.IsManagedIdentity = false;
            this.Connection.ConnectionString = connectionstring;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppConfigurationOptions"/> class with a URI and a flag indicating whether managed identity is used.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="isManagedIdentity">A flag indicating whether managed identity is used. Defaults to true.</param>
        public AppConfigurationOptions(Uri uri, bool isManagedIdentity = true)
        {
            this.Connection ??= new ConnectionInfo();

            this.Connection.Uri = uri;
            this.Connection.IsManagedIdentity = isManagedIdentity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppConfigurationOptions"/> class with a URI, tenant ID, client ID, and client secret.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="clientId">The client ID.</param>
        /// <param name="clientSecret">The client secret.</param>
        public AppConfigurationOptions(Uri uri, string? tenantId, string? clientId, string? clientSecret) : this(uri, false)
        {
            this.Connection ??= new ConnectionInfo();

            this.Connection.TenantId = tenantId;
            this.Connection.ClientId = clientId;
            this.Connection.ClientSecret = clientSecret;
        }

        /// <summary>
        /// Gets or sets the connection information.
        /// </summary>
        public ConnectionInfo Connection
        {
            get;
            private set;
        }
    }
}
