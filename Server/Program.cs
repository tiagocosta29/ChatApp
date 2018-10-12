using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace TcpServer
{
    class Program
    {
        static Dictionary<string, TcpClient> clientsConnected;

        static void Main(string[] args)
        {
            int port = args.Length == 1 ? int.Parse(args[0]) : 7;
            clientsConnected = new Dictionary<string, TcpClient>();
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
                        Packet packet = await ReadPacketAsync(netStream);
                        if (packet != null)
                        {
                            if(!clientsConnected.ContainsKey(packet.FromUser))
                            {
                                clientsConnected.Add(packet.FromUser, client);                               
                            }

                            switch (packet.MessageType)
                            {
                                case MessageType.CONNECT:
                                    NotifyClients();
                                    break;
                                case MessageType.PRIVATE_CHAT:
                                    SendMessageToClient(packet);
                                    break;
                                case MessageType.BROADCAST:
                                    break;
                                case MessageType.DISCONNECT:
                                    if(clientsConnected.ContainsKey(packet.FromUser))
                                    {
                                        clientsConnected.Remove(packet.FromUser);
                                    }
                                    Console.WriteLine("USER DISCONNECTED " + packet.FromUser);
                                    NotifyClients();
                                    break;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred");
            }
        }

        private static void SendMessageToClient(Packet packet)
        {
            Task.Run(() =>
            {
                TcpClient client = clientsConnected[packet.ToUser];
                if (client.Connected)
                {
                    try
                    {
                        var netstream = client.GetStream();
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(netstream, packet);

                        Console.WriteLine("SENT MESSAGE : " + packet.Message);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("COULD NOT SEND MESSAGE TO CLIENT");
                    }
                }
            });
        }

        private static void NotifyClients()
        {
            List<string> usersToRemove = new List<string>();
            foreach (string user in clientsConnected.Keys)
            {
                TcpClient client = clientsConnected[user];
                if (client.Connected)
                {
                    try
                    {
                        var netStream = client.GetStream();
                        Packet newPacket = new Packet();
                        newPacket.MessageType = MessageType.BROADCAST;
                        newPacket.FromUser = user;
                        newPacket.UsersConnected = clientsConnected.Keys.ToList();
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(netStream, newPacket);
                    }
                    catch (Exception)
                    {
                        usersToRemove.Add(user);
                        throw;
                    }
                }
                else
                {
                    usersToRemove.Add(user);
                }
            }
        }

        private static Task<Packet> ReadPacketAsync(NetworkStream netStream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return Task.Run(() => (Packet)formatter.Deserialize(netStream));
        }

    }
}
