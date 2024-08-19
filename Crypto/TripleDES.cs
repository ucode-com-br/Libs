using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
// ReSharper disable CommentTypo

namespace UCode.Crypto
{
    public class TripleDes : IDisposable
    {
        //private readonly byte[] _rgbIv;
        //private readonly byte[] _rgbKey;

        private readonly ICryptoTransform _decryptTransform;
        private readonly ICryptoTransform _encryptTransform;
        private readonly TripleDESCryptoServiceProvider _tripleDesCryptoServiceProvider;

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
        public void Dispose() =>
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);// TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);

        #endregion
    }
}
