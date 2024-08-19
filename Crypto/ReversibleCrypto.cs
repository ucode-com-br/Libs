using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace UCode.Crypto
{
    public class ReversibleCrypto : IDisposable
    {
        private readonly TripleDes _tripleDes;

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

        public ReversibleCrypto([NotNull] string password)
        {
            //using (SHA256 sha256 = SHA256.Create())
            byte[] iv;
            //_key = sha256.ComputeHash(UTF8Encoding.UTF8.GetBytes(password));

            iv = MD5.HashData(Encoding.UTF8.GetBytes(password));

            this._tripleDes = new TripleDes(DeterministicPasswordLength(password), iv);
        }

        public byte[] Encrypt([NotNull] byte[] bytes) => this._tripleDes.Encrypt(bytes);

        public byte[] Decrypt([NotNull] byte[] bytes) => this._tripleDes.Decrypt(bytes);

        #region IDisposable Support

        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    this._tripleDes.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
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
        public void Dispose() =>
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);// TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);

        #endregion
    }
}
