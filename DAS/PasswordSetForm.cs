using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
using System.Windows.Forms;

namespace ODS
{
    public partial class PasswordSetForm : Form
    {       
        
        private Password Pass = new Password();

        public PasswordSetForm()
        {
            InitializeComponent();
            label1.Parent = pictureBox1;
            label3.Parent = pictureBox1;
        }

        private void PasswordSetForm_Load(object sender, EventArgs e)
        {
            textBox2.Focus();
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)ConsoleKey.Enter)
            {
                string newpass;

                newpass = textBox2.Text;

                Pass.SetPassword = newpass;
                Pass.SavePasswird();
                this.Close();
            }
            return;
        }

        private void imageButton1_Click(object sender, EventArgs e)
        {
            string newpass;

            newpass = textBox2.Text;

            Pass.SetPassword = newpass;
            Pass.SavePasswird();

            this.Close();
            return;
        }

        private void imageButton2_Click(object sender, EventArgs e)
        {
            this.Close();
            return;
        }
    }
}
