using NetworkLibrary;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TransformationLibrary;

namespace MathLibrary
{
    public class DistributedSolution
    {
        public List<Client> Clients { get; set; }
        public Matrix Matrix { get; set; }
        public Vector Vector { get; set; }

        public Vector VectorResult { get; set; }
        private byte[] buffer;

        public DistributedSolution(List<Client> client)
        {
            this.Clients = client;
        }

        public void initParameters(Matrix matrix, Vector vector)
        {
            this.Matrix = new Matrix(matrix.Values, matrix.Row, matrix.Coll);
            this.Vector = new Vector(vector.Values, vector.Size);
        }

        public void Run()
        {
            int sizeCollLastClient = 0;
            List<double[]> sendVectors = new List<double[]>();
            for (int i = 0; i < Clients.Count; i++)
                sendVectors.Add(CreateSendVector(i, Clients.Count));
            List<int[]> parameterVectors = CreateVectorParameners(sendVectors, Matrix.Row, sizeCollLastClient);
            SendParameters(parameterVectors);
            SendMatrix(sendVectors);

            //покучение параметра на который неодходимо делить столбцы

            int indexClient = 0;
            int indexRowVector = 1;
            int indexStartRowVector = 0;

            buffer = new byte[sizeof(int)];
            Clients[indexClient].Receive(buffer);
            int size = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[sizeof(double) * size];

            int countClients = Clients.Count;
            if (sizeCollLastClient == 1)
                countClients--;

            while (indexClient < countClients)
            {
                Clients[indexClient].Receive(buffer);
                double[] matrixAsParameters = Converter.GetDoubles(buffer);
                /*if ((indexClient + 1) == (int)matrixAsParameters[0])
                {*/
                 foreach (var item in matrixAsParameters)
                 {
                     Console.WriteLine(item);
                 }
                if ((int)matrixAsParameters[0] == Clients.Count - 1)
                    matrixAsParameters[2] = 1;
                Console.WriteLine("t\n");
                for (int i = indexClient + 1; i < Clients.Count; i++)
                {
                    Clients[i].Send(Converter.GetBytes(matrixAsParameters));
                }
                Vector.CalculateNewVector(matrixAsParameters[3], indexRowVector, indexStartRowVector);
                if (indexRowVector == Vector.Size - 1)
                {
                    indexStartRowVector++;
                    indexRowVector = indexStartRowVector + 1;
                }
                else
                {
                    indexRowVector++;
                }

                if (Convert.ToBoolean(matrixAsParameters[1]))
                    indexClient++;
                /*   }
                    else
                        indexClient++;*/

            }

            // asyncVector.Output("B");

            Matrix newMatrix = new Matrix(RecieveMatrix(), Matrix.Row, Matrix.Coll);
            // newMatrix.Output("testNewMatrix");
            /*newMatrix.Output("EERGTRT");
            Vector.Output("a tyt");*/
            VectorResult = GaussBackword(newMatrix);
            if (Clients.Count < VectorResult.Size)
                VectorResult.SortVector(Clients.Count);
        }

        private Vector GaussBackword(Matrix matrix)
        {
            Vector vectorResult = new Vector(Vector.Size);
            int indexCountVector = Vector.Size - 1; //позиция для заполнения
            int sizeColl = 1; //сколько необходимо пройти столбцов
            for (int j = matrix.Row - 1; j >= 0; j--)
            {
                for (int i = matrix.Coll - 1; i >= matrix.Coll - sizeColl; i--)
                {
                    if (matrix.Coll - sizeColl == i)
                    {
                        /*if (aMatrix.Values[j, i] == 0)
                            Console.WriteLine(j + " " + i + " " + aMatrix.Values[j, i]);*/
                        vectorResult.Values[indexCountVector] = Vector.Values[indexCountVector] / matrix.Values[j, i];
                        indexCountVector--;
                    }
                    else
                    {
                        Vector.Values[indexCountVector] -= (matrix.Values[j, i] * vectorResult.Values[i]);
                    }
                }
                sizeColl++;
            }

            return vectorResult;
        }

        public double[,] RecieveMatrix()
        {
            double[,] newMatrix = new double[Matrix.Row, Matrix.Coll];
            int indexColl = 0;
            int indexRow = 0;

            /* while(indexColl < coll)
             {
                 while(indexRow < row)
                 {

                 }
             }*/
            //Thread.Sleep(2000);

            int indexClient = 0;
            while (indexClient < Clients.Count)
            {
                byte[] buffer = new byte[sizeof(int)];
                Clients[indexClient].Receive(buffer);
                int size = Converter.GetInt(buffer)[0];
                buffer = new byte[sizeof(double) * size];
                Clients[indexClient].Receive(buffer);
                double[] matrixAsVector = Converter.GetDoubles(buffer);

                for (int i = 0; i < matrixAsVector.Length; i++)
                {
                    newMatrix[indexRow, indexColl] = matrixAsVector[i];

                    indexRow++;
                    if (indexRow == Matrix.Row)
                    {
                        indexRow = 0;
                        indexColl++;
                    }
                }
                indexClient++;
            }

            return newMatrix;
        }

        private List<int[]> CreateVectorParameners(List<double[]> sendVectors, int row, int sizeCollLastClient)
        {

            List<int[]> parameterVectors = new List<int[]>();
            int[] vector = null;
            for (int i = 0; i < Clients.Count; i++)
            {
                vector = new int[3];
                vector[0] = i + 1;
                vector[1] = row;
                vector[2] = sendVectors[i].Length / row;
                parameterVectors.Add(vector);
                if (i == Clients.Count - 1)
                    sizeCollLastClient = sendVectors[i].Length / row;
            }
            return parameterVectors;
        }

        private double[] CreateSendVector(int start, int step)
        {
            double[] vector = new double[FindLengthVector(start, step) * Matrix.Row];
            //  Console.WriteLine("Vector length = " + vector.Length);
            int indexVector = 0;

            for (int i = start; i < Matrix.Coll; i += step)
            {
                for (int j = 0; j < Matrix.Row; j++)
                {
                    vector[indexVector] = Matrix.Values[j, i];
                    indexVector++;
                }
            }
            return vector;
        }

        private int FindLengthVector(int start, int step)
        {
            int length = 0;
            for (int i = start; i < Matrix.Coll; i += step)
            {
                length++;
            }
            return length;
        }

        private void SendParameters(List<int[]> vectors)
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                Clients[i].Send(Converter.GetBytes(vectors[i]));
            }
        }
        private void SendMatrix(List<double[]> vectors)
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                Clients[i].Send(Converter.GetBytes(vectors[i]));
            }
        }
    }
}
