namespace UCode.Extensions
{
    /// <summary>
    /// Represents a pseudo-random number generator.
    /// </summary>
    /// <remarks>
    /// This class provides methods to generate random numbers in a variety of formats,
    /// including integers, floats, and doubles. It uses a seed value to initialize the 
    /// random number generation algorithm, which can be specified to produce predictable 
    /// outputs for testing or random generation without a seed, which results in different 
    /// sequences for each execution.
    /// </remarks>
    public class Random
    {
        private static readonly XorShiftRandom _xorShiftRandom;

        /// <summary>
        /// Initializes a new instance of the <see cref="Random"/> class.
        /// The constructor creates a new instance of the <see cref="XorShiftRandom"/> class,
        /// which is used to generate pseudo-random numbers.
        /// </summary>
        static Random() 
            => _xorShiftRandom = new XorShiftRandom();



        /// <summary>
        /// Generates a random 32-bit integer, optionally within a specified range.
        /// </summary>
        /// <param name="min">The minimum inclusive value of the random integer. If null, the minimum value defaults to <see cref="int.MinValue"/>.</param>
        /// <param name="max">The maximum exclusive value of the random integer. If null, the maximum value defaults to <see cref="int.MaxValue"/>.</param>
        /// <returns>
        /// Returns a randomly generated 32-bit integer. If both <paramref name="min"/> and <paramref name="max"/> are specified,
        /// the generated integer will be within the range [<paramref name="min"/>, <paramref name="max"/>).
        /// </returns>
        public static int GetInt32(int? min = null, int? max = null)
        {
            if (min != null || max != null)
            {
                return _xorShiftRandom.NextInt32(min ?? int.MinValue, max ?? int.MaxValue);
            }
            else
            {
                return _xorShiftRandom.NextInt32();
            }
        }

        /// <summary>
        /// Generates a random 64-bit integer within a specified range.
        /// If both minimum and maximum values are not provided, a random 64-bit integer 
        /// is generated without any constraints.
        /// </summary>
        /// <param name="min">The minimum value (inclusive) of the random number to be generated. 
        /// If null, the minimum value defaults to <c>long.MinValue</c>.</param>
        /// <param name="max">The maximum value (exclusive) of the random number to be generated. 
        /// If null, the maximum value defaults to <c>long.MaxValue</c>.</param>
        /// <returns>
        /// A randomly generated 64-bit integer within the specified range.
        /// If no range is specified, a random 64-bit integer is returned.
        /// </returns>
        public static long GetInt64(int? min = null, int? max = null)
        {
            if (min != null || max != null)
            {
                return _xorShiftRandom.NextInt64(min ?? long.MinValue, max ?? long.MaxValue);
            }
            else
            {
                return _xorShiftRandom.NextInt64();
            }
        }

        /// <summary>
        /// Generates a random unsigned 32-bit integer (uint).
        /// If the optional parameters <paramref name="min"/> and <paramref name="max"/> are provided,
        /// the random integer is generated within the specified range. If both are null, 
        /// a random uint is returned without any constraints.
        /// </summary>
        /// <param name="min">The minimum value of the range (inclusive). If null, defaults to uint.MinValue.</param>
        /// <param name="max">The maximum value of the range (inclusive). If null, defaults to uint.MaxValue.</param>
        /// <returns>A randomly generated unsigned 32-bit integer within the specified range.</returns>
        public static uint GetUInt32(uint? min = null, uint? max = null)
        {
            if (min != null || max != null)
            {
                return _xorShiftRandom.NextUInt32(min ?? uint.MinValue, max ?? uint.MaxValue);
            }
            else
            {
                return _xorShiftRandom.NextUInt32();
            }
        }

        /// <summary>
        /// Generates a random 64-bit unsigned integer (ulong) within the specified range.
        /// If no range is specified, it returns a random ulong with the full range.
        /// </summary>
        /// <param name="min">
        /// The minimum value of the range (inclusive). If null, the minimum value defaults to ulong.MinValue.
        /// </param>
        /// <param name="max">
        /// The maximum value of the range (inclusive). If null, the maximum value defaults to ulong.MaxValue.
        /// </param>
        /// <returns>
        /// A randomly generated 64-bit unsigned integer (ulong) within the specified range.
        /// </returns>
        public static ulong GetUInt64(ulong? min = null, ulong? max = null)
        {
            if (min != null || max != null)
            {
                return _xorShiftRandom.NextUInt64(min ?? ulong.MinValue, max ?? ulong.MaxValue);
            }
            else
            {
                return _xorShiftRandom.NextUInt64();
            }
        }

        /// <summary>
        /// Generates a byte array of the specified size using a pseudo-random number generator.
        /// </summary>
        /// <param name="size">The number of bytes to generate.</param>
        /// <returns>A byte array of the specified size filled with random bytes.</returns>
        /// <remarks>
        /// This method utilizes the <see cref="_xorShiftRandom"/> instance to generate the random bytes.
        /// </remarks>
        public static byte[] GetBytes(int size) => _xorShiftRandom.NextBytes(size);

        /// <summary>
        /// Generates a random decimal value. It can return a value within the specified range,
        /// defined by the optional minimum and maximum parameters. If no parameters are provided,
        /// a random decimal value is generated without bounds.
        /// </summary>
        /// <param name="min">
        /// The optional minimum boundary for the random decimal. If not specified, defaults to decimal.MinValue.
        /// </param>
        /// <param name="max">
        /// The optional maximum boundary for the random decimal. If not specified, defaults to decimal.MaxValue.
        /// </param>
        /// <returns>
        /// A randomly generated decimal value, which can either be within the specified range
        /// or from the full range of decimal values if no boundaries are provided.
        /// </returns>
        public static decimal GetDecimal(decimal? min = null, decimal? max = null)
        {
            if (min != null || max != null)
            {
                return _xorShiftRandom.NextDecimal(min ?? decimal.MinValue, max ?? decimal.MaxValue);
            }
            else
            {
                return _xorShiftRandom.NextDecimal();
            }
        }

        /// <summary>
        /// Generates a random double value within a specified range.
        /// If no minimum or maximum values are provided, a random double value 
        /// between 0.0 and 1.0 is returned.
        /// </summary>
        /// <param name="min">The minimum value of the range. If null, the minimum is set to double.MinValue.</param>
        /// <param name="max">The maximum value of the range. If null, the maximum is set to double.MaxValue.</param>
        /// <returns>A random double value within the specified range, or between 0.0 and 1.0 if no range is specified.</returns>
        public static double GetDouble(double? min = null, double? max = null)
        {
            if (min != null || max != null)
            {
                return _xorShiftRandom.NextDouble(min ?? double.MinValue, max ?? double.MaxValue);
            }
            else
            {
                return _xorShiftRandom.NextDouble();
            }
        }
    }
}
