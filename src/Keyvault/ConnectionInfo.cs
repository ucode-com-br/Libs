using System;

namespace UCode.Keyvault
{
    public class ConnectionInfo
    {

        public Uri Uri
        {
            get; set;
        }

        public bool IsManagedIdentity
        {
            get;
            set;
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
