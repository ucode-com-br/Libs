using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UCode.Extensions
{
    public static class ObjectExtensions
    {
        public static T Sum<T>(this object a, object b) => (T)Sum(a, b);

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

        public static bool IsNumeric<T>(this T source) => IsNumeric(source);


        private static bool IsNumericType<T>() => IsNumericType(typeof(T));

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

    }
}
