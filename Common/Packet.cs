using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [Serializable]
    public enum MessageType
    {
        CONNECT,
        PRIVATE_CHAT,
        BROADCAST,
        DISCONNECT
    }
    
    [Serializable]
    public class Packet
    {
        public string FromUser { get; set; }
        public string ToUser { get; set; }
        public MessageType MessageType { get; set; }
        public string Message { get; set; }
        public List<string> UsersConnected { get; set; }

        public Packet()
        {
            UsersConnected = new List<string>();
        }
    }
}
