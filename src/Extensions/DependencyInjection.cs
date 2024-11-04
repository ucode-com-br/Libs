using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace UCode.Extensions
{

    public static class DependencyInjection
    {
        private static ServiceProvider? __serviceProvider;

        private static ILoggerFactory? __loggerFactory;
        private static ILoggerFactory? LoggerFactory
        {
            get
            {
                if (__loggerFactory == null && __serviceProvider?.GetService<ILoggerFactory>() is ILoggerFactory loggerFactory && loggerFactory != null)
                {
                    __loggerFactory = loggerFactory;
                }

                return __loggerFactory ?? new NullLoggerFactory();
            }
        }

        private static ILogger? __logger;
        private static ILogger? Logger
        {
            get
            {
                if (LoggerFactory != null && __logger == null)
                {
                    __logger = LoggerFactory.CreateLogger($"{nameof(UCode)}.{nameof(Extensions)}.{nameof(DependencyInjection)}");
                }

                return __logger;
            }
        }





        public static void SetLogger<T>([NotNull] this ILogger<T> logger) => logger = CreateLogger<T>();

        public static void SetLogger<T>([NotNull] this ILogger logger) => logger = CreateLogger<T>();

        public static void SetLogger<T>([NotNull] this T instance, Expression<Func<T, ILogger<T>>> expression) => x(instance, expression);
        

        private static void x<T, TE>(T instance, Expression<TE> expression) where TE : Delegate
        {
            Logger.LogDebug($"Call: {nameof(SetLogger)}<T>(...) with ILogger");


            var body = expression.Body as MemberExpression;
            var member = body.Member;

            if (member != null)
            {
                if (member is PropertyInfo propertyInfo)
                {
                    if (propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(instance, LoggerFactory.CreateLogger<T>());
                    }
                    else
                    {
                        var bf = "__BackingField";
                        var nonPublicFields = propertyInfo.DeclaringType?.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(w => w.FieldType == propertyInfo.PropertyType);

                        //find absolute value "<Logger>k__BackingField"
                        if (nonPublicFields.FirstOrDefault(w => w.Name.Equals($"<{propertyInfo.Name}>k{bf}", StringComparison.OrdinalIgnoreCase)) is var firstAbsolute && firstAbsolute != null)
                        {
                            firstAbsolute.SetValue(instance, LoggerFactory.CreateLogger<T>());
                        }
                        else
                        {
                            //find relative value "*<Logger>*__BackingField*"
                            var backingFields = nonPublicFields.Where(w => (w.Name?.IndexOf(bf) ?? -1) >= 0).ToArray() ?? Array.Empty<FieldInfo>();

                            if (backingFields.FirstOrDefault(f => (f.Name?.IndexOf($"<{propertyInfo.Name}>") ?? -1) >= 0) is var firstOrDefault && firstOrDefault != null)
                            {
                                firstOrDefault.SetValue(instance, LoggerFactory.CreateLogger<T>());
                            }
                        }
                    }
                }
                else if (member is FieldInfo fieldInfo)
                {
                    fieldInfo.SetValue(instance, LoggerFactory.CreateLogger<T>());
                }
                else
                {
                    throw new ArgumentException("The member need a property or field", nameof(expression));
                }
            }
            else
            {

                throw new ArgumentException("Expression is not a member access", nameof(expression));
            }
        }

        public static void SetLogger<T>(this T instance, Expression<Func<T, ILogger>> expression) => x(instance, expression);
        



        /// <summary>
        /// Create logger
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ILogger<T>? CreateLogger<T>()
        {
            Logger.LogDebug($"Call: {nameof(CreateLogger)}");

            var r = TryCreateLogger<T>(out var logger) ? logger : null;

            return r;
        }

        /// <summary>
        /// Try create logger
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static bool TryCreateLogger<T>(out ILogger<T>? logger)
        {
            Logger.LogDebug($"Call: {nameof(TryCreateLogger)}");

            logger = logger = LoggerFactory.CreateLogger<T>();

            return logger != null;
        }



        /// <summary>
        /// Creates a <see cref="ServiceProvider"/> containing services from the provided <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceCollection"/> containing service descriptors.</param>
        /// <returns>The <see cref="ServiceProvider"/>.</returns>
        public static ServiceProvider StoreServiceProvider(this ServiceProvider serviceProvider)
        {
            Logger.LogDebug($"Call: {nameof(StoreServiceProvider)}");

            if (__serviceProvider != null)
            {
                throw new InvalidOperationException("ServiceProvider already set.");
            }

            return __serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates a <see cref="ServiceProvider"/> containing services from the provided <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> containing service descriptors.</param>
        /// <returns>The <see cref="ServiceProvider"/>.</returns>
        public static ServiceProvider BuildGlobalServiceProvider(this IServiceCollection services) => __serviceProvider = ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(services);


        /// <summary>
        /// Creates a <see cref="ServiceProvider"/> containing services from the provided <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> containing service descriptors.</param>
        /// <returns>The <see cref="ServiceProvider"/>.</returns>
        public static ServiceProvider BuildServiceProvider(this IServiceCollection services) => __serviceProvider = ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(services);

        /// <summary>
        /// Creates a <see cref="ServiceProvider"/> containing services from the provided <see cref="IServiceCollection"/>
        /// optionally enabling scope validation.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> containing service descriptors.</param>
        /// <param name="validateScopes">
        /// <c>true</c> to perform check verifying that scoped services never gets resolved from root provider; otherwise <c>false</c>.
        /// </param>
        /// <returns>The <see cref="ServiceProvider"/>.</returns>
        public static ServiceProvider BuildServiceProvider(this IServiceCollection services, bool validateScopes) => __serviceProvider = ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(services, validateScopes);

        /// <summary>
        /// Creates a <see cref="ServiceProvider"/> containing services from the provided <see cref="IServiceCollection"/>
        /// optionally enabling scope validation.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> containing service descriptors.</param>
        /// <param name="options">
        /// Configures various service provider behaviors.
        /// </param>
        /// <returns>The <see cref="ServiceProvider"/>.</returns>
        public static ServiceProvider BuildServiceProvider(this IServiceCollection services, ServiceProviderOptions options) => __serviceProvider = ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(services, options);
    }
}
