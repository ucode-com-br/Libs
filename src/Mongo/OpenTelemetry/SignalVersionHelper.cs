using System.Reflection;

namespace UCode.Mongo.OpenTelemetry
{

    /// <summary>
    /// Provides utility methods for handling signal versions.
    /// </summary>
    /// <remarks>
    /// This class contains static methods that assist with determining and manipulating
    /// different versions of signals used in the application.
    /// </remarks>
    internal static class SignalVersionHelper
    {
        /// <summary>
        /// Retrieves the informational version of the assembly that contains the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The type whose assembly's informational version will be retrieved.
        /// </typeparam>
        /// <returns>
        /// A string representing the informational version of the assembly,
        /// excluding any metadata that may follow the '+' character.
        /// </returns>
        public static string GetVersion<T>()
        {
            return typeof(T).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion.Split('+')[0];
        }
    }
}
