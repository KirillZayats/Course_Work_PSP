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

        public void SortVector(int numberClient)
        {
            double[] newValues = new double[Size];

            int[] sizeStep = new int[numberClient];
            for (int i = 0; i < numberClient; i++)
            {
                sizeStep[i] = FindLengthStep(i, numberClient, Size);
            }

            int indexValues = 0;
            for (int i = 0; i < sizeStep.Length; i++)
            {
                newValues[i] = Values[indexValues];
                indexValues++;

                int addValue = 0;
                for (int j = 1; j < sizeStep[i]; j++)
                {
                    newValues[i + addValue + numberClient] = Values[indexValues];
                    indexValues++;
                    addValue += numberClient;
                }
            }
            Values = newValues;
        }

        static int FindLengthStep(int start, int step, int size)
        {
            int length = 0;
            for (int i = start; i < size; i += step)
            {
                length++;
            }
            return length;
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
