using System;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using UCode.Mongo.Models;

namespace UCode.Mongo
{

    /// <summary>
    /// Represents an identifier generator that implements the IIdGenerator interface.
    /// This class is responsible for generating unique identifiers.
    /// </summary>
    /// <remarks>
    /// This class can be extended to include various identifier generation strategies such as 
    /// sequential IDs, GUIDs, or custom formats.
    /// </remarks>
    public class IdGenerator : IIdGenerator
    {
        /// <summary>
        /// Static instance of the IdGenerator class.
        /// This instance is used for generating unique identifiers throughout the application.
        /// </summary>
        private static readonly IdGenerator __instance = new IdGenerator();

        /// <summary>
        /// Gets the singleton instance of the <see cref="IdGenerator"/> class.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="IdGenerator"/> class.
        /// </value>
        public static IdGenerator Instance => __instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class.
        /// </summary>
        public IdGenerator()
        {
        }

        /// <summary>
        /// Determines whether a specified type implements the IObjectBase interface recursively.
        /// </summary>
        /// <param name="type">
        /// The type to check for the implementation of IObjectBase. This can be null.
        /// </param>
        /// <returns>
        /// Returns true if the specified type or any of its base types implement the 
        /// IObjectBase interface; otherwise, returns false.
        /// </returns>
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


            // Se o tipo base existe, verifica recursivamente na hierarquia de classes
            return type.BaseType != null && this.ImplementsIObjectIdRecursively(type.BaseType);
        }

        /// <summary>
        /// Checks if the given object implements the IObjectId interface recursively.
        /// </summary>
        /// <param name="obj">The object to check for the interface implementation.</param>
        /// <returns>
        /// True if the object or any of its base types implements the IObjectId interface; otherwise, false.
        /// </returns>
        public bool ImplementsIObjectIdRecursively(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            // Usa o tipo do objeto e chama a função recursiva
            return this.ImplementsIObjectIdRecursively(obj.GetType());
        }

        /// <summary>
        /// Handles the completion of an operation related to a document.
        /// This method creates an instance of a generic event arguments class 
        /// based on the document's type and sets relevant properties such as 
        /// the generated ID, the document itself, and its container.
        /// It then invokes the document's <see cref="IObjectBase.OnProcessCompleted"/> method 
        /// with the created event arguments.
        /// </summary>
        /// <param name="document">The document that has been processed.</param>
        /// <param name="idValue">The generated ID value associated with the document.</param>
        /// <param name="container">The container associated with the document.</param>
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

        /// <summary>
        /// Generates a new identifier for the specified document if it implements the IObjectId interface.
        /// It checks the type of the ID property and generates an ID accordingly.
        /// </summary>
        /// <param name="container">The container object that holds the document.</param>
        /// <param name="document">The document for which the ID needs to be generated.</param>
        /// <returns>
        /// Returns the newly generated ID or the existing ID if already present.
        /// If the document does not implement IObjectId, it returns the default value of the ID type.
        /// </returns>
        public object GenerateId(object container, object document)
        {
            object? result = default;

            if (this.ImplementsIObjectIdRecursively(document))
            {
                var docType = document.GetType();

                var idProp = docType.GetProperty(nameof(IObjectBase.Id), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                var objectidGenerated = ObjectId.GenerateNewId();

                result = idProp.GetValue(document);
                if (result != default)
                {
                    this.OnCompleted(ref document, ref result, ref container);

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
                    this.OnCompleted(ref document, ref result, ref container);

                    return result;
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the given identifier is considered empty.
        /// </summary>
        /// <param name="id">The identifier to check, which can be an object.</param>
        /// <returns>
        /// Returns true if the id is null or if it is equal to ObjectId.Empty;
        /// otherwise, returns false.
        /// </returns>
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
