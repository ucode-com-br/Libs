using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;

namespace UCode.Crypto
{
    public class KeyVaultCrypto
    {
        public enum Key
        {
            RSA2048, RSA3072, RSA4096,
            P256, P384, P521, P256K
        }

        public class VaultParameters
        {
            public VaultParameters(string defaultVaultUri, string tenantId = null, string clientId = null, string clientSecret = null)
            {
                this.Uri = new Uri(defaultVaultUri);
                this.TenantId = tenantId;
                this.ClientId = clientId;
                this.ClientSecret = clientSecret;
            }

            public VaultParameters(Uri uri, string tenantId = null, string clientId = null, string clientSecret = null)
            {
                this.Uri = uri;
                this.TenantId = tenantId;
                this.ClientId = clientId;
                this.ClientSecret = clientSecret;
            }

            public Uri Uri
            {
                get; set;
            }
            public string TenantId
            {
                get; set;
            }
            public string ClientId
            {
                get; set;
            }
            public string ClientSecret
            {
                get; set;
            }
        }

        private readonly KeyClient _client;
        private readonly TokenCredential _credential;
        private readonly string _cryptoKeyName;

        private readonly Task<Response<KeyVaultKey>> _responseKeyVaultKeyTask;
        private KeyVaultKey KeyVaultKey
        {
            get
            {
                if (!this._responseKeyVaultKeyTask.IsCompleted)
                {
                    this._responseKeyVaultKeyTask.Wait();
                }

                return this._responseKeyVaultKeyTask.Result;
            }
        }

        private CryptographyClient _cryptographyClient;
        private CryptographyClient CryptographyClient
        {
            get
            {
                this._cryptographyClient ??= new CryptographyClient(this.KeyVaultKey.Id, this._credential);
                return this._cryptographyClient;
            }
        }

        public EncryptionAlgorithm EncryptionAlgorithm { get; set; } = EncryptionAlgorithm.RsaOaep;

