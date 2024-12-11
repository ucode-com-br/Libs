using System.Reflection;

namespace UCode.Mongo.OpenTelemetry
{


    internal static class SignalVersionHelper
    {
        public static string GetVersion<T>()
        {
            return typeof(T).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion.Split('+')[0];
        }
    }
}
