using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTelemetry.Instrumentation.StackExchangeRedis;
using OpenTelemetry.Trace;
using StackExchange.Redis;

namespace UCode.RedisDatabase
{
    public static class Extension
    {
        private static RedisOptions CreateDefaultRedisOptions() => new()
        {
            Serializer = new Serializer.JsonSerializer(),
            Compressor = new Compressor.NoneCompressor()
        };

        private static RedisOptions ConfigureOptions(Action<RedisOptions>? options = null)
        {
            var redisOptions = CreateDefaultRedisOptions();

            options?.Invoke(redisOptions);

            return redisOptions;
        }

        public static IServiceCollection AddRedisCache([NotNull] this IServiceCollection services, [NotNull] string connectionString, Action<RedisOptions>? options = null)
        {
            if (services.Any(x => x.ServiceType == typeof(IDistributedCache)))
            {
                throw new InvalidOperationException($"The type {typeof(IDistributedCache).FullName} is already registered.");
            }

            var redisOptions = ConfigureOptions(options);


            services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = ConfigurationOptions.Parse(connectionString);

                options.ConfigurationOptions.DefaultDatabase = 1;
            });

            services.AddSingleton<RedisDistributedCache>();


            return services;
        }

        public static TracerProviderBuilder AddAppRedisInstrumentation(this TracerProviderBuilder builder, Action<StackExchangeRedisInstrumentationOptions>? action = null)
        {
            builder.AddRedisInstrumentation(o =>
            {
                o.SetVerboseDatabaseStatements = true;
                o.Enrich = (activity, command) => { activity.DisplayName = $"Redis ({command.Command})"; };
                action?.Invoke(o);
            });
            return builder;
        }

        /// <summary>
        /// For one database, injecttion of "RedisDefaultDatabase"
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IServiceCollection AddRedisDatabase([NotNull] this IServiceCollection services, [NotNull] Action<IServiceProvider, ConstructorArguments> configure)
        {
            if (services.Any(x => x.ServiceType == typeof(RegistedDatabase)))
            {
                throw new InvalidOperationException($"The type {typeof(RegistedDatabase).FullName} is already registered.");
            }

            var redisOptions = ConfigureOptions();


            return services.AddScoped<RegistedDatabase>((serviceProvider) =>
            {
                var constructorArguments = new ConstructorArguments(redisOptions);

                var loggerFactory = serviceProvider.GetService<ILoggerFactory>() ?? new NullLoggerFactory();

                configure.Invoke(serviceProvider, constructorArguments);

                return new RedisDefaultDatabase(loggerFactory, constructorArguments.ConnectionString, constructorArguments.Database, constructorArguments.RedisOptions);
            });
        }

        /// <summary>
        /// For multiple database, injection of "T". Than "T" is "RegistedDatabase"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="implementationFactory"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IServiceCollection AddRedisDatabase<T>([NotNull] this IServiceCollection services,
            [NotNull] Func<IServiceProvider, T> implementationFactory,
            Action<RedisOptions>? options = null) where T : RegistedDatabase
        {
            if (services.Any(x => x.ServiceType == typeof(T)))
            {
                throw new InvalidOperationException($"The type {typeof(T).FullName} is already registered.");
            }

            var redisOptions = ConfigureOptions(options);

            services.AddScoped(implementationFactory);

            return services;

        }


        public record ConstructorArguments
        {
            internal ConstructorArguments([NotNull] RedisOptions redisOptions) => this.RedisOptions = redisOptions;

            public string ConnectionString
            {
                get; set;
            }

            public string Database
            {
                get; set;
            }

            public RedisOptions RedisOptions
            {
                get;
            }
        }


        public static IServiceCollection AddRedisDatabase<T>([NotNull] this IServiceCollection services, [NotNull] Action<IServiceProvider, ConstructorArguments> configure) where T : RegistedDatabase
        {
            if (services.Any(x => x.ServiceType == typeof(T)))
            {
                throw new InvalidOperationException($"The type {typeof(T).FullName} is already registered.");
            }

            var redisOptions = ConfigureOptions();

            services.AddScoped((serviceProvider) =>
            {
                var constructorArguments = new ConstructorArguments(redisOptions);

                var loggerFactory = serviceProvider.GetService<ILoggerFactory>() ?? new NullLoggerFactory();

                configure.Invoke(serviceProvider, constructorArguments);

                return (T)Activator.CreateInstance(typeof(T), loggerFactory, constructorArguments.ConnectionString, constructorArguments.Database, constructorArguments.RedisOptions);
            });

            return services;

        }
    }
}
