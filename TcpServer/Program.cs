using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TcpServer
{
    class Program
    {
        static List<TcpClient> clientsConnected;
        static void Main(string[] args)
        {
            int port = args.Length == 1 ? int.Parse(args[0]) : 7;
            clientsConnected = new List<TcpClient>();
            Start(port);
            Console.ReadLine();
        }

        private async static void Start(int port)
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();

            Console.WriteLine("Server is running");
            Console.WriteLine("Listening on port " + port);

            while (true)
            {
                Console.WriteLine("Waiting for connections...");
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    HandleConnectionAsync(client);
                }
                catch (Exception)
                {
                    Console.WriteLine("Error occurred");
                }
            }
        }

        private static async void HandleConnectionAsync(TcpClient client)
        {
            string clientInfo = client.Client.RemoteEndPoint.ToString();
            Console.WriteLine("Got connection request from " + clientInfo);
            try
            {
                var netStream = client.GetStream();
                
                while (true)
                {
                    if (netStream.DataAvailable)
                    {
                        var reader = new StreamReader(netStream);
                        var writer = new StreamWriter(netStream);

                        writer.AutoFlush = true;

                        var data = await reader.ReadLineAsync();
                        if (string.IsNullOrEmpty(data)) break;
                        await writer.WriteLineAsync("From Server - " + data);

                        clientsConnected.Add(client);
                        BroadCastMessage(clientInfo, "Client " + clientInfo + " connected.");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred");
            }
            finally
            {
                //Console.WriteLine("Closing the client connection" + clientInfo);
                //client.Close();
            }
        }

        private async static void BroadCastMessage(string fromClient, string message)
        {
            foreach (TcpClient c in clientsConnected)
            {
                if (c.Client.RemoteEndPoint.ToString().Equals(fromClient))
                    return;

                var netStream = c.GetStream();
                var writer = new StreamWriter(netStream);
                writer.AutoFlush = true;
                await writer.WriteLineAsync(message);
            }
        }
    }
}
