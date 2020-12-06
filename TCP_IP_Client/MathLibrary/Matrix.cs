using System;

namespace MatrixLibrary
{
    public class Matrix
    {
        public double[,] Values { get; set; }
        public int Row { get; set; }
        public int Coll { get; set; }

        private const double epx = 0.000000001;


        /// <summary>
        /// Конструктор матрицы
        /// </summary>
        /// <param name="values"></param>
        /// <param name="row"></param>
        /// <param name="coll"></param>
        public Matrix(double[,] values, int row, int coll)
        {
            this.Row = row;
            this.Coll = coll;
            this.Values = new double[row, coll];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < coll; j++)
                {
                    this.Values[i, j] = values[i, j];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Matrix()
        {
            Input();
        }

        public Matrix(int row, int coll)
        {
            this.Row = row;
            this.Coll = coll;
            Values = new double[Row, Coll];
        }

        public Matrix(double[] array, int row, int coll)
        {
            this.Row = row;
            this.Coll = coll;
            Values = new double[row, coll];
            for (int i = 0; i < row; i++)
                for (int j = 0; j < coll; j++)
                {
                    Values[i, j] = array[i + j * row];
                }
        }


        /// <summary>
        /// Генерация матрицы
        /// </summary>
        private void GenerateMatrix()
        {
            Random random = new Random();
            double colculateValue = 0;
            for (int i = 0; i < Row; i++)
                for (int j = 0; j < Coll; j++)
                {
                    colculateValue = Math.Round(random.NextDouble() * 100, 2);
                    Values[i, j] = Math.Round(random.NextDouble() * colculateValue, 2);
                }
        }

        
        public void CalculateNewMatrix(double koef, int row, int startRow, int coll, int startColl)
        {
            for (int i = startColl; i < coll; i++)
            {
                Values[row, i] = koef * Values[startRow, i] + Values[row, i];
                if (Values[row, i] < epx && Values[row, i] > -epx)
                    Values[row, i] = 0;
            }
        }

        public double[] ConvertMatrixToArray()
        {
            double[] vector = new double[Row * Coll];
            int indexVector = 0;

            for (int i = 0; i < Coll; i++)
            {
                for (int j = 0; j < Row; j++)
                {
                    vector[indexVector] = Values[j, i];
                    indexVector++;
                }
            }
            return vector;
        }

        /// <summary>
        /// Метод ввода матрицы
        /// </summary>
        public void Input()
        {
            bool isDimension = false;
            while (isDimension == false)
            {
                Console.WriteLine("Введите количество строк в матрице:");
                Row = int.Parse(Console.ReadLine());
                Console.WriteLine("Введите количество столбцов в матрице");
                Coll = int.Parse(Console.ReadLine());
                if (Row > 1 && Coll > 1)
                {
                    Values = new double[Row, Coll];
                    Console.WriteLine("Введите элементы матрицы");
                    for (int i = 0; i < Row; i++)
                    {
                        for (int j = 0; j < Coll; j++)
                        {
                            Console.Write("Элемент матрицы [{0}, {1}] = ", i + 1, j + 1);
                            Values[i, j] = double.Parse(Console.ReadLine());
                        }
                    }
                    isDimension = true;
                }

                else
                {
                    Console.WriteLine("Размерность должна быть больше единицы !!!");
                    isDimension = false;
                }
            }
        }

        /// <summary>
        /// Метод вывода матрицы
        /// </summary>
        /// <param name="nameMatrix">Имя матрицы</param>
        public async void Output(string nameMatrix)
        {
            Console.WriteLine("\nМатрица {0}: ", nameMatrix);
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Coll; j++)
                {
                    Console.Write("{0} ", Math.Round(Values[i, j], 2));
                }
                Console.WriteLine();
            }
        }
    }
}
