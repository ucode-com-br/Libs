
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MathNet.Numerics;
using Microsoft.AspNetCore.Http;
using NPOI.SS.Formula.Functions;
using NPOI.XSSF.Streaming.Values;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static UCode.Extensions.ReflectionExtensions;

namespace UCode.Extensions
{
    /// <summary>
    /// Entensions using reflection
    /// </summary>
    public static class ReflectionExtensions
    {
        #region PublicMemberInfoAction
        /// <summary>
        /// Actions and member proxy for each call in <see cref="MemberInfoCall{TInstance}"/>
        /// </summary>
        /// <typeparam name="TInstance">Instance type of</typeparam>
        public readonly struct MemberInfoAction<TInstance>
        {
            private readonly TInstance? _instance;
            private readonly MemberInfo _memberInfo;

            private readonly Func<object?, object?[]?, object?>? _invoke;
            private readonly Action<object?, object?>? _setValue;
            private readonly Func<object?, object?>? _getValue;

            private readonly Type? _returnType;
            private readonly Type[]? _argumentTypes;

            internal MemberInfoAction(ref TInstance? instance, [NotNull] MemberInfo memberInfo, Type? returnType, Type[]? argumentTypes,
                Func<object?, object?[]?, object?>? invoke = null,
                Action<object?, object?>? setValue = null,
                Func<object?, object?>? getValue = null)
            {
                this._returnType = returnType;
                this._instance = instance;
                this._memberInfo = memberInfo;
                this._argumentTypes = argumentTypes;

                this._invoke = invoke;
                this._setValue = setValue;
                this._getValue = getValue;
            }

            public void SetValue(object? value)
            {
                if (this._setValue == null)
                    throw new ArgumentNullException($"You can`t set value with member type \"{Enum.GetName(this._memberInfo.MemberType)}\".");


                this._setValue.Invoke(this._instance, value);
            }

            public object? GetValue() => this.GetValue<object>();

            public T? GetValue<T>()
            {
                if (this._getValue == null)
                    throw new ArgumentNullException($"You can`t get value with member type \"{Enum.GetName(this._memberInfo.MemberType)}\".");


                return (T?)this._getValue.Invoke(this._instance);
            }

            public object? Invoke(object?[]? args) => this.Invoke<object>(args);

            public T? Invoke<T>(params object?[]? args)
            {
                if (this._invoke == null)
                    throw new ArgumentNullException($"You can`t invoke a method with member type \"{Enum.GetName(this._memberInfo.MemberType)}\".");

                return (T?)this._invoke!.Invoke(this._instance, args);
            }

            public bool CanInvoke => this._invoke != null;

            public bool CanSetValue => this._setValue != null;

            public bool CanGetValue => this._getValue != null;

            public Type? ReturnType => this._returnType;

            public Type[]? ArgumentTypes => this._argumentTypes;

            public TInstance Instance => this._instance;

            public MemberInfo MemberInfo => this._memberInfo;
        }

        /// <summary>
        /// Action to bbe called for each property, field or method
        /// </summary>
        /// <typeparam name="TInstance">Instance type of</typeparam>
        /// <param name="action">Member info action proxy methods and info</param>
        public delegate void MemberInfoCall<TInstance>(MemberInfoAction<TInstance> action);

        /// <summary>
        /// Process action on Properties, Fields and Methods in the instance
        /// </summary>
        /// <typeparam name="TInstance">Instance Type</typeparam>
        /// <param name="instance">created instance</param>
        /// <param name="action">action to bee taken in each property, field or method</param>
        public static void PublicMemberInfoAction<TInstance>([NotNull] this TInstance instance, Action<MemberInfoAction<TInstance>> action)
        {
            ArgumentNullException.ThrowIfNull(instance, nameof(instance));

            var typ = typeof(TInstance);
            var members = typ.GetMembers(BindingFlags.Public | BindingFlags.Instance);

            foreach (var member in members)
            {

                MemberInfoAction<TInstance> memberInfoAction;
                if (member is FieldInfo f)
                {
                    memberInfoAction = new MemberInfoAction<TInstance>(ref instance, f, f.FieldType, new Type[] { f.FieldType }, null, f.SetValue, f.GetValue);
                }
                else if (member is PropertyInfo p)
                {
                    memberInfoAction = new MemberInfoAction<TInstance>(ref instance, p, p.PropertyType, new Type[] { p.PropertyType }, null, p.SetValue, p.GetValue);
                }
                else if (member is MethodInfo m)
                {
                    if (m.Attributes.HasFlag(MethodAttributes.SpecialName))
                        continue;

                    memberInfoAction = new MemberInfoAction<TInstance>(ref instance, m, m.ReturnType, m.GetParameters().Select(s => s.ParameterType).ToArray(), m.Invoke, null, null);
                }
                else
                {
                    continue;
                }

                action.Invoke(memberInfoAction);
            }
        }
        #endregion PublicMemberInfoAction



        #region PopulateWithDummyData

        private static IDictionary BuildDict(Type type, object[] values)
        {
            var dictType = typeof(Dictionary<,>).MakeGenericType(typeof(string), type.GenericTypeArguments[1]);

            var result = (IDictionary)Activator.CreateInstance(dictType);

            var value = values.FirstOrDefault(x => x.GetType() == type.GenericTypeArguments[1]);

            result.Add($"item0", value);
            result.Add($"item1", value);

            return result;
        }

        private static IList BuildList(Type type, object[] values)
        {
            var listType = typeof(List<>).MakeGenericType(type.GenericTypeArguments[0]);

            var result = (IList)Activator.CreateInstance(listType);

            var value = values.FirstOrDefault(x => x.GetType() == type.GenericTypeArguments[0]);

            result.Add(value);

            return result;
        }

        private static Array BuildArray(Type type, object[] values)
        {
            var result = (Array)Activator.CreateInstance(type, 2);

            var value = values.FirstOrDefault(x => x.GetType() == type.GetElementType());

            result.SetValue(value, 0);
            result.SetValue(value, 1);

            return result;
        }

        private static object? BuildValue(Type type, object[] values) => values.FirstOrDefault(x => x.GetType() == type);

