using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace UCode.Extensions
{

    /// <summary>
    /// Provides extension methods for collections.
    /// </summary>
    /// <remarks>
    /// This class contains static methods that extend the functionality 
    /// of collection types in .NET, such as arrays and lists.
    /// </remarks>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Adds an item to the specified Collection.
        /// </summary>
        /// <typeparam name="T">The type of the items in the Collection.</typeparam>
        /// <param name="source">The Collection to which the item will be added.</param>
        /// <param name="item">The item to add to the Collection.</param>
        /// <returns>
        /// The original Collection with the new item added.
        /// </returns>
        public static Collection<T> Add<T>(this Collection<T> source, T item)
        {
            source.Add(item);

            return source;
        }

        /// <summary>
        /// Adds one or more items to the specified collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The collection to which items will be added.</param>
        /// <param name="itens">The items to add to the collection.</param>
        /// <returns>
        /// The updated collection with the new items added.
        /// </returns>
        public static Collection<T> Add<T>(this Collection<T> source, params T[] itens)
        {
            foreach (var item in itens)
            {
                source.Add(item);
            }

            return source;
        }

        /// <summary>
        /// Removes all elements from the specified <see cref="Collection{T}"/> that satisfy a specified condition.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The collection from which elements will be removed.</param>
        /// <param name="remove">A LINQ expression that defines the conditions of the elements to remove.</param>
        /// <returns>
        /// The <see cref="Collection{T}"/> from which the elements have been removed.
        /// </returns>
        public static Collection<T> RemoveAll<T>(this Collection<T> source, Expression<Func<T, bool>> remove)
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




        /// <summary>
        /// Replaces each element in the specified <see cref="Collection{T}"/> with the result of applying 
        /// a specified function to each element.
        /// </summary>
        /// <typeparam name="T">
        /// The type of elements in the collection.
        /// </typeparam>
        /// <param name="source">
        /// The <see cref="Collection{T}"/> to modify.
        /// </param>
        /// <param name="replace">
        /// An expression that defines the operation to apply to each element in the collection.
        /// The expression should take an element of type <typeparamref name="T"/> 
        /// and return a modified element of the same type.
        /// </param>
        /// <returns>
        /// The modified <see cref="Collection{T}"/> with each element replaced according to the specified function.
        /// </returns>
        public static Collection<T> Replace<T>(this Collection<T> source, Expression<Func<T, T>> replace)
        {
            var compile = replace.Compile();

            for (var i = 0; i < source.Count; i++)
            {
                source[i] = compile(source[i]);
            }

            return source;
        }
    }
}
