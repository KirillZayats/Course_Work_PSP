using MathLibrary;
using System;
using System.IO;

namespace StorageLibrary
{
    public static class FileStream
    {
        public static Matrix ReadMatrix(string nameFile)
        {
            string[] data = File.ReadAllLines(nameFile);

            int row = int.Parse(data[0]);
            int coll = int.Parse(data[1]);

            string[] tmp;
            double[,] matrixValues = new double[row, coll];
            for (int i = 2; i < row + 2; i++)
            {
                tmp = data[i].Split(' ');
                for (int j = 0; j < coll; j++)
                {
                    matrixValues[i - 2, j] = double.Parse(tmp[j]);
                }
            }
            return new Matrix(matrixValues, row, coll);
        }

        public static Vector ReadVector(string nameFile)
        {
            string[] data = File.ReadAllLines(nameFile);

            int length = int.Parse(data[0]);

            string[] tmp = data[1].Split(' ');

            double[] vectorValues = new double[length];


            for (int j = 0; j < length; j++)
            {
                vectorValues[j] = double.Parse(tmp[j]);
            }

            return new Vector(vectorValues, length);
        }

        public static void WriteMatrix(string nameFile, Matrix matrix)
        {
            using (TextWriter tw = new StreamWriter(nameFile))
            {
                tw.Write( matrix.Row);
                tw.WriteLine();
                tw.Write(matrix.Coll);
                tw.WriteLine();
                for (int j = 0; j < matrix.Row; j++)
                {
                    for (int i = 0; i < matrix.Coll; i++)
                    {
                        tw.Write(matrix.Values[i, j] + " ");
                    }
                    tw.WriteLine();
                }
            }
        }

        public static void WriteVector(string nameFile, Vector vector)
        {
            using (TextWriter tw = new StreamWriter(nameFile))
            {
                tw.Write(vector.Size);
                tw.WriteLine();
                for (int i = 0; i < vector.Size; i++)
                {
                    tw.Write(vector.Values[i] + " ");
                }
            }
        }
    }
}
