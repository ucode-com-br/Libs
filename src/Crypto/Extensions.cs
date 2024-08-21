using System;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection;
using static UCode.Crypto.KeyVaultCrypto;

namespace UCode.Crypto
{
    public static class Extensions
    {
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
