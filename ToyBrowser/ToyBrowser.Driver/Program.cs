using System;
using System.Text;
using System.Net.Sockets;

namespace ToyBrowser.Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get a URL and parse it
            Uri uri = new Uri("http://roger.lol/debug.html");

            var scheme = uri.Scheme;
            var host = uri.Host;
            var path = uri.PathAndQuery;

            Console.WriteLine($"Scheme: {scheme}, Host: {host}, Path: {path}");

            // Build out a simple HTTP request
            string request = $"GET {path} HTTP/1.0\r\nHost: {host}\r\n\r\n";
            Console.WriteLine(request);

            // Establish a new socket connection.
            // Warning! Not much error handling here.
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            s.Connect(host, 80);

            byte[] requestAsBuffer = Encoding.ASCII.GetBytes(request);
            int bytesSent = s.Send(requestAsBuffer);

            if(bytesSent == 0)
            {
                // Nothing to do
                Environment.Exit(100);
            }

            // Receive 256 bytes at a time and append the response to a response string
            int receiveBytesPerCall = 256;
            byte[] receivedBuffer = new byte[receiveBytesPerCall];

            int receivedBytes = s.Receive(receivedBuffer, receiveBytesPerCall, SocketFlags.None);
            StringBuilder response = new StringBuilder();
            while(receivedBytes > 0)
            {
                response.Append(Encoding.UTF8.GetString(receivedBuffer, 0, receivedBytes));
                receivedBytes = s.Receive(receivedBuffer, receiveBytesPerCall, SocketFlags.None);
            }

            Console.WriteLine();
            Console.WriteLine(response.ToString());
            Console.ReadKey();

            // Disconnect the socket, and don't need to reuse it, because we are exiting the app.
            Console.WriteLine();
            Console.WriteLine("Disconnected.");
            s.Disconnect(false);
        }
    }
}
