
using System.IO;
using UCode.Extensions.ConsoleApp;

namespace UCode.Extensions
{
    public static class ConsoleExtensions
    {
        public static DualOutput AddFile(this TextWriter textWriter, string filePath, bool overwrite = true)
        {
            var dualOutput = new DualOutput(textWriter, filePath, overwrite);

            System.Console.SetOut(dualOutput);

            return dualOutput;
        }

        public static DualOutput AddFile(this TextWriter textWriter, Stream stream)
        {
            var dualOutput = new DualOutput(textWriter, stream);

            System.Console.SetOut(dualOutput);

            return dualOutput;
        }

    }
}

