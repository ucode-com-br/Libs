namespace UCode.Extensions
{
    public class Random
    {
        private static readonly XorShiftRandom _xorShiftRandom;

        static Random()
            => _xorShiftRandom = new XorShiftRandom();


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

        public static byte[] GetBytes(int size) => _xorShiftRandom.NextBytes(size);

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
