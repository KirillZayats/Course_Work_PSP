using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using MatrixLibrary;

namespace TCP_IP_App
{
    class Server
    {
        static int port = 8080;
        static string address = "0.0.0.0";
        static double epx = 0.000000001;
        static int sizeCollLastClient = 0;
        static string timeSolve = ""; //результат времени работы алгоритма
        static void Main(string[] args)
        {
            Matrix matrix = null;
            Vector vector = null;
            LinearSystem linearSystem = null;
            LinearSystem asyncSystem = null;

            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
            Socket listenSocket = null;

            List<Socket> clients = null;
            try
            {

                Console.WriteLine("Сервер запущен и готов к работе.");

                while (true)
                {
                    int caseSwitch = Menu();
                    switch (caseSwitch)
                    {
                        case 1:
                            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            listenSocket.Bind(ipPoint);
                            Console.WriteLine("Сколько клиентов ожидать на подключение?");
                            int countConnectClient = int.Parse(Console.ReadLine());

                            //DisconnectClients(clients);
                            clients = new List<Socket>();
                            ConnectClients(listenSocket, clients, countConnectClient);
                            
                            Console.WriteLine(clients.Count);
                            break;

                        case 2:
                            Console.WriteLine("Какого клиента удалить (укажите номер)?");
                            int numberClient = int.Parse(Console.ReadLine());
                            DisconnectClient(clients, numberClient - 1);
                            break;

                        case 3:
                            Console.WriteLine("Укажите размерность?");
                            int size = int.Parse(Console.ReadLine());
                            Console.WriteLine(size);
                            matrix = new Matrix(size, size);
                            vector = new Vector(size);
                            vector.GenerateVector();

                           /* int size = 4;
                             matrix = new Matrix(new double[,] { {12, 26, 3.1,4.5 }, {57,6.1,71,8.7 }, {9.5,10.4,11.12,12.5 }, {133,13,5,1.6 } }, size, size);
                             vector = new Vector(new double[] { 1, 2, 3, 4 }, size);*/

                            /*int size = 3;

                            matrix = new Matrix(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9.1 } }, size, size);
                            vector = new Vector(new double[] { 1, 2, 3.2}, size);*/
                            Console.WriteLine("Успешное сохдание матрицы и вектора!");

                            break;
                        case 4:
                            var startTime = System.Diagnostics.Stopwatch.StartNew();

                             Vector vectorResult = AsyncSolution(clients, matrix, vector, asyncSystem);

                            startTime.Stop();
                            var resultTime = startTime.Elapsed;

                            OutputTime(resultTime);

                            vectorResult.Output("Result");
                            break;
                        case 5:
                            startTime = System.Diagnostics.Stopwatch.StartNew();

                            linearSystem = new LinearSystem(matrix, vector);
                            linearSystem.GaussSolve();
                            startTime.Stop();
                            resultTime = startTime.Elapsed;
                            OutputTime(resultTime);
                            linearSystem.xVector.Output("Result");
                            break;
                        case 6:
                            Console.WriteLine("Количество клиентов = " + clients.Count);
                            break;
                        case 7:
                            matrix.Output("A");
                            vector.Output("B");
                            break;
                        case 8:
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void OutputTime(TimeSpan time)
        {
            timeSolve = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
            time.Hours,
            time.Minutes,
            time.Seconds,
            time.Milliseconds);
            Console.WriteLine("\n{0:N2} seconds", timeSolve);
        }

        static Vector AsyncSolution(List<Socket> clients, Matrix matrix, Vector vector, LinearSystem asyncSystem)
        {
            Matrix asyncMatrix = new Matrix(matrix.Values, matrix.Row, matrix.Coll);
            Vector asyncVector = new Vector(vector.Values, vector.Size);
            //matrix.Output("A");
            byte[] buffer = null;
            List<double[]> sendVectors = new List<double[]>();
            for (int i = 0; i < clients.Count; i++)
                sendVectors.Add(CreateSendVector(i, clients.Count, asyncMatrix));
            List<int[]> parameterVectors = CreateVectorParameners(clients, sendVectors, asyncMatrix.Row);
            SendParameters(parameterVectors, clients, buffer);
            SendMatrix(sendVectors, clients, buffer);

            //покучение параметра на который неодходимо делить столбцы

            int indexClient = 0;
            int indexRowVector = 1;
            int indexStartRowVector = 0;

            buffer = new byte[sizeof(int)];
            int bytes = clients[indexClient].Receive(buffer, 0, buffer.Length, SocketFlags.None);
            int size = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[sizeof(double) * size];

            int countClients = clients.Count;
            if (sizeCollLastClient == 1)
                countClients--;

            while (indexClient < countClients)
            {
                bytes = clients[indexClient].Receive(buffer, 0, buffer.Length, SocketFlags.None);
                double[] matrixAsParameters = GetDoubles(buffer);
                /*if ((indexClient + 1) == (int)matrixAsParameters[0])
                {*/
                   /* foreach (var item in matrixAsParameters)
                    {
                        Console.WriteLine(item);
                    }*/
                    if ((int)matrixAsParameters[0] == clients.Count - 1)
                        matrixAsParameters[2] = 1;
                //    Console.WriteLine("t\n");

                    for (int i = indexClient + 1; i < clients.Count; i++)
                    {
                        buffer = GetBytes(matrixAsParameters);
                        clients[i].Send(buffer);
                    }
                    CalculateNewVectorB(matrixAsParameters[3], asyncVector.Values, indexRowVector, indexStartRowVector);
                    if (indexRowVector == asyncVector.Size - 1)
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

            Matrix newMatrix = new Matrix(RecieveMatrix(clients, buffer, bytes, asyncMatrix.Row, asyncMatrix.Coll),
                                                        asyncMatrix.Row, asyncMatrix.Coll);
           // newMatrix.Output("testNewMatrix");

            asyncSystem = new LinearSystem(newMatrix, asyncVector);
            Vector vectorResult = asyncSystem.GaussBackword();
            /*vectorResult.Output("Result");
            Console.WriteLine("RRRR");*/
            return vectorResult;
        }

        static double[,] RecieveMatrix(List<Socket> clients, byte[] buffer, int bytes, int row, int coll)
        {
            double[,] newMatrix = new double[row, coll];
            int indexColl = 0;
            int indexRow = 0;

            /* while(indexColl < coll)
             {
                 while(indexRow < row)
                 {

                 }
             }*/

            int indexClient = 0;
            while (indexClient < clients.Count)
            {
                buffer = new byte[sizeof(int)];
                bytes = clients[indexClient].Receive(buffer, 0, buffer.Length, SocketFlags.None);
                int size = GetInt(buffer)[0];

                buffer = new byte[sizeof(double) * size];
                bytes = clients[indexClient].Receive(buffer, 0, sizeof(double) * size, SocketFlags.None);
                double[] matrixAsVector = GetDoubles(buffer);

                for (int i = 0; i < matrixAsVector.Length; i++)
                {
                    newMatrix[indexRow, indexColl] = matrixAsVector[i];
                    indexRow++;
                    if(indexRow == row)
                    {
                        indexRow = 0;
                        indexColl++;
                    }
                }
                indexClient++;
            }

            return newMatrix;
        }


        static void CalculateNewVectorB(double koef, double[] vector, int row, int startRow)
        {
            vector[row] = koef * vector[startRow] + vector[row];

            if (vector[row] < epx && vector[row] > -epx)
                    vector[row] = 0;
            
        }

        static List<int[]> CreateVectorParameners(List<Socket> clients, List<double[]> sendVectors, int row)
        {

            List<int[]> parameterVectors = new List<int[]>();
            int[] vector = null;
            for (int i = 0; i < clients.Count; i++)
            {
                vector = new int[3];
                vector[0] = i + 1;
                vector[1] = row;
                vector[2] = sendVectors[i].Length / row;
                parameterVectors.Add(vector);
                if (i == clients.Count - 1)
                    sizeCollLastClient = sendVectors[i].Length / row;
            }
            return parameterVectors;
        }

        static void SendNumberClients(List<Socket> clients, byte[] buffer)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                buffer = Encoding.Unicode.GetBytes((i + 1) + "");
                //buffer = BitConverter.GetBytes(i + 1).ToArray();
                clients[i].Send(buffer);
            }
        }

        static void SendParameters(List<int[]> vectors, List<Socket> clients, byte[] buffer)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                buffer = GetBytes(vectors[i]);
                clients[i].Send(buffer);
            }
        }
        static void SendMatrix(List<double[]> vectors, List<Socket> clients, byte[] buffer)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                buffer = GetBytes(vectors[i]);
                clients[i].Send(buffer);
            }
        }

        static double[] CreateSendVector(int start, int step, Matrix matrix)
        {
            double[] vector = new double[FindLengthVector(start, step, matrix) * matrix.Row];
          //  Console.WriteLine("Vector length = " + vector.Length);
            int indexVector = 0;

            for (int i = start; i < matrix.Coll; i += step)
            {
                for (int j = 0; j < matrix.Row; j++)
                {
                    vector[indexVector] = matrix.Values[j, i];
                    indexVector++;
                }
            }
            return vector;
        }

        static int FindLengthVector(int start, int step, Matrix matrix)
        {
            int length = 0;
            for (int i = start; i < matrix.Coll; i += step)
            {
                length++;
            }
            return length;
        }

        static void SendRow(List<Socket> clients, byte[] buffer, int number)
        {
            buffer = Encoding.Unicode.GetBytes(number + "");
            //buffer = BitConverter.GetBytes(number).ToArray();

            foreach (var item in clients)
            {
                item.Send(buffer);
            }
        }

        static void SendColl(List<double[]> vectors, List<Socket> clients, byte[] buffer, int sizeRow)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                buffer = Encoding.Unicode.GetBytes((vectors[i].Length / sizeRow) + "");
                //buffer = BitConverter.GetBytes((int)(vectors[i].Length / sizeRow)).ToArray();
                //Console.WriteLine(BitConverter.ToInt32(buffer, 0));
                clients[i].Send(buffer);
            }

        }


        static void ConnectClients(Socket listenSocket , List<Socket> clients, int countConnectClient)
        {
            Console.WriteLine("Ожидаем клиентов...");
            listenSocket.Listen(10);
            int countConnected = 0;
            while (countConnected < countConnectClient)
            {
                Socket client = listenSocket.Accept();
                Console.WriteLine("Новое подключение...");
                Console.WriteLine(client.RemoteEndPoint.ToString());
                clients.Add(client);
                countConnected++;
            }

            listenSocket.Close();
        }

        static void DisconnectClients(List<Socket> clients)
        {
            if (clients != null)
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    DisconnectClient(clients, 0);
                }
            }

        }

        static void DisconnectClient(List<Socket> clients, int numberClient)
        {
            if (clients.Count > 0)
            {
                if (numberClient < clients.Count)
                {
                    clients[numberClient].Disconnect(true);
                    clients.Remove(clients[numberClient]);
                    Console.WriteLine("Клиент под номером " + numberClient + " успешно удалён!");
                }
                else Console.WriteLine("Клиента под номером " + numberClient + " нет с списке!");
            }
            else
                Console.WriteLine("Список клиентов пуст!");
        }

        static int Menu()
        {
            Console.WriteLine("1. Режим подключения клиентов");
            Console.WriteLine("2. Отключить клиента");
            Console.WriteLine("3. Создать матрицу и вектор B");
            Console.WriteLine("4. Выполнить распределённое решение СЛАУ");
            Console.WriteLine("5. Выполнить линейное решение СЛАУ");
            Console.WriteLine("6. Узнать количество клиентов");
            Console.WriteLine("7. Вывод сгенерированной матрицы и вектора");
            Console.WriteLine("8. Выход");
            Console.WriteLine("\n");
            Console.WriteLine("Выберите номер меню: ");
            int index = int.Parse(Console.ReadLine());
            return index;
        }

        static double[] GetDoubles(byte[] bytes)
        {
            return Enumerable.Range(0, bytes.Length / sizeof(double))
                .Select(offset => BitConverter.ToDouble(bytes, offset * sizeof(double))).ToArray();
        }

        static int[] GetInt(byte[] bytes)
        {
            return Enumerable.Range(0, bytes.Length / sizeof(int))
                .Select(offset => BitConverter.ToInt32(bytes, offset * sizeof(int))).ToArray();
        }

        static double[] GetDoubles(byte[] bytes, int count)
        {
            return Enumerable.Range(0, bytes.Length / sizeof(double))
                .Select(offset => BitConverter.ToDouble(bytes, offset * sizeof(double))).ToArray();
        }

        static byte[] GetBytes(int[] values)
        {
            return values.SelectMany(value => BitConverter.GetBytes(value)).ToArray();
        }

        static byte[] GetBytes(double[] values)
        {
            return values.SelectMany(value => BitConverter.GetBytes(value)).ToArray();
        }

        static void showMatrix(double[,] matrixAsVector, int rang)
        {
            for (int i = 0; i < rang; i++)
            {
                for (int j = 0; j < rang; j++)
                {
                    Console.Write(matrixAsVector[i, j] + " ");
                }
                Console.WriteLine();
            }
        }

        static void showVector(double[] matrixAsVector, int rang)
        {
            for (int i = 0; i < rang; i++)
            {
                for (int j = 0; j < rang; j++)
                {
                    Console.Write(matrixAsVector[i * rang + j] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}



