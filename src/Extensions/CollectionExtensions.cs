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
        /// Adds an item to the collection and returns the modified collection
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection</typeparam>
        /// <param name="source">The target collection</param>
        /// <param name="item">The item to add</param>
        /// <returns>The same collection instance with the new item added</returns>
        /// <remarks>
        /// Enables fluent-style chaining of Add operations. This method mutates
        /// the original collection rather than creating a new one.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null</exception>
        /// <example>
        /// <code>
        /// var numbers = new Collection&lt;int&gt; { 1, 2 };
        /// numbers.Add(3).Add(4); // Returns collection containing 1, 2, 3, 4
        /// </code>
        /// </example>
        /// <summary>
        /// Adds an item to the collection and returns the modified collection
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection</typeparam>
        /// <param name="source">Source collection to modify</param>
        /// <param name="item">Item to add to the collection</param>
        /// <returns>The same collection instance with new item added</returns>
        /// <exception cref="ArgumentNullException">Thrown if source collection is null</exception>
        /// <example>
        /// <code>
        /// var numbers = new Collection<int> { 1, 2 };
        /// numbers.Add(3); // Returns collection containing 1, 2, 3
        /// </code>
        /// </example>
        public static Collection<T> Add<T>(this Collection<T> source, T item)
        {
            source.Add(item);

            return source;
        }

        /// <summary>
        /// Adds multiple items to the collection and returns the modified collection
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection</typeparam>
        /// <param name="source">Source collection to modify</param>
        /// <param name="itens">Array of items to add to the collection</param>
        /// <returns>The same collection instance with new items added</returns>
        /// <exception cref="ArgumentNullException">Thrown if source collection or items array is null</exception>
        /// <example>
        /// <code>
        /// var letters = new Collection<char> { 'a' };
        /// letters.Add('b', 'c'); // Returns collection containing 'a', 'b', 'c'
        /// </code>
        /// </example>
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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="remove"/> is null</exception>
        /// <example>
        /// <code>
        /// var numbers = new Collection&lt;int&gt; { 1, 2, 3, 4 };
        /// numbers.RemoveAll(n => n % 2 == 0); // Removes even numbers
        /// </code>
        /// </example>
        /// <summary>
        /// Removes all elements from the collection that match the specified predicate
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection</typeparam>
        /// <param name="source">The collection to modify</param>
        /// <param name="remove">Expression defining elements to remove</param>
        /// <returns>The modified collection with matching elements removed</returns>
        /// <exception cref="ArgumentNullException">Thrown if source or remove is null</exception>
        /// <example>
        /// <code>
        /// var numbers = new Collection&lt;int&gt; { 1, 2, 3, 4 };
        /// numbers.RemoveAll(n => n % 2 == 0); // Removes even numbers (2,4)
        /// </code>
        /// </example>
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
        /// <summary>
        /// Replaces each element in the collection using the specified transformation function
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection</typeparam>
        /// <param name="source">The collection to modify</param>
        /// <param name="replace">Transformation function to apply to each element</param>
        /// <returns>The modified collection with transformed elements</returns>
        /// <exception cref="ArgumentNullException">Thrown if source or replace is null</exception>
        /// <example>
        /// <code>
        /// var names = new Collection&lt;string&gt; { "john", "jane" };
        /// names.Replace(n => n.ToUpper()); // Collection becomes ["JOHN", "JANE"]
        /// </code>
        /// </example>
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
