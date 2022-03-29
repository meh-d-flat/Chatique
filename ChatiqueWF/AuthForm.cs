using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatiqueWF
{
    public partial class AuthForm : Form
    {
        public AuthForm()
        {
            InitializeComponent();
        }

        private void ConnectButtonClick(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(usernameTextBox.Text) || String.IsNullOrWhiteSpace(usernameTextBox.Text))
                return;

            var chatForm = new ChatForm(usernameTextBox.Text, this);
            //Application.Run(chatForm);
            chatForm.Show();
        }
    }
}
