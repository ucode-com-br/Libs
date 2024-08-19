using System;
using System.Linq.Expressions;

namespace UCode.Extensions
{
    /// <summary>
    /// List Extension
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Remove all based on expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="remove"></param>
        /// <returns></returns>
        public static System.Collections.Generic.List<T> RemoveAll<T>(this System.Collections.Generic.List<T> source, Expression<Func<T, bool>> remove)
        {
            var compile = remove.Compile();
            for (var i = 0; i < source.Count; i++)
            {
                if (compile(source[i]))
                {
                    source.RemoveAt(i--);
                }
            }

            return source;
        }


    }
}
