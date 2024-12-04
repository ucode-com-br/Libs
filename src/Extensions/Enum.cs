using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace UCode.Extensions
{
    /// <summary>
    /// This static class contains extension methods for enums.
    /// </summary>
    /// <remarks>
    /// Extension methods in this class can be used to add additional functionality to enum types.
    /// </remarks>
    public static class EnumExtensions
    {
        /// <summary>
        /// Retrieves the custom attributes of the fields of the specified enum type 
        /// associated with the provided value using a specified evaluation function. 
        /// Optionally joins multiple results with a specified separator if the enum 
        /// type is marked with the <see cref="FlagsAttribute"/>.
        /// </summary>
        /// <typeparam name="TEnum">The enum type which must be a struct and derive from <see cref="System.Enum"/>.</typeparam>
        /// <typeparam name="TAttribute">The type of the attribute to be retrieved from the enum fields.</typeparam>
        /// <param name="value">An optional nullable enum value from which to retrieve custom attributes.</param>
        /// <param name="evaluate">A function that processes the enum value, the attribute, 
        /// and a boolean indicating whether the enum type is a flags enum, returning a string.</param>
        /// <param name="separator">An optional string used to join multiple results. Required 
        /// if the enum has the <see cref="FlagsAttribute"/>.</param>
        /// <returns>A concatenated string of results from the evaluation of the attributes 
        /// if any are found; otherwise, null.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a separator is provided 
        /// but the enum does not have the <see cref="FlagsAttribute"/>.</exception>
        public static string? GetEnumFieldAttributes<TEnum, TAttribute>(this TEnum? value, Func<(TEnum Enum, TAttribute Attribute, bool HasFlag), string?> evaluate, string? separator = null)
            where TEnum : struct, System.Enum
            where TAttribute : Attribute
        {
            if (value == null)
                return null;

            var enumType = value.GetType();

            var hasFlags = enumType.GetCustomAttribute(typeof(FlagsAttribute), false) != null;

            if (!hasFlags && (separator != null))
            {
                throw new InvalidOperationException("Spected flag in enum, but enum not have \"FlagsAttribute\".");
            }

            var flagedEnums = System.Enum.GetValues<TEnum>().Where(v => value?.Equals(v) ?? false).ToArray();

            foreach (var enumValue in flagedEnums)
            {
                var fieldInfo = enumType.GetField(enumValue.ToString());

                var resultList = new List<string>();

                if (fieldInfo != null)
                {
                    IEnumerable<TAttribute> descriptionAttributes = fieldInfo.GetCustomAttributes(typeof(TAttribute)).Select(s=>(TAttribute)s).ToArray();

                    foreach (var attr in descriptionAttributes)
                    {
                        //var type = attr.GetType();

                        //var prop = type.GetProperty("Name");

                        //var result = prop.GetValue(attr);
                        var result = evaluate?.Invoke((value.Value, attr, hasFlags));

                        if (result != null)
                            resultList.Add(result);
                    }
                }

                if (resultList.Count > 0)
                    return string.Join(separator, resultList);
            }
            return default;
        }


        /// <summary>
        /// Retrieves the display name associated with a nullable enumeration value.
        /// </summary>
        /// <typeparam name="TEnum">
        /// The enumeration type. It must be a value type and derive from <see cref="System.Enum"/>.
        /// </typeparam>
        /// <param name="value">
        /// The nullable enumeration value from which to retrieve the display name.
        /// </param>
        /// <returns>
        /// A string representing the display name of the enumeration value; 
        /// or <see langword="null"/> if the value is <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the generic type parameter <typeparamref name="TEnum"/> is not an enumeration type.
        /// </exception>
        public static string? GetDisplayName<TEnum>(this TEnum? value) where TEnum : struct, System.Enum => EnumExtensions.GetDisplayNames<TEnum>(value, null);

        /// <summary>
        /// Gets the display names of the specified enumeration value.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enumeration.</typeparam>
        /// <param name="value">The nullable enumeration value for which to get the display names.</param>
        /// <param name="separator">An optional separator string to join multiple display names.</param>
        /// <returns>
        /// A concatenated string of display names for the enumeration value, separated by the specified separator, 
        /// or null if the value is null.
        /// </returns>
        public static string? GetDisplayNames<TEnum>(this TEnum? value, string? separator = null)
            where TEnum : struct, System.Enum
        {
            Func<(TEnum Enum, DisplayAttribute Attribute, bool HasFlag), string?> func = (args) => {
                var type = args.Attribute.GetType();

                var prop = type.GetProperty("Name");

                return (string)prop.GetValue(args.Attribute);
            };

            return GetEnumFieldAttributes<TEnum, DisplayAttribute>(value, func, separator);
            /*if (value == null)
                return null;

            var enumType = value.GetType();

            var hasFlags = enumType.GetCustomAttribute(typeof(FlagsAttribute), false) != null;

            if (!hasFlags && (separator != null))
            {
                throw new InvalidOperationException("Spected flag in enum, but enum not have \"FlagsAttribute\".");
            }

            var fieldInfo = enumType.GetField(value.ToString());

            var resultList = new List<string>();

            if (fieldInfo != null)
            {
                var descriptionAttributes = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute)).ToArray();

                foreach (var attr in descriptionAttributes)
                {
                    var type = attr.GetType();

                    var prop = type.GetProperty("Name");

                    var result = prop.GetValue(attr);

                    if (result != null)
                        resultList.Add((string)result);
                }
            }

            if(resultList.Count == 0)
                return default;
            else
                return string.Join(separator, resultList);*/
        }


        /// <summary>
        /// Extension method that retrieves the JSON property name for a given enumeration value.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum which must be a value type and derive from System.Enum.</typeparam>
        /// <param name="value">The enumeration value for which to retrieve the JSON property name. Can be null.</param>
        /// <param name="separator">An optional string used to separate parts of the JSON property name, if applicable. Can be null.</param>
        /// <returns>A string representing the JSON property name, or null if the value is null or not found.</returns>
        /// <remarks>
        /// This method calls GetJsonPropertyNames<TEnum> to perform the actual retrieval process.
        /// </remarks>
        public static string? GetJsonPropertyName<TEnum>(this TEnum? value, string? separator = null) where TEnum : struct, System.Enum => GetJsonPropertyNames<TEnum>(value, null);

        /// <summary>
        /// Retrieves the JSON property names associated with the specified enum value.
        /// This method takes an optional separator to format the returned names if multiple names exist.
        /// </summary>
        /// <typeparam name="TEnum">The enum type from which to retrieve the property names. It must be a non-nullable enum.</typeparam>
        /// <param name="value">The enum value for which to get the property names. Can be null.</param>
        /// <param name="separator">Optional separator for formatting multiple property names. Defaults to null.</param>
        /// <returns>
        /// A string representing the JSON property names of the specified enum value, 
        /// or null if the value is null. If there are multiple property names, they 
        /// will be joined by the specified separator.
        /// </returns>
        public static string? GetJsonPropertyNames<TEnum>(this TEnum? value, string? separator = null)
            where TEnum : struct, System.Enum
        {
            Func<(TEnum Enum, JsonPropertyNameAttribute Attribute, bool HasFlag), string?> func = (args) => {
                var type = args.Attribute.GetType();

                var prop = type.GetProperty("Name");

                return (string)prop.GetValue(args.Attribute);
            };

            return GetEnumFieldAttributes<TEnum, JsonPropertyNameAttribute>(value, func, separator);

            /*if (value == null)
            {
                return null;
            }

            var enumType = value.GetType();

            var hasFlags = enumType.GetCustomAttribute(typeof(FlagsAttribute), false) != null;

            if (!hasFlags && (separator != null))
            {
                throw new InvalidOperationException("Spected flag in enum, but enum not have \"FlagsAttribute\".");
            }

            var fieldInfo = enumType.GetField(value.ToString());



            if (fieldInfo != null)
            {
                var descriptionAttributes = fieldInfo.GetCustomAttributes().Where(w => ((Type)w.TypeId).Name == "JsonPropertyNameAttribute").ToArray();

                foreach (var attr in descriptionAttributes)
                {
                    var type = attr.GetType();

                    var prop = type.GetProperty("Name");

                    return (string)prop.GetValue(attr);
                }
            }

            return default;*/
        }


        /// <summary>
        /// Retrieves the enum value corresponding to a given JSON property name, 
        /// utilizing the <see cref="JsonPropertyNameAttribute"/> to map the property name 
        /// to the enum fields.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum, which must be a struct and inherit from <see cref="System.Enum"/>.</typeparam>
        /// <param name="propertyName">The name of the JSON property to retrieve.</param>
        /// <returns>
        /// The enum value that corresponds to the provided JSON property name, or <c>null</c> 
        /// if the name is null, empty, or does not match any enum field's JSON property name.
        /// </returns>
        public static TEnum? GetJsonPropertyName<TEnum>(this string? propertyName) where TEnum : struct, System.Enum
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return default;

            var fieldsInfo = typeof(TEnum).GetFields();

            var fieldInfoWithJsonProp = fieldsInfo.Where(s => s.GetCustomAttributes().Any()).Where(f => f.GetCustomAttributes().Any(w => ((Type)w.TypeId).Name == "JsonPropertyNameAttribute"));

            foreach (var fieldInfo in fieldInfoWithJsonProp)
            {
                var jsonPropertyName = (JsonPropertyNameAttribute)fieldInfo.GetCustomAttributes().First(w => ((Type)w.TypeId).Name == "JsonPropertyNameAttribute");

                if (jsonPropertyName.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    var resultObject = System.Enum.ToObject(typeof(TEnum), fieldInfo.GetValue(null));

                    return (TEnum)resultObject;
                }
            }

            return default;
        }


        /// <summary>
        /// Converts a string representation of the name or numeric value of one or more enumerated constants 
        /// to an equivalent enumerated object of a specified type.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type to convert the string to, which must be a non-nullable enum.</typeparam>
        /// <param name="enumValue">The string representation of the enumerated constant.</param>
        /// <param name="ignoreCase">A <c>bool</c> that indicates whether the case of the string should be ignored in the comparison.</param>
        /// <returns>
        /// An object of type <typeparamref name="TEnum"/> if the conversion is successful; otherwise, <c>null</c> 
        /// if <paramref name="enumValue"/> is null or whitespace.
        /// </returns>
        public static TEnum? FromEnumValue<TEnum>([NotNull] this string enumValue, bool ignoreCase) where TEnum : struct, System.Enum
        {
            if (string.IsNullOrWhiteSpace(enumValue))
                return default;

            var enumType = typeof(TEnum);

            var resultObject = System.Enum.Parse(enumType, enumValue, ignoreCase);

            return (TEnum)resultObject;
        }
    }
}
