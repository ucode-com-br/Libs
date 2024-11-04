using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UCode.Extensions
{
    /// <summary>
    /// This static class provides extension methods for converting various data types.
    /// </summary>
    /// <remarks>
    /// The purpose of this class is to extend the functionality of existing classes 
    /// by providing additional methods that help facilitate data conversion.
    /// </remarks>
    public static class ConvertersExtensions
    {
        /// <summary>
        /// Converts a byte array to its hexadecimal string representation.
        /// If the provided byte array is null, the method returns null.
        /// </summary>
        /// <param name="source">
        /// The byte array to be converted into a hexadecimal string.
        /// </param>
        /// <returns>
        /// A string representing the hexadecimal values of the bytes in the array,
        /// or null if the input byte array is null.
        /// </returns>
        [return: MaybeNull]
        public static string? ToHexString(this byte[]? source)
        {
            if (source == null)
            {
                return null;
            }

            return BitConverter.ToString(source).Replace("-", "");
        }

        /// <summary>
        /// Converts a given nullable <see cref="Stream"/> instance to its corresponding hexadecimal string representation.
        /// If the <paramref name="stream"/> is null, the method returns null.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> instance to be converted to a hexadecimal string.</param>
        /// <returns>
        /// A hexadecimal string representation of the byte contents of the <paramref name="stream"/> 
        /// if it is not null; otherwise, returns null.
        /// </returns>
        /// <remarks>
        /// This method reads the bytes from the stream, converts them to a byte array, 
        /// and then formats the byte array as a string with hexadecimal values separated by hyphens, 
        /// which are subsequently replaced by an empty string to produce the final output.
        /// </remarks>
        [return: MaybeNull]
        public static string? ToHexString(this Stream? stream) => stream == null ? null : BitConverter.ToString(stream.ToBytes()).Replace("-", "");

        /// <summary>
        /// Converts a hexadecimal string representation into an array of bytes.
        /// </summary>
        /// <param name="hexString">The string containing hexadecimal characters. If null, the method returns null.</param>
        /// <returns>
        /// An array of bytes represented by the hexadecimal string. Returns null if the input string is null.
        /// </returns>
        [return: MaybeNull]
        public static byte[]? FromHexString(this string? hexString)
        {
            if (hexString == null)
            {
                return null;
            }

            var length = (hexString.Length + 1) / 3;
            var arr1 = new byte[length];
            for (var i = 0; i < length; i++)
            {
                arr1[i] = Convert.ToByte(hexString.Substring(3 * i, 2), 16);
            }

            return arr1;
        }

        /// <summary>
        /// Converts a Base64 encoded string to a byte array.
        /// </summary>
        /// <param name="hexString">
        /// The Base64 encoded string to convert. This parameter can be null.
        /// </param>
        /// <returns>
        /// A byte array that represents the decoded data, or null if the input string is null.
        /// </returns>
        /// <remarks>
        /// This method is an extension method for the string class and uses the
        /// <see cref="Convert.FromBase64String(string)"/> method to perform the conversion.
        /// </remarks>
        [return: MaybeNull]
        public static byte[]? FromBase64(this string? hexString) => hexString == null ? null : Convert.FromBase64String(hexString);

        /// <summary>
        /// Converts a byte array to its equivalent Base64 string representation.
        /// </summary>
        /// <param name="byte">The byte array to convert. This parameter cannot be null.</param>
        /// <returns>
        /// A Base64 string representation of the byte array if the input is not null; otherwise, returns null.
        /// </returns>
        /// <remarks>
        /// If the <paramref name="byte"/> parameter is null, the method will return null.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="byte"/> is null.
        /// </exception>
        [return: MaybeNull]
        public static string? ToBase64([NotNull] this byte[]? @byte) => @byte == null ? null : Convert.ToBase64String(@byte);

        /// <summary>
        /// Converts a string to a byte array using UTF-8 encoding.
        /// If the input string is null, the method returns null.
        /// </summary>
        /// <param name="source">
        /// The string to convert to a byte array. It may be null.
        /// </param>
        /// <returns>
        /// A byte array representing the UTF-8 encoded version of the input string, 
        /// or null if the input string is null.
        /// </returns>
        [return: MaybeNull]
        public static byte[]? ToBytes([MaybeNull] this string? source)
        {
            if (source == null)
            {
                return null;
            }

            return Encoding.UTF8.GetBytes(source);
        }

        /// <summary>
        /// Converts an object of type T to a byte array by serializing it to a JSON string.
        /// If the source object is null, or if the JSON string representation is null or empty,
        /// the method returns either null or a default byte array.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the source object. This type can be any reference type.
        /// </typeparam>
        /// <param name="source">
        /// The source object to be converted to a byte array. This parameter can be null.
        /// </param>
        /// <returns>
        /// A byte array representing the JSON string of the source object, or null if the 
        /// source object is null or if the JSON string representation is null or empty.
        /// </returns>
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

        /// <summary>
        /// Converts a byte array to an object of type T using UTF-8 encoding.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object that the byte array will be converted to.
        /// </typeparam>
        /// <param name="bytes">
        /// The byte array that contains the data to convert. This parameter can be null.
        /// </param>
        /// <returns>
        /// Returns an object of type T if the byte array is not null; otherwise, returns the default value for type T.
        /// The return value can be null if T is a reference type or a nullable type.
        /// </returns>
        /// <remarks>
        /// This method decodes the byte array into a string using UTF-8 encoding and then deserializes 
        /// that string into an object of type T. It relies on a method named JsonObject<T>() to perform
        /// the deserialization process.
        /// </remarks>
        [return: MaybeNull]
        public static T? ToObject<T>([NotNull] this byte[]? bytes) => bytes == null ? default : Encoding.UTF8.GetString(bytes).JsonObject<T>();

        /// <summary>
        /// Converts a byte array to an object of a specified type.
        /// </summary>
        /// <param name="bytes">The byte array to convert, or null.</param>
        /// <param name="type">The type of the object to convert to.</param>
        /// <returns>
        /// Returns an object of the specified type if the byte array is not null; otherwise, returns null.
        /// The returned object may be null, depending on the JSON deserialization.
        /// </returns>
        /// <remarks>
        /// The method uses UTF-8 encoding to convert the byte array to a string,
        /// and then deserializes the JSON string into an object of the specified type.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="type"/> parameter is null.
        /// </exception>
        [return: MaybeNull]
        public static object? ToObject([NotNull] this byte[]? bytes, Type type) => bytes == null ? null : Encoding.UTF8.GetString(bytes).JsonObject(type);


        /// <summary>
        /// Converts a nullable string into a nullable Stream. 
        /// If the input string is null, the method returns null. 
        /// Otherwise, it first converts the string to a byte array 
        /// and then converts that byte array into a Stream.
        /// </summary>
        /// <param name="source">The string to be converted to a Stream. It can be null.</param>
        /// <returns>A Stream representation of the input string if it is not null; otherwise, null.</returns>
        /// <remarks>
        /// The method uses extension methods `ToBytes()` and `ToStream()` 
        /// which are assumed to be defined elsewhere in the code.
        /// </remarks>
        [return: MaybeNull]
        public static Stream? ToStream([NotNull] this string? source) => source?.ToBytes()?.ToStream();

        /// <summary>
        /// Converts the specified <see cref="Stream"/> to a byte array.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to convert. This parameter may be null.</param>
        /// <returns>
        /// A byte array containing the contents of the stream, or null if the stream is null or cannot be read.
        /// </returns>
        [return: MaybeNull]
        public static byte[]? ToBytes([MaybeNull] this Stream? stream)
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

        /// <summary>
        /// Converts a byte array to a memory stream.
        /// </summary>
        /// <param name="bytes">
        /// A byte array that is converted to a <see cref="MemoryStream"/>. 
        /// Can be null in which case the method returns null.
        /// </param>
        /// <returns>
        /// A <see cref="Stream"/> representing the byte array, or null if the byte array is null.
        /// </returns>
        /// <remarks>
        /// This method uses the <see cref="MemoryStream"/> class to create a stream 
        /// from the provided byte array. If the input byte array is null, the method 
        /// will return null instead of creating a stream.
        /// </remarks>
        /// <example>
        /// <code>
        /// byte[] data = Encoding.UTF8.GetBytes("Hello, world!");
        /// using Stream stream = data.ToStream();
        /// // Use the stream as needed
        /// </code>
        /// </example>
        [return: MaybeNull]
        public static Stream? ToStream([MaybeNull] this byte[]? bytes) => bytes == null ? null : new MemoryStream(bytes);

        /// <summary>
        /// Converts a nullable integer to a byte array. 
        /// If the integer is null, it returns null; otherwise, it converts the integer to a byte array.
        /// </summary>
        /// <param name="int">The nullable integer to convert.</param>
        /// <returns>
        /// A byte array representing the integer, or null if the integer is null.
        /// </returns>
        [return: MaybeNull]
        public static byte[]? ToBytes(this int? @int) => @int == null ? null : BitConverter.GetBytes(@int.Value);

        /// <summary>
        /// Converts an optional nullable long value to a byte array.
        /// </summary>
        /// <param name="long">The nullable long value to convert.</param>
        /// <returns>
        /// A byte array representing the long value if it is not null; otherwise, null.
        /// </returns>
        /// <remarks>
        /// This method uses the <see cref="BitConverter.GetBytes(long)"/> method to perform the 
        /// conversion. If the input value is null, the method returns null.
        /// </remarks>
        [return: MaybeNull]
        public static byte[]? ToBytes(this long? @long) => @long == null ? null : BitConverter.GetBytes(@long.Value);

        /// <summary>
        /// Converts an array of bytes to a nullable Int32.
        /// </summary>
        /// <param name="bytes">An array of bytes to convert. It must be exactly 4 bytes long.</param>
        /// <returns>
        /// Returns the converted Int32 value if the input byte array is valid; otherwise, returns null if the input is null.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the length of the byte array is not equal to 4.
        /// </exception>
        [return: MaybeNull]
        public static int? ToInt32(this byte[]? bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            if (bytes.Length != 4)
            {
                throw new ArgumentException("FOr convert to Int32 need 4 bytes.");
            }

            return BitConverter.ToInt32(bytes);
        }

        /// <summary>
        /// Converts an array of bytes to a nullable long (Int64).
        /// </summary>
        /// <param name="bytes">An optional array of bytes that should be converted to a long. 
        /// The array must contain exactly 8 bytes.</param>
        /// <returns>
        /// Returns a long? (nullable long) if the input byte array is valid; otherwise, returns null 
        /// if the input byte array is null. Throws an ArgumentException if the byte array does not 
        /// contain exactly 8 bytes.
        /// </returns>
        [return: MaybeNull]
        public static long? ToInt64(this byte[]? bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            if (bytes.Length != 8)
            {
                throw new ArgumentException("FOr convert to Int32 need 8 bytes.");
            }

            return BitConverter.ToInt64(bytes);
        }




        #region ToSha256Hash

        /// <summary>
        /// Computes the SHA-256 hash of a given string.
        /// </summary>
        /// <param name="source">
        /// The string that needs to be hashed. It can be null, in which case the method will return null.
        /// </param>
        /// <returns>
        /// A hexadecimal string representation of the SHA-256 hash of the input string.
        /// If the input string is null, the method returns null.
        /// </returns>
        [return: MaybeNull]
        public static string? CalculateSha256Hash([MaybeNull] this string? source)
        {
            if (source == null)
                return null;

            var bytes = source.ToBytes();

            var hash = ToSha256Hash(bytes);

            return hash.ToHexString();
        }

        /// <summary>
        /// Calculates the SHA-256 hash for a given byte array.
        /// </summary>
        /// <param name="bytes">
        /// The byte array for which the SHA-256 hash is to be computed. This parameter may be null.
        /// </param>
        /// <returns>
        /// A string representation of the SHA-256 hash in hexadecimal format, or null if the input byte array is null.
        /// </returns>
        [return: MaybeNull]
        public static string? CalculateSha256Hash([MaybeNull] this byte[]? bytes)
        {
            if (bytes == null)
                return null;

            var hash = ToSha256Hash(bytes);

            return hash.ToHexString();
        }

        /// <summary>
        /// Calculates the SHA-256 hash of the given stream.
        /// </summary>
        /// <param name="stream">
        /// The stream to be hashed. This parameter can be null. If it is null, the method returns null.
        /// </param>
        /// <returns>
        /// A hexadecimal string representation of the SHA-256 hash, or null if the input stream is null.
        /// </returns>
        [return: MaybeNull]
        public static string? CalculateSha256Hash([MaybeNull] this Stream? stream)
        {
            if (stream == null)
                return null;

            var hash = ToSha256Hash(stream);

            return hash.ToHexString();
        }

        /// <summary>
        /// Calculates the SHA256 hash of a serialized representation of the specified instance.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the instance to be serialized and hashed.
        /// </typeparam>
        /// <param name="instance">
        /// The instance to be processed. It can be null, in which case the method returns null.
        /// </param>
        /// <returns>
        /// A string representing the SHA256 hash of the serialized instance in hexadecimal format,
        /// or null if the instance is null.
        /// </returns>
        [return: MaybeNull]
        public static string? CalculateSha256Hash<T>([MaybeNull] this T? instance)
        {
            if (instance == null)
                return null;

            var json = System.Text.Json.JsonSerializer.Serialize<T>(instance);

            var hash = ToSha256Hash(json);

            return hash.ToHexString();
        }


        /// <summary>
        /// Converts the given stream to a SHA256 hash.
        /// </summary>
        /// <param name="stream">The input stream to be hashed. This parameter may be null.</param>
        /// <returns>
        /// A byte array containing the SHA256 hash of the stream's contents, 
        /// or null if the input stream is null.
        /// </returns>
        [return: MaybeNull]
        public static byte[]? ToSha256Hash(this Stream? stream)
        {
            if (stream == null)
            {
                return null;
            }

            byte[]? sourceBytes = stream.ToBytes();

            byte[]? hashedBytes = SHA256.HashData(sourceBytes);

            return hashedBytes;
        }

        /// <summary>
        /// Computes the SHA-256 hash of the given byte array.
        /// </summary>
        /// <param name="source">The byte array to be hashed. It can be null.</param>
        /// <returns>
        /// A byte array containing the SHA-256 hash of the input 
        /// byte array. If the input is null, it returns null.
        /// </returns>
        [return: MaybeNull]
        public static byte[]? ToSha256Hash(this byte[]? source)
        {
            if (source == null)
            {
                return null;
            }

            byte[] bytes = SHA256.HashData(source);

            return bytes;
        }

        /// <summary>
        /// Converts a string into a SHA256 hash byte array.
        /// </summary>
        /// <param name="source">
        /// The input string to be hashed. If the input is null, the method will return null.
        /// </param>
        /// <returns>
        /// A byte array representing the SHA256 hash of the input string, or null if the input is null.
        /// </returns>
        [return: MaybeNull]
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
