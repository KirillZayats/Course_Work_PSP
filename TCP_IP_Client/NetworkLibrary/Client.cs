using System;
using System.Net;
using System.Net.Sockets;

namespace NetworkLibrary
{
    public class Client
    {
        public Socket Socket { get; set; }
        public int Port { get; set; }
        public string Adress { get; set; }
        public int NumberClient { get; set; }

        public Client()
        {
            Port = 8080;
            Adress = "127.0.0.1";
            Connect();
        }

        public Client(string adress, int port)
        {
            this.Port = port;
            this.Adress = adress;
            Connect();
        }

        private void Connect()
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(Adress), Port);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.Connect(ipPoint);
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
