using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace NetworkLibrary
{
    public class Client
    {
        public Socket Socket { get; set; }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="socket"></param>
        public Client(Socket socket)
        {
            this.Socket = socket;
        }

        /// <summary>
        /// Получение данных
        /// </summary>
        /// <param name="buffer"></param>
        public void Receive(byte[] buffer)
        {
            Socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
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
