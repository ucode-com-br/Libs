using System;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Azure.AppConfiguration
{
    public static class Extensions
    {
        /// <summary>
        /// Add Azure App Configuration to the configuration builder
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="hostEnvironment"></param>
        /// <param name="options"></param>
        public static void AddAppConfiguration(this IConfigurationBuilder builder, IHostEnvironment hostEnvironment, Action<AppConfigurationOptions>? options = null)
        {
            var conn = builder.Build();

            var appConfigUrl = conn["AppConfigurationUri"];

            ArgumentException.ThrowIfNullOrEmpty(appConfigUrl);

            var appConfigurationOptions = new AppConfigurationOptions(new Uri(appConfigUrl));

            if (hostEnvironment.IsDevelopment())
            {
                var tenantId = conn["AppConfiguration:TenantId"];
                var clientId = conn["AppConfiguration:ClientId"];
                var clientSecret = conn["AppConfiguration:ClientSecret"];

                if (!(string.IsNullOrEmpty(tenantId) && string.IsNullOrEmpty(clientId) && string.IsNullOrEmpty(clientSecret)))
                {
                    appConfigurationOptions = new AppConfigurationOptions(new Uri(appConfigUrl), tenantId, clientId, clientSecret);
                }

            }

            options?.Invoke(appConfigurationOptions);


            TokenCredential credential = new DefaultAzureCredential(false);

            if (!string.IsNullOrEmpty(appConfigurationOptions.Connection.TenantId) || !string.IsNullOrEmpty(appConfigurationOptions.Connection.ClientId) || !string.IsNullOrEmpty(appConfigurationOptions.Connection.ClientSecret))
            {
                ArgumentException.ThrowIfNullOrEmpty(appConfigurationOptions.Connection.TenantId);
                ArgumentException.ThrowIfNullOrEmpty(appConfigurationOptions.Connection.ClientId);
                ArgumentException.ThrowIfNullOrEmpty(appConfigurationOptions.Connection.ClientSecret);

                credential = new ClientSecretCredential(appConfigurationOptions.Connection.TenantId, appConfigurationOptions.Connection.ClientId, appConfigurationOptions.Connection.ClientSecret);

                builder.AddAzureAppConfiguration(option => option.Connect(appConfigurationOptions.Connection.Uri, credential));
            }
            else if (!string.IsNullOrWhiteSpace(appConfigurationOptions.Connection.ConnectionString))
            {
                builder.AddAzureAppConfiguration(option => option.Connect(appConfigurationOptions.Connection.ConnectionString));
            }
            else if (appConfigurationOptions.Connection.IsManagedIdentity)
            {
                if (string.IsNullOrWhiteSpace(appConfigurationOptions.Connection.ClientId))
                {

                    builder.AddAzureAppConfiguration(option => option.Connect(appConfigurationOptions.Connection.Uri, new ManagedIdentityCredential()));
                }
                else
                {
                    builder.AddAzureAppConfiguration(option => option.Connect(appConfigurationOptions.Connection.Uri, new ManagedIdentityCredential(appConfigurationOptions.Connection.ClientId)));
                }
            }
            else
            {
                builder.AddAzureAppConfiguration(option => option.Connect(appConfigurationOptions.Connection.Uri, credential));
            }


        }
    }
}
