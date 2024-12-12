using System.Diagnostics.CodeAnalysis;

namespace UCode.RedisDatabase.Compressor
{
    /// <summary>
    /// Represents a compressor that utilizes the LZ4 algorithm for data compression.
    /// </summary>
    /// <remarks>
    /// This class implements the ICompressor interface, providing methods for
    /// compressing and decompressing data using the LZ4 compression scheme.
    /// </remarks>
    public class LZ4Compressor : ICompressor
    {
        /// <summary>
        /// Compresses the given byte array using LZ4 compression.
        /// </summary>
        /// <param name="data">
        /// The byte array to compress. This parameter can be null, in which case the method will return null.
        /// </param>
        /// <returns>
        /// A compressed byte array, or null if the input data is null. If the input data is empty,
        /// an empty byte array will be returned.
        /// </returns>
        public byte[]? Compress(byte[]? data)
        {
            if (data == null)
            {
                return null;
            }

            if (data.Length == 0)
            {
                return System.Array.Empty<byte>();
            }

            /*var target = new byte[K4os.Compression.LZ4.LZ4Codec.MaximumOutputSize(source.Length)];
            var encodedLength = K4os.Compression.LZ4.LZ4Codec.Encode(source, target, K4os.Compression.LZ4.LZ4Level.L00_FAST);

            var result = new byte[encodedLength];
            target.CopyTo(result, 0);

            return result;*/

            return K4os.Compression.LZ4.LZ4Pickler.Pickle(data, K4os.Compression.LZ4.LZ4Level.L00_FAST);
        }

        /// <summary>
        /// Decompresses the given byte array using the LZ4 algorithm.
        /// </summary>
        /// <param name="data">The compressed byte array to be decompressed. If it is null or empty, a null or empty array will be returned, respectively.</param>
        /// <returns>
        /// A byte array containing the decompressed data. Returns null if the input data is null; 
        /// returns an empty byte array if the input data is empty.
        /// </returns>
        public byte[]? Decompress(byte[]? data)
        {
            if (data == null)
            {
                return null;
            }

            if (data.Length == 0)
            {
                return System.Array.Empty<byte>();
            }

            return K4os.Compression.LZ4.LZ4Pickler.Unpickle(data);
            /*var buffer = new byte[K4os.Compression.LZ4.LZ4Codec.MaximumOutputSize(data.Length)];
            var decodedLength = K4os.Compression.LZ4.LZ4Codec.Decode(data, buffer);

            var bytes = new byte[decodedLength];
            buffer.CopyTo(bytes, 0);

            return bytes;*/
        }
    }
}
