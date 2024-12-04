using System;

namespace UCode.Keyvault
{
    /// <summary>
    /// Represents the configuration options for accessing a Key Vault.
    /// </summary>
    /// <remarks>
    /// This class is typically used to hold settings such as the Key Vault URL,
    /// client ID, and client secret required for authentication and access to 
    /// Azure Key Vault resources.
    /// </remarks>
    public class KeyVaultOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyVaultOptions"/> class.
        /// </summary>
        /// <param name="uri">The URI of the Key Vault to connect to.</param>
        /// <param name="isManagedIdentity">A boolean value indicating whether to use managed identity for authentication. Default is true.</param>
        public KeyVaultOptions(Uri uri, bool isManagedIdentity = true)
        {
            this.Connection.Uri = uri;
            this.Connection.IsManagedIdentity = isManagedIdentity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyVaultOptions"/> class 
        /// using the specified URI, tenant ID, client ID, and client secret.
        /// </summary>
        /// <param name="uri">The URI of the Key Vault.</param>
        /// <param name="tenantId">The tenant ID used for authentication.</param>
        /// <param name="clientId">The client ID used for authentication.</param>
        /// <param name="clientSecret">The client secret used for authentication.</param>
        /// <returns>
        /// A new instance of the <see cref="KeyVaultOptions"/> class 
        /// configured with the provided details.
        /// </returns>
        public KeyVaultOptions(Uri uri, string tenantId, string clientId, string clientSecret) : this(uri, false)
        {
            this.Connection ??= new ConnectionInfo();

            this.Connection.TenantId = tenantId;
            this.Connection.ClientId = clientId;
            this.Connection.ClientSecret = clientSecret;
        }



        /// <summary>
        /// Gets or sets the connection information for the application.
        /// </summary>
        /// <value>
        /// A <see cref="ConnectionInfo"/> object that contains the connection details.
        /// </value>
        public ConnectionInfo Connection
        {
            get; set;
        }

    }
}
