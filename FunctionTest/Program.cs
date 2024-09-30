using System.Net.Sockets;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace FunctionTest;

public static class Program
{
    public static void Main(string[] args)
    {
        var socketServerListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        socketServerListener.Bind(new IPEndPoint(IPAddress.Any, 7899));

        socketServerListener.Listen(100);

        Console.WriteLine("Server is running...");
        while (true)
        {
            var handle = socketServerListener.Accept();
            Console.WriteLine("Client connected...");

            var clientHandelThead = new Thread(() => HandleClient(handle));
            clientHandelThead.Start();
        }
    }

    private static void HandleClient(object obj)
    {
        var buffer = new byte[1024];
        var handle = (Socket)obj;
        while (true)
        {if (handle.Poll(1000, SelectMode.SelectRead) && handle.Available == 0)
            {
                Console.WriteLine("客户端断开连接");
                handle.Close();
                break;
            }
            var length = handle.Receive(buffer);
            Console.WriteLine($"Receive message: {Encoding.UTF8.GetString(buffer, 0, length)}");

            var msg = "Hello, I'm server."u8.ToArray();
            handle.Send(msg);
        }
    }
}