        /// <summary>
        /// Populate instance with dummy data
        /// </summary>
        /// <typeparam name="T">Type of instance</typeparam>
        /// <param name="instance">created instance</param>
        public static void PopulateWithDummyData<T>([NotNull] this T instance)
        {


            ArgumentNullException.ThrowIfNull(instance, nameof(instance));

            instance.PublicMemberInfoAction((memberInfoAction) =>
            {
                //Func<Type, bool> isEqualOrNullableType = (type) =>
                //{
                //    if (type == null || memberInfoAction.ReturnType == null)
                //        return false;

                //    /* DEBUG
                //    var typeAreEqual = memberInfoAction.ReturnType == type;

                //    if(typeAreEqual)
                //        return typeAreEqual;


                //    var isValueType = (memberInfoAction.ReturnType?.BaseType ?? default) == typeof(ValueType);
                //    var isNullable = Nullable.GetUnderlyingType(memberInfoAction.ReturnType ?? ("", "").GetType()) != null;
                //    var haveOneNullableGenericType = (memberInfoAction.ReturnType?.GenericTypeArguments.Count() ?? 0) == 1;
                //    var isNullableGenericType = (memberInfoAction.ReturnType?.GenericTypeArguments.FirstOrDefault() ?? default) == type;

                //    return (isValueType && isNullable && haveOneNullableGenericType && isNullableGenericType);
                //    */

                //    //if (member is FieldInfo f)

                //    return (memberInfoAction.ReturnType == type) ||
                //        (Nullable.GetUnderlyingType(memberInfoAction.ReturnType) != null && (memberInfoAction.ReturnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type);
                //};

                if (memberInfoAction.CanGetValue && memberInfoAction.CanSetValue)
                {
                    string @string = "";
                    byte @byte = 0x01;
                    decimal @decimal = 0.1m;
                    short @short = 0;
                    int @int = 0;
                    long @long = 0;
                    bool @bool = false;
                    object @object = new
                    {
                        fieldString = @string,
                        fieldByte = @byte,
                        fieldDecimal = @decimal,
                        fieldShort = @short,
                        fieldInt = @int,
                        fieldLong = @long,
                        fieldBool = @bool,
                        fieldObjectNull = (object?)null
                    };

                    var values = new object[] {
                        @string,
                        @byte,
                        @decimal,
                        @short,
                        @int,
                        @long,
                        @bool,
                        @object
                    };


                    object? setValue = null;
                    switch (memberInfoAction.ReturnType)
                    {
                        // primitive types
                        case Type returnType when typeof(string) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildValue(type, values);
                            break;
                        case Type returnType when typeof(byte) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildValue(type, values);
                            break;
                        case Type returnType when typeof(decimal) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildValue(type, values);
                            break;
                        case Type returnType when typeof(short) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildValue(type, values);
                            break;
                        case Type returnType when typeof(int) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildValue(type, values);
                            break;
                        case Type returnType when typeof(long) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildValue(type, values);
                            break;
                        case Type returnType when typeof(bool) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildValue(type, values);
                            break;
                        case Type returnType when typeof(object) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildValue(type, values);
                            break;


                        // Dictionaries
                        case Type returnType when typeof(Dictionary<string, string>) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildDict(type, values);
                            break;
                        case Type returnType when typeof(Dictionary<string, byte>) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildDict(type, values);
                            break;
                        case Type returnType when typeof(Dictionary<string, decimal>) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildDict(type, values);
                            break;
                        case Type returnType when typeof(Dictionary<string, short>) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildDict(type, values);
                            break;
                        case Type returnType when typeof(Dictionary<string, int>) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildDict(type, values);
                            break;
                        case Type returnType when typeof(Dictionary<string, long>) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildDict(type, values);
                            break;
                        case Type returnType when typeof(Dictionary<string, bool>) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildDict(type, values);
                            break;
                        case Type returnType when typeof(Dictionary<string, object>) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildDict(type, values);
                            break;


                        // Lists
                        case Type returnType when typeof(List<string>) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildList(type, values);
                            break;
                        case Type returnType when typeof(List<byte>) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildList(type, values);
                            break;
                        case Type returnType when typeof(List<decimal>) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildList(type, values);
                            break;
                        case Type returnType when typeof(List<short>) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildList(type, values);
                            break;
                        case Type returnType when typeof(List<int>) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildList(type, values);
                            break;
                        case Type returnType when typeof(List<long>) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildList(type, values);
                            break;
                        case Type returnType when typeof(List<bool>) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildList(type, values);
                            break;
                        case Type returnType when typeof(List<object>) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildList(type, values);
                            break;

                        // primitive arrays
                        case Type returnType when typeof(string[]) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildArray(type, values);
                            break;
                        case Type returnType when typeof(byte[]) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildArray(type, values);
                            break;
                        case Type returnType when typeof(decimal[]) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildArray(type, values);
                            break;
                        case Type returnType when typeof(short[]) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildArray(type, values);
                            break;
                        case Type returnType when typeof(int[]) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildArray(type, values);
                            break;
                        case Type returnType when typeof(long[]) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildArray(type, values);
                            break;
                        case Type returnType when typeof(bool[]) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildArray(type, values);
                            break;
                        case Type returnType when typeof(object[]) is Type type
                                && (returnType == type || (Nullable.GetUnderlyingType(returnType) != null && (returnType.GenericTypeArguments.FirstOrDefault() is Type genericType) && genericType == type)):
                            setValue = BuildArray(type, values);
                            break;
                    }
                    memberInfoAction.SetValue(setValue);


                    //if (isEqualOrNullableType(typeof(string)))
                    //{
                    //    memberInfoAction.SetValue("");
                    //}
                    //else if (isEqualOrNullableType(typeof(decimal)))
                    //{
                    //    memberInfoAction.SetValue(0.1m);
                    //}
                    //else if (isEqualOrNullableType(typeof(int)))
                    //{
                    //    memberInfoAction.SetValue((int)0);
                    //}
                    //else if (isEqualOrNullableType(typeof(long)))
                    //{
                    //    memberInfoAction.SetValue((long)0);
                    //}
                    //else if (isEqualOrNullableType(typeof(bool)))
                    //{
                    //    memberInfoAction.SetValue(false);
                    //}
                    //else if (isEqualOrNullableType(typeof(Dictionary<string, int>)))
                    //{
                    //    memberInfoAction.SetValue(new Dictionary<string, int> { { "int", 0 } });
                    //}
                    //else if (isEqualOrNullableType(typeof(Dictionary<string, long>)))
                    //{
                    //    memberInfoAction.SetValue(new Dictionary<string, long> { { "long", 0 } });
                    //}
                    //else if (isEqualOrNullableType(typeof(Dictionary<string, decimal>)))
                    //{
                    //    memberInfoAction.SetValue(new Dictionary<string, decimal> { { "decimal", 0.1m } });
                    //}
                    //else if (isEqualOrNullableType(typeof(Dictionary<string, object>)))
                    //{
                    //    memberInfoAction.SetValue(new Dictionary<string, object> { { "string", " " }, { "decimal", (decimal)0 }, { "int", (int)0 }, { "long", (long)0 }, });
                    //}
                    //else if (isEqualOrNullableType(typeof(Dictionary<string, string>)))
                    //{
                    //    memberInfoAction.SetValue(new Dictionary<string, object> { { "string", " " } });
                    //}
                    //else if (isEqualOrNullableType(typeof(List<string>)))
                    //{
                    //    memberInfoAction.SetValue(new List<string> { " ", " " });
                    //}
                    //else if (isEqualOrNullableType(typeof(List<int>)))
                    //{
                    //    memberInfoAction.SetValue(new List<int> { 0, 1 });
                    //}
                    //else if (isEqualOrNullableType(typeof(List<long>)))
                    //{
                    //    memberInfoAction.SetValue(new List<long> { 0, 1 });
                    //}
                    //else if (isEqualOrNullableType(typeof(List<decimal>)))
                    //{
                    //    memberInfoAction.SetValue(new List<decimal> { 0.1m, 0.2m });
                    //}
                    //else if (isEqualOrNullableType(typeof(string[])))
                    //{
                    //    memberInfoAction.SetValue(new string[] { " ", " " });
                    //}
                    //else if (isEqualOrNullableType(typeof(decimal[])))
                    //{
                    //    memberInfoAction.SetValue(new decimal[] { 0.1m, 0.2m });
                    //}
                    //else if (isEqualOrNullableType(typeof(int[])))
                    //{
                    //    memberInfoAction.SetValue(new int[] { 0, 0 });
                    //}
                    //else if (isEqualOrNullableType(typeof(long[])))
                    //{
                    //    memberInfoAction.SetValue(new long[] { 0, 0 });
                    //}
                    //else if (isEqualOrNullableType(typeof(bool[])))
                    //{
                    //    memberInfoAction.SetValue(new bool[] { false, false });
                    //}
                    //else if (isEqualOrNullableType(typeof(object)))
                    //{
                    //    (string fieldString, int fieldInt, long fieldLong, decimal fieldDecimal, int? fieldIntNull) complexObj = new("", 0, 0, 0.1m, null);
                    //    memberInfoAction.SetValue(complexObj);
                    //}
                    //else if (isEqualOrNullableType(typeof(object[])))
                    //{
                    //    (string fieldString, int fieldInt, long fieldLong, decimal fieldDecimal, int? fieldIntNull) complexObj = new("", 0, 0, 0.1m, null);
                    //    memberInfoAction.SetValue(new object[] { complexObj });
                    //}
                    //else
                    //{

                    //}
                }
            });
        }

