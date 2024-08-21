using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace UCode.Cache
{
    public static class Extension
    {
        /// <summary>
        /// Adds Redis cache to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add the Redis cache to.</param>
        /// <param name="connectionString">The connection string for the Redis cache.</param>
        /// <returns>The modified service collection.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> or <paramref name="connectionString"/> is null or empty.</exception>
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
