using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class LoginForm : Form
    {
        public Socket clientSocket;
        public string strName;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPAddress ipAddress = IPAddress.Parse(txtServerIP.Text);
                //Server is listening on port 
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, Convert.ToInt32 (txtPort.Text));

                //Connect to the server
                clientSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);
            }
            catch (Exception ex)
            { 
                MessageBox.Show(ex.Message, "ChatClient", MessageBoxButtons.OK, MessageBoxIcon.Error); 
             } 
        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
                strName = txtName.Text;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ChatClient", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndConnect(ar);

                //We are connected so we login into the server
                Data msgToSend = new Data ();
                msgToSend.cmdCommand = Command.Login;
                msgToSend.strName = txtName.Text;
                msgToSend.strMessage = null;

                string message = "test";
                byte[] b = msgToSend.ToByte ();
                //byte[] b = Convert.ToByte(message);
                List<byte> result = new List<byte>();
                result.AddRange(BitConverter.GetBytes(message.Length));
                byte[] c = result.ToArray();

                //Send the message to the server
                clientSocket.BeginSend(b, 0, b.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
                //clientSocket.BeginSend(c, 0, c.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            }
            catch (Exception ex)
            { 
                MessageBox.Show(ex.Message, "ChatClient", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            if (txtName.Text.Length > 0 && txtServerIP.Text.Length > 0)
                btnOK.Enabled = true;
            else
                btnOK.Enabled = false;
        }

        private void txtServerIP_TextChanged(object sender, EventArgs e)
        {
            if (txtName.Text.Length > 0 && txtServerIP.Text.Length > 0)
                btnOK.Enabled = true;
            else
                btnOK.Enabled = false;
        }
    }
}