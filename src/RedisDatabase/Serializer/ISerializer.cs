using System.Diagnostics.CodeAnalysis;

namespace UCode.RedisDatabase.Serializer
{
    /// <summary>
    /// Serializer interface
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serialize object to byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        [return: NotNull]
        byte[] Serialize<T>([NotNull] T source);

        /// <summary>
        /// Deserialize byte array to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        [return: NotNull]
        T Deserialize<T>([NotNull] byte[]? source);
    }



}
