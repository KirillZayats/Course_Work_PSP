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

namespace Console_Server
{
    class ConsoleApp
    {
        static Matrix matrix = null;
        static Vector vector = null;

        static LinearSystem linearSystem = null;
        static DistributedSolution distributedSolution = null;
        static Server server = null;

        static Stopwatch startTime;
        static TimeSpan resultTime;

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Сервер запущен и готов к работе.");

                while (true)
                {
                    int caseSwitch = Menu();
                    switch (caseSwitch)
                    {
                        case 1:
                            AddClients();
                            break;
                        case 2:
                            DeleteClient();
                            break;
                        case 3:
                            GenerateData();
                            break;
                        case 4:
                            StartDistributedSolution();
                            break;
                        case 5:
                            StartLinearSystem();
                            break;
                        case 6:
                            OutputInfoClients();
                            break;
                        case 7:
                            OutputInfoMathData();
                            break;
                        case 8:
                            ReadData();
                            break;
                        case 9:
                            WriteData();
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

        /// <summary>
        /// Меню приложения
        /// </summary>
        /// <returns></returns>
        static int Menu()
        {
            Console.WriteLine("1. Подключить/передоключить клиентов");
            Console.WriteLine("2. Отключить клиента");
            Console.WriteLine("3. Создать матрицу и вектор B");
            Console.WriteLine("4. Выполнить распределённое решение СЛАУ");
            Console.WriteLine("5. Выполнить линейное решение СЛАУ");
            Console.WriteLine("6. Узнать данные о клиенте");
            Console.WriteLine("7. Показать сгенерированную матрицу и вектор");
            Console.WriteLine("8. Получить матрицу и вектор с файла");
            Console.WriteLine("9. Записать матрицу и вектор в файл");
            Console.WriteLine("10. Выход");
            Console.WriteLine("\n");
            Console.WriteLine("Выберите номер меню: ");
            int index = CheckInputData(Console.ReadLine(), 0);
            return index;
        }

        /// <summary>
        /// Вывод информации о матрице и вектора
        /// </summary>
        static void OutputInfoMathData()
        {
            if (matrix != null && vector != null)
            {
                matrix.Output("A");
                vector.Output("B");
            }
            else
                Console.WriteLine("Показать матрицу и вектор невозможно! Данные о матрице и векторе отсутствуют...");
        }

        /// <summary>
        /// Вывод информации о клиентах
        /// </summary>
        static void OutputInfoClients()
        {
            if (server != null)
            {
                server.IsConnectedClients();
                if(server.Clients.Count > 0)
                    server.OutputInfoClients();
                else
                    Console.WriteLine("К серверу не подключены клиенты!");
            }
            else
                Console.WriteLine("Данные о клиентах неизвестно! Клиенты ещё не подключились...");
        }

        /// <summary>
        /// Запись матрицы и вектора в файлы
        /// </summary>
        static void WriteData()
        {
            try
            {
                if (matrix != null && vector != null)
                {
                    Console.WriteLine("Введите название файла, в который необходимо записать матрицу...");
                    FileStream.WriteMatrix(Console.ReadLine(), matrix);
                    Console.WriteLine("Введите название файла, в который необходимо записать вектор...");
                    FileStream.WriteVector(Console.ReadLine(), vector);
                }
                else
                    Console.WriteLine("Запись невозможна! Данные о матрице и векторе отсутствуют...");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                WriteData();
            }
        }

        /// <summary>
        /// Запись вектора в файл
        /// </summary>
        /// <param name="vector"></param>
        static void WriteVector(Vector vector)
        {
            try
            {
                Console.WriteLine("Введите название файла, в который необходимо записать вектор...");
                FileStream.WriteVector(Console.ReadLine(), vector);
                Console.WriteLine("Успешная запись!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
                WriteVector(vector);
            }
        }

        /// <summary>
        /// Чтение матрицы и вектора с файла
        /// </summary>
        static void ReadData()
        {
            try
            {
                Console.WriteLine("Введите название файла, из которого необходимо получить матрицу...");
                matrix = FileStream.ReadMatrix(Console.ReadLine());
                Console.WriteLine("Введите название файла, из которого необходимо получить вектор...");
                vector = FileStream.ReadVector(Console.ReadLine());
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
                ReadData();
            }
        }

        /// <summary>
        /// Запуск распределённого решения СЛАУ
        /// </summary>
        static void StartDistributedSolution()
        {
            if (server != null && matrix != null && vector != null)
            {
                server.IsConnectedClients();
                if (server.Clients.Count < 2)
                {
                    Console.WriteLine("Клиентов мало (меньше двух)! Переподключите нужной количесво клиентов...");
                }
                else
                {
                    if (server.Clients.Count <= vector.Size)
                    {
                        distributedSolution = new DistributedSolution(server.Clients);
                        startTime = Stopwatch.StartNew();
                        distributedSolution.InitParameters(matrix, vector);
                        distributedSolution.Run();

                        startTime.Stop();

                        resultTime = startTime.Elapsed;
                        OutputTime(resultTime);

                        OutputVectorResult(distributedSolution.VectorResult);
                    }
                    else
                    {
                        Console.WriteLine("Для правильных расчётов размерность матрицы должна быть больше или равна количеству клиентов! Уменьшите количество подключений клиентов или увеличьте размерность матрицы...");
                    }
                }
            }
            else
            {
                if (server == null)
                    Console.WriteLine("Данные о клиентах и сервере неизвестны! Клиенты ещё не подключились...");
                if (matrix == null && vector == null)
                    Console.WriteLine("Распределённое решение невозможно! Данные о матрице и векторе отсутствуют...");
            }
        }

        /// <summary>
        /// Вывод результата
        /// </summary>
        /// <param name="vector"></param>
        static void OutputVectorResult(Vector vector)
        {
            if (vector.Size >= 1000)
            {
                while (true)
                {
                    Console.WriteLine("Количество полученных неизвестных равна или превышает 1000");
                    int caseSwitch = ChoiceOutputResult();
                    switch (caseSwitch)
                    {
                        case 1:
                            WriteVector(vector);
                            return;
                        case 2:
                            vector.Output("полученных неизвестных");
                            return;
                        case 3:
                            return;
                    }
                }
            }
            else
                vector.Output("полученных неизвестных");
        }

        /// <summary>
        /// Выбор действия для результата
        /// </summary>
        /// <returns></returns>
        static int ChoiceOutputResult()
        {
            Console.WriteLine("1. Сохранить данные в файл.");
            Console.WriteLine("2. Всё равно вывести результат.");
            Console.WriteLine("3. Проигнорировать.");
            Console.WriteLine("\n");
            Console.WriteLine("Выберите номер меню: ");
            int index = CheckInputData(Console.ReadLine(), 0);
            return index;
        }

        /// <summary>
        /// Запуск линейного решения СЛАУ
        /// </summary>
        static void StartLinearSystem()
        {
            if (matrix != null && vector != null)
            {
                startTime = Stopwatch.StartNew();

                linearSystem = new LinearSystem(matrix, vector);
                linearSystem.GaussSolve();

                startTime.Stop();
                resultTime = startTime.Elapsed;
                OutputTime(resultTime);
                OutputVectorResult(linearSystem.xVector);

            }
            else
                Console.WriteLine("Линейное решение невозможно! Данные о матрице и векторе отсутствуют...");
        }

        /// <summary>
        /// Генерация матрицы и вектора
        /// </summary>
        static void GenerateData()
        {
            Console.WriteLine("Укажите размерность?");
            int size = CheckInputData(Console.ReadLine(), 2);
            if (size == 0)
                Console.WriteLine("Размерность матрицы и вектора введены неверно! Размер должен быть не меньше трёх...");
            else
            {
                matrix = new Matrix(size, size);
                matrix.GenerateMatrix();
                vector = new Vector(size);
                vector.GenerateVector();
            }
        }

        /// <summary>
        /// Удаление клиента
        /// </summary>
        static void DeleteClient()
        {
            if (server != null)
            {
                server.IsConnectedClients();
                Console.WriteLine("Какого клиента удалить (укажите номер)?");
                int numberClient = int.Parse(Console.ReadLine());
                server.DisconnectClient(numberClient);
            }
            else
                Console.WriteLine("Удаление клиентов невозможна! Клиенты не присоединились к серверу...");
        }

        /// <summary>
        /// Добавление клиентов
        /// </summary>
        static void AddClients()
        {
            Console.WriteLine("Сколько клиентов ожидать на подключение?");

            int countClients = CheckInputData(Console.ReadLine(), 1);
            if (countClients == 0)
                Console.WriteLine("Количество клиентов было введено неверно! Клиентов должно быть не меньше двух...");
            else
            {
                if (server != null)
                {
                    server.DisconnectAllClient();
                }
                server = new Server();
                server.Bind();
                server.ConnectClients(countClients);
            }
        }

        /// <summary>
        /// Проверка введённых чисел
        /// </summary>
        /// <param name="value"></param>
        /// <param name="numberCheck"></param>
        /// <returns></returns>
        private static int CheckInputData(string value, int numberCheck)
        {
            int number = 0;
            bool success = Int32.TryParse(value, out number);
            if (success)
                if (number > numberCheck)
                    number = int.Parse(value);
                else
                    number = 0;
            return number;
        }

        /// <summary>
        /// Вывод времени выволнения расчётов
        /// </summary>
        /// <param name="time"></param>
        private static void OutputTime(TimeSpan time)
        {
            string timeSolve = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
            time.Hours,
            time.Minutes,
            time.Seconds,
            time.Milliseconds);
            Console.WriteLine("Время выполнения: \n{0:N2} секунд.", timeSolve);
        }
    }
}



