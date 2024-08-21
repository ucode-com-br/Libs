using System;

namespace UCode.Keyvault
{
    public class KeyVaultOptions
    {
        public KeyVaultOptions(Uri uri, bool isManagedIdentity = true)
        {
            this.Connection.Uri = uri;
            this.Connection.IsManagedIdentity = isManagedIdentity;
        }

        public KeyVaultOptions(Uri uri, string tenantId, string clientId, string clientSecret) : this(uri, false)
        {
            this.Connection ??= new ConnectionInfo();

            this.Connection.TenantId = tenantId;
            this.Connection.ClientId = clientId;
            this.Connection.ClientSecret = clientSecret;
        }



        public ConnectionInfo Connection
        {
            get; set;
        }

    }
}
