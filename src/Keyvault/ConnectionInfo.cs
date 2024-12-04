using System;

namespace UCode.Keyvault
{
    /// <summary>
    /// Represents connection information for a database or network connection.
    /// </summary>
    /// <remarks>
    /// This class may include properties such as connection string, timeout, 
    /// and other configuration settings relevant to establishing a connection.
    /// </remarks>
    public class ConnectionInfo
    {

        /// <summary>
        /// Gets or sets the URI associated with this instance.
        /// </summary>
        /// <value>
        /// A <see cref="Uri"/> that represents the resource's uniform resource identifier.
        /// </value>
        public Uri Uri
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the identity is managed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the identity is managed; otherwise, <c>false</c>.
        /// </value>
        public bool IsManagedIdentity
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the identifier for the tenant.
        /// This property is optional and can be null.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the tenant's identifier. 
        /// Returns null if the tenant identifier is not set.
        /// </value>
        public string? TenantId
        {
            get; set;
        }
        /// <summary>
        /// Gets or sets the client identifier.
        /// This property may be null, indicating that the client ID has not been assigned.
        /// </summary>
        /// <value>
        /// A string representing the client identifier. This can be null.
        /// </value>
        public string? ClientId
        {
            get; set;
        }
        /// <summary>
        /// Gets or sets the client secret.
        /// This property is nullable and may contain a string value that represents a secret key
        /// associated with a client in an application.
        /// </summary>
        /// <value>
        /// A string representing the client secret, or null if it is not set.
        /// </value>
        public string? ClientSecret
        {
            get; set;
        }
    }
}
