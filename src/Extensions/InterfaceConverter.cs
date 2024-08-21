using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UCode.Extensions
{
    /// <summary>
    /// Serialize interface to implementation
    /// </summary>
    /// <typeparam name="TImplementation">Class</typeparam>
    /// <typeparam name="TInterface">Interface</typeparam>
    public class InterfaceConverter<TImplementation, TInterface> : JsonConverter<TInterface> where TImplementation : class, TInterface
    {
        /// <summary>
        /// Read the implementation as the interface
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override TInterface? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => JsonSerializer.Deserialize<TImplementation>(ref reader, options);

        /// <summary>
        /// Write the interface as the implementation
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value, options);

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(TInterface);
    }


    /*var deserializerOptions = new JsonSerializerOptions
{
    Converters = { new InterfaceConverterFactory<MyClass, IMyInterface>() }
};

    // Deserialize to the interface.
// This will trigger an exception without the options.
var myInterface = JsonSerializer.Deserialize<IMyInterface>(json, deserializerOptions);
if (myInterface is MyClass myClass2)
{
    // Use Implementation
}
     
     */

    public class InterfaceConverterFactory<TImplementation, TInterface> : JsonConverterFactory where TImplementation : class, TInterface
    {
        public Type ImplementationType
        {
            get;
        }
        public Type InterfaceType
        {
            get;
        }

        public InterfaceConverterFactory()
        {
            this.ImplementationType = typeof(TImplementation);
            this.InterfaceType = typeof(TInterface);
        }
        /// <inheritdoc/>

        public override bool CanConvert(Type typeToConvert) => typeToConvert == this.InterfaceType;

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(InterfaceConverter<,>).MakeGenericType(this.ImplementationType, this.InterfaceType);

            return Activator.CreateInstance(converterType) as JsonConverter;
        }
    }
}
