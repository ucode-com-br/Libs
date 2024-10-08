using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace UCode.DeepCloning.SpecializedCloners
{
    internal sealed class GenericDictionaryCloner : ClonerBase
    {
        public static CloneObjectDelegate<T> Buid<T>()
        {
            var type = typeof(T);
            var method = new DynamicMethod(string.Empty, type, new Type[] { type, typeof(Dictionary<object, object>), typeof(DeepCloningOptions) });
            var il = method.GetILGenerator();

            var lblRepeat = il.DefineLabel();
            var lblMoveNext = il.DefineLabel();
            var t = typeof(KeyValuePair<,>).MakeGenericType(type.GetGenericArguments());

            il.DeclareLocal(type);  // 0: destination
            il.DeclareLocal(typeof(IEnumerator<>).MakeGenericType(t));  // 1: IEnumerator<T>
            il.DeclareLocal(t);  // 2: KeyValuePair<,>
            il.DeclareLocal(typeof(bool));  // 3: DeepCloningOptions.DeepCloneStrings
            il.DeclareLocal(typeof(object));  // 4: existingClone


            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloca_S, 4);  // existingClone
            il.Emit(OpCodes.Call, ObjectDictionaryByObjectTryGetValueMethodInfo);
            var cacheNotAvailable = il.DefineLabel();
            il.Emit(OpCodes.Brfalse, cacheNotAvailable);
            il.Emit(OpCodes.Ldloc_S, 4);  // existingClone
            il.Emit(OpCodes.Ret);
            il.MarkLabel(cacheNotAvailable);


            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, DeepCloningOptionsGetDeepCloneStringsMethodInfo);
            il.Emit(OpCodes.Stloc_3);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, type.GetProperty("Count").GetGetMethod());
            il.Emit(OpCodes.Newobj, type.GetConstructor(new[] { typeof(int) }));
            il.Emit(OpCodes.Stloc_0);

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Call, ObjectDictionaryByObjectAddMethodInfo);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, typeof(IEnumerable<>).MakeGenericType(t).GetMethod("GetEnumerator"));
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Br_S, lblMoveNext);
            il.MarkLabel(lblRepeat);
            il.Emit(OpCodes.Ldloc_S, 1);
            il.Emit(OpCodes.Callvirt, typeof(IEnumerator<>).MakeGenericType(t).GetMethod("get_Current"));
            il.Emit(OpCodes.Stloc_2);  // kv

            il.Emit(OpCodes.Ldloc_0);  // dest

            var tKey = type.GetGenericArguments()[0];

            if (tKey == typeof(string))
            {
                il.Emit(OpCodes.Ldloca_S, 2);  // kv
                il.Emit(OpCodes.Call, t.GetMethod("get_Key"));

                il.Emit(OpCodes.Ldloc_3);
                var skipDeepCloneString = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, skipDeepCloneString);
                il.Emit(OpCodes.Call, StringToCharArrayMethodInfo);
                il.Emit(OpCodes.Newobj, StringCtor);
                il.MarkLabel(skipDeepCloneString);
            }
            else if (!DeepCloning.IsSimpleValueType(tKey))
            {
                il.Emit(OpCodes.Ldsfld, typeof(DeepCloning<>).MakeGenericType(tKey).GetField(nameof(DeepCloning<object>.CloneObject), BindingFlags.NonPublic | BindingFlags.Static));

                il.Emit(OpCodes.Ldloca_S, 2);  // kv
                il.Emit(OpCodes.Call, t.GetMethod("get_Key"));

                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Call, typeof(CloneObjectDelegate<>).MakeGenericType(tKey).GetMethod("Invoke"));
            }
            else
            {
                il.Emit(OpCodes.Ldloca_S, 2);  // kv
                il.Emit(OpCodes.Call, t.GetMethod("get_Key"));
            }

            var tValue = type.GetGenericArguments()[1];

            if (tValue == typeof(string))
            {
                il.Emit(OpCodes.Ldloca_S, 2);  // kv
                il.Emit(OpCodes.Call, t.GetMethod("get_Value"));

                il.Emit(OpCodes.Ldloc_3);
                var skipDeepCloneString = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, skipDeepCloneString);
                il.Emit(OpCodes.Call, StringToCharArrayMethodInfo);
                il.Emit(OpCodes.Newobj, StringCtor);
                il.MarkLabel(skipDeepCloneString);
                il.Emit(OpCodes.Call, type.GetMethod("Add"));
            }
            else if (tValue.IsValueType)
            {
                if (!DeepCloning.IsSimpleValueType(tValue))
                {
                    il.Emit(OpCodes.Ldsfld, typeof(DeepCloning<>).MakeGenericType(tValue).GetField(nameof(DeepCloning<object>.CloneObject), BindingFlags.NonPublic | BindingFlags.Static));

                    il.Emit(OpCodes.Ldloca_S, 2);  // kv
                    il.Emit(OpCodes.Call, t.GetMethod("get_Value"));

                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Call, typeof(CloneObjectDelegate<>).MakeGenericType(tValue).GetMethod("Invoke"));
                }
                else
                {
                    il.Emit(OpCodes.Ldloca_S, 2);  // kv
                    il.Emit(OpCodes.Call, t.GetMethod("get_Value"));
                }

                il.Emit(OpCodes.Call, type.GetMethod("Add"));
            }
            else
            {
                il.Emit(OpCodes.Ldsfld, typeof(DeepCloning<>).MakeGenericType(tValue).GetField(nameof(DeepCloning<T>.CloneObject), BindingFlags.NonPublic | BindingFlags.Static));

                il.Emit(OpCodes.Ldloca_S, 2);  // kv
                il.Emit(OpCodes.Call, t.GetMethod("get_Value"));

                il.Emit(OpCodes.Dup);
                var lblSkipCloneIfNull = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, lblSkipCloneIfNull);

                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Call, typeof(CloneObjectDelegate<>).MakeGenericType(tValue).GetMethod("Invoke"));

                il.Emit(OpCodes.Call, type.GetMethod("Add"));

                var gotoEnd = il.DefineLabel();
                il.Emit(OpCodes.Br, gotoEnd);

                il.MarkLabel(lblSkipCloneIfNull);
                il.Emit(OpCodes.Pop);  // null value
                il.Emit(OpCodes.Pop);  // fld
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Call, type.GetMethod("Add"));

                il.MarkLabel(gotoEnd);
            }

            il.MarkLabel(lblMoveNext);
            il.Emit(OpCodes.Ldloc_S, 1);
            il.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetMethod("MoveNext"));
            il.Emit(OpCodes.Brtrue_S, lblRepeat);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

#if NET5_0_OR_GREATER
            return method.CreateDelegate<CloneObjectDelegate<T>>();
#else
            return (CloneObjectDelegate<T>)method.CreateDelegate(typeof(CloneObjectDelegate<T>));
#endif
        }
    }
}
