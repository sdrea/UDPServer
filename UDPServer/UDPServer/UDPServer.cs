using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace UDPServer
{
    class User
    {
        public string name = "";
        public IPEndPoint ip = new IPEndPoint(0, 0);

        public User(string a, IPEndPoint b)
        {
            name = a;
            ip = b;
        }

    }

    enum UDPCommands //4 bytes
    {
        CONNECT,
        HEARTBEAT,
        MESSAGE
    }

    class UDPServer
    {
        static public LinkedList<User> list = new LinkedList<User>();

        static void OnUdpData(IAsyncResult result)
        {
            UdpClient socket = result.AsyncState as UdpClient;
            IPEndPoint source = new IPEndPoint(IPAddress.Any, 51600);
            try
            {
                byte[] message = socket.EndReceive(result, ref source);
                bool conn = false;
                foreach (var item in list)
                {
                    if (source.Equals(item.ip)) conn = true;
                }

                string rec_name = Encoding.Default.GetString(message, 0, 8).TrimEnd();
                UDPCommands rec_command = (UDPCommands)BitConverter.ToInt32(message, 8);

                Console.WriteLine("Received data from...");
                Console.WriteLine("Name=" + rec_name);
                Console.WriteLine("Command=" + rec_command.ToString());

                if (!conn)
                {
                    var a = new User("test", source);
                    list.AddLast(a);
                    Console.WriteLine("Adding connection to list...");
                }

                if (rec_command == UDPCommands.MESSAGE)
                {
                    byte[] rec_message = new byte[rec_name.Length + 2 + message.Length - 12];
                    byte[] padd = Encoding.Default.GetBytes(": ");
                    Buffer.BlockCopy(message, 0, rec_message, 0, rec_name.Length);
                    Buffer.BlockCopy(padd, 0, rec_message, rec_name.Length, 2);
                    Buffer.BlockCopy(message, 12, rec_message, rec_name.Length + 2, message.Length - 12);
                    Console.WriteLine(Encoding.Default.GetString(rec_message) + " (from " + source.Address + ":" + source.Port + ")");

                    foreach (var item in list)
                    {
                        socket.Send(rec_message, rec_message.Length, item.ip);
                    }
                }
            }
            catch
            {

            }

            socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
        }

        static void Main(string[] args)
        {

            UdpClient socket = new UdpClient(51600);
            socket.BeginReceive(new AsyncCallback(OnUdpData), socket);

            bool endloop = false;
            while (!endloop)
            {
                var input = Console.ReadLine();
                switch (input)
                {
                    case "exit":
                        endloop = true;
                        break;
                    case "users":
                        Console.WriteLine("Listing connected users...");
                        foreach (var item in list)
                        {
                            Console.WriteLine(item.ip.Address + ":" + item.ip.Port);
                        }
                        break;
                    default:
                        Console.WriteLine("Type \"exit\" to terminate the server.");
                        break;
                }
            }
        }
    }
}
