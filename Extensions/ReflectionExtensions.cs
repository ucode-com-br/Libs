
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
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

            var result = (IDictionary) Activator.CreateInstance(dictType);

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
