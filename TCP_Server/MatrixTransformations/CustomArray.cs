using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathLibrary
{
    public static class CustomArray<T>
    {
        /// <summary>
        /// Конвертация матрицы в вектор с определённого столбца
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="columnNumber"></param>
        /// <returns></returns>
        public static T[] GetColumn(T[,] matrix, int columnNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(0))
                    .Select(x => matrix[x, columnNumber])
                    .ToArray();
        }

        /// <summary>
        /// Конвертация матрицы в вектор с определённой строки
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="rowNumber"></param>
        /// <returns></returns>
        public static T[] GetRow(T[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }
    }
}
