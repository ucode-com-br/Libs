using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
// ReSharper disable CommentTypo

namespace UCode.Crypto
{
    /// <summary>
    /// Represents a Triple Data Encryption Standard (3DES) encryption algorithm.
    /// This class provides methods for encrypting and decrypting data using the 3DES algorithm.
    /// </summary>
    /// <remarks>
    /// The 3DES algorithm applies the Data Encryption Standard (DES) cipher algorithm three times to each data block.
    /// It offers enhanced security over standard DES but is considered less secure compared to more modern symmetric encryption algorithms.
    /// </remarks>
    /// <example>
    /// This example demonstrates how to use the TripleDes class to encrypt and decrypt a string.
    /// <code>
    /// using (var tripleDes = new TripleDes())
    /// {
    ///     string originalText = "Hello, World!";
    ///     byte[] encryptedData = tripleDes.Encrypt(originalText);
    ///     string decryptedText = tripleDes.Decrypt(encryptedData);
    /// }
    /// </code>
    /// </example>
    public class TripleDes : IDisposable
    {
        //private readonly byte[] _rgbIv;
        //private readonly byte[] _rgbKey;

        private readonly ICryptoTransform _decryptTransform;
        private readonly ICryptoTransform _encryptTransform;
        private readonly TripleDESCryptoServiceProvider _tripleDesCryptoServiceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TripleDes"/> class.
        /// The constructor requires a key and an initialization vector (IV) for the TripleDES encryption.
        /// </summary>
        /// <param name="rgbKey">A byte array representing the cryptographic key. Must be exactly 24 bytes in length.</param>
        /// <param name="rgbIv">A byte array representing the initialization vector. Must be exactly 16 bytes in length.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="rgbKey"/> or <paramref name="rgbIv"/> is null or does not meet the required lengths.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="rgbKey"/> or <paramref name="rgbIv"/> exceeds the required lengths.</exception>
        public TripleDes([NotNull] byte[] rgbKey, [NotNull] byte[] rgbIv)
        {
            if (rgbKey.Length < 24 || rgbIv.Length < 16)
            {
                throw new ArgumentNullException($"{rgbKey} OR {nameof(rgbIv)} canot be null.");
            }

            if (rgbKey.Length > 24 || rgbIv.Length > 16)
            {
                throw new ArgumentException($"{rgbKey} OR {nameof(rgbIv)} is invalid length.");
            }

            //_rgbKey = rgbKey;
            //_rgbIv = rgbIv;

            this._tripleDesCryptoServiceProvider = new TripleDESCryptoServiceProvider();


            //Criar o encriptador
            this._encryptTransform = this._tripleDesCryptoServiceProvider.CreateEncryptor(rgbKey, rgbIv);

            this._decryptTransform = this._tripleDesCryptoServiceProvider.CreateDecryptor(rgbKey, rgbIv);
        }

        //public string EncryptBase64(string base64)
        //{
        //    //string result = null;

        //    ////Criar o memory stream
        //    //using (MemoryStream ms = new MemoryStream())
        //    //{

        //    //    //Crie um CryptoStream de MemoryStream e Encriptador e grave-o. 
        //    //    using (CryptoStream cs = new CryptoStream(ms, _encryptTransform, CryptoStreamMode.Write))
        //    //    {
        //    //        using (BinaryWriter bw = new BinaryWriter(cs))
        //    //        {
        //    //            var bytes = Convert.FromBase64String(base64);
        //    //            bw.Write(bytes);
        //    //        }
        //    //        //using (StreamWriter sw = new StreamWriter(cs))
        //    //        //{
        //    //        //    var bytes = Convert.FromBase64String(base64);
        //    //        //    string utf8 = System.Text.Encoding.UTF8.GetString(bytes);
        //    //        //    sw.Write(utf8);
        //    //        //}

        //    //        result = Convert.ToBase64String(ms.ToArray());
        //    //    }
        //    //}

        //    //return result;
        //    return Convert.ToBase64String(Encrypt(Convert.FromBase64String(base64)));
        //}

