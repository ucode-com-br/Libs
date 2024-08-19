using System;

namespace Azure.AppConfiguration
{
    public class AppConfigurationOptions
    {


        public AppConfigurationOptions(string connectionstring)
        {
            this.Connection ??= new ConnectionInfo();

            this.Connection.IsManagedIdentity = false;
            this.Connection.ConnectionString = connectionstring;
        }

        public AppConfigurationOptions(Uri uri, bool isManagedIdentity = true)
        {
            this.Connection ??= new ConnectionInfo();

            this.Connection.Uri = uri;
            this.Connection.IsManagedIdentity = isManagedIdentity;
        }

        public AppConfigurationOptions(Uri uri, string? tenantId, string? clientId, string? clientSecret) : this(uri, false)
        {
            this.Connection ??= new ConnectionInfo();

            this.Connection.TenantId = tenantId;
            this.Connection.ClientId = clientId;
            this.Connection.ClientSecret = clientSecret;
        }


        public ConnectionInfo Connection
        {
            get;
            private set;
        }
    }
}
