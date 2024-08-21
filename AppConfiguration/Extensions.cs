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
            // Build the configuration
            var conn = builder.Build();

            // Get the AppConfigurationUri from the configuration
            var appConfigUrl = conn["AppConfigurationUri"];

            // Throw an exception if the AppConfigurationUri is null or empty
            ArgumentException.ThrowIfNullOrEmpty(appConfigUrl);

            // Create a new AppConfigurationOptions object with the AppConfigurationUri
            var appConfigurationOptions = new AppConfigurationOptions(new Uri(appConfigUrl));

            // If in development environment, check if tenantId, clientId, and clientSecret are present in the configuration
            if (hostEnvironment.IsDevelopment())
            {
                var tenantId = conn["AppConfiguration:TenantId"];
                var clientId = conn["AppConfiguration:ClientId"];
                var clientSecret = conn["AppConfiguration:ClientSecret"];

                if (!(string.IsNullOrEmpty(tenantId) && string.IsNullOrEmpty(clientId) && string.IsNullOrEmpty(clientSecret)))
                {
                    // If present, create a new AppConfigurationOptions object with the AppConfigurationUri and credentials
                    appConfigurationOptions = new AppConfigurationOptions(new Uri(appConfigUrl), tenantId, clientId, clientSecret);
                }

            }
            // Invoke the optional configuration options action
            options?.Invoke(appConfigurationOptions);

            // Create a new default Azure credential
            TokenCredential credential = new DefaultAzureCredential(false);

            // Check if tenantId, clientId, and clientSecret are present in the AppConfigurationOptions
            if (!string.IsNullOrEmpty(appConfigurationOptions.Connection.TenantId) || !string.IsNullOrEmpty(appConfigurationOptions.Connection.ClientId) || !string.IsNullOrEmpty(appConfigurationOptions.Connection.ClientSecret))
            {
                // Throw an exception if any of the credentials are null or empty
                ArgumentException.ThrowIfNullOrEmpty(appConfigurationOptions.Connection.TenantId);
                ArgumentException.ThrowIfNullOrEmpty(appConfigurationOptions.Connection.ClientId);
                ArgumentException.ThrowIfNullOrEmpty(appConfigurationOptions.Connection.ClientSecret);

                // Create a new client secret credential using the credentials from AppConfigurationOptions
                credential = new ClientSecretCredential(appConfigurationOptions.Connection.TenantId, appConfigurationOptions.Connection.ClientId, appConfigurationOptions.Connection.ClientSecret);

                // Connect to Azure App Configuration using the URI and credential
                builder.AddAzureAppConfiguration(option => option.Connect(appConfigurationOptions.Connection.Uri, credential));
            }
            else if (!string.IsNullOrWhiteSpace(appConfigurationOptions.Connection.ConnectionString))
            {
                // Connect to Azure App Configuration using the connection string from AppConfigurationOptions
                builder.AddAzureAppConfiguration(option => option.Connect(appConfigurationOptions.Connection.ConnectionString));
            }
            else if (appConfigurationOptions.Connection.IsManagedIdentity)
            {
                // If using managed identity, check if clientId is present in AppConfigurationOptions
                if (string.IsNullOrWhiteSpace(appConfigurationOptions.Connection.ClientId))
                {
                    // Connect to Azure App Configuration using the URI and default managed identity credential
                    builder.AddAzureAppConfiguration(option => option.Connect(appConfigurationOptions.Connection.Uri, new ManagedIdentityCredential()));
                }
                else
                {
                    // Connect to Azure App Configuration using the URI and managed identity credential with the specified clientId
                    builder.AddAzureAppConfiguration(option => option.Connect(appConfigurationOptions.Connection.Uri, new ManagedIdentityCredential(appConfigurationOptions.Connection.ClientId)));
                }
            }
            else
            {
                // Connect to Azure App Configuration
                builder.AddAzureAppConfiguration(option => option.Connect(appConfigurationOptions.Connection.Uri, credential));
            }
        }
    }
}
