using System;
using System.Collections.Generic;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace UCode.Mongo
{
    public class StructBsonSerializer<T> : IBsonSerializer<T>
    {
        public Type ValueType => typeof(T);

        public T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var obj = Activator.CreateInstance<T>();

            var bsonReader = context.Reader;

            bsonReader.ReadStartDocument();

            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName();

                var field = this.ValueType.GetField(name);
                if (field != null)
                {
                    var value = BsonSerializer.Deserialize(bsonReader, field.FieldType);
                    field.SetValue(obj, value);
                }

                var prop = this.ValueType.GetProperty(name);
                if (prop != null)
                {
                    var value = BsonSerializer.Deserialize(bsonReader, prop.PropertyType);
                    prop.SetValue(obj, value, null);
                }
            }

            bsonReader.ReadEndDocument();

            return obj;
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
        {
            var fields = this.ValueType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            var propsAll = this.ValueType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var props = new List<PropertyInfo>();
            foreach (var prop in propsAll)
            {
                if (prop.CanWrite)
                {
                    props.Add(prop);
                }
            }

            var bsonWriter = context.Writer;

            bsonWriter.WriteStartDocument();

            foreach (var field in fields)
            {
                bsonWriter.WriteName(field.Name);
                BsonSerializer.Serialize(bsonWriter, field.FieldType, field.GetValue(value));
            }
            foreach (var prop in props)
            {
                bsonWriter.WriteName(prop.Name);
                BsonSerializer.Serialize(bsonWriter, prop.PropertyType, prop.GetValue(value, null));
            }

            bsonWriter.WriteEndDocument();
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value) => this.Serialize(context, (T)value);

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) => this.Deserialize(context, args);
    }
}