        /// <summary>
        /// Populate instance with dummy data
        /// </summary>
        /// <param name="instance">created instance</param>
        public static void PopulateWithDummyData([NotNull] this object instance)
        {
            ArgumentNullException.ThrowIfNull(instance, nameof(instance));
            PopulateWithDummyData<object>(instance);
        }
        #endregion PopulateWithDummyData





        /// <summary>
        /// Get private field from instance
        /// </summary>
        /// <typeparam name="TInstance">instance for get private field</typeparam>
        /// <typeparam name="TValue">spected type of value</typeparam>
        /// <param name="instance">instance for get private field</param>
        /// <param name="fieldName">private field name of instance</param>
        /// <returns></returns>
        public static TValue? GetPrivateField<TInstance, TValue>(this TInstance instance, string fieldName) => GetPrivateMember<TInstance, TValue>(instance, fieldName, MemberTypes.Field);

        /// <summary>
        /// Get private propery from instance
        /// </summary>
        /// <typeparam name="TInstance">instance for get private field</typeparam>
        /// <typeparam name="TValue">spected type of value</typeparam>
        /// <param name="instance">instance for get private property</param>
        /// <param name="propertyName">private property name of instance</param>
        /// <returns></returns>
        public static TValue? GetPrivateProperty<TInstance, TValue>(this TInstance instance, string propertyName) => GetPrivateMember<TInstance, TValue>(instance, propertyName, MemberTypes.Property);

        /// <summary>
        /// Get private member from instance
        /// </summary>
        /// <typeparam name="TInstance">instance for get private member</typeparam>
        /// <typeparam name="TValue">spected type of value result</typeparam>
        /// <param name="instance">instance for get private member</param>
        /// <param name="memberName">private member name of instance</param>
        /// <param name="memberTypes">type of member, supported only property, field and method</param>
        /// <param name="args">arguments used only if member type is method</param>
        /// <returns></returns>
        public static TValue? GetPrivateMember<TInstance, TValue>(this TInstance instance, string memberName, MemberTypes memberTypes = MemberTypes.Property | MemberTypes.Field | MemberTypes.Method, params object?[]? args)
        {
            ArgumentNullException.ThrowIfNull(instance, nameof(instance));
            ArgumentException.ThrowIfNullOrEmpty(memberName, nameof(memberName));

            if (memberTypes != MemberTypes.Method && args != null && args.Length > 0)
            {
                throw new InvalidOperationException($"MemberType {memberTypes} with arguments not supported.");
            }

            var typ = typeof(TInstance);
            var members = typ.GetMember(memberName, memberTypes, BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var member in members)
            {
                object? val = null;

                if (member is FieldInfo f)
                {
                    val = f.GetValue(instance);
                }
                else if (member is PropertyInfo p)
                {
                    val = p.GetValue(instance);
                }
                else if (member is MethodInfo m)
                {
                    val = m.Invoke(instance, args);
                }
                else
                {
                    continue;
                }


                if (val == null)
                {
                    return (TValue?)val;
                }

                try
                {
                    if (val is TValue result)
                    {
                        return result;
                    }
                }
                catch (Exception)
                {
                }

                try
                {
                    if (val is IConvertible convertible)
                    {
                        return (TValue)convertible.ToType(typeof(TValue), System.Globalization.CultureInfo.GetCultureInfo("pt-br"));
                    }
                }
                catch (Exception)
                {
                }

                try
                {
                    return val.ConvertWithJson<TValue>();
                }
                catch (Exception)
                {
                }


                return default;
            }

            return default;
        }

        /// <summary>
        /// Set private field or property from instance
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="instance"></param>
        /// <param name="memberName"></param>
        /// <param name="memberTypes"></param>
        /// <param name="args"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void SetPrivatePropertyOrFieldMember<TInstance, TValue>(this TInstance instance, string memberName, MemberTypes memberTypes = MemberTypes.Property | MemberTypes.Field | MemberTypes.Method, params object?[] args)
        {
            ArgumentNullException.ThrowIfNull(instance, nameof(instance));
            ArgumentException.ThrowIfNullOrEmpty(memberName, nameof(memberName));

            if (memberTypes == MemberTypes.Method)
            {
                throw new InvalidOperationException($"MemberType {memberTypes} not supported.");
            }

            var typ = typeof(TInstance);
            var members = typ.GetMember(memberName, memberTypes, BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var member in members)
            {

                if (member is FieldInfo f && f.FieldType == typeof(TValue))
                {
                    f.SetValue(instance, args[0]);
                    return;
                }
                else if (member is PropertyInfo p && p.PropertyType == typeof(TValue))
                {
                    p.SetValue(instance, args[0]);
                    return;
                }
                else
                {
                    continue;
                }
            }
        }


