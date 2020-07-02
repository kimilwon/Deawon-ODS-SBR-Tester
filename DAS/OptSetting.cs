//#define DEBUG_MODE

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
    public partial class OptSetting : Form
    {
        MyInterface mControl = null;
        //private __CanControl.__CanDevice__ Can = new __CanControl.__CanDevice__();

        public OptSetting()
        {
            InitializeComponent();
        }
        public OptSetting(MyInterface mControl)
        {
            InitializeComponent();
            this.mControl = mControl;
        }

        ODSPublic Ods = new ODSPublic();

        
        public void create_folder()
        {
            string path = "SETTING";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return;
        }

        public void read_model()
        {
            string aa;
            s_MIN3.Text = Ods.Read_SET(comboBox1.Text, "SLIDE CHK"); //Y축 검사위치1 for SBR

            s_MIN4.Text = Ods.Read_SET(comboBox1.Text, "TILT CHK");  //Y축 검사위치2 for SBR

            s_MAX4.Text = Ods.Read_SET(comboBox1.Text, "HEIGHT CHK"); //Y축 검사위치3 for SBR

            s_MIN2.Text = Ods.Read_SET(comboBox1.Text, "WARMER CHK"); //Z축 복귀위치 for SBR

            s_MAX3.Text = Ods.Read_SET(comboBox1.Text, "VENT CHK");   //Z축 검사위치 for SBR

            min_1.Text = Ods.Read_SET(comboBox1.Text, "SLIDE SECT MIN");   //X축 복귀위치 for ODS
            max_1.Text = Ods.Read_SET(comboBox1.Text, "SLIDE SECT MAX");   //X축 검사위치 for ODS

            min_2.Text = Ods.Read_SET(comboBox1.Text, "TILT SECT MIN");    //Y축 복귀위치 for ODS
            max_2.Text = Ods.Read_SET(comboBox1.Text, "TILT SECT MAX");    //Y축 검사위치1 for ODS

            min_3.Text = Ods.Read_SET(comboBox1.Text, "HEIGHT SECT MIN");  //Z축 복귀위치 for ODS
            max_3.Text = Ods.Read_SET(comboBox1.Text, "HEIGHT SECT MAX");  //Z축 검사위치 for ODS

            SB_P1.Text = Ods.Read_SET(comboBox1.Text, "SLIDE SPEC MIN");   //SBR판정값
            STB_P1.Text = Ods.Read_SET(comboBox1.Text, "SLIDE SPEC MAX");   //SBR판정값 offset

            ID_M1.Text = Ods.Read_SET(comboBox1.Text, "TILT SPEC MIN");     //Y축 검사위치2 for ODS
            STB_M2.Text = Ods.Read_SET(comboBox1.Text, "TILT SPEC MAX");    //Y축 검사위치3 for ODS

            s_MIN1.Text = Ods.Read_SET(comboBox1.Text, "HEIGHT SPEC MIN");  //X축 복귀위치 for SBR
            s_MAX2.Text = Ods.Read_SET(comboBox1.Text, "HEIGHT SPEC MAX");  //X축 검사위치 for SBR

            STB_P2.Text = Ods.Read_SET(comboBox1.Text, "VENT SPEC MIN");    //Z축 검사시간 for ODS
            s_MIN5.Text = Ods.Read_SET(comboBox1.Text, "VENT SPEC MAX");    //Z축 검사시간 for SBR
            s_MAX1.Text = Ods.Read_SET(comboBox1.Text, "B_SENS SPEC MIN");  //Y축 복귀위치 for SBR

            aa = Ods.Read_SET(comboBox1.Text, "chk_P1");    //SBR 검사 확인
            if (aa == "1") 
            { 
                chk_P1.Checked = true; 
            }
            else 
            { 
                chk_P1.Checked = false; 
            }

            if (chk_P1.Checked == true)
                panel4.Enabled = true;
            else panel4.Enabled = false;

            if (chk_P1.Checked == false)
                panel2.Enabled = true;
            else panel2.Enabled = false;

            //s_MAX5.Text = Ods.Read_SET(comboBox1.Text, "SB_M2");            //SBR Loadcell 무반응 무게값 for SBR
            //textBox5.Text = Ods.Read_SET(comboBox1.Text, "SB_P1");            //SBR 무반응 검사 복귀위치 for SBR 

            //textBox8.Text = Ods.Read_SET(comboBox1.Text, "ID_2");           //SBR Loadcell 무반응 무게값 for SBR A4형상

            textBox9.Text = Ods.Read_SET(comboBox1.Text, "ID_3");                       //Z축 교정 offset값

            return;
        }

        public void read_combo()
        {
            comboBox1.Items.Clear();
            DirectoryInfo di = new DirectoryInfo("SETTING");
            FileInfo[] fi = di.GetFiles("*.cfg");
            if (fi.Length == 0) MessageBox.Show("없음");
            else
            {
                string s = "";
                for (int i = 0; i < fi.Length; i++)
                {
                    s += fi[i].Name.ToString() + Environment.NewLine;
                    string[] t = s.Split('.');
                    comboBox1.Items.Add(t[0]);
                    s = "";
                }
            }
        }

    

        private void OptSetting_Load(object sender, EventArgs e)
        {
            if (this.DesignMode == true) return; //디자인 창 안뜨는것 해결함수라 한다.   
            read_combo();
            comboBox1.SelectedIndex = 0;
            read_model();
            DefaultServoOptOpen();
            textBox2.Text = MainForm.JogSpeed;
            
            timer1.Enabled = true;
            return;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            read_model();
            DefaultServoOptOpen();
            return;
        }

        private void DefaultServoOptOpen()
        {
            //검사 다운/상승 속도
            textBox70.Text = Ods.Read_SET(comboBox1.Text, "FIRST_UPDN_SPEED");
            //검사 속도
            textBox65.Text = Ods.Read_SET(comboBox1.Text, "TEST_SPEED");

            //ODA 검사 속도
            textBox71.Text = Ods.Read_SET(comboBox1.Text, "ODA_TEST_SPEED");
            //SBR 검사 속도
            textBox72.Text = Ods.Read_SET(comboBox1.Text, "SBR_TEST_SPEED");

            textBox1.Text = Ods.Read_SET(comboBox1.Text, "TEST_READY_POS");
            return;
        }

        
   
        private void chk_P1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox Chk = sender as CheckBox;

            if (Chk.Checked == true)
                    panel4.Enabled = true;
            else    panel4.Enabled = false;

            if (Chk.Checked == false)
                    panel2.Enabled = true;
            else    panel2.Enabled = false;
            return;
        }

        private void switchLed1_MouseDown(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Left)
            //{
            int Tag;

            Iocomp.Instrumentation.Standard.SwitchLed Led = sender as Iocomp.Instrumentation.Standard.SwitchLed;
            Led.Value.AsBoolean = true;

            Tag = Convert.ToInt32(Led.Tag.ToString());

            PositionRead(Tag);
            //}
            return;
        }

        private void switchLed1_MouseUp(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Left)
            //{
            Iocomp.Instrumentation.Standard.SwitchLed Led = sender as Iocomp.Instrumentation.Standard.SwitchLed;
            Led.Value.AsBoolean = false;
            //}
            return;
        }

        public const short MOTOR_X = 0;
        public const short MOTOR_Y = 1;
        public const short MOTOR_Z = 2;
        public short PosX;
        public short PosY;
        public short PosZ;

        
        private void PositionRead(Int32 Pos)
        {
            PosX = MainForm.kMotorPos[MOTOR_X];
            PosY = MainForm.kMotorPos[MOTOR_Y];
            PosZ = MainForm.kMotorPos[MOTOR_Z];

            switch (Pos)
            {
                case 1: 
                    min_1.Text = PosX.ToString();
                    min_2.Text = PosY.ToString();
                    min_3.Text = PosZ.ToString();
                    break;
                case 2:
                    max_1.Text = PosX.ToString();
                    max_2.Text = PosY.ToString();
                    max_3.Text = PosZ.ToString();
                    break;
                case 3:
                    textBox1.Text = PosZ.ToString();
                    break;
                case 4:
                    textBox71.Text = PosZ.ToString();
                    break;
                case 5:
                    s_MIN1.Text = PosX.ToString();
                    s_MAX1.Text = PosY.ToString();

                    s_MIN2.Text = PosZ.ToString();
                    break;
                case 6:
                    s_MAX2.Text = PosX.ToString();
                    s_MIN3.Text = PosY.ToString();
                    s_MAX3.Text = PosZ.ToString();
                    break;
                case 7:
                    textBox5.Text = PosZ.ToString();
                    break;
                case 8:
                    textBox72.Text = PosZ.ToString();
                    break;                
            }
            return;
        }

        

 
        private void timer1_Tick(object sender, EventArgs e)
        {
            sevenSegmentInteger1.Value.AsInteger = MainForm.kMotorPos[MOTOR_X];
            sevenSegmentInteger2.Value.AsInteger = MainForm.kMotorPos[MOTOR_Y];
            sevenSegmentInteger3.Value.AsInteger = MainForm.kMotorPos[MOTOR_Z];
            return;
        }

        private void OptSetting_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;
            timer1.Enabled = false;
            return;
        }

        private void switchLed4_MouseClick(object sender, MouseEventArgs e)
        {
            //switchLed4.Value.AsBoolean = false;
            return;
        }

   

        private void ButtonEnabledOnOff(bool OnOff)
        {
            switchLed1.Enabled = OnOff;
            switchLed2.Enabled = OnOff;
            switchLed3.Enabled = OnOff;
            switchLed4.Enabled = OnOff;
            switchLed5.Enabled = OnOff;
            switchLed6.Enabled = OnOff;
            switchLed7.Enabled = OnOff;

            imageButton3.Enabled = OnOff;
            imageButton4.Enabled = OnOff;
            imageButton5.Enabled = OnOff;
            imageButton6.Enabled = OnOff;
            imageButton7.Enabled = OnOff;
            imageButton8.Enabled = OnOff;
            imageButton9.Enabled = OnOff;
            imageButton10.Enabled = OnOff;
            return;
        }
       
        private void imageButton1_Click(object sender, EventArgs e)
        {
            if (chk_P1.Checked == true)
            {
                if (0 < textBox70.Text.Length)
                {
                    if (160 < Convert.ToDouble(textBox70.Text)) textBox70.Text = "160";
                }
                else
                {
                    textBox70.Text = "160";
                }
            }
            else
            {
                if (0 < textBox70.Text.Length)
                {
                    if (230 < Convert.ToDouble(textBox70.Text)) textBox70.Text = "230";
                }
                else
                {
                    textBox70.Text = "230";
                }
            }

            create_folder(); // 폴더 유무 확인

            Ods.Write_SET(comboBox1.Text, "SLIDE CHK", s_MIN3.Text); //Y축 검사위치1 for SBR

            Ods.Write_SET(comboBox1.Text, "TILT CHK", s_MIN4.Text); //Y축 검사위치2 for SBR

            Ods.Write_SET(comboBox1.Text, "HEIGHT CHK", s_MAX4.Text);   //Y축 검사위치3 for SBR

            Ods.Write_SET(comboBox1.Text, "WARMER CHK", s_MIN2.Text);   //Z축 복귀위치 for SBR

            Ods.Write_SET(comboBox1.Text, "VENT CHK", s_MAX3.Text); //Z축 검사위치 for SBR


            Ods.Write_SET(comboBox1.Text, "SLIDE SECT MIN", min_1.Text);   //X축 복귀위치 for ODS
            Ods.Write_SET(comboBox1.Text, "SLIDE SECT MAX", max_1.Text);   //X축 검사위치 for ODS

            Ods.Write_SET(comboBox1.Text, "TILT SECT MIN", min_2.Text);    //Y축 복귀위치 for ODS
            Ods.Write_SET(comboBox1.Text, "TILT SECT MAX", max_2.Text);    //Y축 검사위치1  for ODS

            Ods.Write_SET(comboBox1.Text, "HEIGHT SECT MIN", min_3.Text);  //Z축 복귀위치 for ODS
            Ods.Write_SET(comboBox1.Text, "HEIGHT SECT MAX", max_3.Text);  //Z축 검사위치 for ODS

            Ods.Write_SET(comboBox1.Text, "SLIDE SPEC MIN", SB_P1.Text);    //SBR판정값
            Ods.Write_SET(comboBox1.Text, "SLIDE SPEC MAX", STB_P1.Text);   //SBR판정값 offset

            Ods.Write_SET(comboBox1.Text, "TILT SPEC MIN", ID_M1.Text);     //Y축 검사위치2 for ODS
            Ods.Write_SET(comboBox1.Text, "TILT SPEC MAX", STB_M2.Text);    //Y축 검사위치3 for ODS

            Ods.Write_SET(comboBox1.Text, "HEIGHT SPEC MIN", s_MIN1.Text); //X축 복귀위치 for SBR
            Ods.Write_SET(comboBox1.Text, "HEIGHT SPEC MAX", s_MAX2.Text); //X축 검사위치 for SBR

            Ods.Write_SET(comboBox1.Text, "VENT SPEC MIN", STB_P2.Text);    //Z축 검사시간 for ODS
            Ods.Write_SET(comboBox1.Text, "VENT SPEC MAX", s_MIN5.Text);    //Z축 검사시간 for SBR   

            Ods.Write_SET(comboBox1.Text, "B_SENS SPEC MIN", s_MAX1.Text);  //Y축 복귀위치 for SBR


            if (chk_P1.Checked == true)
            {
                Ods.Write_SET(comboBox1.Text, "chk_P1", "1");    //SBR 검사 확인
            }
            else
            {
                Ods.Write_SET(comboBox1.Text, "chk_P1", "0");
            }

            //Ods.Write_SET(comboBox1.Text, "SB_SET", textBox2.Text);    //LoadCell 최대 중량(Kq) for SBR
            //Ods.Write_SET(comboBox1.Text, "SB_M2", s_MAX5.Text);        //SBR Loadcell 무반응 무게값 for SBR
            Ods.Write_SET(comboBox1.Text, "SB_P1", textBox5.Text);     //SBR 무반응 검사 복귀위치 for SBR 

            //Ods.Write_SET(comboBox1.Text, "ID_2", textBox8.Text);       //SBR Loadcell 무반응 무게값 for SBR A4형상

            Ods.Write_SET(comboBox1.Text, "ID_3", textBox9.Text);       //Z축 교정 offset값

            //검사 다운/상승 속도
            Ods.Write_SET(comboBox1.Text, "FIRST_UPDN_SPEED", textBox70.Text);
            //검사 속도
            Ods.Write_SET(comboBox1.Text, "TEST_SPEED", textBox65.Text);

            //ODA 검사 속도
            Ods.Write_SET(comboBox1.Text, "ODA_TEST_SPEED", textBox71.Text);
            //SBR 검사 속도
            Ods.Write_SET(comboBox1.Text, "SBR_TEST_SPEED", textBox72.Text);

            Ods.Write_SET(comboBox1.Text, "TEST_READY_POS", textBox1.Text);

            read_combo();
            return;
        }

        private void imageButton2_Click(object sender, EventArgs e)
        {
            this.Close();
            return;
        }

        private void imageButton3_Click(object sender, EventArgs e)
        {
#if !DEBUG_MODE
            double First;
            double Last;
            bool Flag;

            if (MessageBox.Show("서보 모터를 이동 하시겠습니까?", "선택", MessageBoxButtons.YesNo) == DialogResult.No) return;

            ButtonEnabledOnOff(false);

            //Z축 모터를 0으로 보내고 
            First = mControl.PublicFunction.timeGetTimems();
            Last = mControl.PublicFunction.timeGetTimems();

            mControl.GetServo.AxtPositionMove(MOTOR_Z, Convert.ToDouble(textBox2.Text), Convert.ToDouble(0));
            mControl.PublicFunction.timedelay(500);

            do
            {
                Application.DoEvents();
                Flag = mControl.GetServo.AxtMovingEndCheck(MOTOR_Z);
                Last = mControl.PublicFunction.timeGetTimems();
                if (10000 <= (Last - First)) break;
            } while (Flag == false);

            //X,Y축 모터를 설정 위치로 옮기고
            First = mControl.PublicFunction.timeGetTimems();
            Last = mControl.PublicFunction.timeGetTimems();
            mControl.GetServo.AxtPositionMove(MOTOR_X, Convert.ToDouble(textBox2.Text), Convert.ToDouble(min_1.Text));
            mControl.GetServo.AxtPositionMove(MOTOR_Y, Convert.ToDouble(textBox2.Text), Convert.ToDouble(min_2.Text));
            Flag = true;
            mControl.PublicFunction.timedelay(500);
            do
            {
                Application.DoEvents();
                if ((mControl.GetServo.AxtMovingEndCheck(MOTOR_X) == true) && (mControl.GetServo.AxtMovingEndCheck(MOTOR_Y) == true)) Flag = false;
                Last = mControl.PublicFunction.timeGetTimems();
                if (10000 <= (Last - First)) break;
            } while (Flag == true);

            //Z축 위치를 설정 위치로 옮기고
            First = mControl.PublicFunction.timeGetTimems();
            Last = mControl.PublicFunction.timeGetTimems();
            mControl.GetServo.AxtPositionMove(MOTOR_Z, Convert.ToDouble(textBox2.Text), Convert.ToDouble(min_3.Text));
            mControl.PublicFunction.timedelay(500);
            do
            {
                Application.DoEvents();
                Flag = mControl.GetServo.AxtMovingEndCheck(MOTOR_Z);
                Last = mControl.PublicFunction.timeGetTimems();
                if (10000 <= (Last - First)) break;
            } while (Flag == false);

            ButtonEnabledOnOff(true);
#endif
            return;

        }

        private void imageButton4_Click(object sender, EventArgs e)
        {
#if !DEBUG_MODE
            double First;
            double Last;
            bool Flag;

            if (MessageBox.Show("서보 모터를 이동 하시겠습니까?", "선택", MessageBoxButtons.YesNo) == DialogResult.No) return;

            ButtonEnabledOnOff(false);

            //Z축 모터를 0으로 보내고 
            First = mControl.PublicFunction.timeGetTimems();
            Last = mControl.PublicFunction.timeGetTimems();
            mControl.GetServo.AxtPositionMove(MOTOR_Z, Convert.ToDouble(textBox2.Text), Convert.ToDouble(0));
            mControl.PublicFunction.timedelay(500);
            do
            {
                Application.DoEvents();
                Flag = mControl.GetServo.AxtMovingEndCheck(MOTOR_Z);
                Last = mControl.PublicFunction.timeGetTimems();
                if (10000 <= (Last - First)) break;
            } while (Flag == false);

            //X,Y축 모터를 설정 위치로 옮기고
            First = mControl.PublicFunction.timeGetTimems();
            Last = mControl.PublicFunction.timeGetTimems();
            mControl.GetServo.AxtPositionMove(MOTOR_X, Convert.ToDouble(textBox2.Text), Convert.ToDouble(max_1.Text));
            mControl.GetServo.AxtPositionMove(MOTOR_Y, Convert.ToDouble(textBox2.Text), Convert.ToDouble(max_2.Text));
            mControl.PublicFunction.timedelay(500);
            Flag = true;
            do
            {
                Application.DoEvents();
                if ((mControl.GetServo.AxtMovingEndCheck(MOTOR_X) == true) && (mControl.GetServo.AxtMovingEndCheck(MOTOR_Y) == true)) Flag = false;
                Last = mControl.PublicFunction.timeGetTimems();
                if (10000 <= (Last - First)) break;
            } while (Flag == true);
            /*
            //Z축 위치를 설정 위치로 옮기고
            First = mControl.PublicFunction.timeGetTimems();
            Last = mControl.PublicFunction.timeGetTimems();
            mControl.GetServo.AxtPotionMove(MOTOR_Z, Convert.ToDouble(textBox2.Text), Convert.ToDouble(max_3.Text));
            
            do
            {
                Application.DoEvents();
                Flag = mControl.GetServo.AxtMovingEndCheck(MOTOR_Z);
                Last = mControl.PublicFunction.timeGetTimems();
                if (10000 <= (Last - First)) break;
            } while (Flag == true);
            */
            ButtonEnabledOnOff(true);
#endif
            return;
        }

        private void imageButton5_Click(object sender, EventArgs e)
        {
#if !DEBUG_MODE
            double First;
            double Last;
            bool Flag;

            if (MessageBox.Show("서보 모터를 이동 하시겠습니까?", "선택", MessageBoxButtons.YesNo) == DialogResult.No) return;

            ButtonEnabledOnOff(false);


            //Z축 위치를 설정 위치로 옮기고
            First = mControl.PublicFunction.timeGetTimems();
            Last = mControl.PublicFunction.timeGetTimems();
            mControl.GetServo.AxtPositionMove(MOTOR_Z, Convert.ToDouble(textBox2.Text), Convert.ToDouble(textBox1.Text));
            mControl.PublicFunction.timedelay(500);
            do
            {
                Application.DoEvents();
                Flag = mControl.GetServo.AxtMovingEndCheck(MOTOR_Z);
                Last = mControl.PublicFunction.timeGetTimems();
                if (10000 <= (Last - First)) break;
            } while (Flag == false);

            ButtonEnabledOnOff(true);
#endif
            return;
        }

        private void imageButton6_Click(object sender, EventArgs e)
        {
#if !DEBUG_MODE
            double First;
            double Last;
            bool Flag;

            if (MessageBox.Show("서보 모터를 이동 하시겠습니까?", "선택", MessageBoxButtons.YesNo) == DialogResult.No) return;

            ButtonEnabledOnOff(false);


            //Z축 위치를 설정 위치로 옮기고
            First = mControl.PublicFunction.timeGetTimems();
            Last = mControl.PublicFunction.timeGetTimems();
            mControl.GetServo.AxtPositionMove(MOTOR_Z, Convert.ToDouble(textBox2.Text), Convert.ToDouble(textBox71.Text));
            mControl.PublicFunction.timedelay(500);
            do
            {
                Application.DoEvents();
                Flag = mControl.GetServo.AxtMovingEndCheck(MOTOR_Z);
                Last = mControl.PublicFunction.timeGetTimems();
                if (10000 <= (Last - First)) break;
            } while (Flag == false);

            ButtonEnabledOnOff(true);
#endif
            return;
        }

        private void imageButton8_Click(object sender, EventArgs e)
        {
#if !DEBUG_MODE
            double First;
            double Last;
            bool Flag;

            if (MessageBox.Show("서보 모터를 이동 하시겠습니까?", "선택", MessageBoxButtons.YesNo) == DialogResult.No) return;

            ButtonEnabledOnOff(false);

            //Z축 모터를 0으로 보내고 
            First = mControl.PublicFunction.timeGetTimems();
            Last = mControl.PublicFunction.timeGetTimems();
            mControl.GetServo.AxtPositionMove(MOTOR_Z, Convert.ToDouble(textBox2.Text), Convert.ToDouble(0));
            mControl.PublicFunction.timedelay(500);
            do
            {
                Application.DoEvents();
                Flag = mControl.GetServo.AxtMovingEndCheck(MOTOR_Z);
                Last = mControl.PublicFunction.timeGetTimems();
                if (10000 <= (Last - First)) break;
            } while (Flag == false);

            //X,Y축 모터를 설정 위치로 옮기고
            First = mControl.PublicFunction.timeGetTimems();
            Last = mControl.PublicFunction.timeGetTimems();
            mControl.GetServo.AxtPositionMove(MOTOR_X, Convert.ToDouble(textBox2.Text), Convert.ToDouble(s_MIN1.Text));
            mControl.GetServo.AxtPositionMove(MOTOR_Y, Convert.ToDouble(textBox2.Text), Convert.ToDouble(s_MAX1.Text));
            Flag = true;
            mControl.PublicFunction.timedelay(500);
            do
            {
                Application.DoEvents();
                if ((mControl.GetServo.AxtMovingEndCheck(MOTOR_X) == true) && (mControl.GetServo.AxtMovingEndCheck(MOTOR_Y) == true)) Flag = false;
                Last = mControl.PublicFunction.timeGetTimems();
                if (10000 <= (Last - First)) break;
            } while (Flag == true);

            //Z축 위치를 설정 위치로 옮기고
            First = mControl.PublicFunction.timeGetTimems();
            Last = mControl.PublicFunction.timeGetTimems();
            mControl.GetServo.AxtPositionMove(MOTOR_Z, Convert.ToDouble(textBox2.Text), Convert.ToDouble(s_MIN2.Text));
            mControl.PublicFunction.timedelay(500);
            do
            {
                Application.DoEvents();
                Flag = mControl.GetServo.AxtMovingEndCheck(MOTOR_Z);
                Last = mControl.PublicFunction.timeGetTimems();
                if (10000 <= (Last - First)) break;
            } while (Flag == false);

            ButtonEnabledOnOff(true);
#endif
            return;
        }

        private void imageButton7_Click(object sender, EventArgs e)
        {
#if !DEBUG_MODE
            double First;
            double Last;
            bool Flag;

            if (MessageBox.Show("서보 모터를 이동 하시겠습니까?", "선택", MessageBoxButtons.YesNo) == DialogResult.No) return;

            ButtonEnabledOnOff(false);

            //Z축 모터를 0으로 보내고 
            First = mControl.PublicFunction.timeGetTimems();
            Last = mControl.PublicFunction.timeGetTimems();
            mControl.GetServo.AxtPositionMove(MOTOR_Z, Convert.ToDouble(textBox2.Text), Convert.ToDouble(0));
            mControl.PublicFunction.timedelay(500);
            do
            {
                Application.DoEvents();
                Flag = mControl.GetServo.AxtMovingEndCheck(MOTOR_Z);
                Last = mControl.PublicFunction.timeGetTimems();
                if (10000 <= (Last - First)) break;
            } while (Flag == false);

            //X,Y축 모터를 설정 위치로 옮기고
            First = mControl.PublicFunction.timeGetTimems();
            Last = mControl.PublicFunction.timeGetTimems();
            mControl.GetServo.AxtPositionMove(MOTOR_X, Convert.ToDouble(textBox2.Text), Convert.ToDouble(s_MAX2.Text));
            mControl.GetServo.AxtPositionMove(MOTOR_Y, Convert.ToDouble(textBox2.Text), Convert.ToDouble(s_MIN2.Text));
            Flag = true;
            mControl.PublicFunction.timedelay(500);
            do
            {
                Application.DoEvents();
                if ((mControl.GetServo.AxtMovingEndCheck(MOTOR_X) == true) && (mControl.GetServo.AxtMovingEndCheck(MOTOR_Y) == true)) Flag = false;
                Last = mControl.PublicFunction.timeGetTimems();
                if (10000 <= (Last - First)) break;
            } while (Flag == true);
            /*
            //Z축 위치를 설정 위치로 옮기고
            First = mControl.PublicFunction.timeGetTimems();
            Last = mControl.PublicFunction.timeGetTimems();
            mControl.GetServo.AxtPotionMove(MOTOR_Z, Convert.ToDouble(textBox2.Text), Convert.ToDouble(s_MAX3.Text));

            do
            {
                Application.DoEvents();
                Flag = mControl.GetServo.AxtMovingEndCheck(MOTOR_Z);
                Last = mControl.PublicFunction.timeGetTimems();
                if (10000 <= (Last - First)) break;
            } while (Flag == true);
            */
            ButtonEnabledOnOff(true);
#endif
            return;
        }

        private void imageButton9_Click(object sender, EventArgs e)
        {
#if !DEBUG_MODE
            double First;
            double Last;
            bool Flag;

            if (MessageBox.Show("서보 모터를 이동 하시겠습니까?", "선택", MessageBoxButtons.YesNo) == DialogResult.No) return;

            ButtonEnabledOnOff(false);


            //Z축 위치를 설정 위치로 옮기고
            First = mControl.PublicFunction.timeGetTimems();
            Last = mControl.PublicFunction.timeGetTimems();
            mControl.GetServo.AxtPositionMove(MOTOR_Z, Convert.ToDouble(textBox2.Text), Convert.ToDouble(textBox5.Text));
            mControl.PublicFunction.timedelay(500);
            do
            {
                Application.DoEvents();
                Flag = mControl.GetServo.AxtMovingEndCheck(MOTOR_Z);
                Last = mControl.PublicFunction.timeGetTimems();
                if (10000 <= (Last - First)) break;
            } while (Flag == false);

            ButtonEnabledOnOff(true);
#endif
            return;
        }

        private void imageButton10_Click(object sender, EventArgs e)
        {
#if !DEBUG_MODE
            double First;
            double Last;
            bool Flag;

            if (MessageBox.Show("서보 모터를 이동 하시겠습니까?", "선택", MessageBoxButtons.YesNo) == DialogResult.No) return;

            ButtonEnabledOnOff(false);


            //Z축 위치를 설정 위치로 옮기고
            First = mControl.PublicFunction.timeGetTimems();
            Last = mControl.PublicFunction.timeGetTimems();
            mControl.GetServo.AxtPositionMove(MOTOR_Z, Convert.ToDouble(textBox2.Text), Convert.ToDouble(textBox72.Text));
            mControl.PublicFunction.timedelay(500);
            do
            {
                Application.DoEvents();
                Flag = mControl.GetServo.AxtMovingEndCheck(MOTOR_Z);
                Last = mControl.PublicFunction.timeGetTimems();
                if (10000 <= (Last - First)) break;
            } while (Flag == false);

            ButtonEnabledOnOff(true);
#endif
            return;
        }

        private void imageButton11_Click(object sender, EventArgs e)
        {
            MainForm.JogSpeed = textBox2.Text;
            return;
        }
    }
}
