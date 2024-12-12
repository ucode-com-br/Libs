using System.Diagnostics.CodeAnalysis;

namespace UCode.RedisDatabase.Compressor
{
    /// <summary>
    /// Represents a NoneCompressor class that implements the ICompressor interface.
    /// This compressor performs no compression on the data.
    /// </summary>
    /// <remarks>
    /// This implementation can be useful in scenarios where you want to use a 
    /// consistent interface for different types of compressors but do not wish 
    /// to apply any compression to the data.
    /// </remarks>
    public class NoneCompressor : ICompressor
    {
        /// <summary>
        /// Compresses the given array of bytes.
        /// </summary>
        /// <param name="data">The array of bytes to compress. This parameter cannot be null.</param>
        /// <returns>
        /// Returns a compressed byte array, or null if the input data is null.
        /// </returns>
        public byte[]? Compress([NotNull] byte[]? data) => data;

        /// <summary>
        /// Decompresses the provided byte array. Currently, this method is a placeholder 
        /// and returns the input data without any modification.
        /// </summary>
        /// <param name="data">The byte array to be decompressed. This parameter cannot be null.</param>
        /// <returns>A byte array that is either the original input data or null if the input was null.</returns>
        public byte[]? Decompress([NotNull] byte[]? data) => data;
    }
}
