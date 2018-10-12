using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat
{
    public partial class PrivateChat : Form
    {
        private Dictionary<string, PrivateChat> windowDictionary;
        private string windowUsername;
        private string fromUsername;
        private ClientManager cliManager;        

        public PrivateChat(Dictionary<string,PrivateChat> chatWindowDictionary, string toUsername, ClientManager manager)
        {
            InitializeComponent();
            windowDictionary = chatWindowDictionary;
            windowUsername = toUsername;
            cliManager = manager;
            fromUsername = manager.Username;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            windowDictionary.Remove(windowUsername);
            base.OnClosing(e);
        }

        private void SendMessage()
        {
            Packet packet = new Packet();
            packet.Message = fromUsername + ": " + txtMessage.Text;
            txtMessage.Text = "";
            UpdateHistory(packet.Message);
            packet.MessageType = MessageType.PRIVATE_CHAT;
            packet.ToUser = windowUsername;
            packet.FromUser = fromUsername;

            cliManager.SendMessate(packet);            
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        public void UpdateHistory(string message)
        {
            richMessages.Text += "\n" + message;
        }
        
    }
}