        #region CreateInstance
        /// <summary>
        /// Creates an instance of a generic type with the specified generic type arguments and constructor arguments.
        /// </summary>
        /// <param name="type">The generic type definition from which to create an instance.</param>
        /// <param name="generics">An array of types representing the generic type arguments.</param>
        /// <param name="args">An array of objects to be used as the constructor parameters for the created instance.</param>
        /// <returns>An instance of the specified type, or null if the type cannot be instantiated.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="generics"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not a generic type definition.</exception>
        /// <exception cref="InvalidOperationException">Thrown when an error occurs while creating the type instance, or when no matching constructor is found.</exception>
        public static object? CreateInstance(this Type type, Type[] generics, params object[] args)
        {
            // Validate input parameters
            ArgumentNullException.ThrowIfNull(type, nameof(type));
            ArgumentNullException.ThrowIfNull(generics, nameof(generics));

            // Ensure the type is a generic type definition
            if (!type.IsGenericTypeDefinition)
            {
                throw new ArgumentException("The provided type must be a generic type definition.", nameof(type));
            }

            // Create the concrete type using the specified generic type arguments
            Type concreteType;
            try
            {
                concreteType = type.MakeGenericType(generics);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException("Failed to create a generic type with the specified arguments.", ex);
            }

            // Create an instance of the concrete type using the provided constructor arguments
            try
            {
                return Activator.CreateInstance(concreteType, args);
            }
            catch (MissingMethodException ex)
            {
                throw new InvalidOperationException("No matching constructor found for the specified arguments.", ex);
            }
            catch (TargetInvocationException ex)
            {
                throw new InvalidOperationException("An error occurred while invoking the constructor.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while creating an instance of the type.", ex);
            }
        }

        /// <summary>
        /// Creates an instance of a specified type with the provided generic type parameters 
        /// and constructor arguments.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be created.</typeparam>
        /// <param name="generics">An array of <see cref="Type"/> representing the generic type arguments.</param>
        /// <param name="args">An array of objects representing the arguments to pass to the constructor.</param>
        /// <returns>
        /// An instance of type <typeparamref name="T"/> if successful; otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// This method uses reflection to create a generic type instance given its type 
        /// and the required parameters for its constructor.
        /// </remarks>
        public static T? CreateInstance<T>(Type[] generics, params object[] args)
                    => (T?)CreateInstance(typeof(T), generics, args);
        #endregion CreateInstance

        #region TrySetMember
        /// <summary>
        /// Attempts to set the value of a member (field, property, or method) on a specified instance 
        /// of type <typeparamref name="TInstance"/>. The member is identified by its name and type.
        /// </summary>
        /// <typeparam name="TInstance">The type of the instance on which to set the member.</typeparam>
        /// <typeparam name="TValue">The type of the value to set for the member.</typeparam>
        /// <param name="instance">The instance of type <typeparamref name="TInstance"/> on which the member will be set.</param>
        /// <param name="memberName">The name of the member (field, property, or method) to set.</param>
        /// <param name="bindingFlags">
        /// A bitwise combination of the enumeration values that specify the type of members to consider 
        /// when searching for the member specified by <paramref name="memberName"/>. 
        /// The default value is <c>BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static</c>.
        /// </param>
        /// <param name="memberTypes">
        /// A bitwise combination of the enumeration values of <see cref="MemberTypes"/> that specify 
        /// which member types to consider (properties, fields, or methods). 
        /// The default value is <c>MemberTypes.Property | MemberTypes.Field | MemberTypes.Method</c>.
        /// </param>
        /// <param name="args">An array of arguments to pass to the member if applicable. Must not be null.</param>
        /// <returns>
        /// Returns <c>true</c> if the member was successfully set; otherwise, returns <c>false</c>. 
        /// If the member name is not found or the member cannot accept the specified value, 
        /// the method returns <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="args"/> or <paramref name="instance"/> is null, 
        /// or when <paramref name="memberName"/> is null or an empty string.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="memberTypes"/> is <c>MemberTypes.Property</c> or 
        /// <c>MemberTypes.Field</c> and the length of <paramref name="args"/> is not equal to 1.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="memberTypes"/> specifies an invalid member type 
        /// or when the member is invalid.
        /// </exception>
        public static bool TrySetMember<TInstance, TValue>(this TInstance instance,
                    string memberName,
                    BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static,
                    MemberTypes memberTypes = MemberTypes.Property | MemberTypes.Field | MemberTypes.Method,
                    params object?[] args)
        {
            ArgumentNullException.ThrowIfNull(args, nameof(args));
            ArgumentNullException.ThrowIfNull(instance, nameof(instance));
            ArgumentException.ThrowIfNullOrEmpty(memberName, nameof(memberName));

            if ((memberTypes == MemberTypes.Property || memberTypes == MemberTypes.Field) && args.Length != 1)
            {
                throw new ArgumentOutOfRangeException($"The length of parameter \"args\" is not equal 1 ({args.Length}).");
            }

            if (memberTypes is not MemberTypes.Property or MemberTypes.Field or MemberTypes.Method)
            {
                throw new ArgumentException($"The member is invalid. ({memberTypes})");
            }

            var argsType = args.Select(s => s.GetType()).ToArray();

            var typ = typeof(TInstance);
            var members = typ.GetMember(memberName, memberTypes, bindingFlags);

            foreach (var member in members)
            {
                if (member is FieldInfo f && f.FieldType == typeof(TValue))
                {
                    f.SetValue(instance, args[0]);
                    return true;
                }
                else if (member is PropertyInfo p && p.PropertyType == typeof(TValue))
                {
                    p.SetValue(instance, args[0]);
                    return true;
                }
                else if (member is MethodInfo m && m.ReturnType == typeof(TValue) && m.GetParameters().Length == argsType.Length)
                {
                    var methodArgs = m.GetParameters().Select(s => s.ParameterType).ToArray();

                    if (argsType.All(a => methodArgs.Any(n => n == a)))
                    {
                        m.Invoke(instance, args);
                        return true;
                    }
                }
                else
                {
                    continue;
                }
            }

            return false;
        }


        /// <summary>
        /// Attempts to set a member (field, property, or method) of the specified instance using reflection.
        /// </summary>
        /// <typeparam name="TInstance">The type of the instance whose member is being set.</typeparam>
        /// <typeparam name="TValue">The type of the value to set on the member.</typeparam>
        /// <param name="instance">The instance on which the member will be set.</param>
        /// <param name="funcSelectMember">An expression that selects the member to set.</param>
        /// <param name="bindingFlags">The binding flags that specify how the member is searched for.</param>
        /// <param name="memberTypes">The types of members to include in the search.</param>
        /// <param name="args">The arguments to pass to the member if it is a method.</param>
        /// <returns>
        /// True if the member was successfully set; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="instance"/> or <paramref name="funcSelectMember"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="memberTypes"/> is <see cref="MemberTypes.Property"/> or <see cref="MemberTypes.Field"/> and <paramref name="args"/> length is not equal to 1.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="memberTypes"/> is invalid or the expression is not a valid member expression.</exception>
        public static bool TrySetMember<TInstance, TValue>(this TInstance instance,
                    Expression<Func<TInstance, TValue>> funcSelectMember,
                    BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static,
                    MemberTypes memberTypes = MemberTypes.Property | MemberTypes.Field | MemberTypes.Method,
                    params object[] args)
        {
            // Validate input parameters
            ArgumentNullException.ThrowIfNull(instance, nameof(instance));
            ArgumentNullException.ThrowIfNull(funcSelectMember, nameof(funcSelectMember));

            if ((memberTypes == MemberTypes.Property || memberTypes == MemberTypes.Field) && args.Length != 1)
            {
                throw new ArgumentOutOfRangeException($"The length of parameter \"args\" is not equal 1 ({args.Length}).");
            }

            if (memberTypes is not MemberTypes.Property or MemberTypes.Field or MemberTypes.Method)
            {
                throw new ArgumentException($"The member is invalid. ({memberTypes})");
            }


            var memberExpression = funcSelectMember.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException("The expression is not a valid member expression.");
            }

