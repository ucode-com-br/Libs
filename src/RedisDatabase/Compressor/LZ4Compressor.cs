using System.Diagnostics.CodeAnalysis;

namespace UCode.RedisDatabase.Compressor
{
    public class LZ4Compressor : ICompressor
    {
        /// <summary>
        /// Compress using LZ4 algorithm with level 0 (fastest)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [return: NotNull]
        public byte[] Compress([NotNull] byte[] data)
        {
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
        /// Compress using LZ4 algorithm with level 0 (fastest)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [return: NotNull]
        public byte[] Decompress([NotNull] byte[] data)
        {
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
