using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UCode.Extensions
{
    public static class Reflection
    {
        public static object CallMethod(this object obj, string name, Type[] generics, Type result, params object[] arguments)
        {
            throw new Exception();
        }

        public static object CallMethod(this object obj, Expression<Func<object?>> name, Type[] generics, Type result, params object[] arguments)
        {
            throw new Exception();
        }



    }
}
