using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UCode.Extensions
{
    /// <summary>
    /// Represents a JSON converter that converts objects of type <typeparamref name="TImplementation"/> to and from <typeparamref name="TInterface"/>.
    /// This class allows for easy serialization and deserialization of implementation types to their interface representations.
    /// </summary>
    /// <typeparam name="TImplementation">The type of implementation that implements the interface.</typeparam>
    /// <typeparam name="TInterface">The interface type that the implementation conforms to.</typeparam>
    public class InterfaceConverter<TImplementation, TInterface> : JsonConverter<TInterface> where TImplementation : class, TInterface
    {
        /// <summary>
        /// Reads and deserializes a JSON value into an instance of the specified interface type.
        /// </summary>
        /// <param name="reader">A reference to the Utf8JsonReader that reads the JSON data.</param>
        /// <param name="typeToConvert">The type of the interface to which the JSON will be deserialized.</param>
        /// <param name="options">Serialization options to customize the deserialization process.</param>
        /// <returns>An instance of the interface type, or null if the JSON value was null.</returns>
        /// <exception cref="JsonException">Thrown when there is an error during deserialization.</exception>
        public override TInterface? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => JsonSerializer.Deserialize<TImplementation>(ref reader, options);

        /// <summary>
        /// Writes the specified interface value as JSON to the specified <see cref="Utf8JsonWriter"/>.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="Utf8JsonWriter"/> to write the JSON data to.
        /// </param>
        /// <param name="value">
        /// The instance of the interface <typeparamref name="TInterface"/> to be serialized.
        /// </param>
        /// <param name="options">
        /// The <see cref="JsonSerializerOptions"/> used to control the serialization behavior.
        /// </param>
        /// <remarks>
        /// This method overrides the base implementation for serializing objects of type <typeparamref name="TInterface"/>.
        /// </remarks>
        public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value, options);

        /// <summary>
        /// Determines whether the specified type can be converted by the current converter.
        /// </summary>
        /// <param name="typeToConvert">The type to check for conversion capability.</param>
        /// <returns>
        ///   <c>true</c> if the specified type can be converted; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method overrides the base implementation to provide a specific check
        /// for the type <c>TInterface</c>. It returns true if the <paramref name="typeToConvert"/>
        /// is exactly of type <c>TInterface</c>.
        /// </remarks>
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(TInterface);
    }


    /// <summary>
    /// A factory class for creating JSON converters that convert types of a specified interface
    /// to and from JSON. This class is generic and takes two type parameters: 
    /// TImplementation, which is the concrete class implementing the interface, 
    /// and TInterface, which is the interface being implemented.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the concrete class that implements the specified interface.</typeparam>
    /// <typeparam name="TInterface">The type of the interface that the concrete class implements.</typeparam>
    public class InterfaceConverterFactory<TImplementation, TInterface> : JsonConverterFactory where TImplementation : class, TInterface
    {
        /// <summary>
        /// Represents the type of the implementation that this member pertains to.
        /// This property can be used to retrieve the specific type associated 
        /// with an implementation for a particular member within a class or interface.
        /// </summary>
        /// <value>
        /// A <see cref="Type"/> that gets the implementation type.
        /// </value>
        /// <remarks>
        /// This property is typically used in dependency injection or service registration scenarios 
        /// where you need to know what specific implementation type is being used.
        /// </remarks>
        public Type ImplementationType
        {
            get;
        }
        /// <summary>
        /// Gets the type of the interface that this member represents.
        /// </summary>
        /// <value>
        /// The <see cref="Type"/> of the interface. This property is read-only.
        /// </value>
        public Type InterfaceType
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceConverterFactory{TInterface, TImplementation}"/> class.
        /// The constructor sets the <see cref="ImplementationType"/> and <see cref="InterfaceType"/>
        /// properties to the corresponding types specified by the generic type parameters.
        /// </summary>
        /// <typeparam name="TInterface">
        /// The type of the interface that this factory will convert to.
        /// </typeparam>
        /// <typeparam name="TImplementation">
        /// The type of the implementation that will be created by this factory.
        /// </typeparam>
        public InterfaceConverterFactory()
        {
            this.ImplementationType = typeof(TImplementation);
            this.InterfaceType = typeof(TInterface);
        }
        /// <inheritdoc/>

        /// <summary>
        /// Determines whether the converter can convert the specified type.
        /// </summary>
        /// <param name="typeToConvert">
        /// The type that is checked for conversion capability.
        /// </param>
        /// <returns>
        /// Returns true if the converter can convert the specified type; otherwise, false.
        /// </returns>
        public override bool CanConvert(Type typeToConvert) => typeToConvert == this.InterfaceType;

        /// <summary>
        /// Creates a JSON converter for the specified type.
        /// </summary>
        /// <param name="typeToConvert">The type that needs to be converted.</param>
        /// <param name="options">Options that control the JSON serialization behavior.</param>
        /// <returns>
        /// An instance of <see cref="JsonConverter"/> for the specified type,
        /// or null if the converter could not be created.
        /// </returns>
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(InterfaceConverter<,>).MakeGenericType(this.ImplementationType, this.InterfaceType);

            return Activator.CreateInstance(converterType) as JsonConverter;
        }
    }
}
