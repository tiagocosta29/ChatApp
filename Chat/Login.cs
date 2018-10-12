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
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            if (!txtUsername.Text.Trim().Equals(""))
            {
                ClientManager clientManger = new ClientManager(txtUsername.Text);
                bool connected = await clientManger.Connect("127.0.0.1", 7);

                if(connected)
                {
                    ConnectedUsers connectedForm = new ConnectedUsers(clientManger);
                    connectedForm.Show();
                    Hide(); 
                }
                else
                {
                    MessageBox.Show("Could not connect to the server");
                }
            }
        }
    }
}
