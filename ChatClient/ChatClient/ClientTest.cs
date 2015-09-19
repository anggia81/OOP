using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class ClientTest : Form
    {
        byte[] byteData = new byte[1024];
        public Socket clientSocket;

        public ClientTest()
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
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, Convert.ToInt32(txtPort.Text));

                //Connect to the server
                clientSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);


                byteData = new byte[1024];
                //Start listening to the data asynchronously
                clientSocket.BeginReceive(byteData,
                                           0,
                                           byteData.Length,
                                           SocketFlags.None,
                                           new AsyncCallback(OnReceive),
                                           null);
                btnOK.Enabled = false;
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

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ChatClient", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ConvertByteToString(byte[] data)
        {
            int msgLen = BitConverter.ToInt32(data, 0);
            string strMessage;

            //This checks for a null message field
            if (msgLen > 0)
                strMessage = Encoding.UTF8.GetString(data, 4, msgLen);
            else
                strMessage = null;

            return strMessage;

        }

        private byte[] ConvertStringToByte(string strMessage)
        {
            List<byte> result = new List<byte>();


            //Length of the message
            if (strMessage != null)
                result.AddRange(BitConverter.GetBytes(strMessage.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //And, lastly we add the message text to our array of bytes
            if (strMessage != null)
                result.AddRange(Encoding.UTF8.GetBytes(strMessage));

            return result.ToArray();

        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndReceive(ar);


                string msgReceived = ConvertByteToString(byteData);


                txtChatBox.Text += msgReceived + "\r\n";


                byteData = new byte[1024];
                //Start listening to the data asynchronously
                clientSocket.BeginReceive(byteData,
                                           0,
                                           byteData.Length,
                                           SocketFlags.None,
                                           new AsyncCallback(OnReceive),
                                           null);

            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ChatClient: " , MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                //Fill the info for the message to be send
                string msgToSend;

                
                msgToSend= txtMessage.Text;


                byte[] byteData = ConvertStringToByte(msgToSend);

                //Send it to the server
                clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);

                //txtMessage.Text = null;
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to send message to the server.", "ChatClient: ", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
        private void ClientTest_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;

        
        }

        
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.ShowDialog();
            if (dlg.ShowDialog()== DialogResult.OK )
            {
                string fileName;
                fileName = dlg.FileName;
                txtFile.Text = fileName;
                btnSend2.Enabled = true;
            }
        }

        private void btnSend2_Click(object sender, EventArgs e)
        {
            List<string> fileLines = new List<string>();
            StreamReader reader;
            using (reader = new StreamReader(txtFile.Text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lblTemp.Text  += line + "\r\n";
                    fileLines.Add(line);
                }
            }
            //Fill the info for the message to be send
            string msgToSend;
            msgToSend = lblTemp.Text;
            byte[] byteData = ConvertStringToByte(msgToSend);

            //Send it to the server
            clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);

            
        }

        private void txtMessage_TextChanged(object sender, EventArgs e)
        {
            btnSend.Enabled  = true;
        }

        
    }
}
