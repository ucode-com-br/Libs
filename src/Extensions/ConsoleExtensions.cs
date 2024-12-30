using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using UCode.Extensions.ConsoleApp;

//using SystemConsole = System.Console;
//namespace UCode.Extensions
//{
//    public static class ConsoleExtensions
//    {
//        public static System.Console WriteOut(this System.Console console, string filePath, bool overwrite = true)
//        {
//            var dualOutput = new DualOutput(filePath, overwrite);

//            System.Console.SetOut(dualOutput);

//            return dualOutput;
//        }
//    }
//}

namespace System.Console
{
    public static partial class Out
    {
        public static UCode.Extensions.ConsoleApp.DualOutput File(string filePath, bool overwrite = true)
        {
            var dualOutput = new UCode.Extensions.ConsoleApp.DualOutput(filePath, overwrite);

            //System.Console.SetOut(dualOutput);
            SetConsoleOut(dualOutput);

            return dualOutput;
        }

        /// <summary>
        /// Sets the console output using reflection to call <see cref="System.Console.SetOut"/>.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> to redirect console output to.</param>
        private static void SetConsoleOut(TextWriter textWriter)
        {
            Assembly assembly = Assembly.Load("System.Console");

            Type? type = assembly.GetType("System.Console");

            // Get the SetOut method from System.Console
            var setOutMethod = type.GetMethod("SetOut", BindingFlags.Static | BindingFlags.Public);

            if (setOutMethod == null)
            {
                throw new InvalidOperationException("Unable to find System.Console.SetOut method.");
            }

            // Invoke SetOut with the specified TextWriter
            setOutMethod.Invoke(null, new object[] { textWriter });
        }
    }
}
