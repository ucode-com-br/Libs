using System.Diagnostics.CodeAnalysis;

namespace UCode.RedisDatabase.Compressor
{
    /// <summary>
    /// Not use compression
    /// </summary>
    public class NoneCompressor : ICompressor
    {
        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [return: NotNull]
        public byte[] Compress([NotNull] byte[] data) => data;

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [return: NotNull]
        public byte[] Decompress([NotNull] byte[] data) => data;
    }
}
