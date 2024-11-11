using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace UCode.Mongo
{
    public class BsonObject: BsonObject<string>, IObjectBase, IObjectBaseTenant
    {
        public BsonObject(BsonDocument bsonDocument) : base(bsonDocument)
        {
        }
    }

    public class BsonObject<TObjectId> : BsonObject<TObjectId, string>, IObjectBase<TObjectId>, IObjectBaseTenant
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {
        public BsonObject(BsonDocument bsonDocument) : base(bsonDocument)
        {
        }
    }


    public class BsonObject<TObjectId, TUser> : IObjectBase<TObjectId, TUser>, IObjectBaseTenant
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {
        private readonly BsonDocument _bsonDocument;

        public TObjectId Id
        {
            get => Get(t => t.Id);
            set => Set(t => t.Id, value);
        }

        public TUser CreatedBy
        {
            get => Get(t => t.CreatedBy);
            set => Set(t => t.CreatedBy, value);
        }

        public DateTime CreatedAt
        {
            get => Get(t => t.CreatedAt);
            set => Set(t => t.CreatedAt, value);
        }

        public TUser? UpdatedBy
        {
            get => Get(t => t.UpdatedBy);
            set => Set(t => t.UpdatedBy, value);
        }

        public DateTime? UpdatedAt
        {
            get => Get(t => t.UpdatedAt);
            set => Set(t => t.UpdatedAt, value);
        }

        public Dictionary<string, object?>? ExtraElements
        {
            get => Get(t => t.ExtraElements);
            set => Set(t => t.ExtraElements, value);
        }

        public bool Disabled
        {
            get => Get(t => t.Disabled);
            set => Set(t => t.Disabled, value);
        }
        public Guid Ref
        {
            get => Get(t => t.Ref);
            set => Set(t => t.Ref, value);
        }
        public Guid Tenant
        {
            get => Get(t => t.Tenant);
            set => Set(t => t.Tenant, value);
        }


        public BsonObject(BsonDocument bsonDocument)
        {
            _bsonDocument = bsonDocument;
        }





        #region implicit
        public static implicit operator BsonDocument(BsonObject<TObjectId, TUser> objectId) => objectId._bsonDocument;
        public static implicit operator BsonObject<TObjectId, TUser>(BsonDocument bsonDocument) => new(bsonDocument);
        #endregion implicit

        #region private methods
        private T ConvertBson<T>(BsonValue bsonValue)
        {
            if (bsonValue == null || bsonValue.IsBsonNull)
            {
                return default(T);
            }

            Type targetType = typeof(T);

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
        private Dictionary<string, object> ConvertToDictionary(BsonDocument bsonDocument)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var element in bsonDocument)
            {
                dictionary[element.Name] = ConvertBson<object>(element.Value); // Recursivo para lidar com tipos complexos aninhados
            }

            return dictionary;
        }

        private T Get<T>(Expression<Func<IObjectBase<TObjectId, TUser>, T>> expression) => Get<IObjectBase<TObjectId, TUser>, T>(expression);
        private void Set<T>(Expression<Func<IObjectBase<TObjectId, TUser>, T>> expression, T value) => Set<IObjectBase<TObjectId, TUser>, T>(expression, value);

        private T Get<T>(Expression<Func<IObjectBaseTenant, T>> expression) => Get<IObjectBaseTenant, T>(expression);
        private void Set<T>(Expression<Func<IObjectBaseTenant, T>> expression, T value) => Set<IObjectBaseTenant, T>(expression, value);



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


        public delegate void IdGeneratorCompletedEventHandler(IObjectBase<TObjectId, TUser> sender, IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs);
        public event IdGeneratorCompletedEventHandler IdGeneratorCompleted;
        public void OnProcessCompleted(IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs) => IdGeneratorCompleted?.Invoke(this, eventArgs);
        public void OnProcessCompleted(IdGeneratorCompletedEventArgs<TObjectId, string> eventArgs) => throw new NotImplementedException();
        #endregion private methods
    }
}
