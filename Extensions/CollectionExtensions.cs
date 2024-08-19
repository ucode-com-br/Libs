using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace UCode.Extensions
{

    public static class CollectionExtensions
    {
        public static Collection<T> Add<T>(this Collection<T> source, T item)
        {
            source.Add(item);

            return source;
        }

        public static Collection<T> Add<T>(this Collection<T> source, params T[] itens)
        {
            foreach (var item in itens)
            {
                source.Add(item);
            }

            return source;
        }

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
