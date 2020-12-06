using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading;
using NetworkLibrary;
using TransformationLibrary;
using MatrixLibrary;
using MathLibrary;

namespace TCP_IP_Client
{
    class ConsoleApplication
    {
        // адрес и порт сервера, к которому будем подключаться
        static int port = 8080; // порт сервера
        static string address = "127.0.0.1"; // адрес сервера

        static void Main(string[] args)
        {
            Console.WriteLine("Enter address:");

            //address = Console.ReadLine();
            try
            {
                Client client = new Client(address, port);  
                Console.WriteLine("Клиент успешно подключен!");
                DistributedSolution distributedSolution = new DistributedSolution(client);
                while (true)
                {
                    distributedSolution.GetParametersFromServer();
                    //Console.WriteLine(distributedSolution.Client.NumberClient);
                    //distributedSolution.Matrix.Output("test");
                    distributedSolution.Run();                   
           
                    Console.WriteLine("Успешное выполнение работы клиента");
                }
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.Read();
        }
    }
}
