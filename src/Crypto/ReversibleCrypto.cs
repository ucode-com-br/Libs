using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace UCode.Crypto
{
    /// <summary>
    /// Represents a reversible cryptography implementation that allows for data encryption and decryption.
    /// This class implements the IDisposable interface to manage resources effectively.
    /// </summary>
    public class ReversibleCrypto : IDisposable
    {
        private readonly TripleDes _tripleDes;

        /// <summary>
        /// Generates a deterministic byte array of length 24 based on the provided password.
        /// This byte array is derived from the SHA-1 hash of the password and a constant seed value.
        /// </summary>
        /// <param name="password">The input password used to generate the deterministic byte array.</param>
        /// <returns>
        /// A byte array of length 24 that incorporates the SHA-1 hash of the password and a shifted seed value.
        /// </returns>
        private static byte[] DeterministicPasswordLength(string password)
        {
            var key = new byte[24];
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            var sha1 = SHA1.HashData(passwordBytes);

            var s = new byte[8];


            var seedConst = BitConverter.GetBytes(2147483647);

            for (var i = 0; i < 8; i++)
            {
                if (passwordBytes.Length > i)
                {
                    s[i] = passwordBytes[i];
                }
                else
                {
                    s[i] = seedConst[i];
                }
            }

            var longShift = BitConverter.ToInt64(s);
            longShift >>= 4;
            var shifted = BitConverter.GetBytes(longShift);



            Array.Copy(sha1, key, sha1.Length);
            Array.Copy(shifted, 0, key, sha1.Length, key.Length - sha1.Length);

            return key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReversibleCrypto"/> class.
        /// This constructor takes a password and computes an initialization vector (IV) 
        /// using MD5 hashing. The IV is then used to create an instance of the TripleDes class.
        /// </summary>
        /// <param name="password">The password used to generate cryptographic materials.</param>
        public ReversibleCrypto([NotNull] string password)
        {
            //using (SHA256 sha256 = SHA256.Create())
            byte[] iv;
            //_key = sha256.ComputeHash(UTF8Encoding.UTF8.GetBytes(password));

            iv = MD5.HashData(Encoding.UTF8.GetBytes(password));

            this._tripleDes = new TripleDes(DeterministicPasswordLength(password), iv);
        }

        /// <summary>
        /// Encrypts the provided byte array using Triple DES encryption.
        /// </summary>
        /// <param name="bytes">The byte array to encrypt. This parameter cannot be null.</param>
        /// <returns>A byte array containing the encrypted data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="bytes"/> parameter is null.</exception>
        public byte[] Encrypt([NotNull] byte[] bytes) => this._tripleDes.Encrypt(bytes);

        /// <summary>
        /// Decrypts the given byte array using TripleDES encryption.
        /// </summary>
        /// <param name="bytes">
        /// The encrypted byte array that needs to be decrypted. 
        /// This parameter cannot be null.
        /// </param>
        /// <returns>
        /// A byte array which represents the decrypted data.
        /// </returns>
        public byte[] Decrypt([NotNull] byte[] bytes) => this._tripleDes.Decrypt(bytes);

        #region IDisposable Support

        private bool _disposedValue; // To detect redundant calls

        /// <summary>
        /// Releases the resources used by the current instance of the class.
        /// </summary>
        /// <param name="disposing">
        /// A boolean value indicating whether the method call comes from 
        /// a disposing method (true) or from a finalizer (false).
        /// If disposing is true, the method has been called directly 
        /// or indirectly by a user's code. Managed resources can be disposed. 
        /// If disposing is false, the method has been called by the runtime 
        /// from inside the finalizer and you should not reference 
        /// other managed objects. 
        /// Therefore, only unmanaged resources should be released.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    this._tripleDes.Dispose();
                }
        
                // TODO: free unmanaged resources (unmanaged objects) 
                // and override a finalizer below. 
                // TODO: set large fields to null.
        
                this._disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ReversibleCrypto()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// Finalizer for the <see cref="ReversibleCrypto"/> class.
        /// It is used to clean up unmanaged resources if they are not disposed of by the user.
        /// </summary>
        /// <remarks>
        /// This finalizer should only be uncommented if the <see cref="Dispose(bool)"/> method is implemented 
        /// to free unmanaged resources. It is important to follow the dispose pattern correctly to 
        /// avoid resource leaks and undefined behavior.
        /// </remarks>
        /// <example>
        /// This code should not be modified directly. Cleanup code should be placed in
        /// the <see cref="Dispose(bool)"/> method to ensure proper resource management.
        /// </example>
        /// <seealso cref="IDisposable"/>
        // ~ReversibleCrypto() 
        // { 
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above. 
        //   Dispose(false); 
        // } 
        
        /// <summary>
        /// Disposes the <see cref="ReversibleCrypto"/> instance and releases the resources it holds.
        /// </summary>
        /// <remarks>
        /// This method should be called when the instance is no longer needed.
        /// If this method is called, it is safe to suppress finalization as the resources 
        /// have been cleaned up properly.
        /// </remarks>
        /// <example>
        /// Call this method to free managed and unmanaged resources.
        /// </example>
        /// <seealso cref="Dispose(bool)"/>
        public void Dispose() =>
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);// TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);

        #endregion
    }
}