            var memberName = memberExpression.Member.Name;

            // Get the type of the instance
            var typ = typeof(TInstance);
            var members = typ.GetMember(memberName, memberTypes, bindingFlags);

            // Iterate over the members and set the value
            foreach (var member in members)
            {
                if (member is FieldInfo f && f.FieldType == args[0].GetType())
                {
                    f.SetValue(instance, args[0]);
                    return true;
                }
                else if (member is PropertyInfo p && p.PropertyType == args[0].GetType())
                {
                    p.SetValue(instance, args[0]);
                    return true;
                }
                else if (member is MethodInfo m && m.GetParameters().Length == args.Length)
                {
                    var methodArgs = m.GetParameters().Select(s => s.ParameterType).ToArray();

                    if (args.Select(a => a.GetType()).SequenceEqual(methodArgs))
                    {
                        m.Invoke(instance, args);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to set a member on the provided instance using the specified expression to select the member.
        /// </summary>
        /// <typeparam name="TInstance">The type of the instance on which the member will be set.</typeparam>
        /// <param name="instance">The instance of the type TInstance on which the member is being set.</param>
        /// <param name="funcSelectMember">An expression that selects the member (property, field, or method) to set.</param>
        /// <param name="bindingFlags">The binding flags that specify the type of members to include.</param>
        /// <param name="memberTypes">The types of members to include (properties, fields, and/or methods).</param>
        /// <param name="args">An array of objects to pass as arguments to the member if applicable.</param>
        /// <returns>
        /// A boolean value indicating whether the operation succeeded or failed.
        /// </returns>
        public static bool TrySetMember<TInstance>(this TInstance instance,
                    Expression<Func<TInstance, object>> funcSelectMember,
                    BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static,
                    MemberTypes memberTypes = MemberTypes.Property | MemberTypes.Field | MemberTypes.Method,
                    params object[] args)
                => TrySetMember(instance, funcSelectMember, bindingFlags, memberTypes, args);

        /// <summary>
        /// Attempts to set a member (property, field, or method) of a given instance 
        /// using reflection. This method allows for specifying which members to 
        /// consider as well as the flags to use when looking for those members.
        /// </summary>
        /// <typeparam name="TInstance">The type of the instance whose member is to be set.</typeparam>
        /// <param name="instance">The instance on which the member is to be set.</param>
        /// <param name="memberName">The name of the member to set.</param>
        /// <param name="bindingFlags">
        /// A bitwise combination of the BindingFlags that specify how the search for 
        /// members is conducted (such as non-
        public static bool TrySetMember<TInstance>(this TInstance instance,
                    string memberName,
                    BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static,
                    MemberTypes memberTypes = MemberTypes.Property | MemberTypes.Field | MemberTypes.Method,
                    params object[] args)
                => TrySetMember(instance, memberName, bindingFlags, memberTypes, args);

        /// <summary>
        /// Attempts to set a member (property, field, or method) of a specified object instance
        /// using a provided expression to select the member.
        /// </summary>
        /// <param name="instance">The object instance whose member is to be set.</param>
        /// <param name="funcSelectMember">An expression that selects the member to be set.</param>
        /// <param name="bindingFlags">A bitwise combination of the BindingFlags that specify how 
        /// the search for members is conducted by reflection. The default is 
        /// BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static.</param>
        /// <param name="memberTypes">A bitwise combination of MemberTypes that specifies which 
        /// types of members to search for. The default is MemberTypes.Property | 
        /// MemberTypes.Field | MemberTypes.Method.</param>
        /// <param name="args">An array of objects that are the arguments to pass to the member.</param>
        /// <returns>A boolean value indicating whether the operation succeeded.</returns>
        public static bool TrySetMember(this object instance,
                    Expression<Func<object, object>> funcSelectMember,
                    BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static,
                    MemberTypes memberTypes = MemberTypes.Property | MemberTypes.Field | MemberTypes.Method,
                    params object[] args)
                => TrySetMember(instance, funcSelectMember, bindingFlags, memberTypes, args);

        /// <summary>
        /// Attempts to set a member of a specified object instance by its name.
        /// </summary>
        /// <param name="instance">The object instance whose member is to be set.</param>
        /// <param name="memberName">The name of the member to set.</param>
        /// <param name="bindingFlags">
        /// A bitwise combination of the <see cref="BindingFlags"/> enumeration that 
        /// specifies how the search for members is conducted by reflection.
        /// Default is <c>BindingFlags.NonPublic | BindingFlags.Instance | 
        /// BindingFlags.Public | BindingFlags.Static</c>.
        /// </param>
        /// <param name="memberTypes">
        /// A bitwise combination of the <see cref="MemberTypes"/> enumeration that 
        /// specifies the types of members to consider for the operation. 
        /// Default is <c>MemberTypes.Property | MemberTypes.Field | MemberTypes.Method</c>.
        /// </param>
        /// <param name="args">
        /// An optional array of objects that are the parameters to be passed to the member
        /// method or property setter. This parameter is optional.
        /// </param>
        /// <returns>
        /// <c>true</c> if the member was set successfully; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method allows for flexible member access and modification of an object's properties, 
        /// methods, and fields using reflection. It is particularly useful for dynamic object manipulation.
        /// </remarks>
        public static bool TrySetMember(this object instance,
                    string memberName,
                    BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static,
                    MemberTypes memberTypes = MemberTypes.Property | MemberTypes.Field | MemberTypes.Method,
                    params object[] args)
                => TrySetMember(instance, memberName, bindingFlags, memberTypes, args);
        #endregion TrySetMember

        #region TryGetMember



        /// <summary>
        /// Attempts to retrieve the value of a member (property or field) from the specified instance.
        /// </summary>
        /// <typeparam name="TInstance">The type of the instance from which the member value is being retrieved.</typeparam>
        /// <typeparam name="TValue">The type of the value being returned.</typeparam>
        /// <param name="instance">The instance from which to get the member value.</param>
        /// <param name="memberName">The name of the member to retrieve.</param>
        /// <param name="value">When this method returns, contains the member value, or null if the member was not found.</param>
        /// <param name="bindingFlags">
        /// A bitwise combination of the enumeration values that specify how the search for members is conducted.
        /// The default is set to include both 
        public static bool TryGetMember<TInstance, TValue>(this TInstance instance,
                    string memberName,
                    out TValue? value,
                    BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static,
                    MemberTypes memberTypes = MemberTypes.Property | MemberTypes.Field)
        {
            // Initialize output parameter
            value = default;

            // Validate input parameters
            ArgumentNullException.ThrowIfNull(instance, nameof(instance));
            ArgumentException.ThrowIfNullOrEmpty(memberName, nameof(memberName));

            // Get the type of the instance
            var typ = typeof(TInstance);
            var members = typ.GetMember(memberName, memberTypes, bindingFlags);

            // Iterate over the members and get the value
            foreach (var member in members)
            {
                if (member is FieldInfo f)
                {
                    value = (TValue?)f.GetValue(instance);
                    return true;
                }
                else if (member is PropertyInfo p)
                {
                    value = (TValue?)p.GetValue(instance);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to retrieve the value of a member (field or property) from an instance of a specified type using an expression.
        /// </summary>
        /// <typeparam name="TInstance">The type of the instance from which to retrieve the member value.</typeparam>
        /// <typeparam name="TValue">The type of the value to be retrieved from the member.</typeparam>
        /// <param name="instance">The instance from which to get the member value.</param>
        /// <param name="funcSelectMember">An expression that identifies the member whose value is to be retrieved.</param>
        /// <param name="value">When this method returns, contains the value of the member if successful, or null otherwise.</param>
        /// <param name="bindingFlags">
        /// A bitwise combination of BindingFlags that specifies how the search for members is conducted.
        /// The default is BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static.
        /// </param>
        /// <param name="memberTypes">
        /// A bitwise combination of MemberTypes that specifies the types of members to search for.
        /// The default is MemberTypes.Property | MemberTypes.Field | MemberTypes.Method.
        /// </param>
        /// <returns>
        /// Returns true if the member was found and its value was successfully retrieved; otherwise, false.
        /// </returns>
        public static bool TryGetMember<TInstance, TValue>(this TInstance instance,
                    Expression<Func<TInstance, TValue>> funcSelectMember,
                    out TValue? value,
                    BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static,
                    MemberTypes memberTypes = MemberTypes.Property | MemberTypes.Field | MemberTypes.Method)
        {
            // Initialize output parameter
            value = default;

            // Validate input parameters
            ArgumentNullException.ThrowIfNull(instance, nameof(instance));
            ArgumentNullException.ThrowIfNull(funcSelectMember, nameof(funcSelectMember));

            var memberExpression = funcSelectMember.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException("The expression is not a valid member expression.");
            }

            var memberName = memberExpression.Member.Name;

            var typ = typeof(TInstance);
            var members = typ.GetMember(memberName, memberTypes, bindingFlags);

            foreach (var member in members)
            {
                if (member is FieldInfo f)
                {
                    value = (TValue?)f.GetValue(instance);
                    return true;
                }
                else if (member is PropertyInfo p)
                {
                    value = (TValue?)p.GetValue(instance);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to retrieve a member (property or field) from an instance of the specified type.
        /// </summary>
        /// <typeparam name="TInstance">The type of the instance from which to get the member.</typeparam>
        /// <param name="instance">The instance from which the member will be retrieved.</param>
        /// <param name="memberName">The name of the member to retrieve.</param>
        /// <param name="value">When this method returns, contains the value of the member if found; otherwise, null.</param>
        /// <param name="bindingFlags">
        /// The binding attributes that control the search for members. 
        /// Defaults to NonPublic, Instance, Public, and Static.
        /// </param>
        /// <param name="memberTypes">
        /// The types of members to search for. 
        /// Defaults to Property and Field.
        /// </param>
        /// <returns>
        /// Returns true if the member was found and successfully retrieved; otherwise, false.
        /// </returns>
        public static bool TryGetMember<TInstance>(this TInstance instance,
                    string memberName,
                    out object? value,
                    BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static,
                    MemberTypes memberTypes = MemberTypes.Property | MemberTypes.Field)
                => TryGetMember<TInstance, object>(instance, memberName, out value, bindingFlags, memberTypes);

        /// <summary>
        /// Attempts to retrieve a member value from the specified instance using a provided selector expression.
        /// </summary>
        /// <typeparam name="TInstance">The type of the instance from which to retrieve the member.</typeparam>
        /// <param name="instance">The instance from which the member will be retrieved.</param>
        /// <param name="funcSelectMember">
        /// A lambda expression that selects the member to retrieve. The expression must be in the form of 
        /// 'instance => instance.Member', where 'Member' can be a property, field, or method.
        /// </param>
        /// <param name="value">
        /// The output parameter that will hold the value of the retrieved member if the operation succeeds;
        /// otherwise, it will be null.
        /// </param>
        /// <param name="bindingFlags">
        /// A bitwise combination of BindingFlags that specifies how the search for members is conducted.
        /// The default is set to include non-
        public static bool TryGetMember<TInstance>(this TInstance instance,
                    Expression<Func<TInstance, object>> funcSelectMember,
                    out object? value,
                    BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static,
                    MemberTypes memberTypes = MemberTypes.Property | MemberTypes.Field | MemberTypes.Method)
                => TryGetMember<TInstance, object>(instance, funcSelectMember, out value, bindingFlags, memberTypes);

        /// <summary>
        /// Tries to retrieve the value of a member (property or field) from an instance of an object
        /// based on the provided member name, binding flags, and member types.
        /// </summary>
        /// <param name="instance">The object instance from which to retrieve the member value.</param>
        /// <param name="memberName">The name of the member to retrieve.</param>
        /// <param name="value">When this method returns, contains the value of the member if found; otherwise, null.</param>
        /// <param name="bindingFlags">
        /// A bitwise combination of the enumeration values that specify how the search for members
        /// is conducted. The default is to search for non-
        public static bool TryGetMember(this object instance,
                    string memberName,
                    out object? value,
                    BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static,
                    MemberTypes memberTypes = MemberTypes.Property | MemberTypes.Field)
                => TryGetMember<object, object>(instance, memberName, out value, bindingFlags, memberTypes);

        /// <summary>
        /// Tries to get a member from the specified instance using the provided selector expression.
        /// </summary>
        /// <param name="instance">The object instance from which to get the member.</param>
        /// <param name="funcSelectMember">A lambda expression that specifies the member to get.</param>
        /// <param name="value">When this method returns, contains the value of the member retrieved, or null if the member was not found.</param>
        /// <param name="bindingFlags">A bitmask that specifies the use of the 
        public static bool TryGetMember(this object instance,
                    Expression<Func<object, object>> funcSelectMember,
                    out object? value,
                    BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static,
                    MemberTypes memberTypes = MemberTypes.Property | MemberTypes.Field | MemberTypes.Method)
                => TryGetMember<object, object>(instance, funcSelectMember, out value, bindingFlags, memberTypes);
        #endregion TryGetMember

        #region TryMethod
        /// <summary>
        /// Tries to invoke a method on the provided instance with the specified method name and generic arguments.
        /// </summary>
        /// <param name="instance">The object instance on which the method should be invoked.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="generics">An optional array of types representing the generic type parameters for the method.</param>
        /// <param name="value">When this method returns, contains the return value of the invoked method, if successful; otherwise, null.</param>
        /// <param name="bindingFlags">The binding flags that determine the search for the method. The default value includes non-
        public static bool TryMethod(this object instance,
            string methodName,
            Type[]? generics,
            out object? value,
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static,
            params object?[]? args)
        {

            value = null;

            ArgumentNullException.ThrowIfNull(instance, nameof(instance));
            ArgumentException.ThrowIfNullOrEmpty(methodName, nameof(methodName));
            ArgumentNullException.ThrowIfNull(generics, nameof(generics));

            // Get the type of the instance
            var type = instance.GetType();

            // Get all methods with the specified name and binding flags
            var methods = type.GetMethods(bindingFlags).Where(m => m.Name == methodName);

            foreach (var method in methods)
            {
                MethodInfo methodToInvoke = method;

                // If the method is generic, make sure it matches the provided generic arguments
                if (generics != null && generics.Length > 0)//method.IsGenericMethodDefinition)
                {
                    if (!method.IsGenericMethodDefinition || method.GetGenericArguments().Length != generics.Length)
                    {
                        continue; // Skip if the generic argument count does not match
                    }

                    try
                    {
                        methodToInvoke = method.MakeGenericMethod(generics);
                    }
                    catch
                    {
                        continue; // Skip if unable to make generic method
                    }
                }

                if (args != null && args.Length > 0)
                {

                    // Get method parameters and ensure they match the provided arguments
                    var parameters = methodToInvoke.GetParameters();
                    if (parameters.Length != args.Length)
                    {
                        continue; // Skip if parameter count does not match
                    }

                    // Check if all parameter types match
                    bool parametersMatch = true;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (args[i] != null && !parameters[i].ParameterType.IsInstanceOfType(args[i]))
                        {
                            parametersMatch = false;
                            break;
                        }
                    }

                    if (!parametersMatch)
                    {
                        continue; // Skip if any parameter type does not match
                    }
                }

                // Invoke the method and return the result
                try
                {
                    value = methodToInvoke.Invoke(instance, (args != null && args.Length > 0) ? args : null);
                    return true;
                }
                catch (TargetInvocationException ex)
                {
                    throw new InvalidOperationException("An error occurred while invoking the method.", ex);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("An error occurred while attempting to invoke the method.", ex);
                }
            }

            return false; // Return false if no suitable method was found
        }

        /// <summary>
        /// Tries to invoke a method on the specified instance, determined by the method name and optional generic types.
        /// </summary>
        /// <typeparam name="TInstance">The type of the instance on which the method is to be invoked.</typeparam>
        /// <typeparam name="TValue">The type of the value that is returned from the invoked method.</typeparam>
        /// <param name="instance">The object instance on which the method will be invoked.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="generics">An optional array of types to be used as generic type parameters for the method, or null.</param>
        /// <param name="value">When this method returns, contains the value returned by the invoked method, or null if the invocation failed.</param>
        /// <param name="bindingFlags">A bitwise combination of values that specify the binding flags for the method search. The default is NonPublic | Instance | Public | Static.</param>
        /// <param name="args">An optional array of parameters to pass to the method, or null.</param>
        /// <returns>
        /// Returns true if the method was successfully found and invoked; otherwise, false.
        /// </returns>
        public static bool TryMethod<TInstance, TValue>(this TInstance instance,
            string methodName,
            Type[]? generics,
            out TValue? value,
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static,
            params object?[]? args)
        {
            var result = TryMethod((object)instance, methodName, generics, out var tValue, bindingFlags, args);

            value = (TValue?)tValue;

            return result;
        }
        #endregion TryMethod


        #region RecursiveIterator
        /// <summary>
        /// Represents a delegate that can be used to define a function that takes an integer level,
        /// an integer index, and a reference argument of type T, and returns a result of type TResult.
        /// </summary>
        /// <typeparam name="T">The type of the input argument that is passed by reference.</typeparam>
        /// <typeparam name="TResult">The type of the result returned by the delegate.</typeparam>
        /// <param name="level">An integer parameter that represents the level.</param>
        /// <param name="index">An integer parameter that represents the index.</param>
        /// <param name="arg">A reference parameter of type T that can be modified by the delegate.</param>
        /// <returns>A value of type TResult that is the result of the function.</returns>
        public delegate TResult FuncRef<T, TResult>(int level, int index, ref T arg);


        /// <summary>
        /// Iterates through the members (properties or fields) of an object instance recursively,
        /// applying a specified function to each member found. This method serves as an entry point/// for the recursive member iteration process.
        /// </summary>
        /// <param name="instance">The object instance to iterate over.</param>
        /// <param name="func">A delegate that represents the function to apply to each member.</param>
        /// <param name="memberName">The name of the specific member to look for during the iteration.</param>
        /// <param name="bindingFlags">A bitwise combination of BindingFlags that specify how the search for members is conducted.</param>
        /// <param name="memberTypes">A bitwise combination of MemberTypes that specify which types of members to include in the search.</param>
        public static void RecursiveIterator(this object instance,
            FuncRef<object, object> func,
            string memberName,
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static,
            MemberTypes memberTypes = MemberTypes.Property | MemberTypes.Field)
        {
            // Calls the overloaded RecursiveIterator method to start the iteration process,
            // initializing the depth and index parameters. The depth and index could be used // for tracking the recursion level or providing indices if necessary during recursion.
            RecursiveIterator(instance, 0, 0, func, memberName, bindingFlags, memberTypes);
        }

        /// <summary>
        /// Recursively iterates through the members of an object, invoking a specified function delegate on
        /// members that match the specified type and name. It supports both fields and properties and allows
        /// for the traversal of nested members.
        /// </summary>
        /// <param name="instance">The instance of the object to iterate over.</param>
        /// <param name="level">The current depth of recursion.</param>
        /// <param name="pos">The current position in the iteration.</param>
        /// <param name="func">The function delegate to invoke on matching fields or properties.</param>
        /// <param name="memberName">The name of the member to search for.</param>
        /// <param name="bindingFlags">The binding flags to use for member search. Defaults to 
        /// NonPublic, Instance, Public, and Static members.</param>
        /// <param name="memberTypes">The types of members to look for. Defaults to 
        /// Property and Field types.</param>
        /// <returns>
        /// Returns true if a matching member was found and the function was invoked, 
        /// false otherwise.
        /// </returns>
        private static bool RecursiveIterator(object instance,
                    int level, int pos, FuncRef<object, object> func,
                    string memberName,
                    BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static,
                    MemberTypes memberTypes = MemberTypes.Property | MemberTypes.Field)
        {
            var TValue = func.GetType().GetGenericArguments()[1];

            var typ = instance.GetType();
            var members = typ.GetMember(memberName, memberTypes, bindingFlags);

            foreach (var member in members)
            {
                if (member is FieldInfo f && f.FieldType == TValue)
                {
                    // Get the field value
                    object? fieldValue = f.GetValue(instance);
                    // Call the func delegate with the field reference
                    func.Invoke(level, pos++, ref fieldValue);
                    // Set the field value back
                    f.SetValue(instance, fieldValue);
                    return true;
                }
                else if (member is PropertyInfo p && p.PropertyType == TValue)
                {
                    // Get the property value
                    object propertyValue = p.GetValue(instance);
                    // Call the func delegate with the property reference
                    func.Invoke(level, pos++, ref propertyValue);
                    // Set the property value back
                    p.SetValue(instance, propertyValue);
                    return true;
                }
                else if (member is FieldInfo || member is PropertyInfo)
                {
                    // Handle recursive calls for nested properties or fields
                    object? memberValue = member is FieldInfo ? ((FieldInfo)member).GetValue(instance) : ((PropertyInfo)member).GetValue(instance);

                    if (memberValue != null && (memberValue.GetType().IsClass || memberValue.GetType().IsInterface || memberValue.GetType().IsValueType))
                    {
                        // Recursive call
                        if (RecursiveIterator(memberValue, level + 1, pos, func, memberName, bindingFlags, memberTypes))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        #endregion RecursiveIterator


        /// <summary>
        /// Try convert using implicity operator
        /// </summary>
        /// <param name="instanceType"></param>
        /// <param name="resultType"></param>
        /// <param name="instance"></param>
        /// <param name="converted"></param>
        /// <returns></returns>
        public static bool TryCastImplicityOperator(this object instance, Type instanceType, Type resultType, out object converted)
        {
            converted = default;

            try
            {
                var methods = instanceType.GetMethods(BindingFlags.Static | BindingFlags.Public);

                var method = methods.FirstOrDefault(f => string.Equals(f.Name, "op_Implicit", StringComparison.OrdinalIgnoreCase) &&
                    f.GetParameters().Length == 1 &&
                    ((f.GetParameters()[0].ParameterType.IsGenericType && instanceType.IsGenericType) ? f.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == instanceType.GetGenericTypeDefinition() : ((f.GetParameters()[0].ParameterType.IsGenericType && !instanceType.IsGenericType) ? (f.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == instanceType) : ((!f.GetParameters()[0].ParameterType.IsGenericType && instanceType.IsGenericType) ? (f.GetParameters()[0].ParameterType == instanceType.GetGenericTypeDefinition()) : (f.GetParameters()[0].ParameterType == instanceType)))) &&
                    f.ReturnType == resultType);
                //(f.GetParameters()[0].ParameterType == instanceType.GetType()) &&
                //((f.ReturnType.IsGenericType && resultType.IsGenericType) ? f.ReturnType.GetGenericTypeDefinition() == resultType.GetGenericTypeDefinition() : ((f.ReturnType.IsGenericType && !resultType.IsGenericType) ?(f.ReturnType.GetGenericTypeDefinition() == resultType) :((!f.ReturnType.IsGenericType && resultType.IsGenericType) ? (f.ReturnType == resultType.GetGenericTypeDefinition()) : (f.ReturnType == resultType)))));


                if (method != null)
                {
                    converted = method.Invoke(null, new object?[1] { instance });

                    return converted != null;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Try convert using explicity operator
        /// </summary>
        /// <param name="instanceType"></param>
        /// <param name="resultType"></param>
        /// <param name="instance"></param>
        /// <param name="converted"></param>
        /// <returns></returns>
        public static bool TryCastExplicitOperator(this object instance, Type instanceType, Type resultType, out object converted)
        {
            converted = default;

            try
            {
                var methods = instanceType.GetMethods(BindingFlags.Static | BindingFlags.Public);

                var method = methods.FirstOrDefault(f => string.Equals(f.Name, "op_Explicit", StringComparison.OrdinalIgnoreCase) &&
                    f.GetParameters().Length == 1 &&
                    f.GetParameters()[0].ParameterType == instanceType.GetType() &&
                    f.ReturnType == resultType);


                if (method != null)
                {
                    converted = method.Invoke(null, new object?[1] { instance });

                    return converted != null;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Try convert using implicity operator
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="instance"></param>
        /// <param name="converted"></param>
        /// <returns></returns>
        public static bool TryCastImplicityOperator<TInstance, TValue>(this TInstance instance, out TValue converted)
        {
            converted = default;

            try
            {
                var methods = typeof(TInstance).GetMethods(BindingFlags.Static | BindingFlags.Public);

                var method = methods.FirstOrDefault(f => string.Equals(f.Name, "op_Implicit", StringComparison.OrdinalIgnoreCase) &&
                    f.GetParameters().Length == 1 &&
                    f.GetParameters()[0].ParameterType == typeof(TInstance).GetType() &&
                    f.ReturnType == typeof(TValue));


                if (method != null)
                {
                    converted = (TValue)method.Invoke(null, new object?[1] { instance });

                    return converted != null;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Try convert using explicity operator
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="instance"></param>
        /// <param name="converted"></param>
        /// <returns></returns>
        public static bool TryCastExplicitOperator<TInstance, TValue>(this TInstance instance, out TValue converted)
        {
            converted = default;

            try
            {
                var methods = typeof(TInstance).GetMethods(BindingFlags.Static | BindingFlags.Public);

                var method = methods.FirstOrDefault(f => string.Equals(f.Name, "op_Explicit", StringComparison.OrdinalIgnoreCase) &&
                    f.GetParameters().Length == 1 &&
                    f.GetParameters()[0].ParameterType == typeof(TInstance).GetType() &&
                    f.ReturnType == typeof(TValue));


                if (method != null)
                {
                    converted = (TValue)method.Invoke(null, new object?[1] { instance });

                    return converted != null;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Try convert using implicity or explicity operator
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="instance"></param>
        /// <param name="converted"></param>
        /// <returns></returns>
        public static bool TryCastUsingOperator<TInstance, TValue>(this TInstance instance, out TValue converted) => TryCastImplicityOperator(instance, out converted) || TryCastExplicitOperator(instance, out converted);


        /// <summary>
        /// Try convert using implicity or explicity operator
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="instanceType"></param>
        /// <param name="resultType"></param>
        /// <param name="converted"></param>
        /// <returns></returns>
        public static bool TryCastUsingOperator(this object instance, Type instanceType, Type resultType, out object converted) => TryCastImplicityOperator(instance, instanceType, resultType, out converted) || TryCastExplicitOperator(instance, instanceType, resultType, out converted);

    }
}
