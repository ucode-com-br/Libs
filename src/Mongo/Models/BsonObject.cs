using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UCode.Mongo.Models
{
    /// <summary>
    /// Represents a BSON object with string keys.
    /// This class inherits from the generic BsonObject class, providing functionality
    /// specific to BSON serialization and manipulation.
    /// </summary>
    /// <remarks>
    /// The BsonObject class is designed to work with BSON (Binary JSON) which is a 
    /// data format commonly used in NoSQL databases, particularly MongoDB. 
    /// It implements the IObjectBase and IObjectBaseTenant interfaces, allowing 
    /// it to integrate with systems that require object base functionalities 
    /// and tenant support.
    /// </remarks>
    /// <typeparam name="string">The type of keys used in this BsonObject.</typeparam>
    public class BsonObject : BsonObject<string>, IObjectBase, IObjectBaseTenant
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonObject"/> class 
        /// using the specified BsonDocument.
        /// </summary>
        /// <param name="bsonDocument">The BsonDocument to initialize the BsonObject from.</param>
        public BsonObject(BsonDocument bsonDocument) : base(bsonDocument)
        {
        }
    }

    /// <summary>
    /// Represents a BSON object with a parameterized object identifier type.
    /// </summary>
    /// <typeparam name="TObjectId">The type of the object identifier.</typeparam>
    /// <remarks>
    /// This class extends the functionality of the generic <see cref="BsonObject{TObjectId, TId}"/> class, 
    /// allowing for specialized behavior with a string identifier.
    /// It also implements the <see cref="IObjectBase{TObjectId}"/> and <see cref="IObjectBaseTenant"/> 
    /// interfaces which provide a structure for object-based identifiers and tenant-based functionality.
    /// </remarks>
    public class BsonObject<TObjectId> : BsonObject<TObjectId, string>, IObjectBase<TObjectId>, IObjectBaseTenant
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonObject"/> class using the provided BsonDocument.
        /// </summary>
        /// <param name="bsonDocument">The BsonDocument that will be used to initialize this BsonObject.</param>
        /// <returns>
        /// This constructor does not return a value. It initializes the BsonObject instance.
        /// </returns>
        public BsonObject(BsonDocument bsonDocument) : base(bsonDocument)
        {
        }
    }


    /// <summary>
    /// Represents a BSON object that implements the IObjectBase interface with a specified 
    /// object identifier type and user type. It also implements the IObjectBaseTenant interface.
    /// </summary>
    /// <typeparam name="TObjectId">
    /// The type of the object identifier used as the key for the BSON object.
    /// </typeparam>
    /// <typeparam name="TUser">
    /// The type representing the user associated with the BSON object.
    /// </typeparam>
    /// <remarks>
    /// This class is designed to work with BSON (Binary JSON) data structures 
    /// and is intended for scenarios where object representation and manipulation 
    /// are needed in a type-safe manner while also accommodating multi-tenancy.
    /// </remarks>
    public class BsonObject<TObjectId, TUser> : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {
        private readonly BsonDocument _bsonDocument;

        /// <summary>
        /// Gets or sets the unique identifier for the object.
        /// </summary>
        /// <remarks>
        /// This property retrieves the `Id` from the underlying object using a getter method
        /// and assigns a new value to the object using a setter method.
        /// </remarks>
        /// <value>
        /// The identifier of the object of type <typeparamref name="TObjectId"/>.
        /// </value>
        /// <example>
        /// <code>
        /// var identifier = myObject.Id; // Gets the Id
        /// myObject.Id = new TObjectId(); // Sets a new Id
        /// </code>
        /// </example>
        public TObjectId Id
        {
            get => Get(t => t.Id);
            set => Set(t => t.Id, value);
        }

        /// <summary>
        /// Represents the user who created the entity.
        /// </summary>
        /// <value>
        /// The user who created the entity, of type <typeparamref name="TUser"/>.
        /// </value>
        /// <remarks>
        /// This property provides a way to get or set the user responsible for the creation of the current entity instance.
        /// It utilizes the `Get` and `Set` methods to handle the underlying storage and retrieval.
        /// </remarks>
        public TUser CreatedBy
        {
            get => Get(t => t.CreatedBy);
            set => Set(t => t.CreatedBy, value);
        }

        /// <summary>
        /// Gets or sets the date and time when the instance was created.
        /// </summary>
        /// <value>
        /// A <see cref="DateTime"/> value representing the creation date and time.
        /// </value>
        public DateTime CreatedAt
        {
            get => Get(t => t.CreatedAt);
            set => Set(t => t.CreatedAt, value);
        }

        /// <summary>
        /// Gets or sets the user who updated the entity.
        /// </summary>
        /// <value>
        /// The user who last updated the entity.
        /// </value>
        /// <returns>
        /// A nullable instance of <typeparamref name="TUser"/> representing the user who updated the entity.
        /// </returns>
        public TUser? UpdatedBy
        {
            get => Get(t => t.UpdatedBy);
            set => Set(t => t.UpdatedBy, value);
        }

        /// <summary>
        /// Gets or sets the date and time when the entity was last updated.
        /// </summary>
        /// <value>
        /// A nullable <see cref="DateTime"/> that represents the last updated time.
        /// If no value has been set, it will return <c>null</c>.
        /// </value>
        public DateTime? UpdatedAt
        {
            get => Get(t => t.UpdatedAt);
            set => Set(t => t.UpdatedAt, value);
        }

        /// <summary>
        /// Gets or sets the extra elements associated with the current instance.
        /// This property allows for storing additional dynamic data in a dictionary format.
        /// </summary>
        /// <value>
        /// A dictionary containing string keys and values of type object, allowing for null values.
        /// Returns null if no extra elements are set.
        /// </value>
        public Dictionary<string, object?>? ExtraElements
        {
            get => Get(t => t.ExtraElements);
            set => Set(t => t.ExtraElements, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the object is disabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the object is disabled; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// The property is backed by a getter and setter that utilize
        /// the Get and Set methods to handle the underlying state.
        /// </remarks>
        public bool Disabled
        {
            get => Get(t => t.Disabled);
            set => Set(t => t.Disabled, value);
        }


        /// <summary>
        /// Gets or sets the unique identifier for the reference.
        /// </summary>
        /// <value>
        /// A <see cref="Guid"/> that represents the reference identifier.
        /// </value>
        public Guid Ref
        {
            get => Get(t => t.Ref);
            set => Set(t => t.Ref, value);
        }
        /// <summary>
        /// Gets or sets the unique identifier for the tenant.
        /// </summary>
        /// <value>
        /// A <see cref="Guid"/> representing the tenant's unique identifier.
        /// </value>
        public Guid Tenant
        {
            get => Get(t => t.Tenant);
            set => Set(t => t.Tenant, value);
        }




        /// <summary>
        /// Initializes a new instance of the <see cref="BsonObject"/> class.
        /// </summary>
        /// <param name="bsonDocument">
        /// A <see cref="BsonDocument"/> object that is used to initialize the <see cref="BsonObject"/>.
        /// </param>
        /// <returns>
        /// This constructor does not return a value.
        /// </returns>
        public BsonObject(BsonDocument bsonDocument)
        {
            _bsonDocument = bsonDocument;
        }





        #region implicit
        public static implicit operator BsonDocument(BsonObject<TObjectId, TUser> objectId) => objectId._bsonDocument;
        public static implicit operator BsonObject<TObjectId, TUser>(BsonDocument bsonDocument) => new(bsonDocument);
        #endregion implicit

        #region private methods
        /// <summary>
        /// Converts a BsonValue to a specified type T.
        /// This method handles conversion for various primitive types, nullable types,
        /// dictionaries, and complex types using JSON deserialization.
        /// </summary>
        /// <typeparam name="T">The target type to which the BsonValue should be converted.</typeparam>
        /// <param name="bsonValue">The BsonValue to convert.</param>
        /// <returns>
        /// The converted value of type T. 
        /// If the bsonValue is null or BSON null, the default value for type T is returned.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the conversion type is not supported or if JSON deserialization results in null.
        /// </exception>
        private T ConvertBson<T>(BsonValue bsonValue)
        {
            if (bsonValue == null || bsonValue.IsBsonNull)
            {
                return default;
            }

            var targetType = typeof(T);

            // Dicionário de conversões diretas para tipos primitivos
            var conversions = new Dictionary<Type, Func<BsonValue, object>>()
            {
                { typeof(string), bv => bv.AsString },
                { typeof(int), bv => bv.AsInt32 },
                { typeof(int?), bv => bv.AsInt32 },
                { typeof(long), bv => bv.AsInt64 },
                { typeof(long?), bv => bv.AsInt64 },
                { typeof(double), bv => bv.AsDouble },
                { typeof(double?), bv => bv.AsDouble },
                { typeof(bool), bv => bv.AsBoolean },
                { typeof(bool?), bv => bv.AsBoolean },
                { typeof(DateTime), bv => bv.ToUniversalTime() },
                { typeof(DateTime?), bv => bv.ToUniversalTime() },
                { typeof(Guid), bv => bv.AsGuid },
                { typeof(Guid?), bv => bv.AsGuid },
                { typeof(ObjectId), bv => bv.AsObjectId },
                { typeof(ObjectId?), bv => bv.AsObjectId }
            };

            // Verificar se o tipo está no dicionário de conversões
            if (conversions.TryGetValue(targetType, out var conversion))
            {
                return (T)conversion(bsonValue);
            }

            // Conversão para Dictionary<string, object>
            if (targetType == typeof(Dictionary<string, object>) || targetType == typeof(Dictionary<string, object?>))
            {
                return (T)(object)ConvertToDictionary(bsonValue.AsBsonDocument);
            }

            // Conversão para tipos complexos usando JSON
            try
            {
                var json = bsonValue.ToJson();
                var converted = System.Text.Json.JsonSerializer.Deserialize<T>(json);

                if (converted == null)
                {
                    throw new InvalidOperationException($"Json conversion returned null for: {json}");
                }

                return converted;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Not supported conversion type: {targetType.Name}", ex);
            }
        }

        // Método auxiliar para conversão para Dictionary<string, object>
        /// <summary>
        /// Converts a BsonDocument into a Dictionary with string keys and object values.
        /// This method iterates through each element in the BsonDocument and converts it recursively,
        /// handling nested complex types appropriately.
        /// </summary>
        /// <param name="bsonDocument">The BsonDocument to convert to a Dictionary.</param>
        /// <returns>A Dictionary containing the converted elements of the BsonDocument.</returns>
        private Dictionary<string, object> ConvertToDictionary(BsonDocument bsonDocument)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var element in bsonDocument)
            {
                dictionary[element.Name] = ConvertBson<object>(element.Value); // Recursivo para lidar com tipos complexos aninhados
            }

            return dictionary;
        }

        /// <summary>
        /// Retrieves an object of type T based on the provided expression.
        /// </summary>
        /// <typeparam name="T">The type of the object to be retrieved.</typeparam>
        /// <param name="expression">An expression that defines how to retrieve the object.</param>
        /// <returns>
        /// An object of type T that is retrieved based on the expression provided.
        /// </returns>
        private T Get<T>(Expression<Func<IObjectBase<TObjectId, TUser>, T>> expression) => Get<IObjectBase<TObjectId, TUser>, T>(expression);
        /// <summary>
        /// Sets a value for a given expression of type <see cref="IObjectBase{TObjectId, TUser}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to be set.</typeparam>
        /// <param name="expression">The expression that represents the object base containing the value.</param>
        /// <param name="value">The value to be set.</param>
        /// <remarks>
        /// This method serves as a wrapper to call another overload of the 
        /// <see cref="Set{TObjectBase, T}(Expression{Func{TObjectBase, T}}, T)"/> method.
        /// </remarks>
        private void Set<T>(Expression<Func<IObjectBase<TObjectId, TUser>, T>> expression, T value) => Set<IObjectBase<TObjectId, TUser>, T>(expression, value);

        /// <summary>
        /// Retrieves a value of type <typeparamref name="T"/> by evaluating a specified 
        /// expression against an object that implements the <see cref="IObjectBaseTenant"/> interface.
        /// </summary>
        /// <typeparam name="T">The type of the value to be retrieved.</typeparam>
        /// <param name="expression">An expression that defines the logic for retrieving the value.</param>
        /// <returns>The value of type <typeparamref name="T"/> that corresponds to the evaluated expression.</returns>
        private T Get<T>(Expression<Func<IObjectBaseTenant, T>> expression) => Get<IObjectBaseTenant, T>(expression);
        /// <summary>
        /// Sets the value of a property specified by the given expression for an object 
        /// that implements the <see cref="IObjectBaseTenant"/> interface.
        /// </summary>
        /// <typeparam name="T">The type of the property to set.</typeparam>
        /// <param name="expression">An expression representing the property to set.</param>
        /// <param name="value">The value to assign to the property.</param>
        /// <remarks>
        /// This method is a generic wrapper that calls another overloaded Set method 
        /// to perform the actual setting operation.
        /// </remarks>
        private void Set<T>(Expression<Func<IObjectBaseTenant, T>> expression, T value) => Set<IObjectBaseTenant, T>(expression, value);



        /// <summary>
        /// Retrieves a value of type <typeparamref name="TOut"/> from a BSON document based on the provided 
        /// expression that specifies the member to access.
        /// </summary>
        /// <typeparam name="TIn">The type of the input that the expression operates on.</typeparam>
        /// <typeparam name="TOut">The type of the output value being retrieved from the BSON document.</typeparam>
        /// <param name="expression">
        /// A lambda expression representing the member access. It is expected to be in the form of 
        /// <c>object => object.Member</c> or <c>object => (Type)object.Member</c>.
        /// </param>
        /// <returns>
        /// The value of type <typeparamref name="TOut"/> retrieved from the BSON document associated with
        /// the specified member in the expression.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="expression"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the provided <paramref name="expression"/> is not a member access.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the BSON document does not contain a key matching the extracted element name.
        /// </exception>
        private TOut Get<TIn, TOut>(Expression<Func<TIn, TOut>> expression)
        {
            ArgumentNullException.ThrowIfNull(expression);

            MemberInfo memberInfo = null;

            // Extract MemberInfo from the expression
            if (expression.Body is MemberExpression memberExpression)
            {
                memberInfo = memberExpression.Member;
            }
            else if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operandMemberExpression)
            {
                memberInfo = operandMemberExpression.Member;
            }
            else
            {
                throw new InvalidOperationException("Expression is not a member access");
            }

            var bsonName = this.GetBsonElementName<TOut>(memberInfo);

            if (!this._bsonDocument.TryGetValue(bsonName, out var bsonValue))
            {
                throw new KeyNotFoundException($"Element '{bsonName}' not found in BSON document.");
            }

            return this.ConvertBson<TOut>(bsonValue);
        }

        /// <summary>
        /// Sets a value in a BSON document using an expression that accesses a member.
        /// This method accepts a lambda expression involving the input and output types,
        /// extracts the member information, and assigns the provided value to the corresponding BSON field.
        /// </summary>
        /// <typeparam name="TIn">The type of the input expression parameter.</typeparam>
        /// <typeparam name="TOut">The type of the value being set in the BSON document.</typeparam>
        /// <param name="expression">A lambda expression that specifies the member to set.</param>
        /// <param name="value">The value to set in the BSON document under the specified member.</param>
        private void Set<TIn, TOut>(Expression<Func<TIn, TOut>> expression, TOut value)
        {
            ArgumentNullException.ThrowIfNull(expression);

            MemberInfo memberInfo = null;

            // Extract MemberInfo from the expression
            if (expression.Body is MemberExpression memberExpression)
            {
                memberInfo = memberExpression.Member;
            }
            else if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operandMemberExpression)
            {
                memberInfo = operandMemberExpression.Member;
            }
            else
            {
                throw new InvalidOperationException("Expression is not a member access");
            }

            var bsonName = this.GetBsonElementName<TOut>(memberInfo);

            // Convert the value to BsonValue
            var bsonValue = ConvertToBsonValue(value);

            // Set the value in the BSON document
            _bsonDocument[bsonName] = bsonValue;
        }


        /// <summary>
        /// Retrieves the BSON element name for the specified member information.
        /// If the member is a property or a field with the <see cref="BsonElementAttribute"/> attribute, 
        /// the method returns the element name defined by the attribute. 
        /// If the attribute is not present, it returns the name of the property or field.
        /// </summary>
        /// <typeparam name="T">The type of the object containing the member.</typeparam>
        /// <param name="memberInfo">The member information for which to retrieve the BSON element name.</param>
        /// <returns>
        /// The BSON element name corresponding to the member, either from the <see cref="BsonElementAttribute"/> 
        /// or the member's own name if the attribute is not present.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the specified member information is neither a property nor a field.
        /// </exception>
        private string GetBsonElementName<T>(MemberInfo memberInfo)
        {
            if (memberInfo.MemberType == MemberTypes.Property && memberInfo is PropertyInfo property)
            {
                // Verificar se a propriedade tem o atributo BsonElement
                var bsonElementAttribute = property
                    .GetCustomAttributes(typeof(BsonElementAttribute), inherit: true)
                    .FirstOrDefault() as BsonElementAttribute;

                // Retornar o nome do atributo BsonElement ou o nome da propriedade se o atributo não existir
                return bsonElementAttribute?.ElementName ?? property.Name;
            }


            if (memberInfo.MemberType == MemberTypes.Property && memberInfo is FieldInfo field)
            {
                // Verificar se o campo tem o atributo BsonElement
                var bsonElementAttribute = field
                    .GetCustomAttributes(typeof(BsonElementAttribute), inherit: true)
                    .FirstOrDefault() as BsonElementAttribute;

                // Retornar o nome do atributo BsonElement ou o nome do campo se o atributo não existir
                return bsonElementAttribute?.ElementName ?? field.Name;
            }

            throw new InvalidOperationException($"Member '{memberInfo.Name}'.");
        }

        /// <summary>
        /// Retrieves the BSON element name for a given member (property or field) in a specified type.
        /// If the member has a <see cref="BsonElementAttribute"/>, its <see cref="BsonElementAttribute.ElementName"/> is returned. 
        /// Otherwise, the name of the property or field is used.
        /// </summary>
        /// <typeparam name="T">The type from which to retrieve the member name.</typeparam>
        /// <param name="memberName">The name of the member (property or field) to lookup.</param>
        /// <returns>
        /// The BSON element name corresponding to the specified member name, either from the attribute or the member's name.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the specified member does not exist in the type <typeparamref name="T"/>.
        /// </exception>
        private string GetBsonElementName<T>(string memberName)
        {
            var type = typeof(T);

            // Procurar a propriedade com o nome especificado
            var property = type.GetProperty(memberName);
            if (property != null)
            {
                // Verificar se a propriedade tem o atributo BsonElement
                var bsonElementAttribute = property
                    .GetCustomAttributes(typeof(BsonElementAttribute), inherit: true)
                    .FirstOrDefault() as BsonElementAttribute;

                // Retornar o nome do atributo BsonElement ou o nome da propriedade se o atributo não existir
                return bsonElementAttribute?.ElementName ?? property.Name;
            }

            // Procurar o campo com o nome especificado
            var field = type.GetField(memberName);
            if (field != null)
            {
                // Verificar se o campo tem o atributo BsonElement
                var bsonElementAttribute = field
                    .GetCustomAttributes(typeof(BsonElementAttribute), inherit: true)
                    .FirstOrDefault() as BsonElementAttribute;

                // Retornar o nome do atributo BsonElement ou o nome do campo se o atributo não existir
                return bsonElementAttribute?.ElementName ?? field.Name;
            }

            throw new InvalidOperationException($"Member '{memberName}' does not exist in type '{type.Name}'.");
        }

        /// <summary>
        /// Converts a given value of any type to a <see cref="BsonValue"/>.
        /// The method handles null values, primitive types, commonly used types, 
        /// and complex types by serializing them to BSON documents.
        /// </summary>
        /// <typeparam name="T">The type of the value to convert.</typeparam>
        /// <param name="value">The value to convert to <see cref="BsonValue"/>.</param>
        /// <returns>A <see cref="BsonValue"/> representation of the input value.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the input value cannot be converted to a <see cref="BsonValue"/>.</exception>
        private BsonValue ConvertToBsonValue<T>(T value)
        {
            if (value == null)
            {
                return BsonNull.Value;
            }

            // Direct conversions for primitive and commonly used types
            if (value is string s)
            {
                return new BsonString(s);
            }
            else if (value is int i)
            {
                return new BsonInt32(i);
            }
            else if (value is long l)
            {
                return new BsonInt64(l);
            }
            else if (value is double d)
            {
                return new BsonDouble(d);
            }
            else if (value is bool b)
            {
                return new BsonBoolean(b);
            }
            else if (value is DateTime dt)
            {
                return new BsonDateTime(dt);
            }
            else if (value is Guid g)
            {
                return new BsonBinaryData(g, GuidRepresentation.Standard);
            }
            else if (value is ObjectId oid)
            {
                return new BsonObjectId(oid);
            }
            else if (value is Dictionary<string, object> dict)
            {
                var bsonDocument = new BsonDocument();
                foreach (var kvp in dict)
                {
                    bsonDocument.Add(kvp.Key, ConvertToBsonValue(kvp.Value));
                }
                return bsonDocument;
            }
            else
            {
                // For complex types, serialize to BSON document
                try
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(value);
                    return MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(json);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Cannot convert type '{typeof(T)}' to BsonValue.", ex);
                }
            }
        }


        /// <summary>
        /// Represents the method that will handle the IdGeneratorCompleted event.
        /// </summary>
        /// <typeparam name="TObjectId">The type of the object identifier.</typeparam>
        /// <typeparam name="TUser">The type of the user associated with the object.</typeparam>
        /// <param name="sender">The source of the event, typically an instance of an object that implements the IObjectBase interface.</param>
        /// <param name="eventArgs">An instance of IdGeneratorCompletedEventArgs that contains the event data.</param>
        public delegate void IdGeneratorCompletedEventHandler(IObjectBase<TObjectId, TUser> sender, IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs);
        public event IdGeneratorCompletedEventHandler IdGeneratorCompleted;
        /// <summary>
        /// This method is called when the process of the IdGenerator is completed.
        /// It triggers the IdGeneratorCompleted event, passing along the event arguments.
        /// </summary>
        /// <param name="eventArgs">
        /// An instance of <see cref="IdGeneratorCompletedEventArgs{TObjectId, TUser}"/> containing 
        /// data related to the completion of the Id generation process.
        /// </param>
        public void OnProcessCompleted(IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs) => IdGeneratorCompleted?.Invoke(this, eventArgs);
        /// <summary>
        /// This method is called when the process is completed. 
        /// It currently throws a <see cref="NotImplementedException"/> indicating that 
        /// the method has not been implemented yet.
        /// </summary>
        /// <param name="eventArgs">An instance of <see cref="IdGeneratorCompletedEventArgs{TObjectId, string}"/> 
        /// containing information about the completed event.</param>
        /// <exception cref="NotImplementedException">Thrown when this method is called, as the 
        /// implementation is pending.</exception>
        public void OnProcessCompleted(IdGeneratorCompletedEventArgs<TObjectId, string> eventArgs) => throw new NotImplementedException();
        #endregion private methods
    }
}
