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

        public ChatForm(string clientName, string password, AuthForm authForm)
        {
            name = clientName;
            _authForm = authForm;
            InitializeComponent();
            InitializeSocket(clientName, password);
            richTextBox1.ReadOnly = true;
            ActiveControl = textBox1;
            //_authForm.Hide();
        }

        private void InitializeSocket(string name, string password)
        {
            string[] args = Program.Args;
            string host = args.Length > 0
                ? String.Format("ws://{0}:{1}/", args[0], args[1])
                : "ws://localhost:8087/";

            socket = new WebSocket(host);

            socket.OnMessage += Socket_OnMessage;
            socket.OnError += Socket_OnError;
            socket.OnClose += Socket_OnClose;
            socket.SetCookie(new WebSocketSharp.Net.Cookie("name", name));
            socket.SetCookie(new WebSocketSharp.Net.Cookie("password", password));
            socket.Connect();

            this.Text = String.Format("joined as: {0}", name);
            _authForm.Hide();
        }

        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            if (e.Code == 1011 && e.Reason.Contains("incorrect"))
            {
                MessageBox.Show(e.Reason);
                this.Invoke(new Action(() => this.Hide()));
                _authForm.Invoke(new Action(() => _authForm.Show()));
            }
        }

        private void Socket_OnError(object sender, ErrorEventArgs e)
        {
            string exception = String.Format("{0}\n{1}", e.Message, e.Exception.Message);
            richTextBox1.Invoke(new Action(() => MessageReceived(null, richTextBox1, exception, true)));
            ScrollDown();
        }

        void MessageReceived(TextBoxBase sourceTextBox, TextBoxBase destinationTextBox, string message, bool clear)
        {
            if(clear)
                sourceTextBox.Invoke(new Action(() => sourceTextBox.Text = null));

            destinationTextBox.Invoke(new Action(() => destinationTextBox.Text += String.Format("{0}{1}", message, Environment.NewLine)));
            ScrollDown();
        }

        private void ScrollDown()
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length - 1;
            richTextBox1.ScrollToCaret();
        }

        bool SendMessage()
        {
            bool sent = false;
            try
            {
                if (textBox1.Text != null || !String.IsNullOrWhiteSpace(textBox1.Text))
                    socket.Send(textBox1.Text);
                sent = true;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                sent = false;
            }
            return sent;
        }

        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            richTextBox1.Invoke(new Action(() => MessageReceived(textBox1, richTextBox1, e.Data, false)));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            socket.Close();
            _authForm.Close();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SendMessage();
        }
    }
}
