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
    /// <summary>
    /// Provides extension methods for enhancing existing types.
    /// </summary>
    /// <remarks>
    /// This static class contains extension methods that allow users 
    /// to add new functionality to existing types without modifying 
    /// their source code or creating a new derived type.
    /// </remarks>
    public static class Extension
    {
        /// <summary>
        /// Creates the default Redis options configuration.
        /// </summary>
        /// <returns>
        /// Returns an instance of <see cref="RedisOptions"/> configured with default settings.
        /// </returns>
        private static RedisOptions CreateDefaultRedisOptions() => new()
        {
            Serializer = new Serializer.JsonSerializer(),
            Compressor = new Compressor.NoneCompressor()
        };

        /// <summary>
        /// Configures Redis options by creating default options and allowing for custom configurations.
        /// </summary>
        /// <param name="options">
        /// An optional action to configure the RedisOptions. If no action is provided, the default options are returned.
        /// </param>
        /// <returns>
        /// The configured instance of <see cref="RedisOptions"/>.
        /// </returns>
        private static RedisOptions ConfigureOptions(Action<RedisOptions>? options = null)
        {
            var redisOptions = CreateDefaultRedisOptions();

            options?.Invoke(redisOptions);

            return redisOptions;
        }

        /// <summary>
        /// Adds Redis cache services to the specified IServiceCollection, 
        /// configuring the cache with the provided connection string 
        /// and optional settings.
        /// </summary>
        /// <param name="services">The IServiceCollection to which the Redis cache services will be added.</param>
        /// <param name="connectionString">The connection string to connect to the Redis server.</param>
        /// <param name="options">An optional action to configure additional Redis options.</param>
        /// <returns>The updated IServiceCollection with the Redis cache services registered.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when an IDistributedCache service is already registered in the services collection.
        /// </exception>
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

        /// <summary>
        /// Adds Redis instrumentation to the specified <see cref="TracerProviderBuilder"/>.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="TracerProviderBuilder"/> to which Redis instrumentation will be added.
        /// </param>
        /// <param name="action">
        /// An optional action to customize the <see cref="StackExchangeRedisInstrumentationOptions"/>.
        /// </param>
        /// <returns>
        /// The modified <see cref="TracerProviderBuilder"/> with Redis instrumentation added.
        /// </returns>
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
        /// Adds a Redis database service to the specified service collection. 
        /// This method checks if the Redis database has already been registered; 
        /// if it has, an InvalidOperationException is thrown.
        /// </summary>
        /// <param name="services">The service collection to which the Redis database service will be added.</param>
        /// <param name="configure">An action to configure the service with the specified service provider and constructor arguments.</param>
        /// <returns>An updated <see cref="IServiceCollection"/> containing the added Redis database service.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the <see cref="RegistedDatabase"/> type is already registered.</exception>
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
        /// Registers a Redis database implementation in the given service collection.
        /// </summary>
        /// <typeparam name="T">The type of the database that extends <see cref="RegistedDatabase"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the database implementation will be added.</param>
        /// <param name="implementationFactory">A factory method to create an instance of the database type.</param>
        /// <param name="options">An optional action to configure Redis options.</param>
        /// <returns>The <see cref="IServiceCollection"/> with the registered database implementation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the type <typeparamref name="T"/> is already registered in the <paramref name="services"/>.</exception>
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
            /// <summary>
            /// Initializes a new instance of the <see cref="ConstructorArguments"/> class.
            /// </summary>
            /// <param name="redisOptions">The Redis options to configure the connection.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="redisOptions"/> is <c>null</c>.</exception>
            internal ConstructorArguments([NotNull] RedisOptions redisOptions) => this.RedisOptions = redisOptions;

            /// <summary>
            /// Represents the connection string used to connect to a database.
            /// This property allows getting and setting the connection string value.
            /// </summary>
            /// <value>
            /// A <see cref="string"/> representing the connection string.
            /// </value>
            public string ConnectionString
            {
                get; set;
            }

            /// <summary>
            /// Gets or sets the name of the database.
            /// </summary>
            /// <value>
            /// A <see cref="string"/> representing the name of the database.
            /// </value>
            public string Database
            {
                get; set;
            }

            /// <summary>
            /// Gets the Redis options associated with this instance.
            /// </summary>
            /// <value>
            /// The Redis options.
            /// </value>
            public RedisOptions RedisOptions
            {
                get;
            }
        }


        /// <summary>
        /// Adds a Redis database service to the specified <see cref="IServiceCollection"/>.
        /// This method registers the service of type <typeparamref name="T"/> only if it is not already registered.
        /// </summary>
        /// <typeparam name="T">The type of the database that is being registered, which must derive from <see cref="RegistedDatabase"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the Redis database service will be added.</param>
        /// <param name="configure">An <see cref="Action{IServiceProvider, ConstructorArguments}"/> delegate to configure additional parameters for the database upon registration.</param>
        /// <returns>The updated <see cref="IServiceCollection"/> with the Redis database service added.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the service type <typeparamref name="T"/> is already registered in the <paramref name="services"/>.</exception>
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