        internal KeyVaultCrypto(string cryptoKeyName, VaultParameters vaultParameters)
        {
            this._cryptoKeyName = cryptoKeyName;

            if (string.IsNullOrWhiteSpace(vaultParameters.TenantId) || string.IsNullOrWhiteSpace(vaultParameters.ClientId) || string.IsNullOrWhiteSpace(vaultParameters.ClientSecret))
            {
                this._credential = new DefaultAzureCredential();
            }
            else
            {
                this._credential = new ClientSecretCredential(vaultParameters.TenantId, vaultParameters.ClientId, vaultParameters.ClientSecret);
            }

            this._client = new KeyClient(vaultParameters.Uri, this._credential);

            this._responseKeyVaultKeyTask = Task.Run(async () =>
            {
                try
                {
                    var list = new List<KeyProperties>();

                    foreach (var page in this._client.GetPropertiesOfKeys())
                    {
                        if (page.Enabled.HasValue && !page.Enabled.Value)
                        {
                            continue;
                        }

                        if (page.ExpiresOn != null && page.ExpiresOn.Value < DateTimeOffset.UtcNow)
                        {
                            continue;
                        }

                        if (page.NotBefore != null && page.NotBefore.Value > DateTimeOffset.UtcNow)
                        {
                            continue;
                        }

                        if (page.Name.Equals(cryptoKeyName, StringComparison.Ordinal))
                        {
                            list.Add(page);
                        }
                    }

                    if (list.Count == 0)
                    {
                        await CreateKeyAsync(vaultParameters, cryptoKeyName, Key.RSA2048, null, null, true, false);
                    }
                    else
                    {
                        var last = list.OrderBy(o => o.CreatedOn).OrderBy(o => o.UpdatedOn).OrderBy(o => o.Version).LastOrDefault();

                        if (last == null)
                        {
                            await CreateKeyAsync(vaultParameters, cryptoKeyName, Key.RSA2048, null, null, true, false);
                        }
                    }

                    var response = await this._client.GetKeyAsync(this._cryptoKeyName);

                    if (response.GetRawResponse().Status == 200)
                    {
                        return response;
                    }
                    else
                    {
                        await CreateKeyAsync(vaultParameters, cryptoKeyName, Key.RSA2048, null, null, true, false);

                        return await this._client.GetKeyAsync(this._cryptoKeyName);
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            });
        }




        public async Task<byte[]> SignAsync(byte[] data) => (await this.CryptographyClient.SignDataAsync(SignatureAlgorithm.RS512, data)).Signature;


        public async Task<bool> VerifyAsync(byte[] data, byte[] signature) => (await this.CryptographyClient.VerifyDataAsync(SignatureAlgorithm.RS512, data, signature)).IsValid;


        public async Task<byte[]> EncryptAsync(byte[] source) => (await this.CryptographyClient.EncryptAsync(this.EncryptionAlgorithm, source)).Ciphertext;

        public async Task<byte[]> DecryptAsync(byte[] cipher) => (await this.CryptographyClient.DecryptAsync(this.EncryptionAlgorithm, cipher)).Plaintext;




        public static KeyVaultCrypto Create(string cryptoKeyName, string defaultVaultUri, string tenantId, string clientId, string clientSecret) => new KeyVaultCrypto(cryptoKeyName, new VaultParameters(defaultVaultUri, tenantId, clientId, clientSecret));

        public static KeyVaultCrypto CreateManaged(string cryptoKeyName) => new KeyVaultCrypto(cryptoKeyName, new VaultParameters(Environment.GetEnvironmentVariable("VaultUri")));


        /// <summary>
        /// Creates and stores a new crypto key in Key Vault. If the named key already exists,
        /// Azure Key Vault creates a new version of the key. This operation requires the keys/create permission.
        /// </summary>
        /// <param name="key">Set the key type and size in bits, such as 2048, 3072, or 4096. If null, the service default is used.</param>
        /// <param name="name">The name of the key to create.</param>
        /// <param name="expiresOn">Set a System.DateTimeOffset indicating when the key will expire.</param>
        /// <param name="notBefore">Set a System.DateTimeOffset indicating when the key will be valid.</param>
        /// <param name="hardwareProtected">
        /// True to create a hardware-protected key in a hardware security module (HSM).
        /// The default is false to create a software key.
        /// </param>
        /// <param name="autoUpdateIfExpired">Allow auto update key if expired.</param>
        public static async Task CreateKeyAsync(
            VaultParameters vaultParameters,
            string name, Key key, DateTime? notBefore = null, DateTime? expiresOn = null, bool autoUpdateIfExpired = false, bool hardwareProtected = false)
        {
            TokenCredential _credential;

            if (string.IsNullOrWhiteSpace(vaultParameters.TenantId) || string.IsNullOrWhiteSpace(vaultParameters.ClientId) || string.IsNullOrWhiteSpace(vaultParameters.ClientSecret))
            {
                _credential = new DefaultAzureCredential();
            }
            else
            {
                _credential = new ClientSecretCredential(vaultParameters.TenantId, vaultParameters.ClientId, vaultParameters.ClientSecret);
            }

            var _client = new KeyClient(vaultParameters.Uri, _credential);


            if (key is Key.RSA2048 or Key.RSA3072 or Key.RSA4096)
            {
                var k = new CreateRsaKeyOptions(name, hardwareProtected)
                {
                    KeySize = key == Key.RSA2048 ? 2048 : key == Key.RSA3072 ? 3072 : key == Key.RSA4096 ? 4096 : null,
                    Enabled = true,
                    ExpiresOn = expiresOn,
                    NotBefore = notBefore,
                    //KeyOperations = new List<KeyOperation>(new KeyOperation[] { 
                    //    KeyOperation.Verify,
                    //    KeyOperation.WrapKey,
                    //    KeyOperation.Decrypt,
                    //    KeyOperation.Sign,
                    //    KeyOperation.Encrypt,
                    //    KeyOperation.Import,
                    //    KeyOperation.UnwrapKey
                    //})
                };

                k.Tags.Add("CreateWith", $"{nameof(UCode)}.{nameof(Crypto)}.{nameof(KeyVaultCrypto)}");
                k.Tags.Add("AutoUpdateIfExpired", autoUpdateIfExpired ? "True" : "False");

                _ = await _client.CreateRsaKeyAsync(k);
            }
            else if (key is Key.P256 or Key.P384 or Key.P521 or Key.P256K)
            {
                var k = new CreateEcKeyOptions(name, hardwareProtected)
                {
                    CurveName = key == Key.P256 ? KeyCurveName.P256 :
                        key == Key.P384 ? KeyCurveName.P384 :
                        key == Key.P521 ? KeyCurveName.P521 :
                        key == Key.P256K ? KeyCurveName.P256K : null,
                    Enabled = true,
                    ExpiresOn = expiresOn,
                    NotBefore = notBefore,
                    //KeyOperations = new List<KeyOperation>(new KeyOperation[] { 
                    //    KeyOperation.Verify,
                    //    KeyOperation.WrapKey,
                    //    KeyOperation.Decrypt,
                    //    KeyOperation.Sign,
                    //    KeyOperation.Encrypt,
                    //    KeyOperation.Import,
                    //    KeyOperation.UnwrapKey
                    //})
                };

                k.Tags.Add("CreateWith", $"{nameof(UCode)}.{nameof(Crypto)}.{nameof(KeyVaultCrypto)}");
                k.Tags.Add("AutoUpdateIfExpired", autoUpdateIfExpired ? "True" : "False");

                _ = await _client.CreateEcKeyAsync(k);
            }
        }



    }
}
