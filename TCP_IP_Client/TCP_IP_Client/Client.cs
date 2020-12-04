using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace TCP_IP_Client
{
    class Client
    {
        // адрес и порт сервера, к которому будем подключаться
        static int port = 8080; // порт сервера
        static string address = "127.0.0.1"; // адрес сервера
        static double epx = 0.000000001;
        static void Main(string[] args)
        {
            Console.WriteLine("Enter address:");

            address = Console.ReadLine();
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipPoint);
                Console.WriteLine("Клиент успешно подключен!");
                while (SocketConnected(socket))
                {
                    byte[] buffer = new byte[256];

                    int bytes = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    int[] matrixAsParameters = GetInt(buffer);
                    int numberClient = matrixAsParameters[0];
                    int row = matrixAsParameters[1];
                    int coll = matrixAsParameters[2];
                   /* Console.WriteLine("Номер клиента = {0}", numberClient);
                    Console.WriteLine("Строк = {0}", row);
                    Console.WriteLine("Столбцов = {0}", coll);*/

                    /*int numberClient = int.Parse(Encoding.Unicode.GetString(buffer));
                    //int numberClient = BitConverter.ToInt32(buffer, 0);


                    buffer = new byte[256];
                    bytes = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    int row = int.Parse(Encoding.Unicode.GetString(buffer));
                    Console.WriteLine(bytes);
                    //int row = BitConverter.ToInt32(buffer, 0);

                    buffer = new byte[256];
                    bytes = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    Console.WriteLine(bytes);

                    int coll = int.Parse(Encoding.Unicode.GetString(buffer));
                    //int coll = BitConverter.ToInt32(buffer, 0);*/


                    buffer = new byte[sizeof(double) * row * coll];
                    bytes = socket.Receive(buffer, 0, sizeof(double) * row * coll, SocketFlags.None);
                    double[] matrixAsVector = GetDoubles(buffer);

                   /* for (int i = 0; i < matrixAsVector.Length; i++)
                    {
                        Console.WriteLine(matrixAsVector[i]);
                    }*/
                    //Console.WriteLine("NP");
                    double[,] matrix = new double[row, coll];
                    for (int i = 0; i < row; i++)
                        for (int j = 0; j < coll; j++)
                            matrix[i, j] = matrixAsVector[i + j * row];


                    int startIndexRow = 0;
                    int sizeMessageParameters = 4; 

                    if (numberClient == 1)
                    {
                        SendFromFirstClient(socket, buffer, numberClient, matrix, coll, row, sizeMessageParameters);
                    }
                    else
                    {
                        SendFromClients(socket, buffer, bytes, numberClient, matrix, coll, row, sizeMessageParameters);
                    
                       /* if(lastClient > numberClient)
                        {
                            int testBool = 0;
                            if (index == row - 1)
                                testBool = 1;
                            while (index < row)
                            {
                                double[] sendParameters = new double[4] { numberClient, testBool, lastClient, testNumber++ };
                            }
                        }*/
                    }
                    SendResult(socket, buffer, matrix, row, coll);

                   /* Console.WriteLine("LALALA");
                    for (int i = 0; i < row; i++)
                    {
                        for (int j = 0; j < coll; j++)
                        {
                            Console.Write("{0} ", matrix[i, j]);
                        }
                        Console.WriteLine();
                    }*/
                }
                //Console.WriteLine("O noy");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.Read();
        }

        static void SendResult(Socket socket, byte[] buffer, double[,] matrix, int row, int coll)
        {
            byte[] sizeList = GetBytes(new int[1] { row * coll });
            socket.Send(sizeList);
            buffer = GetBytes(CreateSendVector(row, coll, matrix));
            socket.Send(buffer);
        }

        static double[] CreateSendVector(int row, int coll, double[,] matrix)
        {
            double[] vector = new double[row * coll];
            int indexVector = 0;

            for (int i = 0; i < coll; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    vector[indexVector] = matrix[j, i];
                    indexVector++;
                }
            }
            return vector;
        }

        static void SendFromClients(Socket socket, byte[] buffer, int bytes, int numberClient,
                                    double[,] matrix, int sizeColl, int sizeRow, int sizeMessage)
        {
            buffer = new byte[sizeof(double) * sizeMessage];

            int checkClient = 0; //булевеская проверка последнего клиента
            int lastClient = 0; //булевеская проверка последнего клиента

            int startIndexRow = 0;
            int indexRow = 1; //начальный индекс строки

            while (checkClient < numberClient - 1)
            {
                int checkDataRow = 0;
                int rowData = startIndexRow + 1;

                int startColl = 0;

                int countColl = 0; //сколько столбцов было пройдено
                //коеф который прибавл в строке
                while (checkDataRow == 0)
                {
                    bytes = socket.Receive(buffer, 0, sizeof(double) * sizeMessage, SocketFlags.None);
                    double[] getParameters = GetDoubles(buffer);
                    //Console.WriteLine("R " + getParameters[3]);
                    lastClient = (int)getParameters[2];

                    checkDataRow = (int)getParameters[1];
                    CalculateNewMatrix(getParameters[3], matrix, rowData, startIndexRow, sizeColl, startColl);

                    if (rowData == sizeRow - 1)
                    {
                        //startColl++;
                        startIndexRow++;
                        rowData = startIndexRow + 1;

                        countColl++;
                    }
                    else
                        rowData++;
                }
                checkClient++;
            }
            if(lastClient == 0)
            {
                SendNextClients(socket, buffer, numberClient, matrix, sizeColl, sizeRow, startIndexRow, lastClient);
            }
            if (lastClient == 1 && sizeColl != 1)
            {
                SendNextClients(socket, buffer, numberClient, matrix, sizeColl, sizeRow, startIndexRow, lastClient);
            }

            /* if (lastClient == 0)
             {
                 SendNextClients(socket, buffer, numberClient, matrix, sizeColl, sizeRow, startIndexRow);
             }
             else
             {
                 LastClient(socket, buffer, numberClient, matrix, sizeColl, sizeRow, startIndexRow);
             }*/

        }

        static void LastClient(Socket socket, byte[] buffer, int numberClient,
                                double[,] matrix, int sizeColl, int sizeRow, int startRow)
        {
            int startIndexRow = startRow;
            int indexRow = 1; //начальный индекс строки
            int indexAddition = startRow + 1; //индес, которые необходим для перехода на другую строку

            int collTest = 0;
            while (collTest < sizeColl)
            {

                indexRow = indexAddition;
                while (indexRow < sizeRow)
                {

                    //Console.WriteLine("How");
                    int testBool = 0;
                    if (indexRow == sizeRow - 1 && collTest == sizeColl - 1)
                        testBool = 1;
                    double koeff = -(matrix[indexRow, collTest] / matrix[startIndexRow, collTest]);
                    CalculateNewMatrix(koeff, matrix, indexRow, startIndexRow, sizeColl, collTest);

                    indexRow++;
                }
                startIndexRow++;
                indexAddition++;
                collTest++;
            }
        }

        static void SendNextClients(Socket socket, byte[] buffer, int numberClient,
                                        double[,] matrix, int sizeColl, int sizeRow, int startRow, int lastClient)
        {
            int startIndexRow = startRow;
            int indexRow = 1; //начальный индекс строки
            int indexAddition = startRow + 1; //индес, которые необходим для перехода на другую строку

            int collTest = 0;
            while (collTest < sizeColl)
            {

                indexRow = indexAddition;
                while (indexRow < sizeRow)
                {

                   // Console.WriteLine("How");
                    int testBool = 0;
                    if (indexRow == sizeRow - 1 && collTest == sizeColl - 1)
                        testBool = 1;
                    if(lastClient == 1 && sizeColl != 1)
                    {
                        if (indexRow == sizeRow - 1 && collTest == sizeColl - 2)
                            testBool = 1;
                    }
                    else
                    {
                        if (indexRow == sizeRow - 1 && collTest == sizeColl - 1)
                            testBool = 1;
                    }
                    double koeff = -(matrix[indexRow, collTest] / matrix[startIndexRow, collTest]);
                    double[] sendParameters = new double[4] { numberClient, testBool, 0, koeff };
                    buffer = null;
                    buffer = GetBytes(sendParameters);
                    //Console.WriteLine(buffer.Length);
                    socket.Send(buffer);

                    CalculateNewMatrix(koeff, matrix, indexRow, startIndexRow, sizeColl, collTest);

                    indexRow++;
                }
                startIndexRow++;
                indexAddition++;
                collTest++;
            }
        }
        static void SendFromFirstClient(Socket socket, byte[] buffer, int numberClient,
                                        double[,] matrix, int sizeColl, int sizeRow, int sizeMessage)
        {
            int startIndexRow = 0;
            int indexRow = 1; //начальный индекс строки
            int indexAddition = 1; //индес, которые необходим для перехода на другую строку


            byte[] sizeList = new byte[sizeof(int)];
            sizeList = BitConverter.GetBytes(sizeMessage);
            socket.Send(sizeList);

            int collTest = 0;
            while (collTest < sizeColl)
            {

                indexRow = indexAddition;
                while (indexRow < sizeRow)
                {

                    //Console.WriteLine("How");
                    int testBool = 0;
                    if (indexRow == sizeRow - 1 && collTest == sizeColl - 1)
                        testBool = 1;
                    double koeff = -(matrix[indexRow, collTest] / matrix[startIndexRow, collTest]);
                    double[] sendParameters = new double[4] { numberClient, testBool, 0, koeff };
                    buffer = null;
                    buffer = GetBytes(sendParameters);
                    //Console.WriteLine(buffer.Length);
                    socket.Send(buffer);

                    CalculateNewMatrix(koeff, matrix, indexRow, startIndexRow, sizeColl, collTest);

                    indexRow++;
                }
                startIndexRow++;
                indexAddition++;
                collTest++;
            }
        }

        static void CalculateNewMatrix(double koef, double[,] matrix, int row, int startRow, int coll, int startColl)
        {
            for (int i = startColl; i < coll; i++)
            {
                matrix[row, i] = koef * matrix[startRow, i] + matrix[row, i];
                if (matrix[row, i] < epx && matrix[row, i] > -epx)
                    matrix[row, i] = 0;
            }
        }

        static bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }

        static byte[] GetBytes(double[] values)
        {
            return values.SelectMany(value => BitConverter.GetBytes(value)).ToArray();
        }

        static byte[] GetBytes(int[] values)
        {
            return values.SelectMany(value => BitConverter.GetBytes(value)).ToArray();
        }

        static int[] GetInt(byte[] bytes)
        {
            return Enumerable.Range(0, bytes.Length / sizeof(int))
                .Select(offset => BitConverter.ToInt32(bytes, offset * sizeof(int))).ToArray();
        }
        static double[] GetDoubles(byte[] bytes)
        {
            return Enumerable.Range(0, bytes.Length / sizeof(double))
                .Select(offset => BitConverter.ToDouble(bytes, offset * sizeof(double))).ToArray();
        }

        static void showMatrix(double[] matrixAsVector, int rang)
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
