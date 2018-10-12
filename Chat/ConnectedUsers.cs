using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat
{
    public partial class ConnectedUsers : Form
    {
        private ClientManager clientManager;
        delegate void UpdateUsersCallback(List<string> users);

        delegate void ChatWindowCallback(Packet data);
        private Dictionary<string, PrivateChat> openChat;


        public ConnectedUsers(ClientManager client)
        {
            openChat = new Dictionary<string, PrivateChat>();
            InitializeComponent();
            clientManager = client;
            lblHello.Text = $"Ola {client.Username}";
            listUsers.DoubleClick += ListUsers_DoubleClick;
            UpdateUI();
        }

        private void ListUsers_DoubleClick(object sender, EventArgs e)
        {
            if (listUsers.SelectedItem != null)
            {
                NewChatWindow(listUsers.SelectedItem.ToString());
            }
        }

        private void NewChatWindow(string username)
        {
            string chatWith = username;
            if (!openChat.ContainsKey(chatWith))
            {
                var chat = new PrivateChat(openChat, chatWith, clientManager);
                chat.Show();
                openChat.Add(chatWith, chat);
            }

            openChat[chatWith].BringToFront();

        }

        private void UpdateUI()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    ProcessPackets();
                }
            });
        }

        private void ProcessPackets()
        {
            while (clientManager.Messages.Count > 0)
            {
                Packet data = clientManager.Messages.Dequeue();
                Task.Run(() => ProcessPackets(data));
            }
        }

        private void ProcessPackets(Packet data)
        {
            switch (data.MessageType)
            {
                case MessageType.BROADCAST:
                    UpdateUsers(data.UsersConnected);
                    break;
                case MessageType.PRIVATE_CHAT:
                    ReceiveMessage(data);
                    break;
            }
        }

        private void ReceiveMessage(Packet data)
        {
            if (this.InvokeRequired)
            {
                ChatWindowCallback d = new ChatWindowCallback(ReceiveMessage);
                this.Invoke(d, new object[] { data });
            }
            else
            {
                string chatWith = data.FromUser;
                NewChatWindow(data.FromUser);        

                openChat[chatWith].UpdateHistory(data.FromUser + ": " + data.Message);
                openChat[chatWith].BringToFront();
            }

        }

        private void UpdateUsers(List<string> users)
        {
            if (listUsers.InvokeRequired)
            {
                UpdateUsersCallback d = new UpdateUsersCallback(UpdateUsers);
                this.Invoke(d, new object[] { users });
            }
            else
            {
                users.Remove(clientManager.Username);
                listUsers.Items.Clear();
                users.ForEach(x => listUsers.Items.Add(x));
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                clientManager.Disconnect();
                base.OnClosing(e);
                Environment.Exit(0);

            }
            catch (Exception)
            {
                
            }
        }

    }
    
}
