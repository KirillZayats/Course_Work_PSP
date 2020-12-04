using System;
using System.Collections.Generic;
using System.Text;

namespace MatrixLibrary
{
    public class Vector
    {
        public double[] Values { get; set; }
        public int[] IndexList { get; set; }

        public int Size { get; set; }

        public Vector(int size)
        {
            this.Size = size;
            this.Values = new double[size];
            InitIndex();
        }

        public Vector(double[] vector, int size)
        {
            this.Size = size;
           // this.Values = vector;
            this.Values = new double[size];
            for (int i = 0; i < size; i++)
            {
                this.Values[i] = vector[i];
            }
            InitIndex();
            //GenerateVector();
        }

        /// <summary>
        /// инициализация массива индексов
        /// </summary>
        private void InitIndex()
        {
            IndexList = new int[Size];
            for (int i = 0; i < IndexList.Length; ++i)
                IndexList[i] = i;
        }

        /// <summary>
        /// генерация элементов вектора
        /// </summary>
        public void GenerateVector()
        {
            Random random = new Random();
            double colculateValue = 0;
            for (int j = 0; j < Size; j++)
            {
                colculateValue = Math.Round(random.NextDouble() * 100, 2);
                Values[j] = Math.Round(random.NextDouble() * colculateValue, 2);
            }
        }

        public void Output(string nameMatrix)
        {
            Console.WriteLine("\nВектор {0}: ", nameMatrix);
            for (int j = 0; j < Size; j++)
            {
                Console.Write("{0} ", Math.Round(Values[j], 2));
            }
            Console.WriteLine();
        }
    }
}
