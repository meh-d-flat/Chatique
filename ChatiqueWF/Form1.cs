using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocketSharp;

namespace ChatiqueWF
{
    public partial class ChatForm : Form
    {
        WebSocket socket;

        AuthForm _authForm;

        string name;

        public ChatForm(string clientName, AuthForm authForm)
        {
            name = clientName;
            _authForm = authForm;
            InitializeComponent();
            InitializeSocket();
            richTextBox1.ReadOnly = true;
        }

        private void InitializeSocket()
        {
            socket = new WebSocket("ws://localhost:8087/");
            socket.OnMessage += Socket_OnMessage;
            socket.Connect();

            this.Name = String.Format("joined as: {0}", name);
            _authForm.Hide();
        }

        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            richTextBox1.Invoke(new Action(() => MessageReceived(textBox1, richTextBox1, e.Data)));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                socket.Send(MakeMessage(textBox1.Text));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        void MessageReceived(TextBoxBase sourceTextBox, TextBoxBase destinationTextBox, string message)
        {
            destinationTextBox.Invoke(new Action(() => destinationTextBox.Text += String.Format("{0}{1}", message, Environment.NewLine)));
            sourceTextBox.Invoke(new Action(() => sourceTextBox.Text = null));
            
        }

        string MakeMessage(string message)
        {
            return String.Format("[{0}{1}]: {2}",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        name,
                        message);
        }

        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            socket.Close();
            _authForm.Close();
        }
    }
}
