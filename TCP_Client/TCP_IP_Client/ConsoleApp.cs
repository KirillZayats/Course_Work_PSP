using System;
using NetworkLibrary;
using MathLibrary;

namespace Console_Client
{
    class ConsoleApp
    {
        // адрес и порт сервера, к которому будем подключаться
        static string address = "127.0.0.1"; // адрес сервера

        static void Main(string[] args)
        {
            Console.WriteLine("Введите адресс сервера:");

            address = Console.ReadLine();
            try
            {
                Client client = new Client(address);  
                Console.WriteLine("Клиент успешно подключен!");
                DistributedSolution distributedSolution = new DistributedSolution(client);
                while (client.IsConnected())
                {
                    distributedSolution.GetParametersFromServer();

                    distributedSolution.Run();                   
           
                    Console.WriteLine("Успешное выполнение работы клиента");
                }
                return;
            }
            catch (Exception ex)
            {
                
                Console.WriteLine(ex.Message + "\n");
                Main(args);
            }

            Console.Read();
        }
    }
}
