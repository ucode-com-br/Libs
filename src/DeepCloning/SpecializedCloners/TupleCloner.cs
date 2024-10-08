using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace UCode.DeepCloning.SpecializedCloners
{
    internal sealed class TupleCloner : ClonerBase
    {
        public static CloneObjectDelegate<T> Buid<T>()
        {
            var type = typeof(T);
            var method = new DynamicMethod(string.Empty, type, new Type[] { type, typeof(Dictionary<object, object>), typeof(DeepCloningOptions) });
            var il = method.GetILGenerator();

            il.DeclareLocal(typeof(bool));  // DeepCloneStrings
            il.DeclareLocal(typeof(object));  // existingClone

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloca_S, 1);  // existingClone
            il.Emit(OpCodes.Call, ObjectDictionaryByObjectTryGetValueMethodInfo);
            var cacheNotAvailable = il.DefineLabel();
            il.Emit(OpCodes.Brfalse, cacheNotAvailable);
            il.Emit(OpCodes.Ldloc_1);  // existingClone
            il.Emit(OpCodes.Ret);
            il.MarkLabel(cacheNotAvailable);

            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, DeepCloningOptionsGetDeepCloneStringsMethodInfo);
            il.Emit(OpCodes.Stloc_0);

            foreach (var prop in type.GetProperties().OrderBy(p => p.Name))
            {
                if (!DeepCloning.IsSimpleValueType(prop.PropertyType) && prop.PropertyType != typeof(string))
                {
                    il.Emit(OpCodes.Ldsfld, typeof(DeepCloning<>).MakeGenericType(prop.PropertyType).GetField(nameof(DeepCloning<T>.CloneObject), BindingFlags.NonPublic | BindingFlags.Static));
                }

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, prop.GetGetMethod());

                if (prop.PropertyType.IsValueType)
                {
                    if (!DeepCloning.IsSimpleValueType(prop.PropertyType))
                    {
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldarg_2);
                        il.Emit(OpCodes.Call, typeof(CloneObjectDelegate<>).MakeGenericType(prop.PropertyType).GetMethod("Invoke"));
                    }
                }
                else if (prop.PropertyType == typeof(string))
                {
                    il.Emit(OpCodes.Ldloc_0);
                    var skipDeepCloneString = il.DefineLabel();
                    il.Emit(OpCodes.Brfalse, skipDeepCloneString);
                    il.Emit(OpCodes.Callvirt, StringToCharArrayMethodInfo);
                    il.Emit(OpCodes.Newobj, StringCtor);
                    il.MarkLabel(skipDeepCloneString);
                }
                else
                {
                    il.Emit(OpCodes.Dup);
                    var lblSkipCloneIfNull = il.DefineLabel();
                    il.Emit(OpCodes.Brfalse, lblSkipCloneIfNull);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Call, typeof(CloneObjectDelegate<>).MakeGenericType(prop.PropertyType).GetMethod("Invoke"));

                    var gotoEnd = il.DefineLabel();
                    il.Emit(OpCodes.Br, gotoEnd);

                    il.MarkLabel(lblSkipCloneIfNull);
                    il.Emit(OpCodes.Pop);  // null value
                    il.Emit(OpCodes.Pop);  // field ref
                    il.Emit(OpCodes.Ldnull);  // add back null value 

                    il.MarkLabel(gotoEnd);
                }
            }

            var tupleCreate = typeof(Tuple).GetMethods().First(m => m.Name == "Create" && m.GetGenericArguments().Length == type.GetGenericArguments().Length).MakeGenericMethod(type.GetGenericArguments());
            il.Emit(OpCodes.Call, tupleCreate);
            il.Emit(OpCodes.Ret);

#if NET5_0_OR_GREATER
            return method.CreateDelegate<CloneObjectDelegate<T>>();
#else
            return (CloneObjectDelegate<T>)method.CreateDelegate(typeof(CloneObjectDelegate<T>));
#endif
        }
    }
}
