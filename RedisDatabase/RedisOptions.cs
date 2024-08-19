using System.Diagnostics.CodeAnalysis;
using UCode.RedisDatabase.Compressor;
using UCode.RedisDatabase.Serializer;

namespace UCode.RedisDatabase
{
    public record RedisOptions
    {
        public RedisOptions()
        {

        }

        public RedisOptions(ISerializer serializer, ICompressor compressor)
        {
            this.Serializer = serializer;
            this.Compressor = compressor;
        }

        /// <summary>
        /// Serializer to use
        /// </summary>
        [NotNull]
        public ISerializer Serializer
        {
            get; set;
        }

        /// <summary>
        /// Compressor to use
        /// </summary>
        [NotNull]
        public ICompressor Compressor
        {
            get; set;
        }
    }
}
