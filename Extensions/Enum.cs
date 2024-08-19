using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace UCode.Extensions
{
    public static class EnumExtensions
    {
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


        public static string? GetDisplayName<TEnum>(this TEnum? value) where TEnum : struct, System.Enum => EnumExtensions.GetDisplayNames<TEnum>(value, null);

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


        public static string? GetJsonPropertyName<TEnum>(this TEnum? value, string? separator = null) where TEnum : struct, System.Enum => GetJsonPropertyNames<TEnum>(value, null);

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
