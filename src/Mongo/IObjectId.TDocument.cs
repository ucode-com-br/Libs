using System;
using MongoDB.Bson.Serialization.Attributes;

namespace UCode.Mongo
{
    public interface IObjectId<TObjectId>
        //where TDocument : ICloneable, IComparable, IConvertible, IEquatable<TDocument?>
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {
        static IObjectId()
        {
            //Type type = typeof(TObjectId);

            //foreach (PropertyInfo property in type.GetProperties())
            //{
            //    var declaringType = property.DeclaringType;

            //    if ((declaringType != null && declaringType.IsArray) || (property.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(property.PropertyType)))
            //    {

            //    }
            //    else
            //    {
            //        if (IsStructure(type))
            //            BsonSerializer.RegisterSerializer(typeof(TObjectId), new StructBsonSerializer<TObjectId>());
            //    }
            //}


        }

        public static bool IsStructure(Type LocalType)
        {
            var result = false;

            if (LocalType.IsValueType)
            {
                //Is Value Type
                if (!LocalType.IsPrimitive)
                {
                    /* Is not primitive. Remember that primitives are:
                    Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32,
                    Int64, UInt64, IntPtr, UIntPtr, Char, Double, Single.
                    This way, could still be Decimal, Date or Enum. */
                    if (LocalType != typeof(decimal))
                    {
                        //Is not Decimal
                        if (LocalType != typeof(DateTime))
                        {
                            //Is not Date
                            if (!LocalType.IsEnum)
                            {
                                //Is not Enum. Consequently it is a structure.
                                result = true;
                            }
                        }
                    }
                }
            }

            return result;
        }

        [BsonId]
        public TObjectId Id
        {
            get; set;
        }
    }
}
