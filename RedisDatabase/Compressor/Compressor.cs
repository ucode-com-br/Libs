using System.Diagnostics.CodeAnalysis;

namespace UCode.RedisDatabase.Compressor
{
    /// <summary>
    /// Compressor interface
    /// </summary>
    public interface ICompressor
    {
        /// <summary>
        /// Compress
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [return: NotNull]
        byte[] Compress([NotNull] byte[]? data);

        /// <summary>
        /// Decompress
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [return: NotNull]
        byte[] Decompress([NotNull] byte[]? data);
    }

}
