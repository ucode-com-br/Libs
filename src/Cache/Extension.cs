using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace UCode.Cache
{
    /// <summary>
    /// A static class that contains extension methods for enhancing existing types.
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// Adds Redis cache services to the specified IServiceCollection.
        /// </summary>
        /// <param name="services">
        /// The collection of services to which the Redis cache service will be added.
        /// </param>
        /// <param name="connectionString">
        /// The connection string used to configure the Redis cache.
        /// </param>
        /// <returns>
        /// The original IServiceCollection with the Redis cache services added.
        /// </returns>
        public static IServiceCollection AddRedisCache([NotNull] this IServiceCollection services,
            string connectionString)
        {
            // Add StackExchange.Redis as the cache provider
            services.AddStackExchangeRedisCache(options => options.ConfigurationOptions = ConfigurationOptions.Parse(connectionString));

            // Add a singleton instance of Redis class
            services.AddSingleton<Redis>();

            return services;
        }
    }
}
