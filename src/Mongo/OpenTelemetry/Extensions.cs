using OpenTelemetry.Trace;

namespace UCode.Mongo.OpenTelemetry
{
    /// <summary>
    /// Provides extension methods for various types.
    /// </summary>
    /// <remarks>
    /// This static class cannot be instantiated.
    /// </remarks>
    public static class Extensions
    {
        /// <summary>
        /// Extends the TracerProviderBuilder to add MongoDB instrumentation.
        /// This method configures the tracer to listen to a specific source
        /// for telemetry events related to MongoDB operations.
        /// </summary>
        /// <param name="builder">
        /// The TracerProviderBuilder instance to which the MongoDB instrumentation will be added.
        /// </param>
        /// <returns>
        /// Returns the updated TracerProviderBuilder instance for further configuration.
        /// </returns>
        public static TracerProviderBuilder AddAppMongoInstrumentation(this TracerProviderBuilder builder)
        {
            return builder.AddSource(EventSubscriber.ActivitySourceName);
        }
    }
}
