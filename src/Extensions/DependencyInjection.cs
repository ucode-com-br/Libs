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

    /// <summary>
    /// This class is responsible for handling dependency injection configurations and services.
    /// </summary>
    /// <remarks>
    /// It provides functionality to register and resolve services required by the application.
    /// </remarks>
    public static class DependencyInjection
    {
        private static ServiceProvider? __serviceProvider;

        private static ILoggerFactory? __loggerFactory;
        /// <summary>
        /// Gets the instance of the <see cref="ILoggerFactory"/>. If a logger factory has not been 
        /// initialized, it attempts to retrieve one from the service provider. If no logger factory 
        /// is available, it returns a <see cref="NullLoggerFactory"/> as a fallback.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="ILoggerFactory"/> if available; otherwise, a <see cref="NullLoggerFactory"/>.
        /// </returns>
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
        /// <summary>
        /// Gets the logger instance for dependency injection in the UCode.Extensions namespace.
        /// </summary>
        /// <remarks>
        /// The logger is created using the LoggerFactory if it has not been instantiated yet.
        /// </remarks>
        /// <returns>
        /// An instance of <see cref="ILogger"/> if available; otherwise, <c>null</c>.
        /// </returns>
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





        /// <summary>
        /// Sets a logger instance for the specified generic type T.
        /// </summary>
        /// <typeparam name="T">The type for which the logger is created.</typeparam>
        /// <param name="logger">The logger instance to assign the created logger to. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when the logger parameter is null.</exception>
        /// <remarks>
        /// This method uses the <c>CreateLogger&lt;T&gt;</c> method to create a new logger instance 
        /// for the specified type T and assigns it to the provided logger parameter.
        /// </remarks>
        public static void SetLogger<T>([NotNull] this ILogger<T> logger) => logger = CreateLogger<T>();

        /// <summary>
        /// Sets the logger to an instance of a logger created for the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The type for which the logger is created.
        /// </typeparam>
        /// <param name="logger">
        /// An instance of <see cref="ILogger"/> that will be set to a new logger instance.
        /// This parameter is marked with <see cref="NotNullAttribute"/> indicating it should not be null.
        /// </param>
        /// <remarks>
        /// This method is an extension method for the <see cref="ILogger"/> interface.
        /// It modifies the logger reference to point to a logger created specifically for the 
        /// type specified by <typeparamref name="T"/>.
        /// </remarks>
        public static void SetLogger<T>([NotNull] this ILogger logger) => logger = CreateLogger<T>();

        /// <summary>
        /// Sets a logger for the specified instance using the provided expression to determine the logger type.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the instance that the logger will be set for.
        /// </typeparam>
        /// <param name="instance">
        /// The instance for which the logger is to be set. It cannot be null.
        /// </param>
        /// <param name="expression">
        /// An expression that specifies how to retrieve the logger from the instance.
        /// This expression should evaluate to an <see cref="ILogger{T}"/> for the given type.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="instance"/> parameter is null.
        /// </exception>
        /// <remarks>
        /// This method is an extension method that allows setting up logging for 
        /// instances of type <typeparamref name="T"/>. It leverages the <see cref="ILogger{T}"/>
        /// interface to provide structured logging capabilities.
        /// </remarks>
        public static void SetLogger<T>([NotNull] this T instance, Expression<Func<T, ILogger<T>>> expression) => x(instance, expression);
        

        /// <summary>
        /// Sets a logger for the specified instance's property or field that is identified by the given expression.
        /// The method uses reflection to determine if the property is writable, and if not, it attempts to find and set
        /// the corresponding backing field for the property. 
        /// </summary>
        /// <typeparam name="T">The type of the instance that contains the property or field.</typeparam>
        /// <typeparam name="TE">The type of the delegate used in the expression.</typeparam>
        /// <param name="instance">The instance on which the property or field is set.</param>
        /// <param name="expression">An expression that identifies the property or field to set the logger for.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the member specified in the expression is neither a property nor a field,
        /// or if the expression is not a member access expression.
        /// </exception>
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

        /// <summary>
        /// Sets a logger for the specified instance using the provided expression.
        /// </summary>
        /// <typeparam name="T">The type of the instance to set the logger for.</typeparam>
        /// <param name="instance">The instance on which to set the logger.</param>
        /// <param name="expression">An expression that specifies how to obtain the logger.</param>
        /// <remarks>
        /// This method is an extension method, allowing you to call it directly on any instance of type T.
        /// It utilizes the provided expression to configure the logger appropriately.
        /// </remarks>
        public static void SetLogger<T>(this T instance, Expression<Func<T, ILogger>> expression) => x(instance, expression);
        



        /// <summary>
        /// Creates a logger instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The type for which the logger is being created.</typeparam>
        /// <returns>
        /// A logger instance of type <see cref="ILogger{T}"/> if creation is successful; otherwise, <c>null</c>.
        /// </returns>
        public static ILogger<T>? CreateLogger<T>()
        {
            Logger.LogDebug($"Call: {nameof(CreateLogger)}");

            var r = TryCreateLogger<T>(out var logger) ? logger : null;

            return r;
        }

        /// <summary>
        /// Attempts to create a logger of the specified generic type.
        /// </summary>
        /// <typeparam name="T">The type for which the logger is being created.</typeparam>
        /// <param name="logger">The created logger if successful; otherwise, null.</param>
        /// <returns>
        /// Returns true if the logger was successfully created; otherwise, false.
        /// </returns>
        public static bool TryCreateLogger<T>(out ILogger<T>? logger)
        {
            Logger.LogDebug($"Call: {nameof(TryCreateLogger)}");

            logger = logger = LoggerFactory.CreateLogger<T>();

            return logger != null;
        }



        /// <summary>
        /// Stores the given <see cref="ServiceProvider"/> instance for future use.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="ServiceProvider"/> instance to be stored.</param>
        /// <returns>
        /// The stored <see cref="ServiceProvider"/> instance. 
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when an attempt is made to store a <see cref="ServiceProvider"/> instance
        /// while another instance has already been set.
        /// </exception>
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
        /// Builds a global service provider from the specified service collection.
        /// </summary>
        /// <param name="services">The collection of service descriptors to build the service provider from.</param>
        /// <returns>A service provider that can be used to resolve service instances.</returns>
        /// <remarks>
        /// This method is an extension method for the <see cref="IServiceCollection"/> interface. 
        /// It facilitates the creation of a service provider, which acts as a container for dependency injection.
        /// </remarks>
        public static ServiceProvider BuildGlobalServiceProvider(this IServiceCollection services) => __serviceProvider = ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(services);


        /// <summary>
        /// Builds a <see cref="ServiceProvider"/> from the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> that contains the services to be registered.
        /// </param>
        /// <returns>
        /// A <see cref="ServiceProvider"/> that provides the registered services.
        /// </returns>
        public static ServiceProvider BuildServiceProvider(this IServiceCollection services) => __serviceProvider = ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(services);

        /// <summary>
        /// Builds a service provider from the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to build the service provider from.</param>
        /// <param name="validateScopes">A boolean indicating whether to validate the service scopes.</param>
        /// <returns>An <see cref="IServiceProvider"/> representing the built service provider.</returns>
        /// <remarks>
        /// This method is an extension method for <see cref="IServiceCollection"/> and can be used to 
        /// create an instance of <see cref="IServiceProvider"/> that resolves services from the 
        /// provided <paramref name="services"/> collection. The <paramref name="validateScopes"/> 
        /// parameter specifies whether the service provider should throw an exception if a 
        /// service is requested outside of its defined scope.
        /// </remarks>
        public static ServiceProvider BuildServiceProvider(this IServiceCollection services, bool validateScopes) => __serviceProvider = ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(services, validateScopes);

        /// <summary>
        /// Builds a <see cref="ServiceProvider"/> from the provided <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> containing the service registrations.</param>
        /// <param name="options">Options to configure the <see cref="ServiceProvider"/> behavior.</param>
        /// <returns>A <see cref="ServiceProvider"/> instance that can be used to resolve services.</returns>
        /// <remarks>
        /// This method is an extension method for <see cref="IServiceCollection"/> and enables
        /// easy creation of a <see cref="ServiceProvider"/> with specified options.
        /// </remarks>
        public static ServiceProvider BuildServiceProvider(this IServiceCollection services, ServiceProviderOptions options) => __serviceProvider = ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(services, options);
    }
}
