using System;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection;
using static UCode.Crypto.KeyVaultCrypto;

namespace UCode.Crypto
{
    /// <summary>
    /// Provides extension methods for various types.
    /// </summary>
    /// <remarks>
    /// This static class contains extension methods that can be used to add functionality 
    /// to existing types without modifying their definition. Extension methods can be 
    /// called as if they were instance methods on the extended type.
    /// </remarks>
    public static class Extensions
    {
        /// <summary>
        /// Registers a strong reversible cryptographic service in the provided service collection.
        /// </summary>
        /// <param name="services">The service collection to which the cryptographic service will be added.</param>
        /// <param name="keyName">The name of the key to be used for the cryptographic operations.</param>
        /// <param name="vaultParameters">An optional action to configure the vault parameters.</param>
        /// <returns>The updated service collection with the added cryptographic service.</returns>
        public static IServiceCollection AddStrongReversibleCrypto(this IServiceCollection services, string keyName, Action<VaultParameters> vaultParameters = null)
        {
            if (string.IsNullOrWhiteSpace(keyName))
            {
                throw new ArgumentNullException(nameof(keyName));
            }

            KeyVaultCrypto keyVaultCrypto;

            if (vaultParameters == null)
            {
                keyVaultCrypto = CreateManaged(keyName);
            }
            else
            {
                var vp = new VaultParameters(Environment.GetEnvironmentVariable("VaultUri"));
                vaultParameters(vp);
                keyVaultCrypto = new KeyVaultCrypto(keyName, vp);
            }

            services.AddSingleton(keyVaultCrypto);

            return services;
        }

        /// <summary>
        /// Adds a reversible cryptography service to the provided service collection.
        /// </summary>
        /// <param name="services">The service collection to which the reversible cryptography service will be added.</param>
        /// <param name="password">The password used for the reversible cryptography operations. Must not be null or whitespace.</param>
        /// <returns>The updated service collection with the reversible cryptography service registered.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the password is null or consists only of whitespace.</exception>
        public static IServiceCollection AddReversibleCrypto(this IServiceCollection services, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            var reversibleCrypto = new ReversibleCrypto(password);

            services.AddSingleton(reversibleCrypto);

            return services;
        }

        /// <summary>
        /// Adds reversible cryptography services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to which the reversible cryptography services will be added.</param>
        /// <param name="keyvaultSecretName">The name of the secret in Azure Key Vault that contains the cryptographic key.</param>
        /// <param name="vaultParameters">
        /// An optional action that allows the caller to configure <see cref="VaultParameters" /> used to retrieve the key from Azure Key Vault.
        /// </param>
        /// <returns>
        /// The <see cref="IServiceCollection" /> with reversible cryptography services added.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="keyvaultSecretName"/> is null or whitespace.
        /// </exception>
        public static IServiceCollection AddReversibleCrypto(this IServiceCollection services, string keyvaultSecretName, Action<VaultParameters> vaultParameters = null)
        {
            if (string.IsNullOrWhiteSpace(keyvaultSecretName))
            {
                throw new ArgumentNullException(nameof(keyvaultSecretName));
            }

            var vp = new VaultParameters(Environment.GetEnvironmentVariable("VaultUri"));

            vaultParameters(vp);

            TokenCredential _credential;

            if (string.IsNullOrWhiteSpace(vp.TenantId) || string.IsNullOrWhiteSpace(vp.ClientId) || string.IsNullOrWhiteSpace(vp.ClientSecret))
            {
                _credential = new DefaultAzureCredential();
            }
            else
            {
                _credential = new ClientSecretCredential(vp.TenantId, vp.ClientId, vp.ClientSecret);
            }

            var _client = new SecretClient(vp.Uri, _credential);

            var reversibleCrypto = new ReversibleCrypto(_client.GetSecret(keyvaultSecretName).Value.Value);

            services.AddSingleton(reversibleCrypto);

            return services;
        }
    }
}
