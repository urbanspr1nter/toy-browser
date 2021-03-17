using System;
using System.Text;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ToyBrowser.Core;

namespace ToyBrowser.Driver
{
    class Program
    {
        static string ParseBody(string body)
        {
            bool isInAngle = false;

            StringBuilder content = new StringBuilder();


            foreach (char c in body)
            {
                if (c == '<')
                {
                    isInAngle = true;
                } else if (c == '>')
                {
                    isInAngle = false;
                } else if (!isInAngle)
                {
                    // This is not in a tag
                    content.Append(c);
                }
            }

            return content.ToString();
        }

        static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        static string PerformSslRequest(string host, int port, byte[] requestBuffer)
        {
            TcpClient client = new TcpClient(host, port);
            SslStream sslStream = new SslStream(client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

            sslStream.AuthenticateAsClient(host);
            sslStream.Write(requestBuffer);

            // Receive 256 bytes at a time and append the response to a response string
            int receiveBytesPerCall = 256;
            byte[] receivedBuffer = new byte[receiveBytesPerCall];

            StringBuilder response = new StringBuilder();

            int receivedBytes = sslStream.Read(receivedBuffer, 0, receiveBytesPerCall);
            while (receivedBytes > 0)
            {
                response.Append(Encoding.UTF8.GetString(receivedBuffer, 0, receivedBytes));
                receivedBytes = sslStream.Read(receivedBuffer, 0, receiveBytesPerCall);
            }

            client.Close();

            return response.ToString();
        }

        static string PerformInsecureRequest(string host, int port, byte[] requestBuffer)
        {
            // Establish a new socket connection.
            // Warning! Not much error handling here.
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Connect(host, port);

            s.Send(requestBuffer);

            StringBuilder response = new StringBuilder();

            // Receive 256 bytes at a time and append the response to a response string
            int receiveBytesPerCall = 256;
            byte[] receivedBuffer = new byte[receiveBytesPerCall];

            int receivedBytes = s.Receive(receivedBuffer, receiveBytesPerCall, SocketFlags.None);
            while (receivedBytes > 0)
            {
                response.Append(Encoding.UTF8.GetString(receivedBuffer, 0, receivedBytes));
                receivedBytes = s.Receive(receivedBuffer, receiveBytesPerCall, SocketFlags.None);
            }

            s.Disconnect(false);

            return response.ToString();
        }

        static void Main(string[] args)
        {
            // Get a URL and parse it
            Uri uri = new Uri("https://roger.lol/index.html");

            var scheme = uri.Scheme;
            var host = uri.Host;
            var path = uri.PathAndQuery;
            var port = uri.Port != 0 ? uri.Port : (scheme.Equals("http") ? 80 : 443);

            Console.WriteLine($"Scheme: {scheme}, Host: {host}, Path: {path}");

            // Build out a simple HTTP request
            string request = $"GET {path} HTTP/1.1\r\nHost: {host}\r\nConnection: close\r\n\r\n";
            Console.WriteLine(request);


            byte[] requestAsBuffer = Encoding.ASCII.GetBytes(request);
            string response = scheme.Equals("https") 
                    ? PerformSslRequest(host, port, requestAsBuffer) 
                    : PerformInsecureRequest(host, port, requestAsBuffer);

            string[] responseLines = response.ToString().Split(Environment.NewLine);
            HttpResponse httpResponse = new HttpResponse();
            for (var i = 0; i < responseLines.Length; i++)
            {
                if (i == 0)
                {
                    httpResponse.Status = responseLines[i];
                }
                else if (responseLines[i].Length == 0)
                {
                    httpResponse.Body = responseLines[i + 1];
                    break;
                } else
                {
                    HttpHeader header = new HttpHeader();
                    header.Name = responseLines[i].Split(":")[0].Trim();
                    header.Value = responseLines[i].Split(":")[1].Trim();
                }
            }

            Console.WriteLine("HTML:");
            Console.WriteLine(httpResponse.Body);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Parsed:");
            Console.WriteLine(ParseBody(httpResponse.Body));
            Console.ReadKey();
        }
    }
}
