using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace UCode.Cache
{
    public static class Extension
    {
        public static IServiceCollection AddRedisCache([NotNull] this IServiceCollection services,
            string connectionString)
        {
            services.AddStackExchangeRedisCache(options => options.ConfigurationOptions = ConfigurationOptions.Parse(connectionString));

            services.AddSingleton<Redis>();

            return services;
        }
    }
}
