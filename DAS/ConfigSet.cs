using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ODS
{
    public partial class Config : Form
    {
        MyInterface mControl;
        private __CanControl.__CanDevice__ CanDeivce = new __CanControl.__CanDevice__();

        public Config()
        {
            InitializeComponent();
        }

        public Config(MyInterface mControl)
        {
            InitializeComponent();
            this.mControl = mControl;
        }

        ODSPublic Ods = new ODSPublic();

        public void create_folder()
        {
            string path = "SYSTEM";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return;
        }

        public void read_model()
        {            
            txt_serverIP.Text = Ods.Read_SET("Config", "B_WARR CHK");    //Server IP

            txt_serverPort.Text = Ods.Read_SET("Config", "B_SENS CHK");    //Server port

            txt_clientIP.Text = Ods.Read_SET("Config", "WCS CHK");       //Client IP

            
            txt_clientPort.Text = Ods.Read_SET("Config", "chk_SET");    //Client port

            cmbox_line.Text = Ods.Read_SET("Config", "chk_M1");    //라인번호

            cmbox_wini.Text = Ods.Read_SET("Config", "chk_M2");    //장비번호

            comboBox12.Text = Ods.Read_SET("Config", "ID_SET");  //MT4Y Port buadrate
            comboBox11.Text = Ods.Read_SET("Config", "ID_M1");    //MT4Y Port port
            textBox6.Text = Ods.Read_SET("Config", "ID_M2");    //MT4Y scan time
            comboBox3.Text = Ods.Read_SET("Config", "ID_P1");    //servo port buadrate
            comboBox2.Text = Ods.Read_SET("Config", "ID_P2");    //servo port number

            textBox1.Text = Ods.Read_SET("Config", "STB_SET"); //servo scan time
            ID_SET.Text = Ods.Read_SET("Config", "STB_M1");      //CAN NUMBER
            comboBox5.Text = Ods.Read_SET("Config", "STB_M2");      //Loadcell Port buadrate
            comboBox4.Text = Ods.Read_SET("Config", "STB_P1");      //Loadcell Port number
            textBox4.Text = Ods.Read_SET("Config", "STB_P2");      //Loadcell scan time

            comboBox1.SelectedIndex = Convert.ToInt32(Ods.Read_SET("Config", "X_DIR"));
            comboBox6.SelectedIndex = Convert.ToInt32(Ods.Read_SET("Config", "Y_DIR"));
            comboBox7.SelectedIndex = Convert.ToInt32(Ods.Read_SET("Config", "Z_DIR"));

            textBox2.Text = Ods.Read_SET("Config", "X_ONECYCELTOPULSE");
            textBox3.Text = Ods.Read_SET("Config", "Y_ONECYCELTOPULSE");
            textBox5.Text = Ods.Read_SET("Config", "Z_ONECYCELTOPULSE");
            textBox7.Text = Ods.Read_SET("Config", "MODEL TEXT");
            
            textBox8.Text = Ods.Read_SET("Config", "SBR OFFSET STOKE");
            textBox9.Text = Ods.Read_SET("Config", "SBR OFFSET LOAD");
            textBox11.Text = Ods.Read_SET("Config", "ODS OFFSET STOKE");
            textBox10.Text = Ods.Read_SET("Config", "ODS OFFSET LOAD");
                        
            string aa;

            aa = Ods.Read_SET("TEST SPEC", "chk_P2");    //NOTE PC
            if (aa == "1")
            {
                chk_SET.Checked = true;
            }
            else
            {
                chk_SET.Checked = false;
            }

            if (chk_SET.Checked == true)
            {
                checkBox6.Visible = true;
                aa = Ods.Read_SET("TEST SPEC", "NotLoadCheck");    //노트 PC 일때 무부하 검사 유무
                if (aa == "1")
                {
                    checkBox6.Checked = true;
                }
                else
                {
                    checkBox6.Checked = false;
                }
            }
            else
            {
                checkBox6.Visible = false;
                checkBox6.Checked = false;
            }

            aa = Ods.Read_SET("CAN DEVICE", "CHANNEL");
            if (short.TryParse(aa, out CanDeivce.Channel) == false) CanDeivce.Channel = 0;
            aa = Ods.Read_SET("CAN DEVICE", "DEVICE");
            if (short.TryParse(aa, out CanDeivce.Device) == false) CanDeivce.Device = 0;
            aa = Ods.Read_SET("CAN DEVICE", "ID");
            if (short.TryParse(aa, out CanDeivce.ID) == false) CanDeivce.ID = 0;
            aa = Ods.Read_SET("CAN DEVICE", "SPEED");
            if (short.TryParse(aa, out CanDeivce.Speed) == false) CanDeivce.Speed = 0;

            return;       
        }

      

        private void OptSetting_Load(object sender, EventArgs e)
        {
            comboBox8.Items.Clear();
            comboBox9.Items.Clear();
            
            string Num;

            comboBox1.Items.Clear();
            for (int i = 1; i <= 100; i++)
            {
                Num = (i / 100F).ToString("0.00");
                comboBox1.Items.Add(Num);
            }


            if (mControl.GetCanReWrite != null)
            {
                string[] Device = mControl.GetCanReWrite.GetDevice;

                foreach (string sx in Device)
                {
                    comboBox8.Items.Add(sx);
                }
            }


            comboBox9.Items.Add("5K");
            comboBox9.Items.Add("10K");
            comboBox9.Items.Add("20K");
            comboBox9.Items.Add("33K");
            comboBox9.Items.Add("47K");
            comboBox9.Items.Add("50K");
            comboBox9.Items.Add("83K");
            comboBox9.Items.Add("95K");
            comboBox9.Items.Add("100K");
            comboBox9.Items.Add("125K");
            comboBox9.Items.Add("250K");
            comboBox9.Items.Add("500K");
            comboBox9.Items.Add("800K");
            comboBox9.Items.Add("1M");

            read_model();

            string s;
            string s1;
            string s2;

            //s1 = "(0x" + mControl.GetConfig.Can.Device.ToString("X2") + ")";
            s1 = "Device=" + CanDeivce.Device.ToString();
            s2 = "Channel=" + CanDeivce.Channel.ToString() + "h";


            for (int i = 0; i < comboBox8.Items.Count; i++)
            {
                s = comboBox8.Items[i].ToString();

                if (0 <= s.IndexOf(s1))
                {
                    if (0 <= s.IndexOf(s2))
                    {
                        comboBox8.SelectedIndex = i;
                        break;
                    }
                }
            }


            if ((0 <= CanDeivce.Speed) && (0 < comboBox9.Items.Count) && (CanDeivce.Speed < comboBox9.Items.Count)) comboBox9.SelectedIndex = CanDeivce.Speed;
            return;
        }

   

        private void chk_SET_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_SET.Checked == true)
                    checkBox6.Visible = true;
            else    checkBox6.Visible = false;

            if (chk_SET.Checked == true)
                    groupBox10.Text = "SCANNER";
            else    groupBox10.Text = "MT4Y Meter";

            return;
        }

        private void imageButton1_Click(object sender, EventArgs e)
        {
            create_folder(); // 폴더 유무 확인


            Ods.Write_SET("Config", "B_WARR CHK", txt_serverIP.Text);   //Server IP

            Ods.Write_SET("Config", "B_SENS CHK", txt_serverPort.Text);   //Server port

            Ods.Write_SET("Config", "WCS CHK", txt_clientIP.Text);      //Client IP


            Ods.Write_SET("Config", "chk_SET", txt_clientPort.Text);  //Client port

            Ods.Write_SET("Config", "chk_M1", cmbox_line.Text);   //라인번호

            Ods.Write_SET("Config", "chk_M2", cmbox_wini.Text);   //장비번호

            Ods.Write_SET("Config", "ID_SET", comboBox12.Text);  //MT4Y Port buadrate
            Ods.Write_SET("Config", "ID_M1", comboBox11.Text);    //MT4Y Port number
            Ods.Write_SET("Config", "ID_M2", textBox6.Text);    //MT4Y scan time
            Ods.Write_SET("Config", "ID_P1", comboBox3.Text);    //servo port buadrate
            Ods.Write_SET("Config", "ID_P2", comboBox2.Text);    //servo Port number

            Ods.Write_SET("Config", "STB_SET", textBox1.Text); //servo scan time
            Ods.Write_SET("Config", "STB_M1", ID_SET.Text);      //CAN NUMBER
            Ods.Write_SET("Config", "STB_M2", comboBox5.Text);   //loadcell port buadrate
            Ods.Write_SET("Config", "STB_P1", comboBox4.Text);   //loadcell port number
            Ods.Write_SET("Config", "STB_P2", textBox4.Text);    //loadcell scan time

            Ods.Write_SET("Config", "X_DIR", comboBox1.SelectedIndex.ToString());
            Ods.Write_SET("Config", "Y_DIR", comboBox6.SelectedIndex.ToString());
            Ods.Write_SET("Config", "Z_DIR", comboBox7.SelectedIndex.ToString());

            Ods.Write_SET("Config", "X_ONECYCELTOPULSE", textBox2.Text);
            Ods.Write_SET("Config", "Y_ONECYCELTOPULSE", textBox3.Text);
            Ods.Write_SET("Config", "Z_ONECYCELTOPULSE", textBox5.Text);
            Ods.Write_SET("Config", "MODEL TEXT", textBox7.Text);

            Ods.Write_SET("Config", "SBR OFFSET STOKE", textBox8.Text);
            Ods.Write_SET("Config", "SBR OFFSET LOAD", textBox9.Text);
            Ods.Write_SET("Config", "ODS OFFSET STOKE", textBox11.Text);
            Ods.Write_SET("Config", "ODS OFFSET LOAD", textBox10.Text);

            if (chk_SET.Checked == true)
            {
                Ods.Write_SET("Config", "chk_P2", "1");    //NOTE pc

                Ods.Write_SET("Config", "NotLoadCheck", checkBox6.Checked == true ? "1" : "0");    //노트 PC 일때 무부하 검사 유무
            }
            else
            {
                Ods.Write_SET("Config", "chk_P2", "0");
                Ods.Write_SET("Config", "NotLoadCheck", "0");    //노트 PC 일때 무부하 검사 유무
            }

            if (comboBox8.SelectedItem != null)
            {
                if (0 <= comboBox8.Items.Count)
                {
                    string[] s = comboBox8.Text.Split(':');
                    string[] s2 = s[1].Split(',');

                    string ss1 = s2[0].Substring(s2[0].IndexOf("Device=") + "Device=".Length);
                    string ss2 = s2[2].Substring(s2[2].IndexOf("Channel=") + "Channel=".Length);

                    ss2 = ss2.Replace("h", null);

                    if (short.TryParse(ss1, out CanDeivce.Device) == false) CanDeivce.Device = -1;
                    if (short.TryParse(ss2, out CanDeivce.Channel) == false) CanDeivce.Channel = -1;


                    ss1 = s2[1].Substring(s2[1].IndexOf("ID=") + "ID=".Length);

                    ss2 = ss1.Replace("(", null);
                    ss2 = ss2.Replace(")", null);
                    CanDeivce.ID = (short)mControl.PublicFunction.StringToHex(ss2);
                }
            }

            if (comboBox9.SelectedItem != null)
            {
                if (0 <= comboBox9.SelectedIndex) CanDeivce.Speed = (short)comboBox9.SelectedIndex;
            }
            Ods.Write_SET("CAN DEVICE", "CHANNEL", CanDeivce.Channel.ToString());
            Ods.Write_SET("CAN DEVICE", "DEVICE", CanDeivce.Device.ToString());
            Ods.Write_SET("CAN DEVICE", "ID", CanDeivce.ID.ToString());
            Ods.Write_SET("CAN DEVICE", "SPEED", CanDeivce.Speed.ToString());
            return;
        }

        private void imageButton2_Click(object sender, EventArgs e)
        {
            this.Close();
            return;
        }
    }
}
