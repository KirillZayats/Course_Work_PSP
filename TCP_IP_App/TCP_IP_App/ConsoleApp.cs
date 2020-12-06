using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using MathLibrary;
using System.Diagnostics;
using StorageLibrary;
using NetworkLibrary;

namespace TCP_IP_App
{
    class ConsoleApp
    {
        static double epx = 0.000000001;
        static int sizeCollLastClient = 0;
        static string timeSolve = ""; //результат времени работы алгоритма
        static LinearSystem linearSystem = null;


        static void Main(string[] args)
        {
            Matrix matrix = null;
            Vector vector = null;

            Server server = new Server();

            try
            {

                Console.WriteLine("Сервер запущен и готов к работе.");

                while (true)
                {
                    int caseSwitch = Menu();
                    switch (caseSwitch)
                    {
                        case 1:
                            server.Bind();
                            Console.WriteLine("Сколько клиентов ожидать на подключение?");
                            int countConnectClient = int.Parse(Console.ReadLine());

                            //DisconnectClients(clients);
                            server.ConnectClients(countConnectClient);

                            Console.WriteLine(server.Clients.Count);
                            break;

                        case 2:
                            Console.WriteLine("Какого клиента удалить (укажите номер)?");
                            int numberClient = int.Parse(Console.ReadLine());
                            server.DisconnectClient(numberClient - 1);
                            break;

                        case 3:
                           /* Console.WriteLine("Укажите размерность?");
                            int size = int.Parse(Console.ReadLine());
                            Console.WriteLine(size);
                            matrix = new Matrix(size, size);
                            matrix.GenerateMatrix();
                            vector = new Vector(size);
                            vector.GenerateVector();*/

                            /* int size = 4;
                              matrix = new Matrix(new double[,] { {12, 26, 3.1,4.5 }, {57,6.1,71,8.7 }, {9.5,10.4,11.12,12.5 }, {133,13,5,1.6 } }, size, size);
                              vector = new Vector(new double[] { 1, 2, 3, 4 }, size);*/

                            int size = 3;

                            matrix = new Matrix(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9.1 } }, size, size);
                            vector = new Vector(new double[] { 1, 2, 3.2}, size);
                            Console.WriteLine("Успешное сохдание матрицы и вектора!");

                            break;
                        case 4:
                            DistributedSolution distributedSolution = new DistributedSolution(server.Clients);
                            var startTime = System.Diagnostics.Stopwatch.StartNew();

                            distributedSolution.initParameters(matrix, vector);
                            distributedSolution.Run();
                            startTime.Stop();
                            var resultTime = startTime.Elapsed;

                            OutputTime(resultTime);

                            distributedSolution.VectorResult.Output("Result");
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
                            Console.WriteLine("Количество клиентов = " + server.Clients.Count);
                            break;
                        case 7:
                            matrix.Output("A");
                            vector.Output("B");
                            break;
                        case 8:
                            Console.WriteLine("Введите название файла, из которого необходимо получить матрицу?");
                            matrix = FileStream.ReadMatrix(Console.ReadLine());
                            Console.WriteLine("Введите название файла, из которого необходимо получить вектор?");
                            vector = FileStream.ReadVector(Console.ReadLine());
                            //  matrix.Output("ДА ДА");
                            //  vector.Output("No no");
                            break;
                        case 9:
                            Console.WriteLine("Введите название файла, в который необходимо записать матрицу?");
                            FileStream.WriteMatrix(Console.ReadLine(), matrix);
                            Console.WriteLine("Введите название файла, в который необходимо записать вектор?");
                            FileStream.WriteVector(Console.ReadLine(), vector);
                            break;
                        case 10:
                            return;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"StackTrace: {e.StackTrace}\n\n");

                var trace = new StackTrace(e, true);

                foreach (var frame in trace.GetFrames())
                {
                    var sb = new StringBuilder();

                    sb.AppendLine($"Файл: {frame.GetFileName()}");
                    sb.AppendLine($"Строка: {frame.GetFileLineNumber()}");
                    sb.AppendLine($"Столбец: {frame.GetFileColumnNumber()}");
                    sb.AppendLine($"Метод: {frame.GetMethod()}");

                    Console.WriteLine(sb);
                }
                Console.WriteLine(e.Message);
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


        static int Menu()
        {
            Console.WriteLine("1. Режим подключения клиентов");
            Console.WriteLine("2. Отключить клиента");
            Console.WriteLine("3. Создать матрицу и вектор B");
            Console.WriteLine("4. Выполнить распределённое решение СЛАУ");
            Console.WriteLine("5. Выполнить линейное решение СЛАУ");
            Console.WriteLine("6. Узнать количество клиентов");
            Console.WriteLine("7. Вывод сгенерированной матрицы и вектора");
            Console.WriteLine("8. Получить матрицу и вектор с файла");
            Console.WriteLine("9. Записать матрицу и вектор в файл");
            Console.WriteLine("10. Выход");
            Console.WriteLine("\n");
            Console.WriteLine("Выберите номер меню: ");
            int index = int.Parse(Console.ReadLine());
            return index;
        }
    }
}



