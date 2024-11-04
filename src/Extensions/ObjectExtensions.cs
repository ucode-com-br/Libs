using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace UCode.Extensions
{
    /// <summary>
    /// Contains extension methods for various .NET types.
    /// This class provides utility methods that can be used to enhance the functionality
    /// of existing object types, making them easier to work with in a fluent manner.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Sums two objects and casts the result to a specified type.
        /// </summary>
        /// <typeparam name="T">The type to which the result will be cast.</typeparam>
        /// <param name="a">The first object to sum.</param>
        /// <param name="b">The second object to sum.</param>
        /// <returns>The sum of the two objects cast to type T.</returns>
        /// <exception cref="InvalidCastException">Thrown when the result cannot be cast to type T.</exception>
        public static T Sum<T>(this object a, object b) => (T)Sum(a, b);

        /// <summary>
        /// Sums two objects if they are of the same type and support addition.
        /// The method checks if the objects are numeric types or if they 
        /// overload the addition operator. If neither condition is met, 
        /// an exception is thrown.
        /// </summary>
        /// <param name="a">The first object to be summed.</param>
        /// <param name="b">The second object to be summed.</param>
        /// <returns>
        /// Returns the sum of the two objects if they are compatible, 
        /// otherwise throws an exception.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the two objects are not of the same type and neither 
        /// is a numeric type.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the type of the objects does not support addition.
        /// </exception>
        public static object Sum(this object a, object b)
        {
            Type typeSelected = a.GetType();

            // Verifica se os objetos são do mesmo tipo
            if (a.GetType() != b.GetType() && (!IsNumericType(a.GetType()) || !IsNumericType(b.GetType())))
            {
                throw new ArgumentException("The object is not same type.");
            }


            // Verifica se o tipo é um tipo primitivo numérico
            if (IsNumericType(typeSelected))
            {
                return PerformNumericAddition(a, b, typeSelected);
            }

            // Verifica se o tipo sobrecarrega o operador +
            MethodInfo? addMethod = typeSelected.GetMethod("op_Addition", BindingFlags.Static | BindingFlags.Public);
            if (addMethod != null)
            {
                return addMethod.Invoke(null, new object[] { a, b })!;
            }

            // Se não for possível realizar a adição, lança uma exceção
            throw new InvalidOperationException($"O tipo {typeSelected.Name} não suporta a operação de adição.");
        }

        /// <summary>
        /// Sums up a collection of objects by applying an optional selector function.
        /// </summary>
        /// <param name="items">An enumerable collection of objects to sum.</param>
        /// <param name="selector">An optional function used to select the object to be summed.</param>
        /// <returns>
        /// The sum of the selected objects, or null if no valid items are present.
        /// </returns>
        public static object Sum(this IEnumerable<object?> items, Func<object?, object>? selector = null)
        {
            object? result = null;

            foreach (var item in items.Select(selector))
            {
                if (item == null)
                {
                    continue;
                }

                if (result == null)
                {
                    result = item;
                }
                else
                {
                    result = Sum(result, item);
                }
            }

            return result!;
        }

        /// <summary>
        /// Calculates the sum of a sequence of objects, using an optional selector function
        /// to transform the items before summation. If the selector is not provided, the
        /// original objects are summed. Null values are ignored during the summation process.
        /// </summary>
        /// <param name="items">An enumerable collection of objects to sum.</param>
        /// <param name="selector">
        /// An optional function that is applied to each object in the collection, which takes
        /// an object and its index, returning a transformed object to be summed. If null, 
        /// the items are summed directly.
        /// </param>
        /// <returns>
        /// The sum of the elements in the collection, or null if there are no items to sum.
        /// </returns>
        public static object Sum(this IEnumerable<object?> items, Func<object?, int, object>? selector = null)
        {
            object? result = null;

            foreach (var item in items.Select(selector))
            {
                if (item == null)
                {
                    continue;
                }

                if (result == null)
                {
                    result = item;
                }
                else
                {
                    result = Sum(result, item);
                }
            }

            return result!;
        }

        /// <summary>
        /// Determines whether the specified object can be considered a numeric type.
        /// This method extends the functionality of the object class, allowing
        /// any object to check if it is numeric by evaluating its type or converting
        /// a string representation of a number to the most precise numeric type.
        /// </summary>
        /// <param name="obj">The object to check for numeric type.</param>
        /// <returns>
        /// Returns true if the object is a numeric type or can be converted to a numeric type; 
        /// otherwise, returns false.
        /// </returns>
        public static bool IsNumeric(this object obj)
        {
            if (obj == typeof(string))
            {
                _ = ConvertStringToMostPreciseType((string)obj, out var type);

                if (type != null)
                    return IsNumericType(type);
                else
                    return false;
            }


            return IsNumericType(obj.GetType());
        }

        /// <summary>
        /// Determines whether the specified value is numeric.
        /// </summary>
        /// <typeparam name="T">The type of the source value.</typeparam>
        /// <param name="source">The value to evaluate.</param>
        /// <returns>
        /// True if the source value is numeric; otherwise, false.
        /// </returns>
        /// <remarks>
        /// This extension method checks if the provided source value can be classified as numeric.
        /// It utilizes a generic type parameter to support various numeric types.
        /// </remarks>
        public static bool IsNumeric<T>(this T source) => IsNumeric(source);




        /// <summary>
        /// Determines whether the specified type parameter is a numeric type.
        /// </summary>
        /// <typeparam name="T">The type to be checked for numeric characteristics.</typeparam>
        /// <returns>
        /// True if the type T is a numeric type; otherwise, false.
        /// </returns>
        private static bool IsNumericType<T>() => IsNumericType(typeof(T));

        /// <summary>
        /// Determines whether the specified <see cref="Type"/> is a numeric type.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check.</param>
        /// <returns>
        /// Returns <c>true</c> if the specified <paramref name="type"/> is a numeric type; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Performs numeric addition on two objects after determining the most precise type 
        /// for the addition operation. It first attempts to convert the objects from string 
        /// representations to their most precise types before performing the addition.
        /// </summary>
        /// <param name="a">The first number to be added, which can be of any type including string.</param>
        /// <param name="b">The second number to be added, which can be of any type including string.</param>
        /// <param name="type">The desired return type of the addition result.</param>
        /// <returns>
        /// The result of the addition as an object of the specified type.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the determined type for addition is not supported.
        /// </exception>
        private static object PerformNumericAddition(object a, object b, Type type)
        {
            // Obter os tipos reais de 'a' e 'b'
            Type typeA = a.GetType();
            Type typeB = b.GetType();

            // Verificar se 'a' e 'b' são strings e tentar convertê-los para o tipo mais preciso
            if (typeA == typeof(string))
            {
                a = ConvertStringToMostPreciseType((string)a, out typeA);
            }
            if (typeB == typeof(string))
            {
                b = ConvertStringToMostPreciseType((string)b, out typeB);
            }

            // Determinar o tipo mais preciso entre 'a' e 'b'
            TypeCode typeCodeA = Type.GetTypeCode(typeA);
            TypeCode typeCodeB = Type.GetTypeCode(typeB);
            TypeCode mostPreciseTypeCode = GetMostPreciseTypeCode(typeCodeA, typeCodeB);

            // Realizar a adição no tipo mais preciso
            object result;
            switch (mostPreciseTypeCode)
            {
                case TypeCode.Byte:
                    result = Convert.ToByte(a) + Convert.ToByte(b);
                    break;
                case TypeCode.SByte:
                    result = Convert.ToSByte(a) + Convert.ToSByte(b);
                    break;
                case TypeCode.UInt16:
                    result = Convert.ToUInt16(a) + Convert.ToUInt16(b);
                    break;
                case TypeCode.UInt32:
                    result = Convert.ToUInt32(a) + Convert.ToUInt32(b);
                    break;
                case TypeCode.UInt64:
                    result = Convert.ToUInt64(a) + Convert.ToUInt64(b);
                    break;
                case TypeCode.Int16:
                    result = Convert.ToInt16(a) + Convert.ToInt16(b);
                    break;
                case TypeCode.Int32:
                    result = Convert.ToInt32(a) + Convert.ToInt32(b);
                    break;
                case TypeCode.Int64:
                    result = Convert.ToInt64(a) + Convert.ToInt64(b);
                    break;
                case TypeCode.Decimal:
                    result = Convert.ToDecimal(a) + Convert.ToDecimal(b);
                    break;
                case TypeCode.Double:
                    result = Convert.ToDouble(a) + Convert.ToDouble(b);
                    break;
                case TypeCode.Single:
                    result = Convert.ToSingle(a) + Convert.ToSingle(b);
                    break;
                default:
                    throw new InvalidOperationException($"O tipo {mostPreciseTypeCode} não é suportado para adição numérica.");
            }

            if (result.GetType() == type)
            {
                return result;
            }
            else
            {
                return Convert.ChangeType(result, type)!;
            }
        }

        /// <summary>
        /// Converts a string representation of a number to the most precise numeric type possible.
        /// It attempts to parse the string into various numeric types in order of precision.
        /// </summary>
        /// <param name="value">The string value to convert to a numeric type.</param>
        /// <param name="type">Outputs the <see cref="Type"/> of the converted numeric value, or null if conversion fails.</param>
        /// <returns>
        /// The numeric value of the string if conversion is successful; otherwise, returns null.
        /// </returns>
        private static object? ConvertStringToMostPreciseType(string value, out Type? type)
        {
            // Tentar converter a string para o tipo mais preciso possível
            if (decimal.TryParse(value, out decimal decimalValue))
            {
                type = typeof(decimal);
                return decimalValue;
            }
            if (double.TryParse(value, out double doubleValue))
            {
                type = typeof(double);
                return doubleValue;
            }
            if (float.TryParse(value, out float floatValue))
            {
                type = typeof(float);
                return floatValue;
            }
            if (long.TryParse(value, out long longValue))
            {
                type = typeof(long);
                return longValue;
            }
            if (int.TryParse(value, out int intValue))
            {
                type = typeof(int);
                return intValue;
            }
            if (short.TryParse(value, out short shortValue))
            {
                type = typeof(short);
                return shortValue;
            }
            if (byte.TryParse(value, out byte byteValue))
            {
                type = typeof(byte);
                return byteValue;
            }
            if (sbyte.TryParse(value, out sbyte sbyteValue))
            {
                type = typeof(sbyte);
                return sbyteValue;
            }
            if (ulong.TryParse(value, out ulong ulongValue))
            {
                type = typeof(ulong);
                return ulongValue;
            }
            if (uint.TryParse(value, out uint uintValue))
            {
                type = typeof(uint);
                return uintValue;
            }
            if (ushort.TryParse(value, out ushort ushortValue))
            {
                type = typeof(ushort);
                return ushortValue;
            }

            type = null;
            return (object)null;
            //throw new InvalidCastException($"Não foi possível converter a string '{value}' para um tipo numérico.");
        }

        /// <summary>
        /// Determines the most precise TypeCode between two provided TypeCodes.
        /// </summary>
        /// <param name="typeCodeA">The first TypeCode to compare.</param>
        /// <param name="typeCodeB">The second TypeCode to compare.</param>
        /// <returns>
        /// The TypeCode that is more precise based on a predefined order of numeric types.
        /// </returns>
        private static TypeCode GetMostPreciseTypeCode(TypeCode typeCodeA, TypeCode typeCodeB)
        {
            // Definir a ordem de precisão dos tipos numéricos
            TypeCode[] precisionOrder = {
                TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16,
                TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64,
                TypeCode.Single, TypeCode.Double, TypeCode.Decimal
            };

            // Encontrar o tipo mais preciso
            int indexA = Array.IndexOf(precisionOrder, typeCodeA);
            int indexB = Array.IndexOf(precisionOrder, typeCodeB);

            return indexA > indexB ? typeCodeA : typeCodeB;
        }

        // merge object using json serializer
        /// <summary>
        /// Merges two objects of types Ta and Tb into a single object of type T 
        /// using JSON serialization. The properties of the second object (b) will 
        /// overwrite those of the first object (a) if there are conflicting properties.
        /// </summary>
        /// <typeparam name="T">The type of the resulting merged object.</typeparam>
        /// <typeparam name="TA">The type of the first object to merge.</typeparam>
        /// <typeparam name="TB">The type of the second object to merge.</typeparam>
        /// <param name="a">The first object, which is of type Ta.</param>
        /// <param name="b">The second object, which is of type Tb.</param>
        /// <returns>The merged object of type T, containing properties from both a and b.</returns>
        public static T Merge<T, TA, TB>(this TA a, TB b)
        {
            var aJson = JsonSerializer.Serialize(a);
            var bJson = JsonSerializer.Serialize(b);

            T result = default!;
            JsonElement jsonElement = default;

            try
            {
                result = JsonSerializer.Deserialize<T>(aJson)!;
                aJson = default;
                jsonElement = JsonSerializer.Deserialize<JsonElement>(bJson);
            }
            catch (Exception ex)
            {

            }

            if (result == null && aJson != null)
            {
                try
                {
                    result = JsonSerializer.Deserialize<T>(bJson)!;
                    bJson = default;
                    jsonElement = JsonSerializer.Deserialize<JsonElement>(aJson);
                }
                catch (Exception ex)
                {

                }
            }

            jsonElement.Populate(result);

            return result;
        }


        // merge object using json serializer
        /// <summary>
        /// Merges two objects of the same type using a JSON serializer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be merged.</typeparam>
        /// <param name="a">The first object to merge.</param>
        /// <param name="b">The second object to merge.</param>
        /// <returns>A new object of type T that represents the merged result of the two input objects.</returns>
        public static T Merge<T>(this T a, T b) => Merge<T, T, T>(a, b);


    }
}
