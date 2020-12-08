using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkLibrary
{
    public class Client
    {
        public Socket Socket { get; set; }
        public int Port { get; set; }
        public string Adress { get; set; }
        public int NumberClient { get; set; }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Client()
        {
            Port = 8080;
            Adress = "127.0.0.1";
            Connect();
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="adress"></param>
        /// <param name="port"></param>
        public Client(string adress, int port)
        {
            this.Port = port;
            this.Adress = adress;
            Connect();
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="adress"></param>
        public Client(string adress)
        {
            this.Adress = adress;
            Port = 8080;
            Connect();
        }

        /// <summary>
        /// Подключение клиента к серверу
        /// </summary>
        private void Connect()
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(Adress), Port);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.Connect(ipPoint);
        }

        /// <summary>
        /// Получение данных от сервера
        /// </summary>
        /// <param name="buffer"></param>
        public void Receive(byte[] buffer)
        {
            Socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
        }

        /// <summary>
        /// Отправка данных на сервер
        /// </summary>
        /// <param name="buffer"></param>
        public void Send(byte[] buffer)
        {
            Socket.Send(buffer);
        }

        /// <summary>
        /// Проверка подключения к серверу
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            bool part1 = Socket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (Socket.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }

    }
}
