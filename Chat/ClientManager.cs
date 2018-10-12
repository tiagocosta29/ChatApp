using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    public class ClientManager
    {
        public string Username { get; set; }
        public Queue<Packet> Messages { get; set; }
        private TcpClient client;
        private NetworkStream networkStream;

        public ClientManager(string username)
        {
            Username = username;
            Messages = new Queue<Packet>();
        }

        public async Task<bool> Connect(string host, int port)
        {
            client = new TcpClient();

            try
            {
                await client.ConnectAsync(IPAddress.Parse(host), port);
                if (client.Connected)
                {
                    networkStream = client.GetStream();
                    Packet packet = new Packet();
                    packet.MessageType = MessageType.CONNECT;
                    packet.FromUser = Username;

                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(networkStream, packet);

                    ReadMessages();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void ReadMessages()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (networkStream.DataAvailable)
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        var dataFromServer = formatter.Deserialize(networkStream);
                        if (dataFromServer != null)
                        {
                            Messages.Enqueue((Packet)dataFromServer);
                        }
                    }
                }
            });
        }

        public void SendMessate(Packet packet)
        {
            Task.Run(() =>
            {
                try
                {
                    if (client.Connected)
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(networkStream, packet);
                    }
                }
                catch (Exception)
                {

                }
            });
        }

        public void Disconnect()
        {
            try
            {
                if (client.Connected)
                {
                    Packet packet = new Packet();
                    packet.MessageType = MessageType.DISCONNECT;
                    packet.FromUser = Username;

                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(networkStream, packet);

                }
                if (client != null)
                {
                    client.Close();
                }

                if (networkStream != null)
                {
                    networkStream.Close();
                }
            }
            catch (Exception)
            {
                
            }

        }
    }
}
