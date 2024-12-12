namespace UCode.RedisDatabase.Compressor
{
    /// <summary>
    /// This interface defines the contract for a compressor that can compress data.
    /// Implementing classes should provide the necessary methods to perform compression operations.
    /// </summary>
    public interface ICompressor
    {
        /// <summary>
        /// Compresses the given byte array to reduce its size.
        /// </summary>
        /// <param name="data">The byte array to compress. This can be null, in which case null will be returned.</param>
        /// <returns>
        /// A byte array that contains the compressed data, or null if the input data is null.
        /// </returns>
        byte[]? Compress(byte[]? data);


        /// <summary>
        /// Decompresses the provided byte array of compressed data.
        /// </summary>
        /// <param name="data">An optional byte array containing the compressed data to be decompressed. 
        /// This can be <c>null</c>, in which case the method will return <c>null</c>.</param>
        /// <returns>
        /// A byte array containing the decompressed data, or <c>null</c> if the input 
        /// data is <c>null</c>.</returns>
        byte[]? Decompress(byte[]? data);

    }

}
