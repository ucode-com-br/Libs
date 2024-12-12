using System.Diagnostics.CodeAnalysis;
using UCode.RedisDatabase.Compressor;
using UCode.RedisDatabase.Serializer;

namespace UCode.RedisDatabase
{
    public record RedisOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisOptions"/> class.
        /// This constructor is responsible for setting up any default values 
        /// or performing initialization tasks for the RedisOptions class.
        /// </summary>
        public RedisOptions()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisOptions"/> class.
        /// </summary>
        /// <param name="serializer">The serializer used for serializing data.</param>
        /// <param name="compressor">The compressor used for compressing data.</param>
        /// <returns></returns>
        public RedisOptions(ISerializer serializer, ICompressor compressor)
        {
            this.Serializer = serializer;
            this.Compressor = compressor;
        }

        /// <summary>
        /// Gets or sets the serializer used for serialization purposes.
        /// </summary>
        /// <value>
        /// An instance of <see cref="ISerializer"/> that handles the serialization process.
        /// </value>
        /// <remarks>
        /// The property is marked with <see cref="NotNullAttribute"/> to indicate that it should not be null when used.
        /// </remarks>
        [NotNull]
        public ISerializer Serializer
        {
            get; set;
        }

        /// <summary>
        /// Represents the compressor used for compression operations.
        /// </summary>
        /// <remarks>
        /// This property is marked with the <see cref="NotNull"/> attribute, indicating that it should not be null.
        /// </remarks>
        /// <value>
        /// An instance of <see cref="ICompressor"/> that handles compression functionality.
        /// </value>
        [NotNull]
        public ICompressor Compressor
        {
            get; set;
        }
    }
}
