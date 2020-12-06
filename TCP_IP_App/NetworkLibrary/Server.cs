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

        public Server()
        {
            Port = 8080;
            Adress = "0.0.0.0";
            Clients = new List<Client>();

        }

        public Server(string adress, int port)
        {
            this.Port = port;
            this.Adress = adress;
            Clients = new List<Client>();
        }
        public void Bind()
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(Adress), Port);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.Bind(ipPoint);
        }
        public void ConnectClients(int countClients)
        {
            Console.WriteLine("Ожидаем клиентов...");
            Socket.Listen(15);
            int countConnected = 0;
            while (countConnected < countClients)
            {
                Socket client = Socket.Accept();
                Console.WriteLine("Новое подключение...");
                Console.WriteLine(client.RemoteEndPoint.ToString());
                Clients.Add(new Client(client));
                countConnected++;
            }

            Socket.Close();
        }

        public void DisconnectClient(int numberClient)
        {
            if (Clients.Count > 0)
            {
                if (numberClient <  Clients.Count)
                {
                    Clients[numberClient].Socket.Disconnect(true);
                    Clients.Remove(Clients[numberClient]);
                    Console.WriteLine("Клиент под номером " + numberClient + " успешно удалён!");
                }
                else Console.WriteLine("Клиента под номером " + numberClient + " нет с списке!");
            }
            else
                Console.WriteLine("Список клиентов пуст!");
        }

        public byte[] Receive(byte[] buffer)
        {
            Socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
            return buffer;
        }

        public void Send(byte[] buffer)
        {
            Socket.Send(buffer);
        }
    }
}
