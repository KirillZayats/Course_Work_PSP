using System;
using System.Collections.Generic;
using System.Text;

namespace MathLibrary
{
    public class LinearSystem
    {
        public Matrix initialAMatrix { get; set; }
        public Matrix aMatrix { get; set; }    // матрица A
        public Vector xVector { get; set; }    // вектор неизвестных x
        public Vector initialBVector { get; set; }
        public Vector bVector { get; set; }    // вектор b
        public Vector uVector { get; set; }    // вектор невязки U
        private double eps = 0.0001;           // порядок точности для сравнения вещественных чисел 
        private int size { get; set; }         // размерность задачи

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="aMatrix"></param>
        /// <param name="bVector"></param>
        public LinearSystem(Matrix aMatrix, Vector bVector)
        {
            if (aMatrix == null || bVector == null)
                throw new ArgumentNullException("Один из параметров равен null.");

            if (aMatrix.Coll * aMatrix.Row != bVector.Size * bVector.Size)
                throw new ArgumentException(@"Количество строк и столбцов в матрице A должно совпадать с количеством элементров в векторе B.");

            this.initialAMatrix = new Matrix(aMatrix.Values, aMatrix.Row, aMatrix.Coll);  // запоминаем исходную матрицу
            this.aMatrix = new Matrix(aMatrix.Values, aMatrix.Row, aMatrix.Coll); // с её копией будем производить вычисления
            this.initialBVector = new Vector(bVector.Values, bVector.Size);  // запоминаем исходный вектор
            this.bVector = new Vector(bVector.Values, bVector.Size);  // с его копией будем производить вычисления
            this.xVector = new Vector(bVector.Size);
            this.uVector = new Vector(bVector.Size);
            this.size = bVector.Size;
            this.eps = eps;

        }


        /// <summary>
        /// Решение СЛАУ по Гаусса
        /// </summary>
        public void GaussSolve()
        {
            int[] index = bVector.IndexList;
            GaussForwardStroke(index);
            GaussBackwardStroke(index);
            GaussDiscrepancy();
        }

         
        /// <summary>
        /// Прямой ход метода Гаусса
        /// </summary>
        /// <param name="index"></param>
        private void GaussForwardStroke(int[] index)
        {
            // перемещаемся по каждой строке сверху вниз
            for (int i = 0; i < size; ++i)
            {
                // 1) выбор главного элемента
                double r = FindR(i, index);

                // 2) преобразование текущей строки матрицы A
                for (int j = 0; j < size; ++j)
                    aMatrix.Values[i, j] /= r;

                // 3) преобразование i-го элемента вектора b
                bVector.Values[i] /= r;

                // 4) Вычитание текущей строки из всех нижерасположенных строк
                for (int k = i + 1; k < size; ++k)
                {
                    double p = aMatrix.Values[k, index[i]];
                    for (int j = i; j < size; ++j)
                        aMatrix.Values[k, index[j]] -= aMatrix.Values[i, index[j]] * p;
                    bVector.Values[k] -= bVector.Values[i] * p;
                    aMatrix.Values[k, index[i]] = 0.0;
                }
            }
        }

        /// <summary>
        /// Обратный ход метода Гаусса
        /// </summary>
        /// <param name="index"></param>
        private void GaussBackwardStroke(int[] index)
        {
            // перемещаемся по каждой строке снизу вверх
            for (int i = size - 1; i >= 0; --i)
            {
                // 1) задаётся начальное значение элемента x
                double x_i = bVector.Values[i];

                // 2) корректировка этого значения
                for (int j = i + 1; j < size; ++j)
                    x_i -= xVector.Values[index[j]] * aMatrix.Values[i, index[j]];
                xVector.Values[index[i]] = x_i;
            }
        }


        /// <summary>
        /// поиск главного элемента в матрице
        /// </summary>
        /// <param name="row"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private double FindR(int row, int[] index)
        {
            int max_index = row;
            double max = aMatrix.Values[row, index[max_index]];
            double max_abs = Math.Abs(max);
            //if(row < size - 1)
            for (int cur_index = row + 1; cur_index < size; ++cur_index)
            {
                double cur = aMatrix.Values[row, index[cur_index]];
                double cur_abs = Math.Abs(cur);
                if (cur_abs > max_abs)
                {
                    max_index = cur_index;
                    max = cur;
                    max_abs = cur_abs;
                }
            }

            if (max_abs < eps)
            {
                if (Math.Abs(bVector.Values[row]) > eps)
                    throw new GaussSolutionNotFound("Система уравнений несовместна.");
                else
                    throw new GaussSolutionNotFound("Система уравнений имеет множество решений.");
            }

            // меняем местами индексы столбцов
            int temp = index[row];
            index[row] = index[max_index];
            index[max_index] = temp;

            return max;
        }

        /// <summary>
        /// Вычисление невязки решения
        /// U = b - x * A
        /// x - решение уравнения, полученное методом Гаусса
        /// </summary>
        private void GaussDiscrepancy()
        {
            for (int i = 0; i < size; ++i)
            {
                double actual_b_i = 0.0;   // результат перемножения i-строки 
                // исходной матрицы на вектор x
                for (int j = 0; j < size; ++j)
                    actual_b_i += initialAMatrix.Values[i, j] * xVector.Values[j];
                // i-й элемент вектора невязки
                uVector.Values[i] = initialBVector.Values[i] - actual_b_i;
            }
        }
    }
}
