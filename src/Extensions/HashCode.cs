using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace UCode.Extensions
{
    public static class HashCode
    {
        private static bool WithHaveToHashCodeAttributeSubItens(object item)
        {
            var members = item.GetType().GetMembers().Where(w => w is PropertyInfo or FieldInfo);

            return OnlyToHashCodeAttributeSubItens(members).Any();
        }

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

        public static int GetHashCodeFromAttribute(this object item) => GetHashCodeFromAttributeWithEx(item, false);

    }

    public class ToHashCodeAttribute : Attribute
    {

    }
}
