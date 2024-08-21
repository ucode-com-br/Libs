using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;

namespace UCode.Compress
{
    public static class Extension
    {
        /// <summary>
        /// Copies the contents of one stream to another.
        /// </summary>
        /// <param name="src">The source stream.</param>
        /// <param name="dest">The destination stream.</param>
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
        /// <param name="bytes">The byte array to compress.</param>
        /// <returns>The compressed byte array.</returns>
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
        /// Decompresses a byte array that was compressed using GZip compression.
        /// </summary>
        /// <param name="bytes">The compressed byte array.</param>
        /// <returns>The decompressed byte array.</returns>
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
