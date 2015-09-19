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

namespace ChatServer
{
    public partial class ChatTest : Form
    {
        struct ClientInfo
        {
            public Socket socket;   //Socket of the client
            public string strName;  //Name by which the user logged into the chat room
        }

        ArrayList clientList;

        Socket serverSocket;

        byte[] byteData = new byte[1024];

        public ChatTest()
        {
            clientList = new ArrayList();
            InitializeComponent();
        }

        private bool StartServer(int portNumber)
        {
            try
            {
                Control.CheckForIllegalCrossThreadCalls = false;
                //We are using TCP sockets
                serverSocket = new Socket(AddressFamily.InterNetwork,
                                          SocketType.Stream,
                                          ProtocolType.Tcp);

                //Assign the any IP of the machine and listen on port number 
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, portNumber);

                //Bind and listen on the given address
                serverSocket.Bind(ipEndPoint);
                serverSocket.Listen(4);

                //Accept the incoming clients
                serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ChatServer",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

        }

        private void OnAccept(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = serverSocket.EndAccept(ar);
                txtLog.Text += "Receive connection from : " +
                               ((System.Net.IPEndPoint) (clientSocket.LocalEndPoint)).Address + "\r\n"; 

               
                //Start listening for more clients
                serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);

                //Once the client connects then start receiving the commands from her
                clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                    new AsyncCallback(OnReceive), clientSocket);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ChatServer",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                Socket clientSocket = (Socket)ar.AsyncState;
                clientSocket.EndReceive(ar);



                string msgReceived = ConvertByteToString(byteData);
                if (msgReceived == "List")
                {
                    msgReceived = "List of File : \r\n";
                    msgReceived += "1. A.txt \r\n";
                    msgReceived += "2. B.txt \r\n";
                    
                }
                
                if (msgReceived == "1")
                {
                    msgReceived = "";
                    List<string> fileLines = new List<string>();
                    using (var reader = new StreamReader("A.txt"))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            msgReceived += line + "\r\n";
                            fileLines.Add(line);
                        }
                    }
                }
                if (msgReceived == "2")
                {
                    msgReceived = "";
                    List<string> fileLines = new List<string>();
                    using (var reader = new StreamReader("B.txt"))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            msgReceived += line + "\r\n";
                            fileLines.Add(line);
                        }
                    }
                }

                //We will send this object in response the users request
                string  msgToSend = ConvertByteToString(byteData);
                byte[] message;
                
                //Set the text of the message that we will broadcast to all users
                msgToSend = msgReceived+ ", send from server";

                message = ConvertStringToByte(msgToSend);

                //Send the name of the users in the chat room
                clientSocket.BeginSend(message, 0, message.Length, SocketFlags.None,
                        new AsyncCallback(OnSend), clientSocket);
                txtLog.Text += msgReceived + "\r\n";
          
                clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), clientSocket);
              
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ChatServer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public void OnSend(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndSend(ar);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ChatServer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            int portNumber = 0;
            try
            {
                portNumber = Convert.ToInt32(txtPort.Text);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;

            }
            if (StartServer(portNumber))
            {
                btnStart.Enabled = false;
                txtPort.Enabled = false;
                txtLog.Text = "Server started at port : " + txtPort.Text + "\r\n";
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ChatTest_Load(object sender, EventArgs e)
        {

        }

    }

    //class DataTest
    //{
    //    //Default constructor
    //    public DataTest()
    //    {
    //        this.strMessage = null;
    //    }

    //    //Converts the bytes into an object of type Data
    //    public DataTest(byte[] data)
    //    {
    //        //The next four store the length of the message
    //        int msgLen = BitConverter.ToInt32(data,0);

        
    //        //This checks for a null message field
    //        if (msgLen > 0)
    //            this.strMessage = Encoding.UTF8.GetString(data, 4 , msgLen);
    //        else
    //            this.strMessage = null;
    //    }

    //    //Converts the Data structure into an array of bytes
    //    public byte[] ToByte()
    //    {
    //        List<byte> result = new List<byte>();

        
    //        //Length of the message
    //        if (strMessage != null)
    //            result.AddRange(BitConverter.GetBytes(strMessage.Length));
    //        else
    //            result.AddRange(BitConverter.GetBytes(0));

    //        //And, lastly we add the message text to our array of bytes
    //        if (strMessage != null)
    //            result.AddRange(Encoding.UTF8.GetBytes(strMessage));

    //        return result.ToArray();
    //    }

    //    public string strMessage;   //Message text
    //} 
}
