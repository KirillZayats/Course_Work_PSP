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

        private int sizeCollLastClient;

        public Vector VectorResult { get; set; }
        private byte[] buffer;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="client"></param>
        public DistributedSolution(List<Client> client)
        {
            this.Clients = client;
        }

        /// <summary>
        /// Инициализация матрицы и вектора
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="vector"></param>
        public void InitParameters(Matrix matrix, Vector vector)
        {
            this.Matrix = new Matrix(matrix.Values, matrix.Row, matrix.Coll);
            this.Vector = new Vector(vector.Values, vector.Size);
        }

        /// <summary>
        /// Запуск распределённого решения
        /// </summary>
        public void Run()
        {
            SendStartData();
            GetSizeArrayParameters();

            //покучение параметра на который неодходимо делить столбцы
            int indexClient = 0;
            int indexRowVector = 1;
            int indexStartRowVector = 0;
            int countClients = SetCountClients(Clients.Count);

            while (indexClient < countClients)
            {
                Clients[indexClient].Receive(buffer);
                double[] matrixAsParameters = Converter.GetDoubles(buffer);

                if ((int)matrixAsParameters[0] == Clients.Count - 1)
                    matrixAsParameters[2] = 1;

                for (int i = indexClient + 1; i < Clients.Count; i++)
                    Clients[i].Send(Converter.GetBytes(matrixAsParameters));

                Vector.CalculateNewVector(matrixAsParameters[3], indexRowVector, indexStartRowVector);
                if (indexRowVector == Vector.Size - 1)
                {
                    indexStartRowVector++;
                    indexRowVector = indexStartRowVector + 1;
                }
                else
                    indexRowVector++;

                if (Convert.ToBoolean(matrixAsParameters[1]))
                    indexClient++;
            }

            GetResultVector();
        }

        /// <summary>
        /// Получение результирующего вектора
        /// </summary>
        private void GetResultVector()
        {
            Matrix newMatrix = new Matrix(RecieveMatrix(), Matrix.Row, Matrix.Coll);
            VectorResult = GaussBackword(newMatrix);
            if (Clients.Count < VectorResult.Size)
                VectorResult.SortVector(Clients.Count);
        }

        /// <summary>
        /// Получени количество клиентов
        /// </summary>
        /// <param name="countClients"></param>
        /// <returns></returns>
        private int SetCountClients(int countClients)
        {
            if (sizeCollLastClient == 1)
                countClients--;
            return countClients;
        }

        /// <summary>
        /// Получение параметров
        /// </summary>
        private void GetSizeArrayParameters()
        {
            buffer = new byte[sizeof(int)];
            Clients[0].Receive(buffer);
            int size = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[sizeof(double) * size];
        }

        /// <summary>
        /// Отправка начальных данных
        /// </summary>
        private void SendStartData()
        {
            List<double[]> sendVectors = new List<double[]>();
            for (int i = 0; i < Clients.Count; i++)
                sendVectors.Add(CreateSendVector(i, Clients.Count));
            List<int[]> parameterVectors = CreateVectorParameners(sendVectors, Matrix.Row);
            SendParameters(parameterVectors);
            SendMatrix(sendVectors);
        }

        /// <summary>
        /// Обратный проход по матрице
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Отправка матрицы на клиенты
        /// </summary>
        /// <returns></returns>
        public double[,] RecieveMatrix()
        {
            double[,] newMatrix = new double[Matrix.Row, Matrix.Coll];
            int indexColl = 0;
            int indexRow = 0;

            int indexClient = 0;
            while (indexClient < Clients.Count)
            {
                buffer = GetDataFromClient(indexClient);
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

        /// <summary>
        /// Получение данных от клиента
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private byte[] GetDataFromClient(int index)
        {
            byte[] buffer = new byte[sizeof(int)];
            Clients[index].Receive(buffer);
            int size = Converter.GetInt(buffer)[0];
            buffer = new byte[sizeof(double) * size];
            Clients[index].Receive(buffer);
            return buffer;
        }

        /// <summary>
        /// Создание параметров для отправки
        /// </summary>
        /// <param name="sendVectors"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private List<int[]> CreateVectorParameners(List<double[]> sendVectors, int row)
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

        /// <summary>
        /// Получение вектора для отправки
        /// </summary>
        /// <param name="start"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        private double[] CreateSendVector(int start, int step)
        {
            double[] vector = new double[FindLengthVector(start, step) * Matrix.Row];
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

        /// <summary>
        /// Получени длины вектора
        /// </summary>
        /// <param name="start"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        private int FindLengthVector(int start, int step)
        {
            int length = 0;
            for (int i = start; i < Matrix.Coll; i += step)
            {
                length++;
            }
            return length;
        }

        /// <summary>
        /// Отправка массива параметров на клиенты
        /// </summary>
        /// <param name="vectors"></param>
        private void SendParameters(List<int[]> vectors)
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                Clients[i].Send(Converter.GetBytes(vectors[i]));
            }
        }

        /// <summary>
        /// Отправка матрицы в виде массива на клиенты
        /// </summary>
        /// <param name="vectors"></param>
        private void SendMatrix(List<double[]> vectors)
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                Clients[i].Send(Converter.GetBytes(vectors[i]));
            }
        }
    }
}
