using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;

namespace UCode.Compress
{
    /// <summary>
    /// Contains extension methods for various types to enhance their functionality.
    /// </summary>
    /// <remarks>
    /// This static class cannot be instantiated and is used to define extension methods
    /// which can be called as if they were instance methods on the extended types.
    /// </remarks>
    public static class Extension
    {
        /// <summary>
        /// Copies data from the source stream to the destination stream in blocks of 4096 bytes.
        /// </summary>
        /// <param name="src">The source stream from which to read data. Must not be null.</param>
        /// <param name="dest">The destination stream to which data will be written. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when either src or dest is null.</exception>
        public static void CopyTo([NotNull] this Stream src, [NotNull] Stream dest)
        {
            var bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        /// <summary>
        /// Compresses a byte array using GZip compression.
        /// </summary>
        /// <param name="bytes">The byte array to be compressed. This parameter is required and cannot be null.</param>
        /// <returns>A new byte array that contains the compressed data.</returns>
        public static byte[] Zip([NotNull] this byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    // Copy the contents of the source stream to the compression stream
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        /// <summary>
        /// Decompresses a byte array that has been compressed using GZip compression.
        /// </summary>
        /// <param name="bytes">The byte array to be decompressed. Must not be null.</param>
        /// <returns>
        /// A byte array containing the decompressed data.
        /// </returns>
        public static byte[] Unzip([NotNull] this byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    // Copy the contents of the compression stream to the destination stream
                    CopyTo(gs, mso);
                }

                return mso.ToArray();
            }
        }
    }
}
