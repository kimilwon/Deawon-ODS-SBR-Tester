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
    public partial class PasswordCheckForm : Form
    {
        bool Result;
        private Password Pass = new Password();

        public PasswordCheckForm()
        {
            InitializeComponent();
            label1.Parent = pictureBox1;
            label2.Parent = pictureBox1;
        }

    
        public bool result
        {
            get { return Result; }
        }

        private void PasswordCheckForm_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
            return;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {            
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //EventArgs x;

            if (e.KeyChar == (char)ConsoleKey.Enter)
            {
                if (Pass.GetPassword == textBox1.Text)
                {
                    Result = true;
                }
                else
                {
                    if (Pass.GetMaster != textBox1.Text)
                    {
                        MessageBox.Show("입력한 암호가 일치하지 않습니다.");
                        Result = false;
                    }
                    else
                    {
                        Result = true;
                    }
                }
                this.Close();
            }
            return;
        }

     
        private void imageButton1_Click(object sender, EventArgs e)
        {
            if (Pass.GetPassword == textBox1.Text)
            {
                Result = true;
            }
            else
            {
                if (Pass.GetMaster != textBox1.Text)
                {
                    MessageBox.Show("입력한 암호가 일치하지 않습니다.");
                    Result = false;
                }
                else
                {
                    Result = true;
                }
            }
            this.Close();
            return;
        }

        private void imageButton2_Click(object sender, EventArgs e)
        {
            Result = false;
            this.Close();
            return;
        }

        private void imageButton3_Click(object sender, EventArgs e)
        {
            if (Pass.GetPassword != textBox1.Text)
            {
                if (Pass.GetMaster != textBox1.Text)
                {
                    MessageBox.Show("입력한 암호가 일치하지 않습니다.");
                    return;
                }
            }
            PasswordSetForm PassSet = new PasswordSetForm();
            PassSet.ShowDialog();
            this.Close();
            Result = false;
            return;
        }
    }
}
