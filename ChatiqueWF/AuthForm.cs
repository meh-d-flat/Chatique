using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
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
            {
                loginStatusLabel.Invoke(new Action(() => loginStatusLabel.Text = "username is required"));
                return;
            }

            if (String.IsNullOrEmpty(passwordTextBox.Text) || String.IsNullOrWhiteSpace(passwordTextBox.Text))
            {
                loginStatusLabel.Invoke(new Action(() => loginStatusLabel.Text = "password is required"));
                return;
            }

            string password = passwordTextBox.Text;
            using (SHA512 sha = SHA512Managed.Create())
            {
                password = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
                password = password.Replace("-", "");
                password = password.ToLower();
            }

            var chatForm = new ChatForm(usernameTextBox.Text, password, this);
            passwordTextBox.Invoke(new Action(() => passwordTextBox.Text = String.Empty));
            usernameTextBox.Invoke(new Action(() => usernameTextBox.Text = String.Empty));
            //Application.Run(chatForm);
            chatForm.Show();

            return;
        }
    }
}
