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
    /// <summary>
    /// Represents a cryptographic functionality within the Azure Key Vault.
    /// This class provides methods for encryption, decryption, signing, and verification 
    /// using the Key Vault's cryptographic keys.
    /// </summary>
    public class KeyVaultCrypto
    {
        /// <summary>
        /// Defines an enumeration representing various types of cryptographic keys.
        /// </summary>
        /// <remarks>
        /// The <see cref="Key"/> enum includes RSA keys of varying sizes and elliptic curve keys.
        /// </remarks>
        public enum Key
        {
            RSA2048, RSA3072, RSA4096,
            P256, P384, P521, P256K
        }

        /// <summary>
        /// Represents the parameters for a vault configuration.
        /// This class contains properties and methods to manage 
        /// and manipulate vault parameters effectively.
        /// </summary>
        public class VaultParameters
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VaultParameters"/> class.
            /// </summary>
            /// <param name="defaultVaultUri">The default URI of the vault.</param>
            /// <param name="tenantId">The tenant ID associated with the vault (optional).</param>
            /// <param name="clientId">The client ID associated with the vault (optional).</param>
            /// <param name="clientSecret">The client secret associated with the vault (optional).</param>
            public VaultParameters(string defaultVaultUri, string tenantId = null, string clientId = null, string clientSecret = null)
            {
                this.Uri = new Uri(defaultVaultUri);
                this.TenantId = tenantId;
                this.ClientId = clientId;
                this.ClientSecret = clientSecret;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="VaultParameters"/> class.
            /// </summary>
            /// <param name="uri">The URI of the vault.</param>
            /// <param name="tenantId">The tenant ID associated with the vault. This parameter is optional.</param>
            /// <param name="clientId">The client ID for authentication. This parameter is optional.</param>
            /// <param name="clientSecret">The client secret for authentication. This parameter is optional.</param>
            public VaultParameters(Uri uri, string tenantId = null, string clientId = null, string clientSecret = null)
            {
                this.Uri = uri;
                this.TenantId = tenantId;
                this.ClientId = clientId;
                this.ClientSecret = clientSecret;
            }

            /// <summary>
            /// Gets or sets the URI associated with this instance.
            /// </summary>
            /// <value>
            /// A <see cref="Uri"/> object representing the URI. 
            /// Can be null if no URI is set.
            /// </value>
            public Uri Uri
            {
                get; set;
            }
            /// <summary>
            /// Gets or sets the Tenant ID associated with the entity.
            /// This property holds the identifier for the tenant, which can be used
            /// to link data or actions to a specific tenant in a multi-tenant environment.
            /// </summary>
            /// <value>
            /// A string representing the unique identifier for the tenant.
            /// </value>
            public string TenantId
            {
                get; set;
            }
            /// <summary>
            /// Gets or sets the unique identifier for the client.
            /// </summary>
            /// <value>
            /// A string representing the client identifier.
            /// </value>
            public string ClientId
            {
                get; set;
            }
            /// <summary>
            /// Gets or sets the client secret.
            /// </summary>
            /// <value>
            /// A <see cref="string"/> representing the client secret. 
            /// This secret is typically used for authentication purposes in secure applications.
            /// </value>
            public string ClientSecret
            {
                get; set;
            }
        }

        private readonly KeyClient _client;
        private readonly TokenCredential _credential;
        private readonly string _cryptoKeyName;

        private readonly Task<Response<KeyVaultKey>> _responseKeyVaultKeyTask;
        /// <summary>
        /// Represents the KeyVaultKey property that retrieves the result of the 
        /// asynchronous operation that fetches the KeyVaultKey.
        /// </summary>
        /// <returns>
        /// Returns an instance of the <see cref="KeyVaultKey"/> class once the 
        /// asynchronous task associated with <see cref="_responseKeyVaultKeyTask"/> 
        /// is completed.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the task has not completed successfully, and an 
        /// attempt to access its result is made.
        /// </exception>
        /// <remarks>
        /// This property will block the calling thread until the 
        /// <see cref="_responseKeyVaultKeyTask"/> has completed. 
        /// It's particularly useful when the KeyVaultKey is needed 
        /// immediately and ensures that the caller 
        /// waits for the task to finish.
        /// </remarks>
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
        /// <summary>
        /// Gets the CryptographyClient instance. If the instance does not already exist,
        /// it is created using the KeyVaultKey's Id and the provided credential.
        /// </summary>
        /// <returns>
        /// The CryptographyClient instance that is used for cryptographic operations.
        /// </returns>
        private CryptographyClient CryptographyClient
        {
            get
            {
                this._cryptographyClient ??= new CryptographyClient(this.KeyVaultKey.Id, this._credential);
                return this._cryptographyClient;
            }
        }

        /// <summary>
        /// Gets or sets the encryption algorithm used for encrypting data.
        /// The default value is <see cref="EncryptionAlgorithm.RsaOaep"/>.
        /// </summary>
        /// <value>
        /// The encryption algorithm, represented by the <see cref="EncryptionAlgorithm"/> enumeration.
        /// </value>
        public EncryptionAlgorithm EncryptionAlgorithm { get; set; } = EncryptionAlgorithm.RsaOaep;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyVaultCrypto"/> class.
        /// This constructor sets up the crypto key using the provided key name and vault parameters.
        /// It attempts to authenticate with Azure Key Vault and retrieves the key's properties.
        /// If the specified key does not exist or is invalid, it creates a new key.
        /// </summary>
        /// <param name="cryptoKeyName">The name of the cryptographic key to be managed.</param>
        /// <param name="vaultParameters">An instance of <see cref="VaultParameters"/> containing the Azure Key Vault's properties.</param>
        /// <returns>
        /// A Task that represents the asynchronous operation.
        /// It will complete with the key response from Azure Key Vault or null if an exception occurs.
        /// </returns>
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




        /// <summary>
        /// Asynchronously signs the provided data using the RS512 signature algorithm.
        /// </summary>
        /// <param name="data">The byte array representing the data to be signed.</param>
        /// <returns>
        /// A Task that represents the asynchronous operation. The task result contains 
        /// a byte array of the signature generated for the specified data.
        /// </returns>
        /// <remarks>
        /// This method utilizes a cryptographic client to perform the signing operation.
        /// It is important to ensure that the data provided is valid and that the 
        /// CryptographyClient is properly initialized before calling this method.
        /// </remarks>
        public async Task<byte[]> SignAsync(byte[] data) => (await this.CryptographyClient.SignDataAsync(SignatureAlgorithm.RS512, data)).Signature;


        /// <summary>
        /// Asynchronously verifies a digital signature against the provided data using the RS512 signature algorithm.
        /// </summary>
        /// <param name="data">The byte array containing the data to be verified.</param>
        /// <param name="signature">The byte array containing the digital signature to verify against the data.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is true if the signature is valid; otherwise, false.
        /// </returns>
        public async Task<bool> VerifyAsync(byte[] data, byte[] signature) => (await this.CryptographyClient.VerifyDataAsync(SignatureAlgorithm.RS512, data, signature)).IsValid;


        /// <summary>
        /// Asynchronously encrypts the specified byte array using the configured encryption algorithm.
        /// </summary>
        /// <param name="source">A byte array representing the data to be encrypted.</param>
        /// <returns>Returns a task that represents the asynchronous operation, containing the encrypted byte array (ciphertext).</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
        /// <exception cref="CryptographicException">Thrown when an error occurs during the encryption process.</exception>
        public async Task<byte[]> EncryptAsync(byte[] source) => (await this.CryptographyClient.EncryptAsync(this.EncryptionAlgorithm, source)).Ciphertext;

        /// <summary>
        /// Asynchronously decrypts the given byte array.
        /// </summary>
        /// <param name="cipher">A byte array representing the encrypted data to be decrypted.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the decrypted plaintext as a byte array.</returns>
        /// <exception cref="CryptographicException">Thrown when decryption fails.</exception>
        public async Task<byte[]> DecryptAsync(byte[] cipher) => (await this.CryptographyClient.DecryptAsync(this.EncryptionAlgorithm, cipher)).Plaintext;




        /// <summary>
        /// Creates an instance of the <see cref="KeyVaultCrypto"/> class using the specified parameters.
        /// </summary>
        /// <param name="cryptoKeyName">The name of the cryptographic key to be used.</param>
        /// <param name="defaultVaultUri">The URI of the Azure Key Vault where the crypto key is stored.</param>
        /// <param name="tenantId">The ID of the Azure Active Directory tenant.</param>
        /// <param name="clientId">The client ID of the application accessing the Key Vault.</param>
        /// <param name="clientSecret">The client secret for the application accessing the Key Vault.</param>
        /// <returns>A new instance of the <see cref="KeyVaultCrypto"/> class initialized with the provided parameters.</returns>
        public static KeyVaultCrypto Create(string cryptoKeyName, string defaultVaultUri, string tenantId, string clientId, string clientSecret) => new KeyVaultCrypto(cryptoKeyName, new VaultParameters(defaultVaultUri, tenantId, clientId, clientSecret));

        /// <summary>
        /// Creates an instance of <see cref="KeyVaultCrypto"/> using the specified key name.
        /// </summary>
        /// <param name="cryptoKeyName">The name of the cryptographic key to be used.</param>
        /// <returns>A new instance of <see cref="KeyVaultCrypto"/> initialized with the specified key name and vault parameters.</returns>
        public static KeyVaultCrypto CreateManaged(string cryptoKeyName) => new KeyVaultCrypto(cryptoKeyName, new VaultParameters(Environment.GetEnvironmentVariable("VaultUri")));


        /// <summary>
        /// Asynchronously creates a cryptographic key in the specified key vault.
        /// The type of key to create is determined by the provided Key parameter.
        /// </summary>
        /// <param name="vaultParameters">Parameters required to access the key vault, including TenantId, ClientId, and ClientSecret.</param>
        /// <param name="name">The name of the key to be created.</param>
        /// <param name="key">The type of key to create (e.g., RSA or EC).</param>
        /// <param name="notBefore">The time before which the key is not valid. Optional.</param>
        /// <param name="expiresOn">The expiration time of the key. Optional.</param>
        /// <param name="autoUpdateIfExpired">Indicates whether to automatically update the key if it expires.</param>
        /// <param name="hardwareProtected">Indicates whether the key should be hardware protected.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
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
