using System;
using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace UCode.Keyvault
{

    /// <summary>
    /// Keyvault automatic extensions for aspnet core
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="hostEnvironment"></param>
        /// <param name="options"></param>
        public static void AddAzureKeyVaultDependencies(this IConfigurationBuilder builder, IHostEnvironment hostEnvironment, Action<KeyVaultOptions>? options = null)
        {
            var conn = builder.Build();

            var kvUrl = conn["KeyVaultUri"];

            ArgumentException.ThrowIfNullOrEmpty(kvUrl);

            var keyVaultOptions = new KeyVaultOptions(new Uri(kvUrl));

            if (hostEnvironment.IsDevelopment())
            {
                var tenantId = conn["KeyVault:TenantId"];
                var clientId = conn["KeyVault:ClientId"];
                var clientSecret = conn["KeyVault:ClientSecret"];

                if (!(string.IsNullOrEmpty(tenantId) && string.IsNullOrEmpty(clientId) && string.IsNullOrEmpty(clientSecret)))
                {
                    keyVaultOptions = new KeyVaultOptions(new Uri(kvUrl), tenantId, clientId, clientSecret);
                }

            }

            options?.Invoke(keyVaultOptions);

            TokenCredential credential = new DefaultAzureCredential(false);

            if (!string.IsNullOrEmpty(keyVaultOptions.Connection.TenantId) || !string.IsNullOrEmpty(keyVaultOptions.Connection.ClientId) || !string.IsNullOrEmpty(keyVaultOptions.Connection.ClientSecret))
            {
                ArgumentException.ThrowIfNullOrEmpty(keyVaultOptions.Connection.TenantId);
                ArgumentException.ThrowIfNullOrEmpty(keyVaultOptions.Connection.ClientId);
                ArgumentException.ThrowIfNullOrEmpty(keyVaultOptions.Connection.ClientSecret);

                credential = new ClientSecretCredential(keyVaultOptions.Connection.TenantId, keyVaultOptions.Connection.ClientId, keyVaultOptions.Connection.ClientSecret);
            }
            else if (keyVaultOptions.Connection.IsManagedIdentity)
            {
                if (!string.IsNullOrWhiteSpace(keyVaultOptions.Connection.ClientId))
                {
                    credential = new ManagedIdentityCredential(keyVaultOptions.Connection.ClientId);
                }
                else
                {
                    credential = new ManagedIdentityCredential();
                }
            }

            var client = new SecretClient(new Uri(kvUrl), credential);



            builder.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());
        }
    }
}
