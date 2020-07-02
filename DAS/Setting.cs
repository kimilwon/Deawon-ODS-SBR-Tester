#define MODEL_DECAR
//#undef MODEL_DECAR

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
    public partial class Setting : Form
    {
        private string ODS_HeaterImageName = "";
        private string ODS_NoneHeaterImageName = "";
        private string SBR_ImageName = "";

        public Setting()
        {
            InitializeComponent();
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

            SB_P1.Text = Ods.Read_SET("TEST SPEC", "SLIDE SPEC MIN");   //SBR판정값
            STB_P1.Text = Ods.Read_SET("TEST SPEC", "SLIDE SPEC MAX");   //SBR판정값 offset


            ODS_HeaterImageName = Ods.Read_SET("TEST SPEC", "ODS HEATERIMAGE");
            ODS_NoneHeaterImageName = Ods.Read_SET("TEST SPEC", "ODS NONEHEATERIMAGE");
            SBR_ImageName = Ods.Read_SET("TEST SPEC", "SBR IMAGE");

            string ModelText = Ods.Read_SET("Config", "MODEL TEXT");


            if (ODS_HeaterImageName == "")
            {
                ODS_HeaterImageName = Application.StartupPath + "\\Image" + "\\ODS_Heater.jpg";
            }
            if (ODS_NoneHeaterImageName == "")
            {
                ODS_HeaterImageName = Application.StartupPath + "\\Image" + "\\ODS_NoneHeater.jpg";
            }
            if (SBR_ImageName == "")
            {
                SBR_ImageName = Application.StartupPath + "\\Image" + "\\" + ModelText + "_SBR.jpg";
            }

            if (File.Exists(ODS_HeaterImageName) == true)
                textBox5.Text = Path.GetFileName(ODS_HeaterImageName);
            else textBox5.Text = "";

            if (File.Exists(ODS_NoneHeaterImageName) == true)
                textBox6.Text = Path.GetFileName(ODS_NoneHeaterImageName);
            else textBox6.Text = "";

            if (File.Exists(SBR_ImageName) == true)
                textBox9.Text = Path.GetFileName(SBR_ImageName);
            else textBox9.Text = "";

            aa = Ods.Read_SET("TEST SPEC", "ConnectorCheck");    //NOTE PC
            if (aa == "1")
            {
                checkBox4.Checked = true;
            }
            else
            {
                checkBox4.Checked = false;
            }

            textBox2.Text = Ods.Read_SET("TEST SPEC", "SB_SET");        ////LoadCell 최대 중량(Kq) for SBR
            textBox3.Text = Ods.Read_SET("TEST SPEC", "SB_M1");         //LoadCell 최대 중량(Kg)    for ODS
            s_MAX5.Text = Ods.Read_SET("TEST SPEC", "SB_M2");            //SBR Loadcell 무반응 무게값 for SBR
            textBox8.Text = Ods.Read_SET("TEST SPEC", "ID_2");           //SBR Loadcell 무반응 무게값 for SBR A4형상

            textBox1.Text = Ods.Read_SET("TEST SPEC", "READCOUNT");       //ODS DATA READ COUNT

            aa = Ods.Read_SET("MEMORY", "READ");       //ODS Memory Read
            if (aa == "1")
                checkBox7.Checked = true;
            else checkBox7.Checked = false;

            aa = Ods.Read_SET("MEMORY", "CONTENTS");       //ODS Read Contents
            if (aa == "1")
                checkBox8.Checked = true;
            else checkBox8.Checked = false;            

            //----------------------------------------------------------------[비통풍 히터]
            

            SB_M2.Text = Ods.Read_SET("TEST SPEC", "WARMER SPEC MIN"); //Img Capacitance Value1 max Load Seat
            ID_M2.Text = Ods.Read_SET("TEST SPEC", "WARMER SPEC MAX"); //real Capacitance Value2 max Load Seat

            ID_P2.Text = Ods.Read_SET("TEST SPEC", "B_SENS SPEC MAX");   //rom id
            //ID_P1.Text = Ods.Read_SET("TEST SPEC", "WCS NO");           //Vehicle ID
            SB_SET.Text = Ods.Read_SET("TEST SPEC", "WCS PART");      //Img Capacitance Value1 max  Empty
            STB_M1.Text = Ods.Read_SET("TEST SPEC", "WCS LOT");       //real Capacitance Value2 max
            STB_SET.Text = Ods.Read_SET("TEST SPEC", "WCS VEHICLE");   //Cal Checksum
            SB_M1.Text = Ods.Read_SET("TEST SPEC", "WCS SOFTVER");   //OPR R3 number
            SB_P2.Text = Ods.Read_SET("TEST SPEC", "WCS PARAVER");   //Lock Byte
            textBox4.Text = Ods.Read_SET("TEST SPEC", "ODS CON_DATA");   //Connection Data , DE - 0xb0 , YB - 0xb2                        

#if MODEL_DECAR
            if (textBox4.Text == "") textBox4.Text = "B0";
#else
            if (textBox4.Text == "") textBox4.Text = "B2";
#endif
            

            

            textBox7.Text = Ods.Read_SET("TEST SPEC", "ID_1");          //Cal checksum Non-heater type


            textBox12.Text = Ods.Read_SET("TEST SPEC", "GIG");          //Img Capacitance Value1 min  Empty

            textBox11.Text = Ods.Read_SET("TEST SPEC", "UMTYPE");       //real Capacitance Value1 min  Empty

            textBox10.Text = Ods.Read_SET("TEST SPEC", "I_C_V_M_L");          //Img Capacitance Value1 min  Load

            textBox13.Text = Ods.Read_SET("TEST SPEC", "R_C_V_M_L");       //real Capacitance Value1 min  Load

            textBox16.Text = Ods.Read_SET("TEST SPEC", "NON_GIG");          //NON_ Img Capacitance Value1 min  Empty

            textBox15.Text = Ods.Read_SET("TEST SPEC", "NON_UMTYPE");       //NON_ real Capacitance Value1 min  Empty

            textBox14.Text = Ods.Read_SET("TEST SPEC", "NON_I_C_V_M_L");          //NON_ Img Capacitance Value1 min  Load

            textBox17.Text = Ods.Read_SET("TEST SPEC", "NON_R_C_V_M_L");       //NON_ real Capacitance Value1 min  Load

            textBox20.Text = Ods.Read_SET("TEST SPEC", "NON_GIG_H");          //NON_ Img Capacitance Value1 max  Empty

            textBox19.Text = Ods.Read_SET("TEST SPEC", "NON_UMTYPE_H");       //NON_ real Capacitance Value1 max  Empty

            textBox18.Text = Ods.Read_SET("TEST SPEC", "NON_I_C_V_M_L_H");          //NON_ Img Capacitance Value1 max  Load

            textBox21.Text = Ods.Read_SET("TEST SPEC", "NON_R_C_V_M_L_H");       //NON_ real Capacitance Value1 max  Load


            textBox1.Text = Ods.Read_SET("TEST SPEC", "READCOUNT");       //ODS DATA READ COUNT

            aa = Ods.Read_SET("MEMORY", "READ");       //ODS Memory Read
            if (aa == "1")
                    checkBox7.Checked = true;
            else    checkBox7.Checked = false;

            aa = Ods.Read_SET("MEMORY", "CONTENTS");       //ODS Read Contents
            if (aa == "1")
                    checkBox8.Checked = true;
            else    checkBox8.Checked = false;            


            //----------------------------------------------------------------[통풍 히터]
            

            textBox26.Text = Ods.Read_SET("VENT_HETER SPEC", "WARMER SPEC MIN"); //Img Capacitance Value1 max Load Seat
            textBox29.Text = Ods.Read_SET("VENT_HETER SPEC", "WARMER SPEC MAX"); //real Capacitance Value2 max Load Seat

            textBox41.Text = Ods.Read_SET("VENT_HETER SPEC", "B_SENS SPEC MAX");   //rom id
            //ID_P1.Text = Ods.Read_SET("TEST SPEC", "WCS NO");           //Vehicle ID
            textBox28.Text = Ods.Read_SET("VENT_HETER SPEC", "WCS PART");      //Img Capacitance Value1 max  Empty
            textBox42.Text = Ods.Read_SET("VENT_HETER SPEC", "WCS LOT");       //real Capacitance Value2 max
            textBox43.Text = Ods.Read_SET("VENT_HETER SPEC", "WCS VEHICLE");   //Cal Checksum
            textBox42.Text = Ods.Read_SET("VENT_HETER SPEC", "WCS SOFTVER");   //OPR R3 number
            textBox40.Text = Ods.Read_SET("VENT_HETER SPEC", "WCS PARAVER");   //Lock Byte

            ID_P1.Text = Ods.Read_SET("VENT_HETER SPEC", "ODS CON_DATA");   //Connection Data , DE - 0xb0 , YB - 0xb2                        

#if MODEL_DECAR
            if (ID_P1.Text == "") ID_P1.Text = "B0";
#else
            if (ID_P1.Text == "") ID_P1.Text = "B2";
#endif

            textBox39.Text = Ods.Read_SET("VENT_HETER SPEC", "ID_1");          //Cal checksum Non-heater type

            textBox24.Text = Ods.Read_SET("VENT_HETER SPEC", "GIG");          //Img Capacitance Value1 min  Empty

            textBox23.Text = Ods.Read_SET("VENT_HETER SPEC", "UMTYPE");       //real Capacitance Value1 min  Empty

            textBox22.Text = Ods.Read_SET("VENT_HETER SPEC", "I_C_V_M_L");          //Img Capacitance Value1 min  Load

            textBox25.Text = Ods.Read_SET("VENT_HETER SPEC", "R_C_V_M_L");       //real Capacitance Value1 min  Load

            textBox32.Text = Ods.Read_SET("VENT_HETER SPEC", "NON_GIG");          //NON_ Img Capacitance Value1 min  Empty

            textBox31.Text = Ods.Read_SET("VENT_HETER SPEC", "NON_UMTYPE");       //NON_ real Capacitance Value1 min  Empty

            textBox30.Text = Ods.Read_SET("VENT_HETER SPEC", "NON_I_C_V_M_L");          //NON_ Img Capacitance Value1 min  Load

            textBox33.Text = Ods.Read_SET("VENT_HETER SPEC", "NON_R_C_V_M_L");       //NON_ real Capacitance Value1 min  Load

            textBox36.Text = Ods.Read_SET("VENT_HETER SPEC", "NON_GIG_H");          //NON_ Img Capacitance Value1 max  Empty

            textBox35.Text = Ods.Read_SET("VENT_HETER SPEC", "NON_UMTYPE_H");       //NON_ real Capacitance Value1 max  Empty

            textBox34.Text = Ods.Read_SET("VENT_HETER SPEC", "NON_I_C_V_M_L_H");          //NON_ Img Capacitance Value1 max  Load

            textBox37.Text = Ods.Read_SET("VENT_HETER SPEC", "NON_R_C_V_M_L_H");       //NON_ real Capacitance Value1 max  Load

            return;
        }

        
        
        private void Setting_Load(object sender, EventArgs e)
        {
            if (this.DesignMode == true) return; //디자인 창 안뜨는것 해결함수라 한다.   
            read_model();
            return;
        }

        

        private void imageButton1_Click(object sender, EventArgs e)
        {
            create_folder(); // 폴더 유무 확인

            Ods.Write_SET("TEST SPEC", "SLIDE SPEC MIN", SB_P1.Text);    //SBR판정값
            Ods.Write_SET("TEST SPEC", "SLIDE SPEC MAX", STB_P1.Text);   //SBR판정값 offset

            Ods.Write_SET("TEST SPEC", "ODS HEATERIMAGE", ODS_HeaterImageName);
            Ods.Write_SET("TEST SPEC", "ODS NONEHEATERIMAGE", ODS_NoneHeaterImageName);
            Ods.Write_SET("TEST SPEC", "SBR IMAGE", SBR_ImageName);

            if (checkBox4.Checked == true)
            {
                Ods.Write_SET("TEST SPEC", "ConnectorCheck", "1");    //Connection Message Check
            }
            else
            {
                Ods.Write_SET("TEST SPEC", "ConnectorCheck", "0");
            }
            Ods.Write_SET("TEST SPEC", "SB_SET", textBox2.Text);    //LoadCell 최대 중량(Kq) for SBR
            Ods.Write_SET("TEST SPEC", "SB_M1", textBox3.Text);     //LoadCell 최대 중량(Kq) for ODS
            Ods.Write_SET("TEST SPEC", "SB_M2", s_MAX5.Text);        //SBR Loadcell 무반응 무게값 for SBR
            Ods.Write_SET("TEST SPEC", "ID_2", textBox8.Text);       //SBR Loadcell 무반응 무게값 for SBR A4형상
                                                                     //----------------------------------------------------------------[통풍 히터]


            Ods.Write_SET("TEST SPEC", "WARMER SPEC MIN", SB_M2.Text);  //Img Capacitance Value1 Load Seat
            Ods.Write_SET("TEST SPEC", "WARMER SPEC MAX", ID_M2.Text);  //Img Capacitance Value2 Load Seat

            Ods.Write_SET("TEST SPEC", "B_SENS SPEC MAX", ID_P2.Text);    //rom id

            //textBox4.Text = ID_P1.Text;
            //Ods.Write_SET("TEST SPEC", "WCS NO", ID_P1.Text);        //Vehicle ID
            Ods.Write_SET("TEST SPEC", "WCS PART", SB_SET.Text);      //Img Capacitance Value
            Ods.Write_SET("TEST SPEC", "WCS LOT", STB_M1.Text);       //Img Capacitance Value
            Ods.Write_SET("TEST SPEC", "WCS VEHICLE", STB_SET.Text);   //Cal Checksum
            Ods.Write_SET("TEST SPEC", "WCS SOFTVER", SB_M1.Text);   //OPR R3 number
            Ods.Write_SET("TEST SPEC", "WCS PARAVER", SB_P2.Text);   //Lock Byte
            Ods.Write_SET("TEST SPEC", "ODS CON_DATA", textBox4.Text);   //Connection Data , DE - 0xb0 , YB - 0xb2          


            Ods.Write_SET("TEST SPEC", "SB_P2", SB_P2.Text);

            Ods.Write_SET("TEST SPEC", "ID_1", textBox7.Text);      //Non-Heater type


            Ods.Write_SET("TEST SPEC", "GIG", textBox12.Text);                    //Img Capacitance Value1 min  Empty

            Ods.Write_SET("TEST SPEC", "UMTYPE", textBox11.Text);                 //real Capacitance Value1 min  Empty 

            Ods.Write_SET("TEST SPEC", "I_C_V_M_L", textBox10.Text);              //Img Capacitance Value1 min  Load

            Ods.Write_SET("TEST SPEC", "R_C_V_M_L", textBox13.Text);              //real Capacitance Value1 min  Load 

            Ods.Write_SET("TEST SPEC", "NON_GIG", textBox16.Text);                    //Img Capacitance Value1 min  Empty

            Ods.Write_SET("TEST SPEC", "NON_UMTYPE", textBox15.Text);                 //real Capacitance Value1 min  Empty 

            Ods.Write_SET("TEST SPEC", "NON_I_C_V_M_L", textBox14.Text);              //Img Capacitance Value1 min  Load

            Ods.Write_SET("TEST SPEC", "NON_R_C_V_M_L", textBox17.Text);              //Img Capacitance Value1 min  Load

            Ods.Write_SET("TEST SPEC", "NON_GIG_H", textBox20.Text);          //NON_ Img Capacitance Value1 max  Empty

            Ods.Write_SET("TEST SPEC", "NON_UMTYPE_H", textBox19.Text);       //NON_ real Capacitance Value1 max  Empty

            Ods.Write_SET("TEST SPEC", "NON_I_C_V_M_L_H", textBox18.Text);          //NON_ Img Capacitance Value1 max  Load

            Ods.Write_SET("TEST SPEC", "NON_R_C_V_M_L_H", textBox21.Text);       //NON_ real Capacitance Value1 max  Load

            Ods.Write_SET("TEST SPEC", "READCOUNT", textBox1.Text);       //ODS DATA READ COUNT

            Ods.Write_SET("MEMORY", "READ", checkBox7.Checked == true ? "1" : "0");       //ODS Memory Read
            Ods.Write_SET("MEMORY", "CONTENTS", checkBox8.Checked == true ? "1" : "0");       //ODS Read Contents

            //----------------------------------------------------------------[통풍 히터]


            Ods.Write_SET("VENT_HETER SPEC", "WARMER SPEC MIN", textBox26.Text); //Img Capacitance Value1 max Load Seat
            Ods.Write_SET("VENT_HETER SPEC", "WARMER SPEC MAX", textBox29.Text); //real Capacitance Value2 max Load Seat

            Ods.Write_SET("VENT_HETER SPEC", "B_SENS SPEC MAX", textBox41.Text);   //rom id
            //ID_P1.Text = Ods.Write_SET("TEST SPEC", "WCS NO");           //Vehicle ID
            Ods.Write_SET("VENT_HETER SPEC", "WCS PART", textBox28.Text);      //Img Capacitance Value1 max  Empty
            Ods.Write_SET("VENT_HETER SPEC", "WCS LOT", textBox27.Text);       //real Capacitance Value2 max
            Ods.Write_SET("VENT_HETER SPEC", "WCS VEHICLE", textBox43.Text);   //Cal Checksum
            Ods.Write_SET("VENT_HETER SPEC", "WCS SOFTVER", textBox42.Text);   //OPR R3 number
            Ods.Write_SET("VENT_HETER SPEC", "WCS PARAVER", textBox40.Text);   //Lock Byte

            Ods.Write_SET("VENT_HETER SPEC", "ODS CON_DATA", ID_P1.Text);   //Connection Data , DE - 0xb0 , YB - 0xb2                        


            Ods.Write_SET("VENT_HETER SPEC", "ID_1", textBox39.Text);          //Cal checksum Non-heater type

            Ods.Write_SET("VENT_HETER SPEC", "GIG", textBox24.Text);          //Img Capacitance Value1 min  Empty

            Ods.Write_SET("VENT_HETER SPEC", "UMTYPE", textBox23.Text);       //real Capacitance Value1 min  Empty

            Ods.Write_SET("VENT_HETER SPEC", "I_C_V_M_L", textBox22.Text);          //Img Capacitance Value1 min  Load

            Ods.Write_SET("VENT_HETER SPEC", "R_C_V_M_L", textBox25.Text);       //real Capacitance Value1 min  Load

            Ods.Write_SET("VENT_HETER SPEC", "NON_GIG", textBox32.Text);          //NON_ Img Capacitance Value1 min  Empty

            Ods.Write_SET("VENT_HETER SPEC", "NON_UMTYPE", textBox31.Text);       //NON_ real Capacitance Value1 min  Empty

            Ods.Write_SET("VENT_HETER SPEC", "NON_I_C_V_M_L", textBox30.Text);          //NON_ Img Capacitance Value1 min  Load

            Ods.Write_SET("VENT_HETER SPEC", "NON_R_C_V_M_L", textBox33.Text);       //NON_ real Capacitance Value1 min  Load

            Ods.Write_SET("VENT_HETER SPEC", "NON_GIG_H", textBox36.Text);          //NON_ Img Capacitance Value1 max  Empty

            Ods.Write_SET("VENT_HETER SPEC", "NON_UMTYPE_H", textBox35.Text);       //NON_ real Capacitance Value1 max  Empty

            Ods.Write_SET("VENT_HETER SPEC", "NON_I_C_V_M_L_H", textBox34.Text);          //NON_ Img Capacitance Value1 max  Load

            Ods.Write_SET("VENT_HETER SPEC", "NON_R_C_V_M_L_H", textBox37.Text);       //NON_ real Capacitance Value1 max  Load
            return;
        }

        private void imageButton2_Click(object sender, EventArgs e)
        {
            this.Close();
            return;
        }

        private void imageButton3_Click(object sender, EventArgs e)
        {
            //ODS Heater

            openFileDialog1.FileName = "";
            openFileDialog1.InitialDirectory = Application.StartupPath + "\\image";
            openFileDialog1.Filter = "ODS 히터타입 컨넥터(*.Jpg)|*.jpg";
            openFileDialog1.DefaultExt = ".jpg";
            openFileDialog1.Title = "컨넥터 사진 선택";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ODS_HeaterImageName = openFileDialog1.FileName;
                textBox5.Text = Path.GetFileName(openFileDialog1.FileName);
            }

            return;
        }

        private void imageButton4_Click(object sender, EventArgs e)
        {
            //ODS None Heater
            openFileDialog1.FileName = "";
            openFileDialog1.InitialDirectory = Application.StartupPath + "\\image";
            openFileDialog1.Filter = "ODS 넌 히터타입 컨넥터(*.Jpg)|*.jpg";
            openFileDialog1.DefaultExt = ".jpg";
            openFileDialog1.Title = "컨넥터 사진 선택";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ODS_NoneHeaterImageName = openFileDialog1.FileName;
                textBox6.Text = Path.GetFileName(openFileDialog1.FileName);
            }
            return;
        }

        private void imageButton5_Click(object sender, EventArgs e)
        {
            //SBR 
            openFileDialog1.FileName = "";
            openFileDialog1.InitialDirectory = Application.StartupPath + "\\image";
            openFileDialog1.Filter = "SBR 컨넥터(*.Jpg)|*.jpg";
            openFileDialog1.DefaultExt = ".jpg";
            openFileDialog1.Title = "컨넥터 사진 선택";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SBR_ImageName = openFileDialog1.FileName;
                textBox9.Text = Path.GetFileName(openFileDialog1.FileName);
            }
            return;
        }
    }
}
