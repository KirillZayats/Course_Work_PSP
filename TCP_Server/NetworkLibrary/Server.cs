using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkLibrary
{
    public class Server
    {
        public Socket Socket { get; set; }
        public int Port { get; set; }
        public string Adress { get; set; }
        public List<Client> Clients { get; set; }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Server()
        {
            Port = 8080;
            Adress = "0.0.0.0";
            Clients = new List<Client>();

        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="adress"></param>
        /// <param name="port"></param>
        public Server(string adress, int port)
        {
            this.Port = port;
            this.Adress = adress;
            Clients = new List<Client>();
        }

        /// <summary>
        /// Связывание сокета с локальной конечной точкой
        /// </summary>
        public void Bind()
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(Adress), Port);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream , ProtocolType.Tcp);
            Socket.Bind(ipPoint);
        }

        /// <summary>
        /// Подключени клиентов
        /// </summary>
        /// <param name="countClients"></param>
        public void ConnectClients(int countClients)
        {
            if(Clients.Count != 0)
            {
                for (int i = 0; i < Clients.Count;)
                {
                    DisconnectClient(i);
                }
            }

            Console.WriteLine("Ожидаем клиентов...");
            
            Socket.Listen(15);
            int countConnected = 0;
            while (countConnected < countClients)
            {
                Socket client = Socket.Accept();
                Console.WriteLine("Новое подключение...");
                Clients.Add(new Client(client));
                Console.WriteLine("Клиент № " + Clients.Count +  " - " + client.RemoteEndPoint.ToString());

                countConnected++;
            }

            Socket.Close();
        }

        /// <summary>
        /// Вывод информации о клиентах
        /// </summary>
        public void OutputInfoClients()
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                Console.WriteLine("Клиент № " + (i + 1) + " c IP-адресом: " + Clients[i].Socket.RemoteEndPoint.ToString());
            }
        }
 
        /// <summary>
        /// Оключение клиента от сервера
        /// </summary>
        /// <param name="numberClient"></param>
        public void DisconnectClient(int numberClient)
        {
            if (Clients.Count > 0)
            {
                if (numberClient - 1 <  Clients.Count)
                {
                    if(numberClient - 1 > 1)
                    {
                        Clients[numberClient - 1].Socket.Disconnect(true);
                        Clients.Remove(Clients[numberClient - 1]);
                        Console.WriteLine("Клиент под номером " + numberClient + " успешно удалён!");
                    }
                    else
                        Console.WriteLine("Удалить клиента невозможно! Клиентов должно быть минимум два...");
                }
                else Console.WriteLine("Клиента под номером " + numberClient + " нет с списке!");
            }
            else
                Console.WriteLine("Список клиентов пуст!");
        }

        /// <summary>
        /// Отключение всех клиентов
        /// </summary>
        public void DisconnectAllClient()
        {
            for (int i = 0; i < Clients.Count;)
            {
                    Clients[i].Socket.Disconnect(true);
                    Clients.Remove(Clients[i]);
            }

        }

        /// <summary>
        /// Проверка клиентов на подключение
        /// </summary>
        public void IsConnectedClients()
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                if (!SocketConnected(Clients[i].Socket))
                {
                    Clients.Remove(Clients[i]);
                    i--;
                }
            }
        }

        /// <summary>
        /// Проверка подключения
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        bool SocketConnected(Socket client)
        {
            bool part1 = client.Poll(1000, SelectMode.SelectRead);
            bool part2 = (client.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Получение данных
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public byte[] Receive(byte[] buffer)
        {
            Socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
            return buffer;
        }

        /// <summary>
        /// Отправка данных
        /// </summary>
        /// <param name="buffer"></param>
        public void Send(byte[] buffer)
        {
            Socket.Send(buffer);
        }
    }
}
