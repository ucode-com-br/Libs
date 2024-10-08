using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace UCode.DeepCloning.SpecializedCloners
{
    internal sealed class MultiDimArrayCloner : ClonerBase
    {
        public static CloneObjectDelegate<T> Buid<T>()
        {
            var type = typeof(T);
            var method = new DynamicMethod(string.Empty, type, new Type[] { type, typeof(Dictionary<object, object>), typeof(DeepCloningOptions) });
            var il = method.GetILGenerator();
            var t = type.GetElementType();
            var arrRank = type.GetArrayRank();

            il.DeclareLocal(type);  // dest
            il.DeclareLocal(typeof(bool));  // DeepCloneStrings
            il.DeclareLocal(type);  // type-casted src
            il.DeclareLocal(typeof(object));  // existingClone

            var localCount = 4;

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloca_S, 3);  // existingClone
            il.Emit(OpCodes.Call, ObjectDictionaryByObjectTryGetValueMethodInfo);
            var cacheNotAvailable = il.DefineLabel();
            il.Emit(OpCodes.Brfalse, cacheNotAvailable);
            il.Emit(OpCodes.Ldloc_3);  // existingClone
            il.Emit(OpCodes.Ret);
            il.MarkLabel(cacheNotAvailable);


            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, DeepCloningOptionsGetDeepCloneStringsMethodInfo);
            il.Emit(OpCodes.Stloc_1);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, type);
            il.Emit(OpCodes.Stloc_2);

            for (var i = 0; i < arrRank; i++)
            {
                il.Emit(OpCodes.Ldloc_2);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Call, type.GetMethod("GetLength"));
            }

            il.Emit(OpCodes.Newobj, type.GetConstructor(Enumerable.Range(0, arrRank).Select(l => typeof(int)).ToArray()));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Call, ObjectDictionaryByObjectAddMethodInfo);

            for (var i = 0; i < arrRank; i++)
            {
                il.DeclareLocal(typeof(int));  // rx

                il.Emit(OpCodes.Ldloc_2);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Call, type.GetMethod("GetLength"));
                il.Emit(OpCodes.Stloc, localCount + i);  // rx = GetLength(x)
            }

#if NETCOREAPP
            var labelGroups = new Stack<(Label, Label)>();
#else
            var labelGroups = new Stack<Tuple<Label, Label>>();
#endif

            for (var i = 0; i < arrRank; i++)
            {
                il.DeclareLocal(typeof(int));  // x
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stloc, localCount + arrRank + i);  // x

                var evaluate = il.DefineLabel();
                il.Emit(OpCodes.Br, evaluate);

                var repeat = il.DefineLabel();
                il.MarkLabel(repeat);

#if NETCOREAPP
                labelGroups.Push((evaluate, repeat));
#else
                labelGroups.Push(Tuple.Create(evaluate, repeat));
#endif
            }

            il.Emit(OpCodes.Ldloc_0);  // dest

            for (var i = 0; i < arrRank; i++)
            {
                var x = localCount + arrRank + i;
                il.Emit(OpCodes.Ldloc, x);
            }

            if (!DeepCloning.IsSimpleValueType(t) && t != typeof(string))
            {
                il.Emit(OpCodes.Ldsfld, typeof(DeepCloning<>).MakeGenericType(t).GetField(nameof(DeepCloning<T>.CloneObject), BindingFlags.NonPublic | BindingFlags.Static));
            }

            il.Emit(OpCodes.Ldloc_2);  // src

            for (var i = 0; i < arrRank; i++)
            {
                var x = localCount + arrRank + i;
                il.Emit(OpCodes.Ldloc, x);
            }

            il.Emit(OpCodes.Call, type.GetMethod("Get"));

            if (t.IsValueType)
            {
                if (!DeepCloning.IsSimpleValueType(t))
                {
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Call, typeof(CloneObjectDelegate<>).MakeGenericType(t).GetMethod("Invoke"));
                }

                il.Emit(OpCodes.Call, type.GetMethod("Set"));
            }
            else if (t == typeof(string))
            {
                il.Emit(OpCodes.Ldloc_1);
                var skipDeepCloneString = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, skipDeepCloneString);
                il.Emit(OpCodes.Call, StringToCharArrayMethodInfo);
                il.Emit(OpCodes.Newobj, StringCtor);
                il.MarkLabel(skipDeepCloneString);

                il.Emit(OpCodes.Call, type.GetMethod("Set"));
            }
            else
            {
                il.Emit(OpCodes.Dup);
                var lblSkipCloneIfNull = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, lblSkipCloneIfNull);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Call, typeof(CloneObjectDelegate<>).MakeGenericType(t).GetMethod("Invoke"));
                il.Emit(OpCodes.Call, type.GetMethod("Set"));
                var lblAvoidPopIfNotNull = il.DefineLabel();
                il.Emit(OpCodes.Br, lblAvoidPopIfNotNull);

                il.MarkLabel(lblSkipCloneIfNull);
                il.Emit(OpCodes.Pop);  // pop null value
                il.Emit(OpCodes.Pop);  // pop field ref
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Call, type.GetMethod("Set"));
                il.MarkLabel(lblAvoidPopIfNotNull);
            }

            for (var i = arrRank - 1; i > -1; i--)
            {
                var x = localCount + arrRank + i;
                var rx = localCount + i;

                il.Emit(OpCodes.Ldloc, x);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Stloc, x);

                var labels = labelGroups.Pop();
                var evaluate = labels.Item1;
                var repeat = labels.Item2;

                il.MarkLabel(evaluate);
                il.Emit(OpCodes.Ldloc, x);
                il.Emit(OpCodes.Ldloc, rx);
                il.Emit(OpCodes.Blt, repeat);
            }

            il.Emit(OpCodes.Ldloc_0);  // dest
            il.Emit(OpCodes.Ret);

#if NET5_0_OR_GREATER
            return method.CreateDelegate<CloneObjectDelegate<T>>();
#else
            return (CloneObjectDelegate<T>)method.CreateDelegate(typeof(CloneObjectDelegate<T>));
#endif
        }
    }
}
