using System;
using System.Linq;

namespace TransformationLibrary
{
    public static class Converter
    {
        /// <summary>
        /// Конвертация массива чисел двойной точности в массив байтов 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static byte[] GetBytes(double[] values)
        {
            return values.SelectMany(value => BitConverter.GetBytes(value)).ToArray();
        }

        /// <summary>
        /// Конвертация массива целых чисел в массив байтов 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static byte[] GetBytes(int[] values)
        {
            return values.SelectMany(value => BitConverter.GetBytes(value)).ToArray();
        }

        /// <summary>
        /// Конвертация массива байтов в массив целых чисел 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static int[] GetInt(byte[] bytes)
        {
            return Enumerable.Range(0, bytes.Length / sizeof(int))
                .Select(offset => BitConverter.ToInt32(bytes, offset * sizeof(int))).ToArray();
        }

        /// <summary>
        /// Конвертация массива байтов в массив чисел двойной точности
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double[] GetDoubles(byte[] bytes)
        {
            return Enumerable.Range(0, bytes.Length / sizeof(double))
                .Select(offset => BitConverter.ToDouble(bytes, offset * sizeof(double))).ToArray();
        }
    }
}
