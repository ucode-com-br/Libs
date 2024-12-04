using System;
using System.Linq.Expressions;

namespace UCode.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="System.Collections.Generic.List{T}"/> class.
    /// </summary>
    /// <remarks>
    /// This class contains utility methods that enhance the functionality of lists.
    /// </remarks>
    public static class ListExtensions
    {
        /// <summary>
        /// Removes all elements from the specified list that match the criteria defined by the provided predicate.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="source">The list from which elements are to be removed.</param>
        /// <param name="remove">An expression that defines the conditions of the elements to remove.</param>
        /// <returns>
        /// The modified list containing only the elements that do not match the specified predicate.
        /// </returns>
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
