using System;
using System.Linq;

namespace TransformationLibrary
{
    public static class Converter
    {
        public static byte[] GetBytes(double[] values)
        {
            return values.SelectMany(value => BitConverter.GetBytes(value)).ToArray();
        }

        public static byte[] GetBytes(int[] values)
        {
            return values.SelectMany(value => BitConverter.GetBytes(value)).ToArray();
        }

        public static int[] GetInt(byte[] bytes)
        {
            return Enumerable.Range(0, bytes.Length / sizeof(int))
                .Select(offset => BitConverter.ToInt32(bytes, offset * sizeof(int))).ToArray();
        }
        public static double[] GetDoubles(byte[] bytes)
        {
            return Enumerable.Range(0, bytes.Length / sizeof(double))
                .Select(offset => BitConverter.ToDouble(bytes, offset * sizeof(double))).ToArray();
        }
    }
}
