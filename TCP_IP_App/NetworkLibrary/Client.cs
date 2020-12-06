using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace NetworkLibrary
{
    public class Client
    {
        public Socket Socket { get; set; }

        public Client(Socket socket)
        {
            this.Socket = socket;
        }

        public byte[] Receive(byte[] buffer)
        {
            Console.WriteLine(buffer.Length);
            Socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
            return buffer;
        }

        public void Send(byte[] buffer)
        {
            //Console.WriteLine(buffer.Length);
            Socket.Send(buffer);
        }
    }
}