        /// <summary>
        /// Encrypts a base64 encoded string by converting it to bytes, 
        /// processing it through encryption, and returning the 
        /// result as a base64 encoded string.
        /// </summary>
        /// <param name="base64">
        /// A base64 encoded string representing the data to be encrypted.
        /// </param>
        /// <returns>
        /// A base64 encoded string of the encrypted data.
        /// </returns>
        [return: NotNull]
        public byte[] Encrypt([NotNull] byte[] bytes)
        {
            //Criar o memory stream
            using var ms = new MemoryStream();
            //Crie um CryptoStream de MemoryStream e Encriptador e grave-o. 
            using var cs = new CryptoStream(ms, this._encryptTransform, CryptoStreamMode.Write);
            using (var bw = new BinaryWriter(cs))
            {
                bw.Write(bytes);
            }

            var result = ms.ToArray();

            return result;
        }


        //public string DecryptBase64(string base64)
        //{
        //    //string result = null;

        //    ////Criar o memory stream
        //    //using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64)))
        //    //{

        //    //    //Crie um CryptoStream de MemoryStream e Encriptador e grave-o. 
        //    //    using (CryptoStream cs = new CryptoStream(ms, _decryptTransform, CryptoStreamMode.Read))
        //    //    {
        //    //        //using (StreamReader sr = new StreamReader(cs))
        //    //        //    result = sr.ReadToEnd();
        //    //        using (BinaryReader br = new BinaryReader(cs))
        //    //        using (MemoryStream brms = new MemoryStream())
        //    //        {
        //    //            var buffer = new byte[1024];
        //    //            var bufferCount = 0;
        //    //            while ((bufferCount = br.Read(buffer, 0, buffer.Length)) > 0)
        //    //            {
        //    //                brms.Write(buffer, 0, bufferCount);
        //    //            }
        //    //            result = Convert.ToBase64String(brms.ToArray());
        //    //        }
        //    //    }
        //    //}

        //    //return result;
        //    return Convert.ToBase64String(Decrypt(Convert.FromBase64String(base64)));
        //}
        /// <summary>
        /// Decrypts the provided byte array that has been encrypted.
        /// </summary>
        /// <param name="bytes">The byte array to decrypt.</param>
        /// <returns>
        /// The decrypted byte array.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the provided byte array is null.
        /// </exception>
        [return: NotNull]
        public byte[] Decrypt([NotNull] byte[] bytes)
        {
            //Criar o memory stream
            using var ms = new MemoryStream(bytes);
            //Crie um CryptoStream de MemoryStream e Encriptador e grave-o. 
            using var cs = new CryptoStream(ms, this._decryptTransform, CryptoStreamMode.Read);
            using var b = new BufferedStream(cs);
            using var bms = new MemoryStream();
            b.CopyTo(bms);

            var result = bms.ToArray();

            //using (BinaryReader br = new BinaryReader(cs))
            //using (MemoryStream brms = new MemoryStream())
            //{
            //    var buffer = new byte[1024];
            //    var bufferCount = 0;
            //    while ((bufferCount = br.Read(buffer, 0, buffer.Length)) > 0)
            //    {
            //        brms.Write(buffer, 0, bufferCount);
            //    }
            //    result = brms.ToArray();
            //}

            return result;
        }


        #region IDisposable Support

        private bool _disposedValue; // To detect redundant calls

        /// <summary>
        /// Releases the resources used by the current instance of the class.
        /// </summary>
        /// <param name="disposing">
        /// A boolean value that indicates whether the method was called 
        /// directly or indirectly by user code (true) or by the 
        /// runtime from inside the finalizer (false).
        /// </param>
        /// <remarks>
        /// If disposing is true, the method will dispose of all managed resources, 
        /// otherwise, it will only release unmanaged resources.
        /// </remarks>
        /// <notes>
        /// After disposing, the state of the instance should not be used 
        /// until it is recreated.
        /// </notes>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    this._encryptTransform.Dispose();
                    this._decryptTransform.Dispose();
                    this._tripleDesCryptoServiceProvider.Dispose();
                }
        
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
        
                this._disposedValue = true;
            }
        }


        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TripleDES()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// Implements the IDisposable interface to provide a standard way to release unmanaged resources
        /// and support a deterministic clean-up of resources when they are no longer needed.
        /// </summary>
        /// <remarks>
        /// It is important to override the finalizer only if Dispose(bool disposing) contains code to 
        /// free unmanaged resources. If the finalizer is overridden, ensure that the Dispose method calls
        /// GC.SuppressFinalize(this) to prevent the finalizer from being run after the resources have
        /// already been cleaned up.
        /// </remarks>
        /// <example>
        /// To correctly implement the disposable pattern, consider using the following pattern:
        /// <code>
        /// 
        public void Dispose() =>
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);// TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);

        #endregion
    }
}
