using System;

namespace Azure.AppConfiguration
{
    public class ConnectionInfo
    {
        public Uri Uri
        {
            get; set;
        }

        public bool IsManagedIdentity
        {

            get; set;
        }

        public string? ConnectionString
        {
            get; set;
        }

        public string? TenantId
        {
            get; set;
        }
        public string? ClientId
        {
            get; set;
        }
        public string? ClientSecret
        {
            get; set;
        }
    }
}
