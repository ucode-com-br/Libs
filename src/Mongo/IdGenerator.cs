using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using Polly.Simmy.Outcomes;
using UCode.Mongo.Models;

namespace UCode.Mongo
{

    public class IdGenerator : IIdGenerator
    {
        private static readonly IdGenerator __instance = new IdGenerator();

        public static IdGenerator Instance => __instance;

        public IdGenerator()
        {
        }

        public bool ImplementsIObjectIdRecursively(Type type)
        {
            if (type == null)
                return false;

            foreach (var item in type.GetInterfaces())
            {
                if (item.IsGenericType && (item.GetGenericTypeDefinition() == typeof(IObjectBase<,>) || item.GetGenericTypeDefinition() == typeof(IObjectBase<>) || item.GetGenericTypeDefinition() == typeof(IObjectBase)))
                {
                    return true;
                }
            }

            //// Verifica se o próprio tipo implementa IObjectId<,>
            //if (type.GetInterfaces().Any(i =>
            //    i.IsGenericType &&
            //    i.GetGenericTypeDefinition() == typeof(IObjectId<,>)))
            //{
            //    return true;
            //}

            // Se o tipo base existe, verifica recursivamente na hierarquia de classes
            return type.BaseType != null && ImplementsIObjectIdRecursively(type.BaseType);
        }

        public bool ImplementsIObjectIdRecursively(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            // Usa o tipo do objeto e chama a função recursiva
            return ImplementsIObjectIdRecursively(obj.GetType());
        }

        private void OnCompleted(ref object document, ref object idValue, ref object container)
        {
            var docType = document.GetType();

            Type tObjectId = docType.GetProperty(nameof(IObjectBase.Id), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).PropertyType;

            Type tUser = docType.GetProperty(nameof(IObjectBase.CreatedBy), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).PropertyType;

            var type = typeof(IdGeneratorCompletedEventArgs<,>).MakeGenericType(tObjectId, tUser);

            var eventArgs = Activator.CreateInstance(type);

            var resulProp = type.GetProperty(nameof(IdGeneratorCompletedEventArgs<string, string>.Result), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var documentProp = type.GetProperty(nameof(IdGeneratorCompletedEventArgs<string, string>.Document), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var containerProp = type.GetProperty(nameof(IdGeneratorCompletedEventArgs<string, string>.Container), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            documentProp.SetValue(eventArgs, document);
            resulProp.SetValue(eventArgs, idValue);
            containerProp.SetValue(eventArgs, container);

            var onProcessCompleted = docType.GetMethod(nameof(IObjectBase.OnProcessCompleted), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            _ = onProcessCompleted.Invoke(document, new object[] { eventArgs });
        }

        public object GenerateId(object container, object document)
        {
            object result = default;

            if (ImplementsIObjectIdRecursively(document))
            {
                var docType = document.GetType();

                var idProp = docType.GetProperty(nameof(IObjectBase.Id), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                var objectidGenerated = ObjectId.GenerateNewId();

                result = idProp.GetValue(document);
                if (result != default)
                {
                    OnCompleted(ref document, ref result, ref container);

                    return result;
                }

                if (idProp.PropertyType == typeof(string))
                {
                    result = objectidGenerated.ToString();
                }
                else if (idProp.PropertyType == typeof(Guid))
                {
                    result = Guid.NewGuid();
                }
                else if (idProp.PropertyType == typeof(ObjectId))
                {
                    result = objectidGenerated;
                }



                if (result != default)
                {
                    OnCompleted(ref document, ref result, ref container);

                    return result;
                }
            }

            return result;
        }

        public bool IsEmpty(object id)
        {
            if (id != null)
            {
                return (ObjectId)id == ObjectId.Empty;
            }

            return true;
        }
    }
}
