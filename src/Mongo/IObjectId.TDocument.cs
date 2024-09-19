using System;
using MongoDB.Bson.Serialization.Attributes;

namespace UCode.Mongo
{
    /// <summary>
    /// Represents an object with an ID.
    /// </summary>
    /// <typeparam name="TObjectId">The type of the ID.</typeparam>
    public interface IObjectId<TObjectId>
        //where TDocument : ICloneable, IComparable, IConvertible, IEquatable<TDocument?>
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

        /// <summary>
        /// Determines if a type is a structure.
        /// </summary>
        /// <param name="localType">The type to check.</param>
        /// <returns>True if the type is a structure, false otherwise.</returns>
        public static bool IsStructure(Type localType)
        {
            var result = false;

            if (localType.IsValueType)
            {
                // Is a value type
                if (!localType.IsPrimitive)
                {
                    /* Is not primitive. Remember that primitives are:
                    Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32,
                    Int64, UInt64, IntPtr, UIntPtr, Char, Double, Single.
                    This way, could still be Decimal, Date or Enum. */
                    if (localType != typeof(decimal))
                    {
                        //Is not Decimal
                        if (localType != typeof(DateTime))
                        {
                            //Is not Date
                            if (!localType.IsEnum)
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

        /// <summary>
        /// Gets or sets the ID of the object.
        /// </summary>
        [BsonId]
        public TObjectId Id
        {
            get; set;
        }

    }
}
