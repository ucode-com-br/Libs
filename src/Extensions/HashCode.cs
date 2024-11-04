using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace UCode.Extensions
{
    /// <summary>
    /// Provides static methods for generating hash codes.
    /// </summary>
    /// <remarks>
    /// The <see cref="HashCode"/> class can be used to combine multiple hash codes
    /// into a single hash code, which can be useful in implementing the hash function
    /// for composite keys or data structures. It employs various algorithms suitable
    /// for generating high-quality hash codes from one or more inputs.
    /// </remarks>
    public static class HashCode
    {
        /// <summary>
        /// Determines whether the specified object has any members (properties or fields) 
        /// that are decorated with the `OnlyToHashCodeAttribute`.
        /// </summary>
        /// <param name="item">The object to inspect for decorated members.</param>
        /// <returns>
        /// Returns <c>true</c> if at least one member is decorated with the `OnlyToHashCodeAttribute`; 
        /// otherwise, <c>false</c>.
        /// </returns>
        private static bool WithHaveToHashCodeAttributeSubItens(object item)
        {
            var members = item.GetType().GetMembers().Where(w => w is PropertyInfo or FieldInfo);

            return OnlyToHashCodeAttributeSubItens(members).Any();
        }

        /// <summary>
        /// Filters a collection of member information, returning only those that have the
        /// <see cref="ToHashCodeAttribute"/> applied to them.
        /// </summary>
        /// <param name="members">The collection of <see cref="MemberInfo"/> objects to filter.</param>
        /// <returns>
        /// An <see cref="IEnumerable{MemberInfo}"/> containing only the members with a 
        /// <see cref="ToHashCodeAttribute"/>. If no members meet the criteria, an empty collection is returned.
        /// </returns>
        private static IEnumerable<MemberInfo> OnlyToHashCodeAttributeSubItens(IEnumerable<MemberInfo> members)
        {
            //return members.Where(a => a.GetCustomAttributes(typeof(ToHashCodeAttribute), true).Length > 0);

            //return item.GetType().GetProperties().Any(a => a.GetCustomAttributes(typeof(ToHashCodeAttribute), true).Length > 0);

            foreach (var member in members)
            {
                if (member.GetCustomAttributes(typeof(ToHashCodeAttribute), true).Length > 0)
                {
                    yield return member;
                }
            }
        }

        /// <summary>
        /// Retrieves a memory stream containing the byte representation of the properties and fields
        /// of the specified object item. It evaluates each member of the object, calculating hashes
        /// where necessary based on defined attributes and appending the byte values to the memory stream.
        /// </summary>
        /// <param name="item">The object whose properties and fields will be evaluated.</param>
        /// <returns>A <see cref="MemoryStream"/> containing the byte representation of the object's members.</returns>
        private static MemoryStream GetValues(object item)
        {
            var ms = new MemoryStream();

            var members = item.GetType().GetMembers().Where(w => w is PropertyInfo or FieldInfo);

            foreach (var member in OnlyToHashCodeAttributeSubItens(members))
            {
                PropertyInfo propertyInfo;
                FieldInfo fieldInfo;

                if ((propertyInfo = member as PropertyInfo) != null)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Trace.WriteLine($"Calculate MD5 of property \"{propertyInfo.Name}\".");
                    }

                    var v = propertyInfo.GetValue(item);

                    if (v == null)
                    {
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            System.Diagnostics.Trace.WriteLine($"Property \"{propertyInfo.Name}\" is null.");
                        }

                        continue;
                    }

                    byte[] bytes;

                    if (WithHaveToHashCodeAttributeSubItens(v))
                    {
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            System.Diagnostics.Trace.WriteLine($"Property \"{propertyInfo.Name}\" is class.");
                        }

                        var code = GetHashCodeFromAttributeWithEx(v, true);

                        bytes = code.ToBytes();
                    }
                    else
                    {
                        bytes = v.ToBytes();

                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            System.Diagnostics.Trace.WriteLine($"Property \"{propertyInfo.Name}\" have value ({bytes.Length}).");
                        }
                    }

                    ms.Write(bytes);
                }
                else if ((fieldInfo = member as FieldInfo) != null)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Trace.WriteLine($"Calculate MD5 of property \"{fieldInfo.Name}\".");
                    }

                    var v = fieldInfo.GetValue(item);

                    if (v == null)
                    {
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            System.Diagnostics.Trace.WriteLine($"Property \"{fieldInfo.Name}\" is null.");
                        }

                        continue;
                    }

                    byte[] bytes;

                    if (fieldInfo.DeclaringType.IsClass)
                    {
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            System.Diagnostics.Trace.WriteLine($"Property \"{fieldInfo.Name}\" is class.");
                        }

                        var code = GetHashCodeFromAttributeWithEx(v, true);

                        bytes = code.ToBytes();
                    }
                    else
                    {
                        bytes = v.ToBytes();

                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            System.Diagnostics.Trace.WriteLine($"Property \"{fieldInfo.Name}\" have value ({bytes.Length}).");
                        }
                    }

                    ms.Write(bytes);
                }
            }


            ms.Seek(0, SeekOrigin.Begin);

            return ms;
        }

        /// <summary>
        /// Generates a hash code from the values of the specified attribute of an object.
        /// </summary>
        /// <param name="item">The object from which to extract attribute values for hash code generation.</param>
        /// <param name="throwException">Indicates whether to throw an exception if an error occurs during hash code generation.</param>
        /// <returns>
        /// An integer representing the hash code generated from the attribute values of the specified object.
        /// </returns>
        private static int GetHashCodeFromAttributeWithEx(object item, bool throwException)
        {
            var result = 0;

            using (var md5Hasher = MD5.Create())
            using (var ms = GetValues(item))
            {
                try
                {
                    var md5Hash = md5Hasher.ComputeHash(ms);

                    result = BitConverter.ToInt32(md5Hash);
                }
                catch (Exception)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Trace.WriteLine($"Exception \"GetHashCodeFromAttributeWithEx\".");
                    }

                    if (throwException)
                    {
                        throw;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Computes a hash code for the specified object using a specific attribute.
        /// </summary>
        /// <param name="item">The object for which to compute the hash code.</param>
        /// <returns>An integer hash code derived from the specified object's attribute.</returns>
        /// <remarks>
        /// This method extends the functionality of objects by providing a way to
        /// compute a hash code that can be useful in scenarios like storing the
        /// object in a hash-based collection. It 
        public static int GetHashCodeFromAttribute(this object item) => GetHashCodeFromAttributeWithEx(item, false);

    }

    /// <summary>
    /// Represents an attribute that can be applied to a class or a structure to indicate
    /// that hash code generation is to be based on the values of the properties of that
    /// class or structure.
    /// </summary>
    /// <remarks>
    /// This attribute is typically used to facilitate automatic hash code generation,
    /// which can be useful in scenarios such as collections or to ensure proper
    /// behavior in equality checks.
    /// </remarks>
    /// <example>
    /// [ToHashCode]
    /// 
    public class ToHashCodeAttribute : Attribute
    {

    }
}
