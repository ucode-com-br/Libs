using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UCode.Extensions
{
    public static class ConvertersExtensions
    {
        public static string? ToHexString(this byte[]? source)
        {
            if (source == null)
            {
                return null;
            }

            return BitConverter.ToString(source).Replace("-", "");
        }

        public static string ToHexString(this Stream stream) => BitConverter.ToString(stream.ToBytes()).Replace("-", "");

        public static byte[] FromHexString(this string hexString)
        {
            var length = (hexString.Length + 1) / 3;
            var arr1 = new byte[length];
            for (var i = 0; i < length; i++)
            {
                arr1[i] = Convert.ToByte(hexString.Substring(3 * i, 2), 16);
            }

            return arr1;
        }

        public static byte[] FromBase64(this string hexString) => Convert.FromBase64String(hexString);

        [return: NotNull]
        public static string ToBase64([NotNull] this byte[] @byte) => Convert.ToBase64String(@byte);

        [return: MaybeNull]
        public static byte[]? ToBytes([MaybeNull] this string? source)
        {
            if (source == null)
            {
                return null;
            }

            return Encoding.UTF8.GetBytes(source);
        }

        [return: MaybeNull]
        public static byte[]? ToBytes<T>([MaybeNull] this T? source)
        {
            if (source == null)
            {
                return null;
            }

            var json = source.JsonString();

            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            return Encoding.UTF8.GetBytes(json);
        }

        public static T? ToObject<T>([NotNull] this byte[] bytes) => Encoding.UTF8.GetString(bytes).JsonObject<T>();

        [return: NotNull]
        public static object? ToObject([NotNull] this byte[] bytes, Type type) => Encoding.UTF8.GetString(bytes).JsonObject(type);


        [return: NotNull]
        public static Stream ToStream([NotNull] this string source) => source.ToBytes().ToStream();

        [return: MaybeNull]
        public static byte[] ToBytes([MaybeNull] this Stream stream)
        {
            if (stream == null)
            {
                return null;
            }

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            if (stream.CanRead)
            {
                byte[] byteArray = null;
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);

                    byteArray = memoryStream.ToArray();
                }

                return byteArray;
            }

            return null;
        }

        [return: MaybeNull]
        public static Stream ToStream([MaybeNull] this byte[] bytes) => new MemoryStream(bytes);


        [return: NotNull]
        public static byte[] ToBytes(this int @int) => BitConverter.GetBytes(@int);

        [return: NotNull]
        public static byte[] ToBytes(this long @long) => BitConverter.GetBytes(@long);

        [return: NotNull]
        public static int ToInt32(this byte[] bytes)
        {
            if (bytes.Length != 4)
            {
                throw new ArgumentException("FOr convert to Int32 need 4 bytes.");
            }

            return BitConverter.ToInt32(bytes);
        }

        [return: NotNull]
        public static long ToInt64(this byte[] bytes)
        {
            if (bytes.Length != 8)
            {
                throw new ArgumentException("FOr convert to Int32 need 8 bytes.");
            }

            return BitConverter.ToInt64(bytes);
        }


        //public static T AutoMap<T>(this object source)
        //{
        //    if (source == default)
        //        return default;

        //    var configuration = new MapperConfiguration(cfg => { });

        //    var mapper = configuration.CreateMapper();

        //    return mapper.Map<T>(source);
        //}

        //public static IEnumerable<T> AutoMap<T>(this IEnumerable<object> source)
        //{
        //    if (source == default)
        //        return default;

        //    var configuration = new MapperConfiguration(cfg => { cfg.CreateMap<object, T>(); });

        //    var mapper = configuration.CreateMapper();

        //    return mapper.Map<IEnumerable<object>, IEnumerable<T>>(source);
        //}


        #region ToSha256Hash

        [return: MaybeNull]
        /// <summary>
        /// Calculate SHA256 and return HEX result encoded
        /// </summary>
        /// <param name="source">object to be calculated SH256</param>
        /// <returns>Hex string</returns>
        public static string? CalculateSha256Hash([MaybeNull] this string? source)
        {
            var bytes = source.ToBytes();

            var hash = ToSha256Hash(bytes);

            return hash.ToHexString();
        }

        [return: MaybeNull]
        /// <summary>
        /// Calculate SHA256 and return HEX result encoded
        /// </summary>
        /// <param name="bytes">array of bytes to be calculated SH256</param>
        /// <returns>Hex string</returns>
        public static string? CalculateSha256Hash([MaybeNull] this byte[]? bytes)
        {
            var hash = ToSha256Hash(bytes);

            return hash.ToHexString();
        }

        [return: MaybeNull]
        /// <summary>
        /// Calculate SHA256 and return HEX result encoded
        /// </summary>
        /// <param name="stream">stream to be calculated SH256</param>
        /// <returns>Hex string</returns>
        public static string? CalculateSha256Hash([MaybeNull] this Stream? stream)
        {
            var hash = ToSha256Hash(stream);

            return hash.ToHexString();
        }

        [return: MaybeNull]
        /// <summary>
        /// Calculate SHA256 and return HEX result encoded
        /// </summary>
        /// <typeparam name="T">Instance type</typeparam>
        /// <param name="instance">instance object</param>
        /// <returns>Hex string</returns>
        public static string? CalculateSha256Hash<T>([MaybeNull] this T? instance)
        {
            var json = System.Text.Json.JsonSerializer.Serialize<T>(instance);

            var hash = ToSha256Hash(json);

            return hash.ToHexString();
        }



        public static byte[]? ToSha256Hash(this Stream? stream)
        {
            if (stream == null)
            {
                return null;
            }

            byte[] bytes = null;
            bytes = SHA256.HashData(stream.ToBytes());

            return bytes;
        }

        public static byte[]? ToSha256Hash(this byte[]? source)
        {
            if (source == null)
            {
                return null;
            }

            byte[] bytes = null;
            bytes = SHA256.HashData(source);

            return bytes;
        }

        public static byte[]? ToSha256Hash(this string? source)
        {
            if (source == null)
            {
                return null;
            }

            return ToSha256Hash(Encoding.UTF8.GetBytes(source));
        }

        #endregion ToSha256Hash
    }
}
