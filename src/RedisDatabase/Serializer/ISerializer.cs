using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        byte[]? Serialize<T>(T? source);

        /// <summary>
        /// Deserialize byte array to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        T? Deserialize<T>(byte[]? source);
    }



}
