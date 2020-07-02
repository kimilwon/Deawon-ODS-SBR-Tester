//#define DEBUG_MODE

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
//using Ivi.Visa.Interop;
//using Ivi.Driver.Interop;
//using Ivi.DCPwr.Interop;
//using Agilent.AgilentN57xx.Interop;
using System.Threading;
using System.Windows.Forms;

//using MOTION;


//TEXTBOX36, TEXTBOX48 Y축 보귀 위치

namespace ODS
{
    public interface MyInterface
    {
        bool isExit { get; }
        double GetXLimit { get; }
        double GetYLimit { get; }
        double GetZLimit { get; }
        double GetXLoad { get; }
        double GetYLoad { get; }
        double GetZLoad { get; }
        COMMON_FUCTION PublicFunction { get; }
        AxtMotion GetServo { get; }
        __CanControl GetCanReWrite { get; }
    }

    public partial class MainForm : Form, MyInterface
    {
        private int OdsDataSearchCount = 5;
        private const byte MOTOR_X = 0;
        private const byte MOTOR_Y = 1;
        private const byte MOTOR_Z = 2;
        private bool SbrOrOds = false; // treu - sbr, false - ods
        private bool VentHeater = false; // false - 비통풍 히터, true - 통풍히터


        private bool ProductOutFlag = false;
        private bool ProductOutFlag2 = false;
        
        private bool MotorEStopFlag = false;

        private bool ProgramStartFlag = true;
        //private bool ModelChangeFlag = false;
        private bool OriginSignalOutFlag = false;

        private const byte RESULT_NONE = 0;
        private const byte RESULT_PASS = 1;
        private const byte RESULT_REJECT = 2;
        private const byte RESULT_TEST = 3;

        private const byte LOADCELL_STX = 0x02;
        private const byte LOADCELL_ETX = 0x03;

        private int ExitTime = 0;

        private string ModelName = "";
        private string NowDate = "";

        //private bool MotorConfigSetFlag = false;
        //private bool MotorConfigGetFlag = false;
        private byte[] MotorConfigCheckBuf = new byte[300];
        private int ReadMemoryResult = 0;
        private int ReadContentsResult = 0;

        //private UInt16 zmotorPos;
        //private UInt16 zmotorTargetPos;


        private bool FirstMotorOrgRunFlag = false;
        private bool FirstModelChangeFlag = true;
        private bool OrgZSettingFlag = false;
        private byte ConnectionData;
        private int EStopCount;
        private bool TestOkOnFlag = false;

        private bool ExcelFirst = false;
        private short ExcelRow = 0;
        private bool SendFailFlag;

        private const bool ON = true;
        private const bool OFF = false;
        private string ModelText = "";

        private bool ReadMemory = false;
        private bool ReadContents = false;
        private __CanControl.__CanDevice__ CanDeivce = new __CanControl.__CanDevice__();
       
        //private List<string> CanData = new List<string>();

        private List<string> SendList = new List<string>();

        public MainForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        //private Common cmObj = new Common(20, 5);

        //private Member mObj;
        //dong.spec_data spc_d;

        private bool test_process_run = false;
        private bool test_process_stop = false;
        private bool NotLoadToTest = false;
        private bool test_process_estop = false;
        private bool Pop_Selected = true;

        private string ODS_HeaterImageName = "";
        private string ODS_NoneHeaterImageName = "";
        private string SBR_ImageName = "";

        //bool m_Umtype;
        //P32C32
        long OrgFirst;
        long OrgLast;
        short SBRData;
        
        short i = 0;

        /// <summary>
        /// 813
        /// </summary>
        const uint fDataNo = 100;
        const uint hDataNo = 100;
        
        Form xForm;

        //Boolean timer1busy = false;
        //Boolean timer2busy = false;

        ODSPublic Ods = new ODSPublic();

        private bool RunningFlag = false;
        private bool LiveDisplayFlag = false;
        //uint m_HighCan;
        //uint m_LowCan;
        //int m_CanSendInx;
        Queue m_Que = new Queue();
        //int b;

        
        string retest = "1"; //// retest 번호/ DB 바코드 맞춘 후 재검사./
        string linenum; /// 라인 넘버
        string linecom; //// 장비 넘버
        int comitAdd;

        string popdata1;
        
       
        private List<string> SpecList = new List<string>();

        string data1 = "";
        
        int lv_num = 1;
        
        
        int m_NG_count, m_OK_count;

        //string Vehicle_ID;                  //Vehicle ID
        //string Img_Capacitance_Value1;       //Img Capacitance Value
        //string Img_Capacitance_Value2;       //Img Capacitance Value
        //string Cal_Checksum;                //Cal Checksum
        //string OPR_R3_number;               //OPR R3 number
        //string Lock_Byte;                   //Lock Byte

        string LockByteData;
        string UnlockByteData;
        string NoneLoadData;
        string LoadData;

        private float SBROffset;
        private float ODSOffset;
        private float SBR_OffsetStroke;
        private float SBR_OffsetLoad;
        private float ODS_OffsetStroke;
        private float ODS_OffsetLoad;
        private short LoadMode = 0;
        private bool ProgramStartFlagToMotor = true;
        private bool ConfigSetFlag = false;
        private double TestOk_First = 0;
        private double TestOk_Last = 0;


        private bool ConnectionCheckFlag = false;

        private IOControl IOPort;
        private __CanControl CanReWrite;


        // 김일원 추가 변수 
        //--------------------------------------------------------------------------
        public static short[] kMotorPos = new short[3];
        //bool[] kMotorMove = new bool[3];
        double kLoadCell;
        long DataCount;
        //--------------------------------------------------------------------------

        //bool OrgMoveFlag = false;
        short OrgMoveMode = 0;
        
#if !DEBUG_MODE
        private AxtMotion AXTServo = new AxtMotion();
        private COMMON_FUCTION ComF = new COMMON_FUCTION();

       
//        #region CAN

//        public void CAN_HIGH_CLOSE()
//        {
//            NiCanFunc.ncCloseObject(m_HighCan);
//            return;
//        }

//        public void CAN_INI_HIGH()
//        {
//            int ret;

//            //m_CanSendInx = 0;
//            uint[] AttrIdList = new uint[8];
//            uint[] AttrValueList = new uint[8];

//            AttrIdList[0] = NiCanFunc.NC_ATTR_BAUD_RATE;
//            AttrValueList[0] = 500000;

//            AttrIdList[1] = NiCanFunc.NC_ATTR_START_ON_OPEN;
//            AttrValueList[1] = 1;

//            AttrIdList[2] = NiCanFunc.NC_ATTR_READ_Q_LEN;
//            AttrValueList[2] = 150;

//            AttrIdList[3] = NiCanFunc.NC_ATTR_WRITE_Q_LEN;
//            AttrValueList[3] = 0;

//            AttrIdList[4] = NiCanFunc.NC_ATTR_CAN_COMP_STD;
//            AttrValueList[4] = 0;

//            AttrIdList[5] = NiCanFunc.NC_ATTR_CAN_MASK_STD;
//            AttrValueList[5] = NiCanFunc.NC_CAN_MASK_STD_DONTCARE;

//            AttrIdList[6] = NiCanFunc.NC_ATTR_CAN_COMP_XTD;
//            AttrValueList[6] = 0;

//            AttrIdList[7] = NiCanFunc.NC_ATTR_CAN_MASK_XTD;
//            AttrValueList[7] = NiCanFunc.NC_CAN_MASK_XTD_DONTCARE;

//#if !DEBUG_MODE
//            m_Que.Clear();
//            ret = NiCanFunc.ncConfig(CAN_number, 8, AttrIdList, AttrValueList);
//            ret = NiCanFunc.ncOpenObject(CAN_number, ref m_HighCan);      //High Can
//            //ret = NiCanFunc.ncCreateNotification(m_HighCan, NiCanFunc.NC_ST_READ_AVAIL | NiCanFunc.NC_ST_ERROR, 5000, 0, CanRxEvent);
//#endif
//        }


//        public void CAN_INI_LOW()
//        {
//            int ret;

//            //m_CanSendInx = 0;
//            uint[] AttrIdList = new uint[8];
//            uint[] AttrValueList = new uint[8];

//            AttrIdList[0] = NiCanFunc.NC_ATTR_BAUD_RATE;
//            AttrValueList[0] = 100000;

//            AttrIdList[1] = NiCanFunc.NC_ATTR_START_ON_OPEN;
//            AttrValueList[1] = 1;

//            AttrIdList[2] = NiCanFunc.NC_ATTR_READ_Q_LEN;
//            AttrValueList[2] = 150;

//            AttrIdList[3] = NiCanFunc.NC_ATTR_WRITE_Q_LEN;
//            AttrValueList[3] = 0;

//            AttrIdList[4] = NiCanFunc.NC_ATTR_CAN_COMP_STD;
//            AttrValueList[4] = 0;

//            AttrIdList[5] = NiCanFunc.NC_ATTR_CAN_MASK_STD;
//            AttrValueList[5] = NiCanFunc.NC_CAN_MASK_STD_DONTCARE;

//            AttrIdList[6] = NiCanFunc.NC_ATTR_CAN_COMP_XTD;
//            AttrValueList[6] = 0;

//            AttrIdList[7] = NiCanFunc.NC_ATTR_CAN_MASK_XTD;
//            AttrValueList[7] = NiCanFunc.NC_CAN_MASK_XTD_DONTCARE;
//#if !DEBUG_MODE
//            m_Que.Clear();
//            ret = NiCanFunc.ncConfig("CAN0", 8, AttrIdList, AttrValueList);
//            ret = NiCanFunc.ncOpenObject("CAN0", ref m_LowCan);         //Low Can

//            //       ret = NiCanFunc.ncCreateNotification(m_HighCan, NiCanFunc.NC_ST_READ_AVAIL | NiCanFunc.NC_ST_ERROR, 5000, 0, CanRxEvent);
//#endif
//            return;
//        }

//        public void CanRxEvent(uint ObjHandle, UInt32 State, int Status, int RefData)
//        {
//            try
//            {
//#if !DEBUG_MODE
//                int ret;
//                if (((State & NiCanFunc.NC_ST_READ_AVAIL) == 1) && (Status == 0))
//                {
//                    NCTYPE_CAN_FRAME_TIMED Frame = new NCTYPE_CAN_FRAME_TIMED();
//                    //ret = NiCanFunc.ncRead(m_HighCan, (uint)Marshal.SizeOf(Frame), ref Frame);
//                    ret = NiCanFunc.ncRead(ObjHandle, (uint)Marshal.SizeOf(Frame), ref Frame);

//                    if ((ret == 0) && m_Que.Count < 1000)
//                    {
//                        m_Que.Enqueue(Frame);
//                    }
//                }
//                //return 0;
//#endif
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
//            }
//        }

//        #endregion
        
        
        #region CAN 관련 메세지 자료

        void WCS_01()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();

            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };

            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 3;
            data.DATA[1] = 0x22;
            data.DATA[2] = 0xFA;
            data.DATA[3] = 0xAB;
            data.DATA[4] = 0;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);		//sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);
        }

        void Diag_start()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();

            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };

            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 4;
            data.DATA[0] = 2;
            data.DATA[0] = 0x10;
            data.DATA[0] = 0x03;
            data.DATA[0] = 0x00;
            data.DATA[0] = 0;
            data.DATA[0] = 0;
            data.DATA[0] = 0;
            data.DATA[0] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);		//sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "DIAG START";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);
            return;
        }


        void ReadDTCinformation()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };

            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 3;
            data.DATA[1] = 0x19;
            data.DATA[2] = 0x02;
            data.DATA[3] = 0xFF;
            data.DATA[4] = 0;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);		//sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "ReadDTCinformation";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);
        }


        void ClearDTCinformation()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();

            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 4;
            data.DATA[1] = 0x14;
            data.DATA[2] = 0xFF;
            data.DATA[3] = 0xFF;
            data.DATA[4] = 0xFF;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);		//sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "AllClearDTCinformation";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);
        }

        void DTC_ALL_stop()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 1;
            data.DATA[1] = 0x60;
            data.DATA[2] = 0x00;
            data.DATA[3] = 0x00;
            data.DATA[4] = 0x00;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);		//sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);
            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "AllClearDTCinformation";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);
        }

        void ECU_RESET()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 2;
            data.DATA[1] = 0x11;
            data.DATA[2] = 0x01;
            data.DATA[3] = 0x0;
            data.DATA[4] = 0x0;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);		//sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "ECU RESET";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);
        }


        void T1_Test_01(bool load_flg)
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 3;
            data.DATA[1] = 0x22;
            data.DATA[2] = 0xFA;
            data.DATA[3] = 0xC1;
            data.DATA[4] = 0;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);		//sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";

            if (load_flg)
            {
                log_data += "IMG CAP READ(LOAD)";
            }
            else
            {
                log_data += "IMG CAP READ(UNLOAD)";
            }

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);

            //plot1.Channels[0].Clear();
            //plot1.Channels[1].Clear();
        }

        void T1_Test_02(bool load_flg)
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 3;
            data.DATA[1] = 0x22;
            data.DATA[2] = 0xFA;
            data.DATA[3] = 0xC2;
            data.DATA[4] = 0;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);		//sizeof(NCTYPE_CAN_FRAME)

            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";

            if (load_flg)
            {
                log_data += "REAL CAP READ(LOAD)";
            }
            else
            {
                log_data += "REAL CAP READ(UNLOAD)";
            }
            //log_data += Environment.NewLine;
            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);

        }

        void T2_Test_01()
        {
            //int ret;
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 4;
            data.DATA[1] = 0x2E;
            data.DATA[2] = 0xFA;
            data.DATA[3] = 0xAB;
            data.DATA[4] = 0xAA;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);		//sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "UNLock Byte Write";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);

            button74.Text = "0x" + data.DATA[4].ToString("X2");
        }

        void T2_Test_02()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 3;
            data.DATA[1] = 0x22;
            data.DATA[2] = 0xFA;
            data.DATA[3] = 0xAB;
            data.DATA[4] = 0;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);		//sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);
            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "UNLOCK Byte REQ";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);
        }


        void T3_Test_01()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 3;
            data.DATA[1] = 0x22;
            data.DATA[2] = 0xFA;
            data.DATA[3] = 0x8C;
            data.DATA[4] = 0;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);       //sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "SERIAL REQ1";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);
            return;
        }

        void SERIAL_REQ3()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 0x30;
            data.DATA[1] = 0x08;
            data.DATA[2] = 0x08;
            data.DATA[3] = 0;
            data.DATA[4] = 0;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);		//sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "SERIAL REQ3";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);
            return;
        }


        void T3_Test_02()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 4;
            data.DATA[0] = 3;
            data.DATA[1] = 0x22;
            data.DATA[2] = 0xFA;
            data.DATA[3] = 0xD8;
            data.DATA[4] = 0;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);       //sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "ROM ID";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);
            return;
        }

        void T4_Test_01_heater()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 7;
            data.DATA[1] = 0x2E;
            data.DATA[2] = 0xFA;
            data.DATA[3] = 0xBE;
            data.DATA[4] = 0x01;
            data.DATA[5] = 0xFF;
            data.DATA[6] = 0x40;
            data.DATA[7] = 0x60;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);		//sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "Heater write";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);

            textBox29.Text = "0x" + Convert.ToUInt32((data.DATA[4] << 24) | (data.DATA[5] << 16) | (data.DATA[6] << 8) | data.DATA[7]).ToString("X8");

            return;
        }


        void T4_Test_01_no_heater()
        {//int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 7;
            data.DATA[1] = 0x2E;
            data.DATA[2] = 0xFA;
            data.DATA[3] = 0xBE;
            data.DATA[4] = 0x00;
            data.DATA[5] = 0xFF;
            data.DATA[6] = 0x41;
            data.DATA[7] = 0xF0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);       //sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "no Heater write";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);

            textBox29.Text = "0x" + Convert.ToUInt32((data.DATA[4] << 24) | (data.DATA[5] << 16) | (data.DATA[6] << 8) | data.DATA[7]).ToString("X8");
            return;
        }


        void T4_Test_02()
        {

            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 0x06;
            data.DATA[1] = 0x23;
            data.DATA[2] = 0x13;
            data.DATA[3] = 0x00;
            data.DATA[4] = 0x21;
            data.DATA[5] = 0x0F;
            data.DATA[6] = 0x05;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);       //sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "OPA R3 REQ";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);
            return;
        }


        void T6_Test_01()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 3;
            data.DATA[1] = 0x22;
            data.DATA[2] = 0xFA;
            data.DATA[3] = 0xBE;
            data.DATA[4] = 0;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);       //sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "READ ALGO_PARAM_SELECTION";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);
            return;
        }


        void T6_Test_02()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 3;
            data.DATA[1] = 0x22;
            data.DATA[2] = 0xFA;
            data.DATA[3] = 0xD9;
            data.DATA[4] = 0;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);       //sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "READ CAL CHECKSUM";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);
            return;
        }


        void T7_Test_01()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 4;
            data.DATA[1] = 0x2E;
            data.DATA[2] = 0xFA;
            data.DATA[3] = 0x10;
            data.DATA[4] = 0xAA;
            data.DATA[5] = 0x00;
            data.DATA[6] = 0x00;
            data.DATA[7] = 0x00;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);       //sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "EOL TEST Flg write";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);

            button46.Text = "0x" + data.DATA[4].ToString("X2");
            return;
        }


        void T7_Test_02()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 3;
            data.DATA[1] = 0x22;
            data.DATA[2] = 0xFA;
            data.DATA[3] = 0x10;
            data.DATA[4] = 0x00;
            data.DATA[5] = 0x00;
            data.DATA[6] = 0x00;
            data.DATA[7] = 0x00;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);       //sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "EOL TEST Flg READ";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);
            return;
        }


        void T8_Test_01()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 4;
            data.DATA[1] = 0x2E;
            data.DATA[2] = 0xFA;
            data.DATA[3] = 0xAB;
            data.DATA[4] = 0x55;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);       //sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "Write Lock BYTE 0x55";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);

            button74.Text = "0x" + data.DATA[4].ToString("X2");
            return;
        }

        void T8_Test_02()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 3;
            data.DATA[1] = 0x22;
            data.DATA[2] = 0xFA;
            data.DATA[3] = 0xAB;
            data.DATA[4] = 0;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);       //sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "LOCK BYTE REQ";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);
            return;
        }



        void Enter_security_access_read_seed()
        {

            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 4;
            data.DATA[0] = 2;
            data.DATA[1] = 0x27;
            data.DATA[2] = 0x01;
            data.DATA[3] = 0x00;
            data.DATA[4] = 0;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);       //sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "SEED REQ";

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);
            return;
        }

        void Enter_security_access_write_key()
        {

            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 6;
            data.DATA[1] = 0x27;
            data.DATA[2] = 0x02;
            data.DATA[3] = (byte)(key_eeff >> 8); //$ee
            data.DATA[4] = (byte)key_eeff;    //$ff
            data.DATA[5] = (byte)(key_gghh >> 8);    //$gg
            data.DATA[6] = (byte)key_gghh;    //&hh    
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);       //sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);

            int ccc = data.ID;
            string log_data = "";
            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += "Key Send";
            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);
            return;
        }

        void WCS_02()
        {
            //int ret;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            data.ID = 0x07c3;
            //data.IsRemote = 0;
            data.Length = 8;
            data.DATA[0] = 0x4;
            data.DATA[1] = 0x2e;
            data.DATA[2] = 0xfa;
            data.DATA[3] = 0xab;
            data.DATA[4] = 0xaa;
            data.DATA[5] = 0;
            data.DATA[6] = 0;
            data.DATA[7] = 0;
            //ret = NiCanFunc.ncWrite(m_HighCan, (uint)Marshal.SizeOf(data), ref data);		//sizeof(NCTYPE_CAN_FRAME)
            CanReWrite.WriteCan(CanChannel, data, false);
            return;
        }
       
        #endregion
        private void frmMain_Load(object sender, EventArgs e)
        {
            if (this.DesignMode == true) return; //디자인 창 안뜨는것 해결함수라 한다.   

            txt_pass.Indicator.ColorActive = Color.Lime;
            txt_pass.Indicator.ColorInactive = Color.Silver;
            txt_pass.Indicator.ColorInactiveAuto = false;
            txt_pass.Value.AsBoolean = false;

            txt_reset.Indicator.ColorActive = Color.Yellow;
            txt_reset.Indicator.ColorInactive = Color.Silver;
            txt_reset.Indicator.ColorInactiveAuto = false;
            txt_reset.Value.AsBoolean = false;

            txt_prd.Indicator.ColorActive = Color.DeepSkyBlue;
            txt_prd.Indicator.ColorInactive = Color.Gray;
            txt_prd.Indicator.ColorInactiveAuto = false;
            txt_prd.Value.AsBoolean = false;

            led10.Indicator.ColorActive = Color.Red;
            led10.Indicator.ColorInactive = Color.Gray;
            led10.Indicator.ColorInactiveAuto = false;
            led10.Value.AsBoolean = false;

            led1.Indicator.ColorActive = Color.Lime;
            led1.Indicator.ColorInactive = Color.Silver;
            led1.Indicator.ColorInactiveAuto = false;
            led1.Value.AsBoolean = false;

            led2.Indicator.ColorActive = Color.Lime;
            led2.Indicator.ColorInactive = Color.Silver;
            led2.Indicator.ColorInactiveAuto = false;
            led2.Value.AsBoolean = false;

            led3.Indicator.ColorActive = Color.Lime;
            led3.Indicator.ColorInactive = Color.Silver;
            led3.Indicator.ColorInactiveAuto = false;
            led3.Value.AsBoolean = false;

            led4.Indicator.ColorActive = Color.Lime;
            led4.Indicator.ColorInactive = Color.Silver;
            led4.Indicator.ColorInactiveAuto = false;
            led4.Value.AsBoolean = false;

            led5.Indicator.ColorActive = Color.Lime;
            led5.Indicator.ColorInactive = Color.Silver;
            led5.Indicator.ColorInactiveAuto = false;
            led5.Value.AsBoolean = false;

            led6.Indicator.ColorActive = Color.Lime;
            led6.Indicator.ColorInactive = Color.Silver;
            led6.Indicator.ColorInactiveAuto = false;
            led6.Value.AsBoolean = false;

            led7.Indicator.ColorActive = Color.Lime;
            led7.Indicator.ColorInactive = Color.Silver;
            led7.Indicator.ColorInactiveAuto = false;
            led7.Value.AsBoolean = false;

            led8.Indicator.ColorActive = Color.Lime;
            led8.Indicator.ColorInactive = Color.Silver;
            led8.Indicator.ColorInactiveAuto = false;
            led8.Value.AsBoolean = false;

            Test_OK.Indicator.ColorActive = Color.Lime;
            Test_OK.Indicator.ColorInactive = Color.Silver;
            Test_OK.Indicator.ColorInactiveAuto = false;
            Test_OK.Value.AsBoolean = false;

            test_ing.Indicator.ColorActive = Color.Lime;
            test_ing.Indicator.ColorInactive = Color.Silver;
            test_ing.Indicator.ColorInactiveAuto = false;
            test_ing.Value.AsBoolean = false;

            txt_jig.Indicator.ColorActive = Color.Lime;
            txt_jig.Indicator.ColorInactive = Color.Silver;
            txt_jig.Indicator.ColorInactiveAuto = false;
            txt_jig.Value.AsBoolean = false;

            led11.Indicator.ColorActive = Color.Red;
            led11.Indicator.ColorInactive = Color.Black;
            led11.Indicator.ColorInactiveAuto = false;
            led11.Value.AsBoolean = false;

            led12.Indicator.ColorActive = Color.Red;
            led12.Indicator.ColorInactive = Color.Black;
            led12.Indicator.ColorInactiveAuto = false;
            led12.Value.AsBoolean = false;

            led13.Indicator.ColorActive = Color.Red;
            led13.Indicator.ColorInactive = Color.Black;
            led13.Indicator.ColorInactiveAuto = false;
            led13.Value.AsBoolean = false;


            led14.Indicator.ColorActive = Color.DeepSkyBlue;
            led14.Indicator.ColorInactive = Color.Silver;
            led14.Indicator.ColorInactiveAuto = false;
            led14.Value.AsBoolean = false;

            led15.Indicator.ColorActive = Color.DeepSkyBlue;
            led15.Indicator.ColorInactive = Color.Silver;
            led15.Indicator.ColorInactiveAuto = false;
            led15.Value.AsBoolean = false;

            TestOk_First = 0;
            TestOk_Last = 0;
#if !DEBUG_MODE

            IOPort = new IOControl(this);
            IOPort.IOInit(0);

            CanReWrite = new __CanControl(this);            

#endif
            //CanData.Clear();

            SendFailFlag = false;
            imageButton8.ButtonColor = Color.Lime;
            RunningFlag = false;
            ProgramStartFlag = true;
            Z_PosToDefailtOutToPLCFlag = false;

            SendList.Clear();

            read_Config();

#if !DEBUG_MODE
            if (AXTServo.Init(XOneCycleToPulse, YOneCycleToPulse, ZOneCycleToPulse, XOneCycleToStroke, YOneCycleToStroke, ZOneCycleToStroke) == false)
            {
                MessageBox.Show("서보모터 초기화 실패");
            }
            else
            {
                AXTServo.AxtMotorInit(ServoXDir, ServoYDir, ServoZDir);
            }
#endif
#if !DEBUG_MODE && !DEBUG_MODE

            X_servo_on();
            Y_servo_on();
            Z_servo_on();
#endif

            ScreenInit();
            read_combo();/// 모델 정보 읽기//
            comboBox1.SelectedItem = 0;
            read_model();

            DisplayStatus(RESULT_NONE);
            

            BTN_AU_MA.Text = "좌표 표시";
            panel6.Visible = true;
            plot1.Visible = true;
            BTN_AU_MA.BackColor = Color.Lime;
            
            comboBox1.SelectedIndex = 0;

            SerialOpen();

            //모션 보드를 사용하기 때문에 사용하지 않음
            //    comportopen2(); 
            TimeTIMER.Interval = Convert.ToInt32(servo_scan_time);
            TimeTIMER.Enabled = true;

            create_folder();

            comopen3();

            Log_SAVE("Program Start!!");

            Server.LocalPort = Convert.ToInt32(textBox52.Text);
            Server.Listen();
            Client.Close();
            Client.Connect(textBox53.Text, Convert.ToInt32(textBox54.Text));

            //MainTimer.Enabled = true;

            timer3.Enabled = true;
            timer2.Enabled = true;

            ProgramStartFlag = false;
            read_Position();
            DefaultServoOptOpen();
            COUNT_READ();
            ReadToLastBarcode();

#if DEBUG_MODE
            axfpSpread1.Visible = true;
            CreateDataFile();
#endif
            //SetExcelLayOut();
            //            SaveData(RESULT_REJECT);     
            timer1.Enabled = true;
            led9.Indicator.Text = "READY";
            //panel14.Visible =true;
            //ConnectorOutOnOff(ON);


            return;
        }

        public void SerialOpen()
        {
            try
            {
#if !DEBUG_MODE
                if (SP1.IsOpen)
                {
                    SP1.Close();

                    SP1.PortName = mt4y_port_number;
                    SP1.BaudRate = Convert.ToInt32(mt4y_port_buadrate);
                    SP1.Open();

                }
                else
                {
                    SP1.PortName = mt4y_port_number;
                    SP1.BaudRate = Convert.ToInt32(mt4y_port_buadrate);
                    SP1.Open();

                    timer2.Interval = Convert.ToInt32(mt4y_scan_time);
                    timer2.Enabled = true;
                }
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        
        public void comopen3()
        {
            try
            {
#if !DEBUG_MODE
                if (serialPort1.IsOpen)       //Loadcell BS-3520
                {
                    serialPort1.Close();
                }

#if DE_CAR
                serialPort1.DataBits = 7;
#else
                serialPort1.DataBits = 8;
#endif
                serialPort1.PortName = loadcell_port_number;
                serialPort1.BaudRate = Convert.ToInt32(loadcell_port_buadrate);
                serialPort1.Open();

                serialPort1.DiscardInBuffer();
                serialPort1.ReceivedBytesThreshold = 13;
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }



        string mt4y_port_buadrate; //MT4Y port buadrate
        string mt4y_port_number; //MT4Y port number
        string mt4y_scan_time; //MT4Y scan time
        //string servo_port_buadrate; //servo port buadrate
        //string servo_port_number; //servo port number
        string servo_scan_time;   //servo scan time
        string CAN_number;         //CAN number
        string loadcell_port_buadrate; //Loadcell Port buadrate
        string loadcell_port_number; //Loadcell Port number
        //string loadcell_scan_time; //Loadcell scan time
        //string z_servo_limite;    //Z축 최대거리(mm)
        //string loadcell_max;      //LoadCell 최대중량(Kg)

        public void read_Config()
        {
            try
            {
                textBox51.Text = Ods.Read_SET("Config", "B_WARR CHK");    //Server IP
                textBox52.Text = Ods.Read_SET("Config", "B_SENS CHK");      //Server Port 
                textBox53.Text = Ods.Read_SET("Config", "WCS CHK");          //Client IP
                textBox54.Text = Ods.Read_SET("Config", "chk_SET");      //Client Port
                linenum = Ods.Read_SET("Config", "chk_M1");     //라인번호
                linecom = Ods.Read_SET("Config", "chk_M2");    //장비번호

                mt4y_port_buadrate = Ods.Read_SET("Config", "ID_SET"); //MT4Y port buadrate
                mt4y_port_number = Ods.Read_SET("Config", "ID_M1"); //MT4Y port number
                mt4y_scan_time = Ods.Read_SET("Config", "ID_M2"); //MT4Y scan time
                //servo_port_buadrate = Ods.Read_SET("Config", "ID_P1"); //servo port buadrate
                //servo_port_number = Ods.Read_SET("Config", "ID_P2"); //servo port number

                servo_scan_time = Ods.Read_SET("Config", "STB_SET"); //servo scan time
                CAN_number = Ods.Read_SET("Config", "STB_M1");   //can number
                loadcell_port_buadrate = Ods.Read_SET("Config", "STB_M2");    //Loadcell Port buadrate
                loadcell_port_number = Ods.Read_SET("Config", "STB_P1");    //Loadcell Port number
                //loadcell_scan_time = Ods.Read_SET("Config", "STB_P2");    //Loadcell scan time

                ServoXDir = Convert.ToInt32(Ods.Read_SET("Config", "X_DIR"));
                ServoYDir = Convert.ToInt32(Ods.Read_SET("Config", "Y_DIR"));
                ServoZDir = Convert.ToInt32(Ods.Read_SET("Config", "Z_DIR"));

                XOneCycleToPulse = Convert.ToDouble(Ods.Read_SET("Config", "X_ONECYCELTOPULSE"));
                YOneCycleToPulse = Convert.ToDouble(Ods.Read_SET("Config", "Y_ONECYCELTOPULSE"));
                ZOneCycleToPulse = Convert.ToDouble(Ods.Read_SET("Config", "Z_ONECYCELTOPULSE"));

                string aa;

                aa = Ods.Read_SET("Config", "chk_P2");    //NOTE pc
                if (aa == "1")
                {
                    checkBox8.Checked = true;
                }
                else
                {
                    checkBox8.Checked = false;
                }

                aa = Ods.Read_SET("Config", "NotLoadCheck");    //노트 PC 일때 무부하 검사 유무
                if (aa == "1")
                {
                    NotLoadToTest = true;
                }
                else
                {
                    NotLoadToTest = false;
                }


                ModelText = Ods.Read_SET("Config", "MODEL TEXT");
                if (ModelText == "")
                {
                    ModelText = "DE";
                }

                aa = Ods.Read_SET("Config", "SBR OFFSET STOKE");
                if (aa == "")
                    SBR_OffsetStroke = 0;
                else SBR_OffsetStroke = (float)GetToNumDouble(aa, false, false);

                aa = Ods.Read_SET("Config", "SBR OFFSET LOAD");
                if (aa == "")
                    SBR_OffsetLoad = 0;
                else SBR_OffsetLoad = (float)GetToNumDouble(aa, false, false);

                aa = Ods.Read_SET("Config", "ODS OFFSET STOKE");
                if (aa == "")
                    ODS_OffsetStroke = 0;
                else ODS_OffsetStroke = (float)GetToNumDouble(aa, false, false);

                aa = Ods.Read_SET("Config", "ODS OFFSET LOAD");
                if (aa == "")
                    ODS_OffsetLoad = 0;
                else ODS_OffsetLoad = (float)GetToNumDouble(aa, false, false);


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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }


        public void read_model()
        {
            try
            {
                string aa;


                button78.Text = Ods.Read_SET("TEST SPEC", "SLIDE SPEC MIN");     //SBR판정값
                textBox39.Text = Ods.Read_SET("TEST SPEC", "SLIDE SPEC MAX");    //SBR판정값 offset

                textBox32.Text = Ods.Read_SET("TEST SPEC", "WARMER SPEC MIN");  //Img Capacitance Value Load Seat
                textBox31.Text = Ods.Read_SET("TEST SPEC", "WARMER SPEC MAX");  //real Capacitance Value

                button66.Text = Ods.Read_SET("TEST SPEC", "B_SENS SPEC MAX");    //ROM ID


                //button69.Text = Ods.Read_SET("TEST SPEC", "WCS NO");                  //Vehicle ID
                textBox8.Text = Ods.Read_SET("TEST SPEC", "WCS PART");       //Img Capacitance Value Empty Seat
                textBox11.Text = Ods.Read_SET("TEST SPEC", "WCS LOT");       //real Capacitance Value
                button43.Text = Ods.Read_SET("TEST SPEC", "WCS VEHICLE");                //Cal Checksum
                button67.Text = Ods.Read_SET("TEST SPEC", "WCS SOFTVER");               //OPR R3 number


                ODS_HeaterImageName = Ods.Read_SET("TEST SPEC", "ODS HEATERIMAGE");
                ODS_NoneHeaterImageName = Ods.Read_SET("TEST SPEC", "ODS NONEHEATERIMAGE");
                SBR_ImageName = Ods.Read_SET("TEST SPEC", "SBR IMAGE");

                string xModelText = Ods.Read_SET("Config", "MODEL TEXT");


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
                    SBR_ImageName = Application.StartupPath + "\\Image" + "\\" + xModelText + "_SBR.jpg";
                }


                aa = Ods.Read_SET("TEST SPEC", "ODS CON_DATA");

                ConnectionData = GetToNumByte(aa, true, true);
                button69.Text = aa;

                aa = Ods.Read_SET("TEST SPEC", "ConnectorCheck");    //NOTE PC
                if (aa == "1")
                {
                    ConnectionCheckFlag = true;
                }
                else
                {
                    ConnectionCheckFlag = false;
                }

                button81.Text = Ods.Read_SET("TEST SPEC", "SB_SET");       //LoadCell 최대 중량(Kq) for SBR
                button61.Text = Ods.Read_SET("TEST SPEC", "SB_M1");          //LoadCell 최대중량(Kg) for ODS
                textBox12.Text = Ods.Read_SET("TEST SPEC", "SB_M2");          //LoadCell 무반응(Kg) for SBR
                //                textBox13.Text = Ods.Read_SET("TEST SPEC", "SB_P1");          //무반응 검사 Z축 복귀위치 for SBR



                button90.Text = Ods.Read_SET("TEST SPEC", "ID_1");

                textBox57.Text = Ods.Read_SET("TEST SPEC", "ID_2");

                textBox60.Text = Ods.Read_SET("TEST SPEC", "ID_3");

                textBox62.Text = Ods.Read_SET("TEST SPEC", "GIG");          //Img Capacitance Value1 min  Empty

                textBox61.Text = Ods.Read_SET("TEST SPEC", "UMTYPE");       //real Capacitance Value1 min  Empty

                textBox64.Text = Ods.Read_SET("TEST SPEC", "I_C_V_M_L");          //Img Capacitance Value1 min  Load

                textBox63.Text = Ods.Read_SET("TEST SPEC", "R_C_V_M_L");       //real Capacitance Value1 min  Load               


                textBox76.Text = Ods.Read_SET("TEST SPEC", "NON_GIG");          //NON_ Img Capacitance Value1 min  Empty

                textBox75.Text = Ods.Read_SET("TEST SPEC", "NON_UMTYPE");       //NON_ real Capacitance Value1 min  Empty

                textBox74.Text = Ods.Read_SET("TEST SPEC", "NON_I_C_V_M_L");          //NON_ Img Capacitance Value1 min  Load

                textBox73.Text = Ods.Read_SET("TEST SPEC", "NON_R_C_V_M_L");       //NON_ real Capacitance Value1 min  Load

                textBox78.Text = Ods.Read_SET("TEST SPEC", "NON_GIG_H");          //NON_ Img Capacitance Value1 max  Empty

                textBox77.Text = Ods.Read_SET("TEST SPEC", "NON_UMTYPE_H");       //NON_ real Capacitance Value1 max  Empty

                textBox80.Text = Ods.Read_SET("TEST SPEC", "NON_I_C_V_M_L_H");          //NON_ Img Capacitance Value1 max  Load

                textBox79.Text = Ods.Read_SET("TEST SPEC", "NON_R_C_V_M_L_H");       //NON_ real Capacitance Value1 max  Load

                aa = "";
                aa = Ods.Read_SET("TEST SPEC", "READCOUNT");       //ods data read count

                if (aa == "")
                    OdsDataSearchCount = 5;
                else OdsDataSearchCount = Convert.ToInt32(aa);

                if (OdsDataSearchCount <= 0) OdsDataSearchCount = 5;


                aa = Ods.Read_SET("MEMORY", "READ");       //ODS Memory Read
                if (aa == "1")
                    ReadMemory = true;
                else ReadMemory = false;

                aa = Ods.Read_SET("MEMORY", "CONTENTS");       //ODS Read Contents
                if (aa == "1")
                    ReadContents = true;
                else ReadContents = false;

                //----------------------------------------------------------------[통풍 히터]
                SpecList.Clear();
                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "WARMER SPEC MIN")); //Img Capacitance Value1 max Load Seat
                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "WARMER SPEC MAX")); //real Capacitance Value2 max Load Seat

                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "B_SENS SPEC MAX"));   //rom id
                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "WCS PART"));      //Img Capacitance Value1 max  Empty
                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "WCS LOT"));       //real Capacitance Value2 max
                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "WCS VEHICLE"));   //Cal Checksum
                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "WCS SOFTVER"));   //OPR R3 number
                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "WCS PARAVER"));   //Lock Byte

                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "ODS CON_DATA"));   //Connection Data , DE - 0xb0 , YB - 0xb2                        


                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "ID_1"));          //Cal checksum Non-heater type

                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "GIG"));          //Img Capacitance Value1 min  Empty

                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "UMTYPE"));       //real Capacitance Value1 min  Empty

                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "I_C_V_M_L"));          //Img Capacitance Value1 min  Load

                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "R_C_V_M_L"));       //real Capacitance Value1 min  Load

                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "NON_GIG"));          //NON_ Img Capacitance Value1 min  Empty

                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "NON_UMTYPE"));       //NON_ real Capacitance Value1 min  Empty

                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "NON_I_C_V_M_L"));          //NON_ Img Capacitance Value1 min  Load

                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "NON_R_C_V_M_L"));       //NON_ real Capacitance Value1 min  Load

                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "NON_GIG_H"));          //NON_ Img Capacitance Value1 max  Empty

                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "NON_UMTYPE_H"));       //NON_ real Capacitance Value1 max  Empty

                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "NON_I_C_V_M_L_H"));          //NON_ Img Capacitance Value1 max  Load

                SpecList.Add(Ods.Read_SET("VENT_HETER SPEC", "NON_R_C_V_M_L_H"));       //NON_ real Capacitance Value1 max  Load

                CheckToNotePC();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public void read_Position()
        {
            try
            {
                string aa;

                displayString1.Value.AsString = comboBox1.Text;
                textBox47.Text = Ods.Read_SET(comboBox1.Text, "SLIDE CHK");     //Y축 검사위치1 for SBR

                textBox44.Text = Ods.Read_SET(comboBox1.Text, "TILT CHK");      //Y축 검사위치2 for SBR

                textBox43.Text = Ods.Read_SET(comboBox1.Text, "HEIGHT CHK");    //Y축 검사위치3 for SBR

                textBox46.Text = Ods.Read_SET(comboBox1.Text, "WARMER CHK");    //Z축 복귀위치 for SBR

                textBox45.Text = Ods.Read_SET(comboBox1.Text, "VENT CHK");      //Z축 검사위치 for SBR


                textBox33.Text = Ods.Read_SET(comboBox1.Text, "SLIDE SECT MIN"); //X축 복귀위치

                textBox34.Text = Ods.Read_SET(comboBox1.Text, "SLIDE SECT MAX"); //X축 검사위치

                textBox36.Text = Ods.Read_SET(comboBox1.Text, "TILT SECT MIN"); //Y축 복귀위치
                textBox35.Text = Ods.Read_SET(comboBox1.Text, "TILT SECT MAX"); //Y축 검사위치1

                textBox38.Text = Ods.Read_SET(comboBox1.Text, "HEIGHT SECT MIN");   //Z축 복귀위치
                textBox37.Text = Ods.Read_SET(comboBox1.Text, "HEIGHT SECT MAX");   //Z축 검사위치


                button78.Text = Ods.Read_SET(comboBox1.Text, "SLIDE SPEC MIN");     //SBR판정값
                textBox39.Text = Ods.Read_SET(comboBox1.Text, "SLIDE SPEC MAX");    //SBR판정값 offset
                textBox40.Text = Ods.Read_SET(comboBox1.Text, "TILT SPEC MIN");     //Y축 검사위치2
                textBox41.Text = Ods.Read_SET(comboBox1.Text, "TILT SPEC MAX");     //Y축 검사위치3
                textBox50.Text = Ods.Read_SET(comboBox1.Text, "HEIGHT SPEC MIN");   //X축 복귀위치 for SBR
                textBox49.Text = Ods.Read_SET(comboBox1.Text, "HEIGHT SPEC MAX");   //X축 검사위치 for SBR

                textBox26.Text = Ods.Read_SET(comboBox1.Text, "VENT SPEC MIN");      //Z축 검사시간 for ODS
                textBox42.Text = Ods.Read_SET(comboBox1.Text, "VENT SPEC MAX");      //Z축 검사시간 for SBR

                textBox48.Text = Ods.Read_SET(comboBox1.Text, "B_SENS SPEC MIN");    //Y축 복귀위치 for SBR

                aa = Ods.Read_SET(comboBox1.Text, "chk_P1");    //SBR 검사 확인
                if (aa == "1")
                {
                    SbrOrOds = true;
                }
                else
                {
                    SbrOrOds = false;
                }

                //button81.Text = Ods.Read_SET(comboBox1.Text, "SB_SET");       //LoadCell 최대 중량(Kq) for SBR
                //button61.Text = Ods.Read_SET(comboBox1.Text, "SB_M1");          //LoadCell 최대중량(Kg) for ODS

                textBox13.Text = Ods.Read_SET(comboBox1.Text, "SB_P1");          //무반응 검사 Z축 복귀위치 for SBR

                textBox60.Text = Ods.Read_SET(comboBox1.Text, "ID_3");

                DisplaySpec();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }

        private void BTN_AU_MA_Click(object sender, EventArgs e)
        {
            return;
        }

      
        public void read_combo()
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
        }

        private void StateTimer_Tick(object sender, EventArgs e)
        {
            StateTimer.Enabled = false;
            //if (timer1busy == false)
            //{
            create_folder();
            //timer1busy = true;
            //timer1busy = false;
            //}

            if (IOPort.inportb((short)DI_IN.HeaterConnection) == true)
            {
                if (led11.Value.AsBoolean == false)
                {
                    led11.Value.AsBoolean = true;
                }

                if (led13.Value.AsBoolean == false)
                {
                    led13.Value.AsBoolean = true;
                }

                if (led10.Value.AsBoolean == false)
                {
                    led10.Value.AsBoolean = true;
                }
            }
            else
            {
                if (led11.Value.AsBoolean == true)
                {
                    led11.Value.AsBoolean = false;
                }

                if (led13.Value.AsBoolean == true)
                {
                    led13.Value.AsBoolean = false;
                }
                if (led10.Value.AsBoolean == true)
                {
                    led10.Value.AsBoolean = false;
                }
            }
            StateTimer.Enabled = true;
            return;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ProgramStartFlag == true) return;

            ModelChange();
            return;
        }

        private void button8_Click(object sender, EventArgs e)
        {

        }

        /*
        private const int pass_p = 0;
        private const int reset_p = 1;
        private const int emg_sw_p = 2;
        private const int lh_sel_p = 3;
        private const int HeaterConnection = 4;
        private const int auto_manual_p = 5;
        private const int x_left_p = 6;
        private const int x_right_p = 7;
        private const int y_fwd_p = 8;
        private const int y_bwd_p = 9;
        private const int z_up_p = 10;
        private const int z_down_p = 11;
        private const int jig_up_p = 12;
        private const int spare_in_p = 13;
        private const int seat_det_p = 14;
        private const int turn_p = 16;
        private const int return_p = 17;
        private const int unlock_p = 19;
        private const int lock_p = 18;
        */
        /*
        private const int bz_op = 0;
        private const int lamp_g_op = 1;
        private const int lamp_y_op = 2;
        private const int lamp_r_op = 3;
        private const int test_ok_op = 4;
        private const int test_out_op = 5;
        private const int seat_det_out_op = 7;
        private const int ods_power_op = 8;
        private const int turn_op = 9;
        private const int return_op = 10;
        private const int unlock_op = 11;
        */

        private bool Z_PosToDefailtOutToPLCFlag = false;
        private short DefaultConnectionMsg = 0;
        private UInt16 Z_DefaultPos;
        private bool SettingMotorPosFlag = false;
        public static string JogSpeed;



        public int ServoXDir = 4;//TwoCcwCwHigh
        public int ServoYDir = 4;//TwoCcwCwHigh
        public int ServoZDir = 6;//TwoCwCCwHigh
        public double XOneCycleToPulse = 8000.0;
        public double YOneCycleToPulse = 8000.0;
        public double ZOneCycleToPulse = 8000.0;
        public const float XOneCycleToStroke = 5F;
        public const float YOneCycleToStroke = 5F;
        public const float ZOneCycleToStroke = 5F;

        private enum DO_OUT
        {
            MONITOR_ONOFF,
            RESERVED2,
            RESERVED3,
            Z_MOTOR_ORG,
            TEST_END,
            TEST_ORIGIN,//plc 로 대기 위치여부를 준다.
            TEST_ING,
            PRODUCT_IN,
            ODS_POWER,
            SERVO_Z_BRACK,
            RESERVED4,
            RESERVED5,
            BUZZER,
            LAMP_GREEN,
            LAMP_YELLOW,
            LAMP_RED
        };
        private enum DI_IN
        {
            pass_p = 0,
            reset_p = 1,
            emg_sw_p = 2,
            HeaterConnection = 3,
            rh_sel_p = 4,
            auto_manual_p = 5,
            x_left_p = 6,
            x_right_p = 7,
            y_fwd_p = 8,
            y_bwd_p = 9,
            z_up_p = 10,
            z_down_p = 11,
            jig_up_p = 12,
            lh_sel_p = 13,
            product_in = 14,
            power_off = 15,
            return_p = 16,
            unlock_p = 17,
            lock_p = 18
        }

        private int OrgCount;

        private long PopFailFirst = 0;
        private long PopFailLast = 0;
        private bool PopFailCheckFlag = false;

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            MainTimer.Enabled = false;

            if ((button82.BackColor == Color.Orange) && (button83.BackColor == Color.Orange))
            {
                PopFailCheckFlag = false;
                //POP 연결이 잘 된 상태
                if (panel17.Visible == true)
                {
                    if ((ODS_HeaterPanel.Visible == false) && (ODS_NoneHeaterPanel.Visible == false) && (SBR_Panel.Visible == false))
                    {
                        ConnectingMsgDisplay(DefaultConnectionMsg, true);
                    }
                    panel17.Visible = false;
                    PopFailFirst = ComF.timeGetTimems();
                    PopFailLast = ComF.timeGetTimems();
                    label160.BackColor = Color.Red;
                }
            }
            else
            {
                //POP 연결이 잘 안된 상태
                if (PopFailCheckFlag == false)
                {
                    if (panel17.Visible == false)
                    {
                        panel17.Visible = true;
                        panel17.BringToFront();

                        if ((ODS_HeaterPanel.Visible == true) || (ODS_NoneHeaterPanel.Visible == true) || (SBR_Panel.Visible == true))
                        {
                            ConnectingMsgDisplay(DefaultConnectionMsg, false);
                        }
                    }
                    else
                    {
                        PopFailLast = ComF.timeGetTimems();
                        if (label160.BackColor == Color.Red)
                        {
                            if (500 <= (PopFailLast - PopFailFirst))
                            {
                                PopFailFirst = ComF.timeGetTimems();
                                PopFailLast = ComF.timeGetTimems();
                                label160.BackColor = Color.DarkRed;
                            }
                        }
                        else
                        {
                            if (700 <= (PopFailLast - PopFailFirst))
                            {
                                PopFailFirst = ComF.timeGetTimems();
                                PopFailLast = ComF.timeGetTimems();
                                label160.BackColor = Color.Red;
                            }
                        }
                    }
                }
            }



            if (panel5.Visible == true)
            {
                OrgLast = (long)ComF.timeGetTimems();
                if ((OrgMoveMode == 0) || (7000 <= (OrgLast - OrgFirst)))
                {
                    if (OrgMoveMode == 0)
                    {
#if !DEBUG_MODE
                        if ((AXTServo.OriginMoveEnd(MOTOR_Z) == true) || (7000 <= (OrgLast - OrgFirst)))
                        {
                            if (label126.Text != "X/Y 축 원점 복귀중")
                            {
                                AXTServo.AxtClearPosition(MOTOR_Z);
                                OrgCount = 0;
                                SerovoAllOrgMove2();
                                ComF.timedelay(1000);
                            }
                        }
#else
                        OrgCount = 0;
                        SerovoAllOrgMove2();
#endif
                    }
                    else
                    {
#if !DEBUG_MODE
                        if ((AXTServo.OriginMoveEnd(MOTOR_X) == true) && (AXTServo.OriginMoveEnd(MOTOR_Y) == true) && (AXTServo.OriginMoveEnd(MOTOR_Z) == true))
                        {
                            if (10 <= OrgCount)
                            {
                                AXTServo.AxtClearPosition(MOTOR_X);
                                AXTServo.AxtClearPosition(MOTOR_Y);
                                panel5.Visible = false;

                                IOPort.outportb((short)DO_OUT.LAMP_RED, false);
                                IOPort.outportb((short)DO_OUT.LAMP_GREEN, false);
                                IOPort.outportb((short)DO_OUT.LAMP_YELLOW, true);
                                Z_PosToDefailtOutToPLC(true);
                                OrgZSettingFlag = false;
                                //xForm.Close();                            
                                imageButton6.Enabled = true;
                                btn_STOP.Enabled = true;
                                checkBox9.Enabled = true;
                                test_process_stop = false;
                                test_process_estop = false;
                                test_process_run = false;
                                if (FirstModelChangeFlag == true)
                                {
                                    ModelName = "";
                                    ModelChange();
                                }
                                FirstModelChangeFlag = false;
                            }
                            else
                            {
                                OrgCount++;
                            }
                        }
#else
                        Z_PosToDefailtOutToPLC(true);
                        panel5.Visible = false;
                        OrgZSettingFlag = false;
                        //xForm.Close();                            
                        button24.Enabled = true;
                        btn_STOP.Enabled = true;
                        checkBox9.Enabled = true;
                        test_process_stop = false;
                        test_process_estop = false;
                        test_process_run = false;
                        if (FirstModelChangeFlag == true)
                        {
                            ModelName = "";
                            ModelChange();
                        }
                        FirstModelChangeFlag = false;
#endif
                    }
                }
                else
                {
#if !DEBUG_MODE
                    if ((AXTServo.OriginMoveEnd(MOTOR_X) == true) && (AXTServo.OriginMoveEnd(MOTOR_Y) == true) && (AXTServo.OriginMoveEnd(MOTOR_Z) == true))
                    {
                        if (10 <= OrgCount)
                        {
                            AXTServo.AxtClearPosition(MOTOR_X);
                            AXTServo.AxtClearPosition(MOTOR_Y);
                            Z_PosToDefailtOutToPLC(true);
                            panel5.Visible = false;
                            IOPort.outportb((short)DO_OUT.LAMP_RED, false);
                            IOPort.outportb((short)DO_OUT.LAMP_GREEN, false);
                            IOPort.outportb((short)DO_OUT.LAMP_YELLOW, true);
                            OrgZSettingFlag = false;
                            //xForm.Close();                            
                            imageButton6.Enabled = true;
                            btn_STOP.Enabled = true;
                            checkBox9.Enabled = true;
                            test_process_stop = false;
                            test_process_estop = false;
                            test_process_run = false;
                            if (FirstModelChangeFlag == true)
                            {
                                ModelName = "";
                                ModelChange();
                            }
                            FirstModelChangeFlag = false;
                        }
                        else
                        {
                            OrgCount++;
                        }
                    }
#else
                    Z_PosToDefailtOutToPLC(true);
                    panel5.Visible = false;

                    OrgZSettingFlag = false;
                    //xForm.Close();                            
                    button24.Enabled = true;
                    btn_STOP.Enabled = true;
                    checkBox9.Enabled = true;
                    test_process_stop = false;
                    test_process_estop = false;
                    test_process_run = false;
                    if (FirstModelChangeFlag == true)
                    {
                        ModelName = "";
                        ModelChange();
                    }
                    FirstModelChangeFlag = false;
#endif

                }
            }

            if (Z_PosToDefailtOutToPLCFlag == true)
            {
                if ((kMotorPos[MOTOR_Z] <= (Z_DefaultPos + 1)) && ((Z_DefaultPos - 1) <= kMotorPos[MOTOR_Z]))
                {
                    Z_PosToDefailtOutToPLCFlag = false;
                    Z_PosToDefailtOutToPLC(true);
                }
                else
                {
                    if (kMotorPos[MOTOR_Z] <= (Z_DefaultPos + 1))
                    {
                        Z_PosToDefailtOutToPLCFlag = false;
                        Z_PosToDefailtOutToPLC(true);
                    }
                }
            }

            if (EStopCheck() == true)
            {
                if (test_process_estop == false)
                {
                    AllMotorStop();
                }
                test_process_estop = true;
                OrgZSettingFlag = false;
            }
            /*
            else
            {
                if (test_process_estop == true)
                {
                    P32C32_DI[emg_sw_p] = false; //간혹 설정이 되지 않는 경우가 있어 이곳에서도 설정을 해 준다.
                }
            }*/


            //제품이 없을 때 카운터 초기화를 위한 날짜 검사를 진행한다.
            if (test_process_run == false)
            {
                CheckDate();
            }

            if ((panel14.Visible == true) && (TestOkOnFlag == false))
            {
                TestOk_First = ComF.timeGetTimems();
                TestOk_Last = ComF.timeGetTimems();
            }

            if (IOPort.inportb((short)DI_IN.pass_p) == true)
            {
                if (txt_pass.Value.AsBoolean == false) txt_pass.Value.AsBoolean = true;

                if ((panel14.Visible == true) && (txt_prd.Value.AsBoolean == true) && (TestOkOnFlag == false))
                {
                    //if (button51.Text != "N.G")
                    if(test_process_run == false)
                    {
                        if (ProductResult != RESULT_REJECT)
                        {
                            ConnectorOutOnOff(OFF);
                            test_ing.Value.AsBoolean = false;
                            IOPort.outportb((short)DO_OUT.TEST_ING, false);
                            IOPort.outportb((short)DO_OUT.TEST_END, true);

                            Test_OK.Value.AsBoolean = true;
                            TestOkOnFlag = true;

                            TestOk_First = ComF.timeGetTimems();
                            TestOk_Last = ComF.timeGetTimems();
                        }
                    }
                }
                else if ((IOPort.inportb((short)DI_IN.jig_up_p) == true) && (IOPort.inportb((short)DI_IN.product_in) == true) && (TestOkOnFlag == true))
                {
                    // 지그가 상승 되어 있고 제품이 있고 test ok 신호가 나간적이 있으면
                    // 그때 다시 Pass 버튼이 눌리면 test ok 신호를 다시 내 보낸다.
                    if (test_process_run == false)
                    {
                        if (ProductResult != RESULT_REJECT)
                        {
                            ConnectorOutOnOff(OFF);
                            test_ing.Value.AsBoolean = false;
                            IOPort.outportb((short)DO_OUT.TEST_ING, false);
                            IOPort.outportb((short)DO_OUT.TEST_END, true);

                            Test_OK.Value.AsBoolean = true;
                            TestOkOnFlag = true;

                            TestOk_First = ComF.timeGetTimems();
                            TestOk_Last = ComF.timeGetTimems();
                        }
                    }
                }
                else
                {
                    if ((PopToSbrOdsSelect == true) && ((led1.Value.AsBoolean == true) || (led2.Value.AsBoolean == true))) // sbr, ods 중 한 제품일경우
                    {
                        if (test_process_run == false)
                        {
                            if (TestOkOnFlag == false) // 제품 검사 종료 신호가 꺼져 있을때 (즉 제품이 바꿔었을때)
                            {
                                if (ProductOutFlag == false)
                                {
                                    //제품이 바뀌었을때
                                    PopToSbrOdsSelect = false;
                                    test_process_RUN();
                                }
                            }
                        }
                    }
                    else
                    {
                        if ((TestOkOnFlag == false) && (ProductOutFlag2 == false))
                        {
                            // ods, sbr 어떤 제품도 아닐경우 
                            ConnectorOutOnOff(OFF);
                            test_ing.Value.AsBoolean = false;
                            IOPort.outportb((short)DO_OUT.TEST_ING, false);
                            IOPort.outportb((short)DO_OUT.TEST_END, true);

                            Test_OK.Value.AsBoolean = true;
                            TestOkOnFlag = true;
                            TestOk_First = ComF.timeGetTimems();
                            TestOk_Last = ComF.timeGetTimems();
                        }
                        else if ((test_process_run == false) && (TestOkOnFlag == false) && (txt_prd.Value.AsBoolean == true) && (ProductResult != RESULT_REJECT) && ((led1.Value.AsBoolean == true) || (led2.Value.AsBoolean == true)) && (ProductOutFlag2 == true)) //간혹 양품이 빠지지 않을경우가 있어서
                        {
                            // ods, sbr 어떤 제품도 아닐경우 
                            ConnectorOutOnOff(OFF);
                            test_ing.Value.AsBoolean = false;
                            IOPort.outportb((short)DO_OUT.TEST_ING, false);
                            IOPort.outportb((short)DO_OUT.TEST_END, true);

                            Test_OK.Value.AsBoolean = true;
                            TestOkOnFlag = true;
                            TestOk_First = ComF.timeGetTimems();
                            TestOk_Last = ComF.timeGetTimems();
                        }
                    }
                }
            }
            else
            {
                if (txt_pass.Value.AsBoolean == true) txt_pass.Value.AsBoolean = false;

            }

            if (IOPort.inportb((short)DI_IN.reset_p) == true)
            {
                if (txt_reset.Value.AsBoolean == false)
                {
                    if (test_process_run == false)
                    {
                        if (panel14.Visible == true)
                        {
                            //panel14.Visible = false;
                            ConnectorOutOnOff(OFF);
                            test_ing.Value.AsBoolean = false;
                            IOPort.outportb((short)DO_OUT.TEST_ING, false);
                            IOPort.outportb((short)DO_OUT.TEST_END, false);
                            Test_OK.Value.AsBoolean = false;
                            PopToSbrOdsSelect = true;
                        }
                    }
                }
                if (Test_OK.Value.AsBoolean == true)
                {
                    IOPort.outportb((short)DO_OUT.TEST_END, false);
                    Test_OK.Value.AsBoolean = false;
                }
                ProductOutFlag = false;
                ProductOutFlag2 = false;

                if (txt_reset.Value.AsBoolean == false) txt_reset.Value.AsBoolean = true;
            }
            else
            {
                if (txt_reset.Value.AsBoolean == true) txt_reset.Value.AsBoolean = false;

            }

            if (IOPort.inportb((short)DI_IN.emg_sw_p) == true)
            {
                if (imageButton5.BackColor != SystemColors.Control) imageButton5.BackColor = SystemColors.Control;
            }
            else
            {
                if (OrgZSettingFlag == false)
                {
                    if (imageButton5.BackColor != Color.Red)
                    {
                        imageButton5.BackColor = Color.Red;

                        X_servo_off();
                        Y_servo_off();
                        Z_servo_off();

                        button39.BackColor = SystemColors.Control;

                        //IOPort.outportb((short)DO_OUT.LAMP_RED, false);
                        //IOPort.outportb((short)DO_OUT.LAMP_GREEN, false);
                        //IOPort.outportb((short)DO_OUT.LAMP_YELLOW, false);

                        IOPort.outportb((short)DO_OUT.ODS_POWER, false);

                        FirstMotorOrgRunFlag = true;
                    }
                }
            }

            if ((Test_OK.Value.AsBoolean == true) && (TestOkOnFlag == true))
            {
                TestOk_Last = ComF.timeGetTimems();
                if (1000 <= (TestOk_Last - TestOk_First))
                {
                    IOPort.outportb((short)DO_OUT.TEST_END, false);
                    Test_OK.Value.AsBoolean = false;
                    TestOkOnFlag = false;
                    ConnectorOutOnOff(OFF);
                }
            }



            if ((IOPort.inportb((short)DI_IN.auto_manual_p) == false) || (SettingMotorPosFlag == true))
            {
                if (SettingMotorPosFlag == true)
                {
                    if (JogSpeed != textBox83.Text) textBox83.Text = JogSpeed;
                }
                if (led9.Indicator.Text != "AUTO")
                {
                    led9.Indicator.Text = "AUTO";
                    groupBox6.Enabled = false;
                    tabControl1.SelectedIndex = 0;

                    IOPort.outportb((short)DO_OUT.LAMP_YELLOW, true);
                    IOPort.outportb((short)DO_OUT.LAMP_GREEN, false);
                    IOPort.outportb((short)DO_OUT.LAMP_RED, false);
                    BTN_AU_MA.Text = "검사모드";

                    panel6.Visible = true;
                    plot1.Visible = true;
                    BTN_AU_MA.BackColor = Color.LightGray;
                    ConnectingMsgDisplay(DefaultConnectionMsg, true);
                }
            }
            else
            {
                if (led9.Indicator.Text != "MANUAL")
                {
                    led9.Indicator.Text = "MANUAL";
                    groupBox6.Enabled = true;
                    tabControl1.SelectedIndex = 1;

                    IOPort.outportb((short)DO_OUT.LAMP_YELLOW, true);
                    IOPort.outportb((short)DO_OUT.LAMP_GREEN, false);
                    IOPort.outportb((short)DO_OUT.LAMP_RED, false);
                    BTN_AU_MA.Text = "좌표 표시";
                    panel6.Visible = false;
                    plot1.Visible = false;
                    BTN_AU_MA.BackColor = Color.Lime;
                    ConnectingMsgDisplay(DefaultConnectionMsg, false);
                    ConnectingMsgDisplay(1, false);
                }
            }

            if ((led9.Indicator.Text == "MANUAL") || (SettingMotorPosFlag == true))
            {
                //수동 모드에서만 조그 기능을 먹도록 했다.
                if (IOPort.inportb((short)DI_IN.x_left_p) == true)
                {
#if !DEBUG_MODE
                    if (button47.BackColor != Color.Lime) AXTServo.AxtJogLeftMove(MOTOR_X, Convert.ToDouble(textBox83.Text));
#endif
                    button47.BackColor = Color.Lime;
                }
                else
                {
#if !DEBUG_MODE
                    if (button47.BackColor == Color.Lime) AXTServo.Axt_EStop(MOTOR_X);
#endif

                    button47.BackColor = SystemColors.Control;

                }

                if (IOPort.inportb((short)DI_IN.x_right_p) == true)
                {
#if !DEBUG_MODE
                    if (button56.BackColor != Color.Lime) AXTServo.AxtJogRightMove(MOTOR_X, Convert.ToDouble(textBox83.Text));
#endif
                    button56.BackColor = Color.Lime;
                }
                else
                {
#if !DEBUG_MODE
                    if (button56.BackColor == Color.Lime) AXTServo.Axt_EStop(MOTOR_X);
#endif
                    button56.BackColor = SystemColors.Control;
                }

                if (IOPort.inportb((short)DI_IN.y_fwd_p) == true)
                {
#if !DEBUG_MODE
                    if (button35.BackColor != Color.Lime) AXTServo.AxtJogForwordMove(MOTOR_Y, Convert.ToDouble(textBox83.Text));
#endif
                    button35.BackColor = Color.Lime;
                }
                else
                {
#if !DEBUG_MODE
                    if (button35.BackColor == Color.Lime) AXTServo.Axt_EStop(MOTOR_Y);
#endif
                    button35.BackColor = SystemColors.Control;
                }

                if (IOPort.inportb((short)DI_IN.y_bwd_p) == true)
                {
#if !DEBUG_MODE
                    if (button34.BackColor != Color.Lime) AXTServo.AxtJogBackwordMove(MOTOR_Y, Convert.ToDouble(textBox83.Text));
#endif
                    button34.BackColor = Color.Lime;
                }
                else
                {
#if !DEBUG_MODE
                    if (button34.BackColor == Color.Lime) AXTServo.Axt_EStop(MOTOR_Y);
#endif
                    button34.BackColor = SystemColors.Control;

                }

                if (IOPort.inportb((short)DI_IN.z_up_p) == true)
                {
#if !DEBUG_MODE
                    if (button37.BackColor != Color.Lime) AXTServo.AxtJogUpMove(MOTOR_Z, Convert.ToDouble(textBox83.Text));
#endif
                    button37.BackColor = Color.Lime;

                }
                else
                {
#if !DEBUG_MODE
                    if (button37.BackColor == Color.Lime) AXTServo.Axt_EStop(MOTOR_Z);
#endif
                    button37.BackColor = SystemColors.Control;
                }

                if (IOPort.inportb((short)DI_IN.z_down_p) == true)
                {
#if !DEBUG_MODE
                    if (button36.BackColor != Color.Lime) AXTServo.AxtJogDownMove(MOTOR_Z, Convert.ToDouble(textBox83.Text));
#endif
                    button36.BackColor = Color.Lime;

                }
                else
                {
#if !DEBUG_MODE
                    if (button36.BackColor == Color.Lime) AXTServo.Axt_EStop(MOTOR_Z);
#endif
                    button36.BackColor = SystemColors.Control;
                }
            }
            if (IOPort.inportb((short)DI_IN.jig_up_p) == true)
            {
                txt_jig.Value.AsBoolean = true;

            }
            else
            {
                txt_jig.Value.AsBoolean = false;

            }

            if (IOPort.inportb((short)DI_IN.product_in) == true)
            {
                txt_prd.Value.AsBoolean = true;
                IOPort.outportb((short)DO_OUT.PRODUCT_IN, true);
            }
            else
            {
                ProductOutFlag = false;
                ProductOutFlag2 = false;

                txt_prd.Value.AsBoolean = false;
                IOPort.outportb((short)DO_OUT.PRODUCT_IN, false);
                //if (test_process_run == false)
                //{
                //if (Test_OK.Value.AsBoolean == true)
                //{             
                if (TestOkOnFlag == true)
                {
                    IOPort.outportb((short)DO_OUT.TEST_END, false);
                    Test_OK.Value.AsBoolean = false;
                    TestOkOnFlag = false;
                }
                ProductResult = RESULT_NONE;
                //}
                //}
                if (panel14.Visible == true) ConnectorOutOnOff(OFF);
            }

            if (IOPort.inportb((short)DI_IN.unlock_p) == true)
            {
                button95.BackColor = Color.Lime;
            }
            else
            {
                button95.BackColor = SystemColors.Control;
            }

            if (IOPort.inportb((short)DI_IN.lock_p) == true)
            {
                button96.BackColor = Color.Lime;
            }
            else
            {
                button96.BackColor = SystemColors.Control;
            }

//#if !YB_OR_NEWDE
//            if (IOPort.inportb((short)DI_IN.turn_p] == true)
//            {
//                button97.BackColor = Color.Lime;
//            }
//            else
//            {
//                button97.BackColor = SystemColors.Control;
//            }
//#else
            double PowerFirst = 0;
            double PowerLast = 0;


            if (IOPort.inportb((short)DI_IN.power_off) == true)
            {
                if (led12.Value.AsBoolean == false) led12.Value.AsBoolean = true;
                PowerFirst = ComF.timeGetTimems();
                PowerLast = ComF.timeGetTimems();
                if (panel15.Visible == true) panel15.Visible = false;
                if (ExitTime != 0) ExitTime = 0;
            }
            else
            {
                if (led12.Value.AsBoolean == true)
                {
                    led12.Value.AsBoolean = false;
                }
                PowerLast = ComF.timeGetTimems();

                if (panel15.Visible == false)
                {
                    label150.Text = "10초 후 윈도우가 종료 됩니다.";
                    panel15.Visible = true;
                    panel15.BringToFront();
                }
                if (ExitTime != (int)((PowerLast - PowerFirst) / 1000.0))
                {
                    ExitTime = (int)((PowerLast - PowerFirst) / 1000.0);
                    label150.Text = (10 - (PowerLast - PowerFirst) / 1000).ToString() + "초 후 윈도우가 종료 됩니다.";
                }
                if (10500 <= (PowerLast - PowerFirst))
                {
                    this.Close();
                }
            }
            if (led12.Value.AsBoolean != true) led12.Value.AsBoolean = true;
//#endif

            if (IOPort.inportb((short)DI_IN.return_p) == true)
            {
                button98.BackColor = Color.Lime;
            }
            else
            {
                button98.BackColor = SystemColors.Control;
            }
            if (FirstMotorOrgRunFlag == true)
            {
                if (IOPort.inportb((short)DI_IN.emg_sw_p) == true)
                {
                    timer4.Enabled = true;
                    FirstMotorOrgRunFlag = false;
                }
            }
            MainTimer.Enabled = true;
        }


        #region 버튼 이벤트
        private void btn_Manual0_Click(object sender, EventArgs e)
        {

        }

        private void button7_MouseDown(object sender, MouseEventArgs e)
        {
            IOPort.outportb((short)DO_OUT.ODS_POWER, true);
            button7.BackColor = Color.Lime;
        }

        private void button7_MouseUp(object sender, MouseEventArgs e)
        {
            IOPort.outportb((short)DO_OUT.ODS_POWER, false);
            button7.BackColor = Color.WhiteSmoke;
        }

        private void button15_MouseDown(object sender, MouseEventArgs e)
        {
            //P32_C32_DO(12, true);
            button15.BackColor = Color.Lime;
        }

        private void button15_MouseUp(object sender, MouseEventArgs e)
        {
            //P32_C32_DO(12, false);
            button15.BackColor = Color.WhiteSmoke;
        }

        private void button12_MouseDown(object sender, MouseEventArgs e)
        {
            //P32_C32_DO(13, true);
            button12.BackColor = Color.Lime;
        }

        private void button12_MouseUp(object sender, MouseEventArgs e)
        {
            //P32_C32_DO(13, false);
            button12.BackColor = Color.WhiteSmoke;
        }

        private void button10_MouseDown(object sender, MouseEventArgs e)
        {
            //P32_C32_DO(14, true);
            button10.BackColor = Color.Lime;
        }

        private void button10_MouseUp(object sender, MouseEventArgs e)
        {
            //P32_C32_DO(14, false);
            button10.BackColor = Color.WhiteSmoke;
        }

        private void button11_MouseDown(object sender, MouseEventArgs e)
        {
            IOPort.outportb((short)DO_OUT.SERVO_Z_BRACK, true);
            button11.BackColor = Color.Lime;
        }

        private void button11_MouseUp(object sender, MouseEventArgs e)
        {
            IOPort.outportb((short)DO_OUT.SERVO_Z_BRACK, false);
            button11.BackColor = Color.WhiteSmoke;
        }

        private void button9_MouseDown(object sender, MouseEventArgs e)
        {
//#if !YB_OR_NEWDE
//            IOPort.outportb((short)DO_OUT.TURN_SOL, true);
//#endif
            button9.BackColor = Color.Lime;
        }

        private void button9_MouseUp(object sender, MouseEventArgs e)
        {
//#if !YB_OR_NEWDE
//            IOPort.outportb((short)DO_OUT.TURN_SOL, false);
//#endif
            button9.BackColor = Color.WhiteSmoke;
        }


        #endregion

        //uint rxcan_0x5fc_rx_time = 0;
        short did_fac1 = 0, did_fac2 = 0;
        int did_fack1_ct = 0, /*did_fac1_ct = 0, */did_fac2_ct = 0;

        uint key_eeff = 0;
        uint key_gghh = 0;

        ASCIIEncoding asc = new ASCIIEncoding();

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            //if (timer2busy == false)
            //{
            Byte temp;


            int ccc;
            string log_data = "";
            //NCTYPE_CAN_FRAME_TIMED Frame;
            __CanControl.__CanMsg Frame;
            //CanRxEvent(m_HighCan, 131, 0, 0);

            //__CanControl.__CanMsg CanMsg = new __CanControl.__CanMsg()
            //{
            //    DATA = new byte[8]
            //};

            //__CanControl.__CanMsg CanMsg = CanReWrite.ReadCan(CanChannel, false);

            //if (CanMsg.ID != -1)
            //{
            //    m_Que.Enqueue(CanMsg);
            //}


            if (RunningFlag == true) testttt.Value.AsInteger = (int)(ComF.timeGetTimems() * 0.001);
                        
            if (0 < m_Que.Count)
            {
                Frame = (__CanControl.__CanMsg)m_Que.Dequeue();

                if (Frame.ID == 0x5fc)      //ODS3
                {
                    //textBox9.AppendText("0X5FC[RECV] ");
                    temp = Frame.DATA[0];
                    //textBox9.AppendText(temp.ToString("X2") + " ");
                    if (temp == ConnectionData) // ConnectionData = DE - 0xb0, YB - 0xb2
                    {
                        button58.BackColor = Color.Lime;
                        //rxcan_0x5fc_rx_time = 0;
                        button40.Text = "0x" + temp.ToString("X2");
                        //button40.Text = "0x80";
                    }

                    ccc = Frame.ID;
                    log_data = "[RECV]:" + "0x" + ccc.ToString("X4") + " ";
                    temp = Frame.DATA[0];
                    log_data += temp.ToString("X2") + " ";
                    temp = Frame.DATA[1];
                    log_data += temp.ToString("X2") + " ";
                    temp = Frame.DATA[2];
                    log_data += temp.ToString("X2") + " ";
                    temp = Frame.DATA[3];
                    log_data += temp.ToString("X2") + " ";

                    temp = Frame.DATA[4];
                    log_data += temp.ToString("X2") + " ";

                    if (Frame.Length > 5)
                    {
                        temp = Frame.DATA[5];
                        log_data += temp.ToString("X2") + " ";
                        temp = Frame.DATA[6];
                        log_data += temp.ToString("X2") + " ";
                        temp = Frame.DATA[7];
                        log_data += temp.ToString("X2") + " ";
                    }

                    Log_SAVE(log_data);

                }

                if (Frame.ID == 0x5fa)      //ODS1
                {
                    //textBox9.AppendText("0X5FC[RECV] ");
                    temp = Frame.DATA[0];
                    //textBox9.AppendText(temp.ToString("X2") + " ");

                    ccc = Frame.ID;
                    log_data = "[RECV]:" + "0x" + ccc.ToString("X4") + " ";
                    temp = Frame.DATA[0];
                    log_data += temp.ToString("X2") + " ";
                    temp = Frame.DATA[1];
                    log_data += temp.ToString("X2") + " ";
                    temp = Frame.DATA[2];
                    log_data += temp.ToString("X2") + " ";
                    temp = Frame.DATA[3];
                    log_data += temp.ToString("X2") + " ";

                    //DTC
                    if (DTCCheckFlag == true)
                    {
                        DTCCode = Frame.DATA[4];
                        DTCCheckReadFlag = true;
                    }

                    temp = Frame.DATA[4];
                    log_data += temp.ToString("X2") + " ";
                    temp = Frame.DATA[5];
                    if (Frame.Length > 5)
                    {
                        log_data += temp.ToString("X2") + " ";
                        temp = Frame.DATA[6];
                        log_data += temp.ToString("X2") + " ";
                        temp = Frame.DATA[7];
                        log_data += temp.ToString("X2") + " ";
                    }
                    richTextBox1.AppendText(log_data + '\n');
                    Log_SAVE(log_data);

                }

                if (Frame.ID == 0x7cb)
                {

                    ccc = Frame.ID;
                    log_data = "[RECV]:" + "0x" + ccc.ToString("X4") + " ";
                    temp = Frame.DATA[0];
                    log_data += temp.ToString("X2") + " ";
                    temp = Frame.DATA[1];
                    log_data += temp.ToString("X2") + " ";
                    temp = Frame.DATA[2];
                    log_data += temp.ToString("X2") + " ";
                    temp = Frame.DATA[3];
                    log_data += temp.ToString("X2") + " ";
                    temp = Frame.DATA[4];
                    log_data += temp.ToString("X2") + " ";
                    temp = Frame.DATA[5];
                    log_data += temp.ToString("X2") + " ";
                    temp = Frame.DATA[6];
                    log_data += temp.ToString("X2") + " ";
                    temp = Frame.DATA[7];
                    log_data += temp.ToString("X2") + " ";
                    //log_data += Environment.NewLine;
                    Log_SAVE(log_data);

                    //textBox9.AppendText(log_data + '\n');
                    log_data = log_data + '\n';
                    richTextBox1.AppendText(log_data);

                    if (ConnectionCheckFlag == false)
                    {
                        if (Frame.DATA[0] == 0x02 && Frame.DATA[1] == 0x50 && Frame.DATA[2] == 0x03)
                        {
                            button58.BackColor = Color.Lime;
                            //rxcan_0x5fc_rx_time = 0;
                            button40.Text = "0x80";
                        }
                    }


                    if (Frame.DATA[2] == 0xFA && Frame.DATA[3] == 0xC1)
                    {
                        string ssc;
                        //To read Image capacitance value(Empty seat)
                        //if (did_fac1_ct == 0)
                        //        did_fac1 = 0;
                        //else
                        // {
                        did_fac1 += (short)((Frame.DATA[4] << 8) + Frame.DATA[5]);
                        did_fack1_ct++;
                        //}
                        ssc = string.Format("KDATA = {0:d}", (Frame.DATA[4] << 8) + Frame.DATA[5]);
                        //textBox9.AppendText(log_data + '\n');
                        log_data = log_data + '\n';
                        richTextBox1.AppendText(log_data);

                        //did_fac1_ct++;

                        if (0 < did_fack1_ct)
                        {
                            int iData;
                            short sData;

                            iData = (did_fac1 / did_fack1_ct);
                            sData = (short)iData;

                            button48.Text = sData.ToString("#");
                        }
                        else
                        {
                            button48.Text = "0";
                        }

                        if (led1.Value.AsBoolean == true)
                        {
                            if (0 < did_fack1_ct)
                                SBRData = (short)(did_fac1 / did_fack1_ct);
                            else SBRData = 999;
                        }

                        /*
                        if (did_fac1_ct > 4)
                        {
                            if (0 < did_fack1_ct)
                                 button48.Text = (did_fac1 / did_fack1_ct).ToString("#");
                            else button48.Text = "999";
                            if (led1.Value.AsBoolean == true)
                            {
                                if (0 < did_fack1_ct)
                                        SBRData = (did_fac1 / did_fack1_ct);
                                else    SBRData = 999;
                            }
                        }
                        */
                    }

                    if (Frame.DATA[2] == 0xFA && Frame.DATA[3] == 0xC2)
                    {
                        //To read Real capacitance value(Empty seat)
                        did_fac2 += (short)((Frame.DATA[4] << 8) + Frame.DATA[5]);

                        did_fac2_ct++;
                        if (did_fac2_ct > 4)
                        {
                            short sData;
                            int iData;

                            if ((did_fac2 / did_fac2_ct) == 0)
                            {
                                button49.Text = "0";
                            }
                            else
                            {
                                iData = (did_fac2 / did_fac2_ct);
                                sData = (short)iData;
                                button49.Text = sData.ToString("#");
                            }
                        }
                    }

                    if (Frame.DATA[2] == 0xFA && Frame.DATA[3] == 0x10)
                    {
                        //T7
                        if (Frame.DATA[1] == 0x62)
                        {
                            button68.Text = "0x" + (Frame.DATA[4]).ToString("X2");
                        }
                        t7_t1_rx_ok = true;
                    }


                    if (Frame.DATA[0] == 0x06 && Frame.DATA[1] == 0x67 && Frame.DATA[2] == 0x01)
                    {
                        //Read SEED
                        uint seed_aabb = (uint)(Frame.DATA[3] << 8) + Frame.DATA[4];//$aabb
                        uint seed_ccdd = (uint)(Frame.DATA[5] << 8) + Frame.DATA[6];//$ccdd

                        key_eeff = 0x6d * (seed_aabb) + 0x428d;
                        key_gghh = 0x6d * (seed_ccdd) + 0x428d;

                        read_seed_ok_flg = true;

                        //Debug.WriteLine("SEED_aabb=" + seed_aabb.ToString("X4") + ",SEED_ccdd=" + seed_ccdd.ToString("X4") + ",KEY_eeff=" + key_eeff.ToString("X4") + ",KEY_gghh=" + key_gghh.ToString("X4"));

                    }

                    if (Frame.DATA[1] == 0x62 && Frame.DATA[2] == 0xFA && Frame.DATA[3] == 0xD9)
                    {
                        //T6

                        button50.Text = "0x" + ((uint)(Frame.DATA[4] << 8) + Frame.DATA[5]).ToString("X2");

                    }

                    if (Frame.DATA[0] == 0x6 && Frame.DATA[1] == 0x63)
                    {
                        //T4 OPA

                        button45.Text = "0x" + (Frame.DATA[2]).ToString("X2") + (Frame.DATA[3]).ToString("X2") + (Frame.DATA[4]).ToString("X2") + (Frame.DATA[5]).ToString("X2") + (Frame.DATA[6]).ToString("X2");

                    }
                    if (Frame.DATA[1] == 0x62 && Frame.DATA[2] == 0xFA && Frame.DATA[3] == 0xD8)
                    {
                        //T6

                        button44.Text = "0x" + ((uint)(Frame.DATA[4] << 8) + Frame.DATA[5]).ToString("X2");

                    }

                    if (Frame.DATA[2] == 0x67 && Frame.DATA[3] == 0x02 && Frame.DATA[4] == 0xAA)
                    {

                        t2_t1_rx_ok = true;
                    }


                    if (Frame.DATA[1] == 0x62 && Frame.DATA[2] == 0xFA && Frame.DATA[3] == 0xAB)
                    {
                        button75.Text = "0x" + Frame.DATA[4].ToString("X2");

                        if (Frame.DATA[4] == 0xAA)
                        {
                            t2_t1_rx_ok = true;
                            UnlockByteData = button75.Text;
                        }
                        if (Frame.DATA[4] == 0x55)
                        {
                            t8_t1_rx_ok = true;
                            LockByteData = button75.Text;
                        }
                    }

                    if (Frame.DATA[1] == 0x62 && Frame.DATA[2] == 0xFA && Frame.DATA[3] == 0xBE)
                    {

                        textBox30.Text = "0x" + Convert.ToUInt32((Frame.DATA[4] << 24) | (Frame.DATA[5] << 16) | (Frame.DATA[6] << 8) | Frame.DATA[7]).ToString("X8");
                    }

                    if (Frame.DATA[3] == 0xFA && Frame.DATA[4] == 0x8c)     //SERIAL_REQ2
                    {
                        SERIAL_REQ3();
                        //textBox73.Text = Convert.ToUInt16((Frame.Data5 << 8) | Frame.Data6).ToString("D");
                    }

                    if (Frame.DATA[0] == 0x21 && Frame.DATA[6] == 0xaa && Frame.DATA[7] == 0xaa)     //SERIAL_REQ4
                    {

                        textBox81.Text = Convert.ToUInt64((Frame.DATA[1] << 8) | Frame.DATA[2]).ToString("X") + Convert.ToUInt64((Frame.DATA[3] << 16) | (Frame.DATA[4] << 8) | Frame.DATA[5]).ToString("X");

                    }
                    if (Frame.DATA[0] == 0x03 && Frame.DATA[1] == 0x63) // Read memory data
                    {
                        textBox85.Text = Convert.ToUInt32((Frame.DATA[2] << 8) | (Frame.DATA[3] << 0)).ToString("X4").ToUpper();
                    }
                    if (Frame.DATA[0] == 0x30 && Frame.DATA[1] == 0x08 && Frame.DATA[2] == 0x08) // Write memory data
                    {
                        textBox86.Text = Convert.ToUInt32((Frame.DATA[0] << 16) | (Frame.DATA[1] << 8) | (Frame.DATA[2] << 0)).ToString("X6").ToUpper();
                    }
                    if (Frame.DATA[0] == 0x10 && Frame.DATA[1] == 0x23 && Frame.DATA[2] == 0x62) // Check the content is as expected
                    {
                        textBox87.Text = Convert.ToUInt32((Frame.DATA[3] << 8) | (Frame.DATA[4] << 0)).ToString("X4").ToUpper();
                    }

                    if (textBox87.Text == "FAB1") // Memory read Request 전송후 라면
                    {
                        if (Frame.DATA[0] == 0x21 && Frame.DATA[1] == 0x01 && Frame.DATA[2] == 0x28) // Check the content is as expected
                        {
                            string s;

                            s = Convert.ToUInt16(Frame.DATA[0]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[1]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[2]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[3]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[4]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[5]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[6]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[7]).ToString("X2").ToUpper();
                            textBox88.Text = s;
                        }
                        if (Frame.DATA[0] == 0x22 && Frame.DATA[1] == 0xA2 && Frame.DATA[2] == 0x00) // Check the content is as expected
                        {
                            string s;

                            s = Convert.ToUInt16(Frame.DATA[0]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[1]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[2]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[3]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[4]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[5]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[6]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[7]).ToString("X2").ToUpper();
                            textBox89.Text = s;
                        }
                        if (Frame.DATA[0] == 0x23 && Frame.DATA[1] == 0xFF && Frame.DATA[2] == 0xFF) // Check the content is as expected
                        {
                            string s;

                            s = Convert.ToUInt16(Frame.DATA[0]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[1]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[2]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[3]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[4]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[5]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[6]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[7]).ToString("X2").ToUpper();
                            textBox90.Text = s;
                        }
                        if (Frame.DATA[0] == 0x24 && Frame.DATA[1] == 0xFF && Frame.DATA[2] == 0xFF) // Check the content is as expected
                        {
                            string s;

                            s = Convert.ToUInt16(Frame.DATA[0]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[1]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[2]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[3]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[4]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[5]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[6]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[7]).ToString("X2").ToUpper();
                            textBox91.Text = s;
                        }
                        if (Frame.DATA[0] == 0x25 && Frame.DATA[1] == 0xFF && Frame.DATA[2] == 0xAA) // Check the content is as expected
                        {
                            string s;

                            s = Convert.ToUInt16(Frame.DATA[0]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[1]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[2]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[3]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[4]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[5]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[6]).ToString("X2").ToUpper() + " ";
                            s += Convert.ToUInt16(Frame.DATA[7]).ToString("X2").ToUpper();
                            textBox92.Text = s;
                        }
                    }
                }
            }

           
            timer1.Enabled = true;
            return;
        }
        private void button16_Click(object sender, EventArgs e)
        {
            WCS_1.Clear();
            WCS_2.Clear();
            WCS_3.Clear();
            WCS_4.Clear();
            WCS_5.Clear();
            WCS_6.Clear();
            WCS_01();
            Thread.Sleep(100);
            WCS_02();
        }


        private void button19_Click(object sender, EventArgs e)
        {
            //DTC check
            ReadDTCinformation();

        }


        private bool deley_stop_check(int deley_time)
        {
            int wait_time = 0;

            do
            {
                //ComF.timedelay(100);
                Thread.Sleep(100);
                if (test_process_stop == true)
                {
                    list(lv_num, "", "검사 강제 중지");
                    //STOP_WATCH.Stop();
                    //test_process_stop = false;
                    //test_process_run = false;
                    return true;
                }

                if (test_process_estop == true)
                {
                    list(lv_num, "", "비상 중지");
                    //                    STOP_WATCH.Stop();
                    //test_process_estop = false;
                    //test_process_run = false;
                    return true;
                }
                wait_time++;

                if (EStopCheck() == true)
                {
                    list(lv_num, "", "비상 중지");
                    //                    STOP_WATCH.Stop();
                    test_process_estop = true;
                    //test_process_run = false;
                    return true;

                }

                //if (checkBox8.Checked) return false;

            } while (wait_time < (deley_time * .01));

            return false;
        }

        private bool DTCCheckFlag = false;
        private bool DTCCheckReadFlag = false;
        private byte DTCCode = 0x00;
        private int Count = 0;

        private bool deley_stop_checkToDTC(int deley_time)
        {
            int wait_time = 0;

            DTCCheckFlag = true;
            DTCCode = 0xFF;
            DTCCheckReadFlag = false;
            Count = 0;

            do
            {
                if (DTCCheckReadFlag == true)
                {
                    if (DTCCode == 0x00)
                    {
                        DTCCheckFlag = false;
                        return false;
                    }
                    else
                    {
                        Count++;
                        DTCCheckReadFlag = false;
                        if (2 <= Count) return false;
                    }
                }

                //ComF.timedelay(100);
                Thread.Sleep(100);
                if (test_process_stop == true)
                {
                    list(lv_num, "", "검사 강제 중지");
                    //STOP_WATCH.Stop();
                    //test_process_stop = false;
                    //test_process_run = false;
                    return true;
                }

                if (test_process_estop == true)
                {
                    list(lv_num, "", "비상 중지");
                    //                    STOP_WATCH.Stop();
                    //test_process_estop = false;
                    //test_process_run = false;
                    return true;
                }
                wait_time++;

                if (EStopCheck() == true)
                {
                    list(lv_num, "", "비상 중지");
                    //                    STOP_WATCH.Stop();
                    test_process_estop = true;
                    //test_process_run = false;
                    return true;

                }

                //if (checkBox8.Checked) return false;

            } while (wait_time < (deley_time * .01));

            return false;
        }

        private void test_val_clr()
        {
            button40.Text = "";
            button49.Text = "";
            button50.Text = "";
            button44.Text = "";
            button45.Text = "";
            button48.Text = "";
            button46.Text = "";
            //button51.Text = "";
            button79.Text = "0";
            textBox67.Text = "";
            textBox66.Text = "";
            textBox68.Text = "";
            textBox69.Text = "";
            textBox85.Text = "";
            textBox86.Text = "";
            textBox87.Text = "";
            textBox88.Text = "";
            textBox89.Text = "";
            textBox90.Text = "";
            textBox91.Text = "";
            textBox92.Text = "";

            button58.BackColor = SystemColors.Control;

            for (int i = 0; i < listView1.Items.Count; i++)
            {
                listView1.Items[i].Focused = false;
                listView1.Items[i].Selected = false;
            }

            ReadMemoryResult = 0;
            ReadContentsResult = 0;
            DisplayStatus(RESULT_TEST);
        }


        private void SBR_test()
        {

            int ng_ct = 0;
            int over_time_check = 0;

            list(lv_num, "", "OK");
            lv_num++;
            SelectItem();

            NoneLoadData = "";
            LoadData = "";


            SelectItem();
            if (checkBox8.Checked == false) // Note PC가 클릭되어 있지 않으면
            {
                X_move_set(textBox49.Text, Convert.ToUInt16(textBox70.Text));      //검사위치 이동
                Y_move_set(textBox47.Text, Convert.ToUInt16(textBox70.Text));      //검사위치 이동

                SBR_no_reactive_Z_down_test();   //SBR 무반응 검사

                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    AllMotorStop();
                    return;
                }
                if (LadcellOverCheck() == true)
                {
                    test_process_stop = true;
                    AllMotorStop();
                    return;
                }
                NoneLoadData = button80.Text;
                if (Convert.ToDouble(button80.Text) >= Convert.ToDouble(ReadMinSpec(lv_num)))
                {
                    list(lv_num, button80.Text, "OK");
                }
                else
                {
                    list(lv_num, button80.Text, "NG");
                    ng_ct++;
                }
                lv_num++;
                SelectItem();


                Z_down_test();
                if (EStopCheck() == true)
                {
                    NG_Final();
                    return;

                }
                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    NG_Final();
                    return;
                }
                //list(lv_num, "Z축 압력 값(Kq)", button81.Text, button79.Text, "OK");

                //lv_num++;
                LoadData = button80.Text;
                if (Convert.ToDouble(ReadMinSpec(lv_num)) >= Convert.ToDouble(button80.Text))
                {
                    list(lv_num, button80.Text, "OK");
                }
                else
                {
                    list(lv_num, button80.Text, "NG");
                    ng_ct++;
                }

                lv_num++;
                SelectItem();

                //                STOP_WATCH.Stop();

                if (ng_ct != 0)
                {
                    //button51.Text = "NG";
                    DisplayStatus(RESULT_REJECT);
                    NG_COUNT();
                    NG_Final();
                }
                else
                {
                    //button51.Text = "OK";
                    DisplayStatus(RESULT_PASS);
                    OK_COUNT();
                    OK_Final();
                }

                Data_Send();
                if (ng_ct != 0)
                {
                    SaveData(RESULT_REJECT);
                }
                else
                {
                    SaveData(RESULT_PASS);
                }


                list(lv_num, "", button51.Text);
                lv_num++;
                SelectItem();


                if (checkBox8.Checked == false)
                {

                    over_time_check = 0;

                    do
                    {
                        if (X_move_check() && Y_move_check() && Z_move_check())
                        {
                            break;
                        }
                        ComF.timedelay(100);
                        over_time_check++;
                        if (EStopCheck() == true)
                        {
                            NG_Final();
                            return;

                        }
                        if ((test_process_stop == true) || (test_process_estop == true))
                        {
                            NG_Final();
                            return;
                        }
                    } while (over_time_check < 200);        //wait 20sec

                    //  turn();

                }
            }

        }

        //int time_delay = 0;

        private short CanChannel = 0;
        public short CanPosition()
        {
#if !DEBUG_MODE
            //short ID = 0;

            string[] Device = CanReWrite.GetDevice;

            //for (short i = 0; i < Device.Length; i++)
            //{
            //    string s = Device[i];
            //    string s1 = "0x" + Pos.ToString("X2");

            //    if (0 <= s.IndexOf(s1))
            //    {
            //        ID = i;
            //        break;
            //    }
            //}
            short Pos = -1;
            string s1 = "Device=" + CanDeivce.Device.ToString();
            string s2 = "Channel=" + CanDeivce.Channel.ToString() + "h";

            foreach (string s in Device)
            {

                if (0 <= s.IndexOf(s1))
                {
                    if (0 <= s.IndexOf(s2))
                    {
                        string ss = s.Substring(s.IndexOf("ID=") + "ID=".Length);
                        string[] ss1 = ss.Split(',');
                        if (1 < ss1.Length)
                        {
                            string ss2 = ss1[0].Replace("(", null);

                            ss2 = ss2.Replace(")", null);
                            Pos = (short)ComF.StringToHex(ss2);
                        }
                    }
                }
            }

            if (Pos == -1)
            {
                Pos = CanDeivce.ID;
            }
            CanChannel = Pos;
            return Pos;
#else
            return 0;
#endif
        }
        private void Test_process_m()
        {
            bool MemoryCheckFlag;
            int ng_ct = 0;
            double SpecMin;
            double SpecMax;
            double Value;
            double First;
            double Last;
            double xTime;
            bool CheckFlag;

            int ReTestCount;
            bool ReTestFlag;
            string s;

            IOPort.outportb((short)DO_OUT.LAMP_RED, false);
            IOPort.outportb((short)DO_OUT.LAMP_GREEN, false);
            IOPort.outportb((short)DO_OUT.LAMP_YELLOW, false);

            SetTestEnd(false);
            Z_PosToDefailtOutToPLCFlag = false;

            if (SbrOrOds == true)
                Z_DefaultPos = Convert.ToUInt16(textBox70.Text);//for SBR
            else Z_DefaultPos = Convert.ToUInt16(textBox70.Text);//for ODS

            Z_PosToDefailtOutToPLC(false);
            StartButtonOnoff(false);
            //CAN_INI_HIGH();

            if (0 <= CanDeivce.Device)
            {
                CanReWrite.OpenCan(0, CanPosition(), (short)CanDeivce.Speed, false);
                if (CanReWrite.isOpen(0) == true)
                {
                    m_Que.Clear();
                    CanClsoeFlag = false;
                    ThreadSetting();
                }
            }
                        
            plot1.Channels[0].Clear();
            plot1.Channels[1].Clear();
            //            plot1.Channels[0].AddXY(0,0);
            CheckForIllegalCrossThreadCalls = false;

            checkBox9.Enabled = false;

            test_val_clr();
            //CanData.Clear();
            richTextBox1.Clear();
            richTextBox1.Focus();
            //textBox9.Clear();

            lv_num = 1;
            //cmObj.Delete();
            //cmObj.SetListView(listView1);
            ScreenInit();
            IOPort.outportb((short)DO_OUT.TEST_ING, true);
            //MessageBox.Show("DDDD");
            if (checkBox8.Checked == false) // Note PC가 클릭되어 있지 않으면
            {
                if (kLoadCell != 0.0) IndZeroSetting();
                ComF.timedelay(100);
            }

            if (SbrOrOds == true)
            {
                if (checkBox8.Checked == false) // Note PC가 클릭되어 있지 않으면
                {
                    LiveDisplayFlag = true;
                    SBR_test();
                    LiveDisplayFlag = false;
                    //                STOP_WATCH.Stop();
                    RunningFlag = false;
                    test_process_run = false;

                    if ((test_process_stop == true) || (test_process_estop == true))
                    {
                        StopProcess();
                        if (test_process_stop == true) DefaultPositionMove();
                        SetTestEnd(true);
                        return;
                    }
                    else
                    {
                        DefaultPositionMove();
                        Z_PosToDefailtOutToPLCFlag = true;
                    }
                }
                else
                {
                    MessageBox.Show("노트북 피씨 검사 모드로 설정되어 있어 SBR 검사를 진행할 수 없습니다.");
                    return;
                }
            }
            else
            {
                //                return_pp();

                IOPort.outportb((short)DO_OUT.ODS_POWER, true);


                if (deley_stop_check(600))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }
                //BOSE Connected
                if (button58.BackColor == Color.Lime)
                {
                    list(lv_num, "", "OK");
                }
                else
                {
                    if (ConnectionCheckFlag == true)
                    {
                        list(lv_num, "", "NG");
                        ng_ct++;

                        if (checkBox1.Checked == true) goto ODS_TestEnd;
                    }
                }
                if (ConnectionCheckFlag == true)
                {
                    lv_num++;
                    SelectItem();
                }

                ReTestCount = 0;
                ReTestFlag = false;

            ReTest1:
                Diag_start();

                if (ReTestFlag == false)
                {
                    if (checkBox8.Checked == false) // Note PC가 클릭되어 있지 않으면
                    {
                        X_move_set(textBox34.Text, Convert.ToUInt16(textBox70.Text));      //검사위치 이동
                        Y_move_set(textBox35.Text, Convert.ToUInt16(textBox70.Text));      //검사위치 이동                    
                    }
                }

                //Thread.Sleep(250);
                if (deley_stop_check(250))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }

                if (ConnectionCheckFlag == false)
                {
                    if (deley_stop_check(400))
                    {
                        StopProcess();
                        SetTestEnd(true);
                        return;
                    }
                    //BOSE Connected
                    if (button58.BackColor == Color.Lime)
                    {
                        list(lv_num, "", "OK");
                    }
                    else
                    {
                        list(lv_num, "", "NG");
                        Thread.Sleep(1500);
                        ng_ct++;

                        if (checkBox1.Checked == true) goto ODS_TestEnd;

                    }
                    lv_num++;
                    SelectItem();
                }

                T1_T5_TEST(false);
                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }


                if (deley_stop_check(500))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }

                if (ReTestFlag == false)
                {
                    if (checkBox8.Checked == false) // Note PC가 클릭되어 있지 않으면
                    {
                        if (!X_move_check() || !Y_move_check())
                        {
                            First = ComF.timeGetTimems();
                            Last = ComF.timeGetTimems();
                            do
                            {
                                //X 축 및 Y 축 모터 이동이 완료될 때까지 대기한다.
                                if (X_move_check() && Y_move_check()) break;

                                if ((test_process_stop == true) || (test_process_estop == true))
                                {
                                    AllMotorStop();
                                    StopProcess();
                                    SetTestEnd(true);
                                    return;
                                }
                                Application.DoEvents();
                                Last = ComF.timeGetTimems();
                                xTime = Last - First;
                            } while (xTime < 20000);        //wait 20sec
                        }

                        s = textBox82.Text;
                        Z_move_set(s, Convert.ToUInt16(textBox70.Text));
                    }
                }
                //   NOTE 검사가 아니거나            NOTE PC 검사 이면서             무 부하 검사 일경우
                if ((checkBox8.Checked == false) || ((checkBox8.Checked == true) && (NotLoadToTest == true)))
                {
                    if (0 < ReadMaxSpec(lv_num).Length)
                        SpecMax = Convert.ToDouble(ReadMaxSpec(lv_num));
                    else SpecMax = 0;


                    if (0 < ReadMinSpec(lv_num).Length)
                        SpecMin = Convert.ToDouble(ReadMinSpec(lv_num));
                    else SpecMin = 0;

                    //if (button48.Text.Length == 0) button48.Text = "0";
                    if (0 < button48.Text.Length)
                        Value = Convert.ToDouble(button48.Text);
                    else Value = 9999;

                    //Seat img unload
                    if ((Value <= SpecMax) && (SpecMin <= Value))
                    {
                        list(lv_num, button48.Text, "OK");
                    }
                    else
                    {
                        ReTestCount++;
                        if (3 <= ReTestCount)
                        {
                            list(lv_num, button48.Text, "NG");
                            ng_ct++;
                            if (checkBox1.Checked == true) goto ODS_TestEnd;
                        }
                        else
                        {
                            ReTestFlag = true;
                            goto ReTest1;
                        }
                    }


                    lv_num++;
                    SelectItem();
                    
                    textBox68.Text = button48.Text;
                    textBox69.Text = button49.Text;

                    if (button49.Text.Length == 0)
                    {
                        ReTestCount++;
                        if (3 <= ReTestCount)
                        {
                            list(lv_num, button49.Text, "NG");
                            ng_ct++;
                            if (checkBox1.Checked == true) goto ODS_TestEnd;
                        }
                        else
                        {
                            ReTestFlag = true;
                            goto ReTest1;
                        }
                    }
                    else
                    {
                        list(lv_num, button49.Text, "OK");
                    }
                    lv_num++;
                    SelectItem();
                }
                else
                {
                    textBox68.Text = button48.Text;
                    textBox69.Text = button49.Text;
                }
                if (deley_stop_check(100))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }

                ReTestCount = 0;
                ReTestFlag = false;
            ReTest2:
                Enter_Security_Access();

                Thread.Sleep(250);

                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }

                if (T2_test_process() == false)
                {
                    if (deley_stop_check(250))
                    {
                        StopProcess();
                        SetTestEnd(true);
                        return;
                    }

                    ReTestCount++;
                    if (ReTestCount < 3)
                    {
                        goto ReTest2;
                    }
                    else
                    {
                        list(lv_num, button75.Text, "NG"); //불량 처리 항목이 없어 아래 항목으로 불량 처리를 함 (UNLOCK CHeck 항목 포함)
                        if (checkBox1.Checked == true) goto ODS_TestEnd;
                    }
                }

                if (deley_stop_check(250))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }


                ReTestCount = 0;
                ReTestFlag = false;
            ReTest3:
                if (ReTestFlag == false)
                {
                    T3_test_process(); // ROM ID 읽기 함수가 포함되어 있다.
                }
                else
                {
                    T3_Test_02(); //ROM ID 읽기 명령함수
                    ComF.timedelay(can_tx_wait_time + 10);
                }

                if (deley_stop_check(250))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }
                if (button44.Text == "") // ROI ID Rading 이 되지 않았다면
                {
                    ReTestCount++;
                    ReTestFlag = true;
                    if (ReTestCount < 3)
                    {
                        goto ReTest3;
                    }
                    else
                    {
                        list(lv_num + 1, button44.Text, "NG");
                        ng_ct++;
                        if (checkBox1.Checked == true) goto ODS_TestEnd;
                    }
                }


                T4_test_process(); // 이 함수에 opa 데이타 읽기 명령어를 같이 출력한다.
                //unlock

                if (ReadMinSpec(lv_num) == button75.Text)
                {
                    list(lv_num, button75.Text, "OK");
                }
                else
                {
                    list(lv_num, button75.Text, "NG");
                    ng_ct++;
                    if (checkBox1.Checked == true) goto ODS_TestEnd;
                }

                lv_num++;
                SelectItem();

                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }

                if (deley_stop_check(250))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }
                //ROM ID
                if (button44.Text == ReadMinSpec(lv_num))
                {
                    list(lv_num, button44.Text, "OK");
                }
                else
                {
                    list(lv_num, button44.Text, "NG");
                    ng_ct++;
                    if (checkBox1.Checked == true) goto ODS_TestEnd;
                }
                lv_num++;
                SelectItem();

                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }


                //--------------------------------------------------------
                //Read Memory and write memory 20160808 추가

                ReTestCount = 0;
                ReTestFlag = false;
                MemoryCheckFlag = false;

            ReTestToReadMemory:
                if (ReadMemory == true)
                {
                    if (ReadMemoryCheck() == 1)
                    {
                        if (MemoryCheckFlag == false)
                            list(lv_num, "", "OK");
                        else goto ReTestToWriteMemory;

                        ReadMemoryResult = 1;
                    }
                    else
                    {
                        if (MemoryCheckFlag == false)
                        {
                            ReTestCount++;
                            ReTestFlag = true;
                            if (ReTestCount < 3)
                            {
                                goto ReTestToReadMemory;
                            }
                            else
                            {
                                list(lv_num, button44.Text, "NG");
                                ng_ct++;
                                ReadMemoryResult = 2;
                                if (checkBox1.Checked == true) goto ODS_TestEnd;
                            }
                        }
                        else
                        {
                            ReadMemoryResult = 0;
                            goto ReTestToWriteMemory;
                        }
                    }
                    lv_num++;
                    SelectItem();
                }
                else
                {
                    ReadMemoryResult = 0;
                }
                //--------------------------------------------------------

                ReTestCount = 0;
                ReTestFlag = false;

            OPA_RECHECK:
                if (ReTestFlag == true)
                {
                    T4_Test_02(); //To read OPA file R3 number
                }
                if (deley_stop_check(250))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }
                //OPA File Number
                if (ReadMinSpec(lv_num) == button45.Text)
                {
                    list(lv_num, button45.Text, "OK");
                }
                else
                {
                    ReTestCount++;
                    ReTestFlag = true;
                    if (2 < ReTestCount)
                    {
                        list(lv_num, button45.Text, "NG");
                        ng_ct++;
                    }
                    else
                    {
                        goto OPA_RECHECK;
                    }
                }
                lv_num++;
                SelectItem();

                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }

                if (deley_stop_check(250))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }

                //load seat test start ----------



                if (checkBox8.Checked == false) // Note PC가 클릭되어 있지 않으면
                {
                    LiveDisplayFlag = true;
                    Z_down_test();
                    LiveDisplayFlag = false;
                    if ((test_process_stop == true) || (test_process_estop == true))
                    {
                        StopProcess();
                        SetTestEnd(true);
                        return;
                    }

                    //list(lv_num, "Z축 압력 값(Kq)", button61.Text, button79.Text, "OK");
                    //lv_num++;

                    First = ComF.timeGetTimems();
                    Last = ComF.timeGetTimems();
                    do
                    {
                        if (Z_move_check()) //모터가 이동을 시작하면 빠져 나간다.
                        {
                            break;
                        }

                        if (LadcellOverCheck() == true)
                        {
                            AllMotorStop();
                            DefaultPositionMove();
                            test_process_stop = true;
                            RunningFlag = false;
                            SetTestEnd(true);
                            return;
                        }

                        if ((test_process_stop == true) || (test_process_estop == true))
                        {
                            StopProcess();
                            SetTestEnd(true);
                            return;
                        }

                        //ComF.timedelay(100);
                        //over_time_check++;
                        Last = ComF.timeGetTimems();
                        xTime = Last - First;
                        Application.DoEvents();
                    } while (xTime < 20000);        //wait 20sec
                    CheckFlag = true;
                }
                else
                {
                    DialogResult ret;

                    ret = MessageBox.Show("장비를 이용하여 원하는 압력을 가한후 확인 버튼을 클릭하면 검사를 진행합니다", "측정 대기 메시지", MessageBoxButtons.YesNo);

                    if (ret == DialogResult.Yes)
                    {
                        T1_T5_TEST(true);
                        CheckFlag = true;
                    }
                    else
                    {
                        CheckFlag = false;
                    }
                }

                if (deley_stop_check(100))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }

                textBox67.Text = button48.Text;
                textBox66.Text = button49.Text;

                if (CheckFlag == true)
                {
                    if (0 < ReadMaxSpec(lv_num).Length)
                        SpecMax = Convert.ToDouble(ReadMaxSpec(lv_num));
                    else SpecMax = 0;

                    if (0 < ReadMinSpec(lv_num).Length)
                        SpecMin = Convert.ToDouble(ReadMinSpec(lv_num));
                    else SpecMin = 0;

                    if (0 < button48.Text.Length)
                        Value = Convert.ToDouble(button48.Text);
                    else Value = 9999;

                    //seat Img Load
                    if (SpecMax >= Value && Value >= SpecMin) //img
                    {
                        list(lv_num, button48.Text, "OK");
                    }
                    else
                    {
                        list(lv_num, button48.Text, "NG");
                        ng_ct++;
                        if (checkBox1.Checked == true) goto ODS_TestEnd;
                    }
                }
                lv_num++;
                SelectItem();

                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }
                
                if (button49.Text.Length == 0)
                {
                    list(lv_num, button49.Text, "NG");
                    ng_ct++;
                    if (checkBox1.Checked == true) goto ODS_TestEnd;
                }
                else
                {
                    list(lv_num, button49.Text, "OK");
                }
                lv_num++;
                SelectItem();
                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }
                //z_down_test 에서 데이타를 받지 못했으면 3번 걸처 다시 읽도록 함
                //load seat test end ----------

                if (checkBox8.Checked == false)
                {
                    //power off
                    Log_SAVE("ODS Power OFF");
                    IOPort.outportb((short)DO_OUT.ODS_POWER, false);


                    // 2016.01.23 kimilwon   if (deley_stop_check(3000)) 
                    if (deley_stop_check(2000))
                    {
                        StopProcess();
                        SetTestEnd(true);
                        return;
                    }

                    //power on

                    IOPort.outportb((short)DO_OUT.ODS_POWER, true);
                    Log_SAVE("ODS Power ON");
                }

                if (deley_stop_check(250))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }

                ReTestCount = 0;
                ReTestFlag = false;
            ReTest4:
                T6_test_process();

                if (deley_stop_check(250))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }
                //algo parameter

                if (ReTestFlag == true)
                {
                    T6_Test_01();
                    if (deley_stop_check(250))
                    {
                        StopProcess();
                        SetTestEnd(true);
                        return;
                    }
                }

                if (ReadMinSpec(lv_num) == textBox30.Text)
                {
                    list(lv_num, textBox30.Text, "OK");
                }
                else
                {
                    ReTestCount++;
                    ReTestFlag = true;
                    if (3 < ReTestCount)
                    {
                        list(lv_num, textBox30.Text, "NG");
                        ng_ct++;
                        if (checkBox1.Checked == true) goto ODS_TestEnd;
                    }
                    else
                    {
                        goto ReTest4;
                    }
                }
                lv_num++;
                SelectItem();

                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }


                if (deley_stop_check(250))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }

                //cal Checksum
                if (led8.Value.AsBoolean == true)
                {
                    //heater type
                    if (button50.Text == ReadMinSpec(lv_num))
                    {
                        list(lv_num, button50.Text, "OK");
                    }
                    else
                    {
                        ReTestCount++;
                        ReTestFlag = true;

                        if (3 < ReTestCount)
                        {
                            list(lv_num, button50.Text, "NG");
                            ng_ct++;
                            if (checkBox1.Checked == true) goto ODS_TestEnd;
                        }
                        else
                        {
                            lv_num--;
                            goto ReTest4;
                        }
                    }
                }
                else if (led7.Value.AsBoolean == true)
                {
                    //Non-heater type
                    if (button50.Text == ReadMinSpec(lv_num))
                    {
                        list(lv_num, button50.Text, "OK");
                    }
                    else
                    {
                        ReTestCount++;
                        ReTestFlag = true;

                        if (3 < ReTestCount)
                        {
                            list(lv_num, button50.Text, "NG");
                            ng_ct++;
                            if (checkBox1.Checked == true) goto ODS_TestEnd;
                        }
                        else
                        {
                            lv_num--;
                            goto ReTest4;
                        }
                    }
                }
                else
                {
                    list(lv_num, "0x", "NG");
                    ng_ct++;
                }
                lv_num++;
                SelectItem();

                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }

                if (deley_stop_check(250))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }

                //--------------------------------------------------------
                //Read Memory and write memory 20160808 추가
                ReTestCount = 0;
                ReTestFlag = false;
                MemoryCheckFlag = true;

            ReTestToWriteMemory:
                if (ReadContents == true)
                {
                    if (CheckToWriteMemory() == 1)
                    {
                        list(lv_num, "", "OK");
                        ReadContentsResult = 1;
                    }
                    else
                    {
                        ReTestCount++;
                        ReTestFlag = true;
                        if (ReTestCount < 3)
                        {
                            goto ReTestToReadMemory;
                        }
                        else
                        {
                            list(lv_num, "", "NG");
                            ng_ct++;
                            ReadContentsResult = 2;
                            if (checkBox1.Checked == true) goto ODS_TestEnd;
                        }
                    }
                    lv_num++;
                    SelectItem();
                }
                else
                {
                    ReadContentsResult = 0;
                }
                //--------------------------------------------------------



                ReTestCount = 0;
                ReTestFlag = false;
            ReTest5:

                T7_test_process();
                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }

                // 2016.01.23 kimilwon  if (deley_stop_check(500))
                if (deley_stop_check(200))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }
                //EOL Byte
                if (ReadMinSpec(lv_num) == button68.Text)
                {
                    list(lv_num, button68.Text, "OK");
                }
                else
                {
                    ReTestCount++;
                    if (3 < ReTestCount)
                    {
                        list(lv_num, button68.Text, "NG");
                        ng_ct++;
                        if (checkBox1.Checked == true) goto ODS_TestEnd;
                    }
                    else
                    {
                        goto ReTest5;
                    }
                }
                lv_num++;
                SelectItem();



                ReTestCount = 0;
                ReTestFlag = false;
            ReTest6:

                T8_test_process();
                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }


                // 2016.01.23 kimilwon  if (deley_stop_check(500))
                if (deley_stop_check(200))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }
                //Lock Byte
                if (ReadMinSpec(lv_num) == button75.Text)
                {
                    list(lv_num, button75.Text, "OK");
                }
                else
                {
                    ReTestCount++;

                    if (3 < ReTestCount)
                    {
                        list(lv_num, button75.Text, "NG");
                        ng_ct++;
                        if (checkBox1.Checked == true) goto ODS_TestEnd;
                    }
                    else
                    {
                        goto ReTest6;
                    }
                }
                lv_num++;
                SelectItem();

                if (deley_stop_check(250))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }
                //Vehicle ID


                if (ReadMinSpec(lv_num) == button40.Text)
                {
                    list(lv_num, button40.Text, "OK");
                }
                else
                {
                    list(lv_num, button40.Text, "NG");
                    ng_ct++;
                    if (checkBox1.Checked == true) goto ODS_TestEnd;
                }
                lv_num++;
                SelectItem();



                //DTC check
                ReadDTCinformation();
                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }
                if (deley_stop_checkToDTC(2500))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }

                if (DTCCode == 0x00)
                {
                    list(lv_num, "0", "OK");
                }
                else
                {
                    list(lv_num, string.Format("{0:X}", DTCCode), "NG");
                    ng_ct++;
                    if (checkBox1.Checked == true) goto ODS_TestEnd;
                }
                lv_num++;
                SelectItem();


                //clearDTCinformation ALL CLEAR
                ClearDTCinformation();
                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }

                if (deley_stop_check(250))
                {
                    StopProcess();
                    SetTestEnd(true);
                    return;
                }

                list(lv_num, "0", "OK");
                lv_num++;
                SelectItem();

            ODS_TestEnd:

                if (checkBox8.Checked == false) // Note PC가 클릭되어 있지 않으면
                {
                    X_move_set(textBox33.Text, Convert.ToUInt16(textBox70.Text));                  //검사대기위치 이동 for ODS
                    Y_move_set(textBox36.Text, Convert.ToUInt16(textBox70.Text));                  //검사대기위치 이동 for ODS
                }
                //power off
                IOPort.outportb((short)DO_OUT.ODS_POWER, false);


                string ods_sn = textBox81.Text + "  ODS SN";

                Log_SAVE(ods_sn);

                if (ng_ct != 0)
                {
                    //button51.Text = "NG";
                    DisplayStatus(RESULT_REJECT);
                    NG_COUNT();
                    //SaveData(RESULT_PASS);
                }
                else
                {
                    DisplayStatus(RESULT_PASS);
                    OK_COUNT();
                    //SaveData(RESULT_REJECT);
                }
                lv_num = 16;
                if (ReadMemory == true) lv_num++;
                if (ReadContents == true) lv_num++;
                list(lv_num, "", button51.Text);
                lv_num++;
                SelectItem();

                Data_Send();

                if (ng_ct != 0)
                {
                    SaveData(RESULT_REJECT);
                }
                else
                {
                    SaveData(RESULT_PASS);
                }
                if (checkBox8.Checked == false) // Note PC가 클릭되어 있지 않으면
                {

                    int over_time_check = 0;

                    do
                    {
                        if (X_move_check() && Y_move_check() && Z_move_check())
                        {
                            break;
                        }
                        ComF.timedelay(100);
                        over_time_check++;
                    } while (over_time_check < 200);        //wait 20sec
                }
            }
            test_process_run = false;
            checkBox9.Enabled = true;
            RunningFlag = false;
            SetTestEnd(true);
            return;
        }

        private void TESTTIMER1_Tick(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public void list(int Num, string Meas, string Result)
        {
            listView1.Items[Num - 1].SubItems[3].Text = Meas;
            listView1.Items[Num - 1].SubItems[5].Text = Result;

            if ((Result == "OK") || (Result == "O.K"))
            {
                listView1.Items[Num - 1].SubItems[5].ForeColor = Color.Green;
            }
            else if ((Result == "NG") || (Result == "N.G"))
            {
                listView1.Items[Num - 1].SubItems[5].ForeColor = Color.Red;
            }
            else
            {
                listView1.Items[Num - 1].SubItems[5].ForeColor = Color.Black;
            }

            listView1.Update();
            return;
        }

        public void list(int Num, string Name, string SpecMin, string SpecMax, string Meas, string Result)
        {
            listView1.Items[Num - 1].SubItems[2].BackColor = Color.WhiteSmoke;
            listView1.Items[Num - 1].SubItems[3].BackColor = Color.LemonChiffon;
            listView1.Items[Num - 1].SubItems[4].BackColor = Color.WhiteSmoke;


            listView1.Items[Num - 1].SubItems[0].Text = Num.ToString();
            listView1.Items[Num - 1].SubItems[1].Text = Name;
            listView1.Items[Num - 1].SubItems[2].Text = SpecMin;
            listView1.Items[Num - 1].SubItems[3].Text = Meas;
            listView1.Items[Num - 1].SubItems[4].Text = SpecMax;
            listView1.Items[Num - 1].SubItems[5].Text = Result;
            listView1.Update();
            return;
        }


        private void button20_Click(object sender, EventArgs e)
        {
            //  list();
        }

        private void button21_Click(object sender, EventArgs e)
        {
            //cmObj.Delete();
            //cmObj.SetListView(listView1);
            ScreenInit();
            return;
        }

        private void button22_Click(object sender, EventArgs e)
        {
            //clearDTCinformation ALL CLEAR
            ClearDTCinformation();
        }


       

        private void btn_STOP_Click(object sender, EventArgs e)
        {
            if (test_process_run == true)
            {
                test_process_stop = true;
            }
            return;
        }

        private void button23_Click_1(object sender, EventArgs e)
        {
            //            SP2.WriteLine("^XA");
            //            SP2.WriteLine("^FO 50,80^BY 3");
            //            SP2.WriteLine("^BCN,100,Y,N,N");
            //            SP2.WriteLine("^FD" + textBox7.Text + "^FS");
            //            SP2.WriteLine("^XZ");
        }

        private void button14_Click(object sender, EventArgs e)
        {

        }

        #region 카운터

        public void COUNT_READ()
        {
#if !DEBUG_MODE

            string s = "";

            TOTAL_CNT.Value.AsInteger = Convert.ToInt32(Ods.Read_CNT("CNT", "TOTAL COUNT"));
            OK_CNT.Value.AsInteger = Convert.ToInt32(Ods.Read_CNT("CNT", "OK COUNT"));
            NG_CNT.Value.AsInteger = Convert.ToInt32(Ods.Read_CNT("CNT", "NG COUNT"));
            NowDate = Ods.Read_CNT("CNT", "DATE");
            //NowDate = string.Format("{0:04d}{1:02d}{2:02d}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            s = Ods.Read_CNT("CNT", "comitNo");
            if (s != "")
                comitAdd = Convert.ToInt32(s);
            else comitAdd = 4000;

            CheckDate();

            if (OK_CNT.Value.AsInteger == 0)
                m_OK_count = 0;
            else m_OK_count = OK_CNT.Value.AsInteger;

            if (NG_CNT.Value.AsInteger == 0)
                m_NG_count = 0;
            else m_NG_count = NG_CNT.Value.AsInteger;
            return;
#endif
        }
        public void count_SAVE()
        {
            create_folder();
            Ods.Write_CNT("CNT", "TOTAL COUNT", string.Format("{0}", m_OK_count + m_NG_count));
            Ods.Write_CNT("CNT", "OK COUNT", m_OK_count.ToString());
            Ods.Write_CNT("CNT", "NG COUNT", m_NG_count.ToString());
            Ods.Write_CNT("CNT", "comitNo", comitAdd.ToString());

            CheckDate();
            return;
        }

        public void OK_COUNT()
        {
            m_OK_count++;
            OK_CNT.Value.AsInteger = m_OK_count;
            TOTAL_CNT.Value.AsInteger = (m_OK_count + m_NG_count);
            Ods.Write_CNT("CNT", "TOTAL COUNT", string.Format("{0}", m_OK_count + m_NG_count));
            Ods.Write_CNT("CNT", "OK COUNT", m_OK_count.ToString());
            Ods.Write_CNT("CNT", "NG COUNT", m_NG_count.ToString());
            Ods.Write_CNT("CNT", "comitNo", comitAdd.ToString());
            CheckDate();
            return;
        }

        public void NG_COUNT()
        {
            m_NG_count++;
            NG_CNT.Value.AsInteger = m_NG_count;
            TOTAL_CNT.Value.AsInteger = (m_OK_count + m_NG_count);
            Ods.Write_CNT("CNT", "TOTAL COUNT", string.Format("{0}", m_OK_count + m_NG_count));
            Ods.Write_CNT("CNT", "OK COUNT", m_OK_count.ToString());
            Ods.Write_CNT("CNT", "NG COUNT", m_NG_count.ToString());
            Ods.Write_CNT("CNT", "comitNo", comitAdd.ToString());
            CheckDate();
            return;
        }
        public void create_folder()
        {
            string path = "CNT";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else { }

            /*
            string path1 = "DATA\\" + DateTime.Now.Year + string.Format("{0:d2}", DateTime.Now.Month) + "월";

            if (!Directory.Exists(path1))
            {
                Directory.CreateDirectory(path1);
//              DB_save_data_OK_table_create();
//                DB_save_data_NG_table_create();
            }
            else { }
            */
            string path2 = "Log\\";
            string path3 = "Log\\" + DateTime.Now.Year + string.Format("{0:d2}", DateTime.Now.Month) + string.Format("{0:d2}", DateTime.Now.Day) + ".txt";

            if (!Directory.Exists(path2))
            {
                Directory.CreateDirectory(path2);

                FileStream file = new FileStream(path3, FileMode.OpenOrCreate, FileAccess.Write);

                StreamWriter sw = new StreamWriter(file, System.Text.Encoding.Default);
                sw.Write(DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + ",");
                sw.Write(DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + System.Environment.NewLine);
                sw.Close();
            }
            else
            {
                FileInfo FI = new FileInfo(path3);

                if (FI.Exists == false) //해당 경로에 파일이 존재할 경우
                {
                    FileStream file = new FileStream(path3, FileMode.OpenOrCreate, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(file, System.Text.Encoding.Default);
                    sw.Write(DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + ",");
                    sw.Write(DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + System.Environment.NewLine);
                    sw.Close();
                }
            }

            string path4 = "BARCODE";

            if (!Directory.Exists(path4))
            {
                Directory.CreateDirectory(path4);
            }
            else { }
            return;
        }

        public void Log_SAVE(string save_data)
        {
            try
            {
                string path = "Log\\" + DateTime.Now.Year + string.Format("{0:d2}", DateTime.Now.Month) + string.Format("{0:d2}", DateTime.Now.Day) + ".txt";

                if (File.Exists(path) == false)
                {
                    if (button51.Text == "O.K")
                    {
                        m_OK_count = 1;
                        m_NG_count = 0;
                    }
                    else
                    {
                        m_OK_count = 0;
                        m_NG_count = 1;
                    }
                    count_SAVE();
                    OK_CNT.Value.AsInteger = m_OK_count;
                    NG_CNT.Value.AsInteger = m_NG_count;
                    TOTAL_CNT.Value.AsInteger = (m_OK_count + m_NG_count);
                }
                FileStream file = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                StreamWriter sw = new StreamWriter(file, System.Text.Encoding.Default);

                sw.Write("[" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + ",");
                sw.Write(DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Millisecond + "]");

                //sw.Write(save_data + System.Environment.NewLine);
                sw.Write(save_data);
                sw.WriteLine();

                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        #endregion


        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void button5_Click(object sender, EventArgs e)
        {
        }


        public const int can_tx_wait_time = 20;
        public bool t2_t1_rx_ok = false;

        private bool T2_test_process()
        {
            int wait_time = 0;

            t2_t1_rx_ok = false;
            //Enter security access

            //Enter_Security_Access();

            ComF.timedelay(100);
            if ((test_process_stop == true) || (test_process_estop == true)) return true;

            T2_Test_01();
            t2_t1_rx_ok = false;

            ComF.timedelay(100);
            if ((test_process_stop == true) || (test_process_estop == true)) return true;

            T2_Test_02();
            t2_t1_rx_ok = false;
            wait_time = 0;
            do
            {
                if (t2_t1_rx_ok) return true;
                ComF.timedelay(100);
                wait_time++;

                if ((test_process_stop == true) || (test_process_estop == true)) return true;

            } while (wait_time < 50); //wait 3sec
            return false;
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            T2_test_process();
        }

        private void T3_test_process()
        {
            //Read & Save BoSe SN
            T3_Test_01();
            ComF.timedelay(can_tx_wait_time + 100);
            if ((test_process_stop == true) || (test_process_estop == true)) return;

            //T3_Test_001();
            ComF.timedelay(can_tx_wait_time + 100);
            if ((test_process_stop == true) || (test_process_estop == true)) return;

            //ROM ID
            T3_Test_02();
            ComF.timedelay(can_tx_wait_time + 10);
            if ((test_process_stop == true) || (test_process_estop == true)) return;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            T3_test_process();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {


        }



        private void button25_Click(object sender, EventArgs e)
        {
        }

        private void button26_Click(object sender, EventArgs e)
        {
        }

        private void button27_Click(object sender, EventArgs e)
        {
        }


        private void T1_T5_TEST(bool load_flg)
        {
            //T1
            did_fac1 = 0;
            did_fac2 = 0;
            //did_fac1_ct = 0;
            did_fack1_ct = 0;
            did_fac2_ct = 0;
            button48.Text = "0";
            button49.Text = "0";

            for (int lp = 0; lp < OdsDataSearchCount; lp++)
            {
                T1_Test_01(load_flg);
                ComF.timedelay(100);

                if ((test_process_stop == true) || (test_process_estop == true)) return;
            }

            for (int lp = 0; lp < OdsDataSearchCount; lp++)
            {
                T1_Test_02(load_flg);
                ComF.timedelay(100);
                if ((test_process_stop == true) || (test_process_estop == true)) return;
            }
        }


        private void button10_Click(object sender, EventArgs e)
        {
            T1_T5_TEST(false);

        }

        private void T4_test_process()
        {
            //To read OPA file R3 number
            if (led8.Value.AsBoolean == true) T4_Test_01_heater();
            if (led7.Value.AsBoolean == true) T4_Test_01_no_heater();
            ComF.timedelay(200);
            if ((test_process_stop == true) || (test_process_estop == true)) return;
            T4_Test_02(); //To read OPA file R3 number
            ComF.timedelay(20);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            T4_test_process();
        }

        bool read_seed_ok_flg = false;

        private void Enter_Security_Access()
        {
            //Enter_security_access
            ushort wait_time = 0;

            Enter_security_access_read_seed();
            read_seed_ok_flg = false;

            do
            {
                if (read_seed_ok_flg) break;
                ComF.timedelay(100);
                wait_time++;

                if ((test_process_estop == true) || (test_process_stop == true)) return;
            } while (wait_time < 50); //wait 3sec

            if (read_seed_ok_flg)
            {
                read_seed_ok_flg = false;
                Enter_security_access_write_key();
            }
        }


        private void button11_Click(object sender, EventArgs e)
        {
            Enter_Security_Access();
        }

        private void T6_test_process()
        {
            //T6 New algorithm calibration
            T6_Test_01();
            ComF.timedelay(100);
            if ((test_process_stop == true) || (test_process_estop == true))
            {
                StopProcess();
                return;
            }
            T6_Test_02();      //Cal checksum
            ComF.timedelay(200);
            if ((test_process_stop == true) || (test_process_estop == true))
            {
                StopProcess();
                return;
            }
            return;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            T6_test_process();
        }

        bool t7_t1_rx_ok = false;

        private void T7_test_process()
        {
            //T7 EOL flg write read

            ushort wait_time = 0;

            T7_Test_01();
            t7_t1_rx_ok = false;

            ComF.timedelay(500);

            T7_Test_02();
            t7_t1_rx_ok = false;
            wait_time = 0;
            do
            {
                if (t7_t1_rx_ok) break;
                ComF.timedelay(100);
                wait_time++;
                if (EStopCheck() == true) break;
                if ((test_process_stop == true) || (test_process_estop == true)) break;
            } while (wait_time < 30); //wait 3sec

        }


        private void button12_Click(object sender, EventArgs e)
        {
            T7_test_process();
        }

        bool t8_t1_rx_ok = false;

        private void T8_test_process()
        {
            //T8
            ushort wait_time = 0;

            T8_Test_01();
            t8_t1_rx_ok = false;

            ComF.timedelay(500);

            T8_Test_02();
            t8_t1_rx_ok = false;
            wait_time = 0;
            do
            {
                if (t8_t1_rx_ok) break;
                ComF.timedelay(100);
                wait_time++;
                if (EStopCheck() == true) break;
                if ((test_process_stop == true) || (test_process_estop == true)) break;
            } while (wait_time < 30); //wait 3sec
        }

        private void button7_Click(object sender, EventArgs e)
        {
            T8_test_process();
        }

        private void button59_Click(object sender, EventArgs e)
        {
            //To read OPA file R3 number
            T4_Test_01_no_heater();
            ComF.timedelay(20);
            T4_Test_02(); //To read OPA file R3 number
            ComF.timedelay(20);
        }


        private void SP1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            //MT4Y or Scanner
            try
            {

#if !DEBUG_MODE
                CheckForIllegalCrossThreadCalls = false;

                double temp_data;

                ushort crc16;

                byte crc16_high, crc16_low;


                int iRecSize = SP1.BytesToRead; // 수신된 데이터 갯수

                if (iRecSize == 0)
                {
                    return;
                }

                //string strRxData;


                byte[] buff1 = new byte[iRecSize + 20];

                SP1.Read(buff1, 0, iRecSize);

                //Debug.WriteLine("MT4Y RX: " + BitConverter.ToString(buff1));

                crc16 = CRC16_R(buff1, iRecSize - 2);

                crc16_high = (byte)((crc16 >> 0) & 0x00ff);
                crc16_low = (byte)((crc16 >> 8) & 0x00ff);

                if (crc16_high == buff1[iRecSize - 2] && crc16_low == buff1[iRecSize - 1])
                {
                    switch (buff1[0])
                    {
                        case 1:                 //B+ 전압
                            temp_data = buff1[3] << 8 | buff1[4];
                            temp_data *= 0.01;

                            button29.Text = temp_data.ToString("0.##");
                            break;
                        case 2:                 //B+ 전류 P/SEAT
                            temp_data = buff1[3] << 8 | buff1[4];
                            if (temp_data > 32767) temp_data = 0;
                            temp_data *= 0.1;
                            button30.Text = temp_data.ToString("0.##");
                            break;
                        case 3:                 //SBR 값 읽기
                            temp_data = buff1[3] << 8 | buff1[4];
                            if (temp_data > 32767) temp_data = 0;
                            //temp_data -= 4.0;
                            temp_data *= 0.01;
                            if (textBox39.Text == "") textBox39.Text = "0";
                            temp_data += Convert.ToDouble(textBox39.Text);

                            temp_data *= 64;
                            if (temp_data >= 1000)
                            {
                                button77.Text = "999";
                            }
                            else
                            {
                                button77.Text = temp_data.ToString("0.##");
                            }

                            if (led2.Value.AsBoolean == true) SBRData = (short)temp_data;
                            break;
                    }
                    SP1.DiscardInBuffer();
                }
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                //Debug.WriteLine(ex.Message);
            }

        }


        byte[] MODBUS_RTU_data = { 0x01, 0x04, 0x00, 0x00, 0x00, 0x04, 0, 0 };	//Read holding reg
        public const UInt16 POLYNORMIAL = 0xA001;

        byte rtu_id = 1;

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Enabled = false;

            //MT4Y
            ushort crc16;

            switch (rtu_id)
            {
                case 1:
                    MODBUS_RTU_data[0] = rtu_id;
                    crc16 = CRC16(MODBUS_RTU_data, MODBUS_RTU_data.Length - 2);
                    MODBUS_RTU_data[6] = (byte)((crc16 >> 0) & 0x00ff);
                    MODBUS_RTU_data[7] = (byte)((crc16 >> 8) & 0x00ff);
                    rtu_id = 2;
                    break;

                case 2:
                    MODBUS_RTU_data[0] = rtu_id;
                    crc16 = CRC16(MODBUS_RTU_data, MODBUS_RTU_data.Length - 2);
                    MODBUS_RTU_data[6] = (byte)((crc16 >> 0) & 0x00ff);
                    MODBUS_RTU_data[7] = (byte)((crc16 >> 8) & 0x00ff);
                    rtu_id = 3;
                    break;

                case 3:
                    MODBUS_RTU_data[0] = rtu_id;
                    crc16 = CRC16(MODBUS_RTU_data, MODBUS_RTU_data.Length - 2);
                    MODBUS_RTU_data[6] = (byte)((crc16 >> 0) & 0x00ff);
                    MODBUS_RTU_data[7] = (byte)((crc16 >> 8) & 0x00ff);
                    rtu_id = 1;
                    break;
            }

#if !DEBUG_MODE
            if (SP1.IsOpen)
            {
                SP1.DiscardInBuffer();
                SP1.ReceivedBytesThreshold = 13;

                if (SP1.IsOpen) SP1.Write(MODBUS_RTU_data, 0, MODBUS_RTU_data.Length); // MT4Y Meter req
                //Debug.WriteLine("MT4Y TX: " + BitConverter.ToString(MODBUS_RTU_data));
            }
#endif
            timer2.Enabled = true;

            return;
        }

        public ushort CRC16(byte[] bytes, int usDataLen)
        {
            ushort crc = 0xffff, flag, ct = 0;

            while (usDataLen != 0)
            {
                crc ^= bytes[ct];
                for (int i = 0; i < 8; i++)
                {
                    flag = 0;
                    if ((crc & 1) == 1) flag = 1;
                    crc >>= 1;
                    if (flag == 1) crc ^= POLYNORMIAL;
                }
                ct++;
                usDataLen--;
            }
            return crc;
        }

        public ushort CRC16_R(byte[] bytes, int usDataLen)
        {
            ushort crc = 0xffff, flag, ct = 0;

            while (usDataLen != 0)
            {
                crc ^= bytes[ct];
                for (int i = 0; i < 8; i++)
                {
                    flag = 0;
                    if ((crc & 1) == 1) flag = 1;
                    crc >>= 1;
                    if (flag == 1) crc ^= POLYNORMIAL;
                }
                ct++;
                usDataLen--;
            }
            return crc;
        }


        byte[] MODBUS_RTU_IOSTATUS = { 0x01, 0x03, 0x00, 0x1c, 0x00, 0x02, 0, 0 };	//I/O status request
        byte[] MODBUS_RTU_OPR_CMD = { 0x01, 0x06, 0x07, 0xD0, 0x00, 0x00, 0, 0 };	//OPR CMD write
        byte[] MODBUS_RTU_POSITION = { 0x01, 0x03, 0x00, 0x10, 0x00, 0x03, 0, 0 };	//I/O Read position
        byte[] MODBUS_RTU_POSITION_REAL = { 0x01, 0x03, 0x00, 0x11, 0x00, 0x03, 0, 0 };	//I/O Read position

        byte[] MODBUS_RTU_P_CMD0 = { 0x01, 0x06, 0x01, 0xF4, 0, 0, 0, 0 };	//P CMD 0 write

        //byte[] MODBUS_RTU_P_CMD0 = { 0x01, 0x06, 0x01, 0xF4 - 1, 0x00, 0x02, 0, 0 };	//P CMD 0 write
        byte[] MODBUS_RTU_P_CMD1 = { 0x01, 0x06, 0x01, 0xF5, 0, 0, 0, 0 };	//P CMD 1 write

        byte[] MODBUS_RTU_P_CMD_R0 = { 0x01, 0x03, 0x01, 0xF4, 0x00, 0x02, 0, 0 };	//P CMD 0 read
        byte[] MODBUS_RTU_P_CMD_R1 = { 0x01, 0x03, 0x01, 0xF5, 0x00, 0x02, 0, 0 };	//P CMD 0 read

        byte[] MODBUS_RTU_CURRENT_ALARM_CALL = { 0x01, 0x50, 0x08, 0x34, 0x00, 0x00, 0, 1, 0, 0 };	//CURRENT ALARM CALL
        byte[] MODBUS_RTU_CURRENT_ALARM_CLR = { 0x01, 0x50, 0x08, 0x35, 0x00, 0x00, 0, 2, 0x15, 0x9b };	//CURRENT ALARM CLR
        byte[] MODBUS_RTU_ALARM_CALL = { 0x01, 0x50, 0x08, 0x36, 0x00, 0x00, 0, 3, 0, 0 };	//ALARM CALL
        byte[] MODBUS_RTU_ALARM_CLR = { 0x01, 0x50, 0x08, 0x37, 0x00, 0x00, 0, 4, 0xec, 0x59 };	//ALARM CLR


        byte[] MODBUS_RTU_JOG_ON = { 0x01, 0x46, 0x08, 0x98, 0x00, 0x00, 0, 1, 0, 0 };	//JOG ON
        byte[] MODBUS_RTU_JOG_OFF = { 0x01, 0x46, 0x08, 0x98, 0x00, 0x00, 0, 2, 0, 0 };	//JOG OFF
        byte[] MODBUS_RTU_JOG_CCW = { 0x01, 0x46, 0x08, 0x98, 0x00, 0x00, 0, 3, 0, 0 };	//CCW
        byte[] MODBUS_RTU_JOG_CW = { 0x01, 0x46, 0x08, 0x98, 0x00, 0x00, 0, 4, 0, 0 };	//CW
        byte[] MODBUS_RTU_JOG_STOP = { 0x01, 0x46, 0x08, 0x98, 0x00, 0x00, 0, 5, 0, 0 };	//JOG STOP
        byte[] MODBUS_RTU_SPEED_SET = { 0x01, 0x06, 0x01, 0x31, 0, 0, 0, 0 };	//GROUP1 SPEED SETTING
        byte[] MODBUS_RTU_DEC_SET = { 0x01, 0x06, 0x01, 0x3A, 0, 0, 0, 0 };	//GROUP1 SPEED SETTING
        byte[] MODBUS_RTU_ACC_SET = { 0x01, 0x06, 0x01, 0x35, 0, 0, 0, 0 };	//GROUP1 SPEED SETTING
        byte[] MODBUS_ORG_SPEED_SET = { 0x01, 0x06, 0x02, 0x58, 0, 0, 0, 0 };	//GROUP1 SPEED SETTING


        //Int32 opr_cmd_1 = 0;
        //Int32 opr_cmd_2 = 0;
        //Int32 opr_cmd_3 = 0;

        //string write_position_xdata = "0";
        //string write_position_ydata = "0";
        //string write_position_zdata = "0";

        //int modbus_rx_mode = 0;

        private void servo_position_read(byte slave)
        {
            int Pos;
#if !DEBUG_MODE
            if (slave == 2)
                kMotorPos[slave] = (short)(65536 - ((AXTServo.AxtReadPosition(slave) / 1.0) * -1.0));
            else kMotorPos[slave] = (short)(65536 - ((AXTServo.AxtReadPosition(slave) / 1.0) * -1.0));
#endif

            if (slave == 1)
            {
                if (SbrOrOds == true)
                {
                    if (int.TryParse(textBox48.Text, out Pos) == false) Pos = 0;

                    if (kMotorPos[slave] <= (Pos + 5))
                    {
                        if (OriginSignalOutFlag == true)
                        {
                            IOPort.outportb((short)DO_OUT.TEST_ORIGIN, false);
                            OriginSignalOutFlag = false;
                        }
                    }
                    else
                    {
                        if (OriginSignalOutFlag == false)
                        {
                            IOPort.outportb((short)DO_OUT.TEST_ORIGIN, true);
                            OriginSignalOutFlag = true;
                        }
                    }
                }
                else
                {
                    if (int.TryParse(textBox36.Text, out Pos) == false) Pos = 0;

                    if (kMotorPos[slave] <= (Pos + 5))
                    {
                        if (OriginSignalOutFlag == true)
                        {
                            IOPort.outportb((short)DO_OUT.TEST_ORIGIN, false);
                            OriginSignalOutFlag = false;
                        }
                    }
                    else
                    {
                        if (OriginSignalOutFlag == false)
                        {
                            IOPort.outportb((short)DO_OUT.TEST_ORIGIN, true);
                            OriginSignalOutFlag = true;
                        }
                    }
                }
            }
            switch (slave)
            {
                case 0:
                    textBox14.Text = kMotorPos[slave].ToString();
                    button31.Text = kMotorPos[slave].ToString();
                    textBox19.Text = AXTServo.AxtErrorRead(slave).ToString();
                    break;
                case 1:
                    textBox15.Text = kMotorPos[slave].ToString();
                    button32.Text = kMotorPos[slave].ToString();
                    textBox18.Text = AXTServo.AxtErrorRead(slave).ToString();
                    break;
                case 2:
                    textBox16.Text = kMotorPos[slave].ToString();
                    button33.Text = kMotorPos[slave].ToString();
                    textBox17.Text = AXTServo.AxtErrorRead(slave).ToString();
                    break;
            }
#endif
        }

        private void servo_iostatus_read(byte slave)
        {
        }

        private void servo_accel_Set(byte slave)
        {
            return;
        }


        private void servo_speed_set(byte slave, UInt16 speed)
        {
        }


        private void button28_Click(object sender, EventArgs e)
        {
            servo_position_read(0);
        }

        private void button29_Click(object sender, EventArgs e)
        {
            //            servo_iostatus_read(1);
        }

        private void button30_Click(object sender, EventArgs e)
        {

            return;
        }


        private void EndianConverter(byte[] val, int startIndex, int count)
        {

            //int len = val.Length;

            byte[] tmp = new byte[count];

            int i, j;

            j = startIndex + count;

            for (i = 0; i < count; i++, j--)
            {
                tmp[i] = val[j - 1];
            }

            for (i = 0; i < count; i++, j++)
            {
                val[j] = tmp[i];
            }
        }


        int modbus_tx_ct = 0;
        //bool modbus_tx_flg = false;
        //int modbus_p_cmd_mode = 0;

        //int CMD_P_0 = 0, CMD_P_1 = 0, CMD_P_2 = 0;


        private void TimeTIMER_Tick(object sender, EventArgs e)
        {
            TimeTIMER.Enabled = false;

#if !DEBUG_MODE
            if ((AXTServo.AxtReadAlarm(MOTOR_X) == true) || (AXTServo.AxtReadAlarm(MOTOR_Y) == true) || (AXTServo.AxtReadAlarm(MOTOR_Z) == true))
            {
                if (AXTServo.AxtReadAlarm(MOTOR_Z) == true)
                {
                    if (button38.BackColor != Color.Red) button38.BackColor = Color.Red;
                }
            }
            else
            {
                if (button38.BackColor != Color.Gainsboro) button38.BackColor = Color.Gainsboro;
            }

            switch (modbus_tx_ct)
            {
                case 1:        //modbus #1 Position read                    
                    servo_position_read(MOTOR_X);
                    modbus_tx_ct++;
                    break;
                case 2:         //modbus #2 Position read
                    servo_position_read(MOTOR_Y);
                    modbus_tx_ct++;
                    break;
                case 3:         //modbus #3 Position read
                    servo_position_read(MOTOR_Z);
                    modbus_tx_ct++;
                    break;
                default: modbus_tx_ct++;
                    if (3 < modbus_tx_ct) modbus_tx_ct = 0;
                    break;
            }
#else
            if (button38.BackColor != Color.Gainsboro) button38.BackColor = Color.Gainsboro;
#endif
            TimeTIMER.Enabled = true;
            return;
        }

        private void send_alarm_clr()
        {
#if !DEBUG_MODE
            AXTServo.Axt_Alarm_Reset(MOTOR_X);
            AXTServo.Axt_Alarm_Reset(MOTOR_Y);
            AXTServo.Axt_Alarm_Reset(MOTOR_Z);
#endif
            return;
        }

        //servo #1 이동
        private void X_move_set(string x_dist, ushort Speed)
        {
            if (x_dist == "") x_dist = "0";
            if (SbrOrOds == true)
            {
                if ((Convert.ToDouble(textBox46.Text) + 3) < kMotorPos[MOTOR_Z]) MoveToDefaultZPos();
            }
            else
            {
                if ((Convert.ToDouble(textBox38.Text) + 3) < kMotorPos[MOTOR_Z]) MoveToDefaultZPos();
            }

#if !DEBUG_MODE
            if (X_move_check())
            {
                AXTServo.AxtPositionMove(MOTOR_X, (double)Speed, Convert.ToDouble(x_dist));
            }
#endif
            return;
        }



        private bool X_move_check()
        {
#if !DEBUG_MODE
            return AXTServo.AxtMovingEndCheck(MOTOR_X);
#else
            return false;
#endif
        }


        private void Y_move_set(string Y_dist, ushort Speed)
        {
            if (Y_dist == "") Y_dist = "0";
#if !DEBUG_MODE
            if (SbrOrOds == true)
            {
                if ((Convert.ToDouble(textBox46.Text) + 3) < kMotorPos[MOTOR_Z]) MoveToDefaultZPos();
            }
            else
            {
                if ((Convert.ToDouble(textBox38.Text) + 3) < kMotorPos[MOTOR_Z]) MoveToDefaultZPos();
            }

            AXTServo.AxtPositionMove(MOTOR_Y, (double)Speed, Convert.ToDouble(Y_dist));
#endif
            return;
        }

        private bool Y_move_check()
        {
#if !DEBUG_MODE
            return AXTServo.AxtMovingEndCheck(MOTOR_Y);
#else
            return false;
#endif
        }



        //servo #3 이동
        private void Z_move_set(string move_distance, ushort Speed)
        {
            if (move_distance == "") move_distance = "0";
#if !DEBUG_MODE
            AXTServo.AxtPositionMove(MOTOR_Z, (double)Speed, Convert.ToDouble(move_distance));
#endif
            return;
        }



        private bool Z_move_check()
        {
#if !DEBUG_MODE
            return AXTServo.AxtMovingEndCheck(MOTOR_Z);
#else
            return false;
#endif
        }


        const int ORG_COM_ON = 0x400;
        const int SVON_CW_LIM_CCW_LIM_ON = 0x1C0;
        const int wait_time = 100;


        private void X_servo_on()
        {
            //#1 SERVO ON
#if !DEBUG_MODE
            AXTServo.Axt_ServoOnOff(MOTOR_X, true);
            AXTServo.AxtMotorInit(ServoXDir, ServoYDir, ServoZDir);
#endif
            return;
        }

        private void X_servo_off()
        {
            //#1 SERVO OFF
#if !DEBUG_MODE
            AXTServo.Axt_ServoOnOff(MOTOR_X, false);
#endif
            return;
        }

        private void Y_servo_on()
        {
            //#2 SERVO ON
#if !DEBUG_MODE
            AXTServo.Axt_ServoOnOff(MOTOR_Y, true);
            AXTServo.AxtMotorInit(ServoXDir, ServoYDir, ServoZDir);
#endif
            return;
        }

        private void Y_servo_off()
        {
            //#2 SERVO OFF
#if !DEBUG_MODE
            AXTServo.Axt_ServoOnOff(MOTOR_Y, false);
#endif
            return;
        }


        private void Z_servo_on()
        {
            //#3 SERVO ON
#if !DEBUG_MODE
            ZMotorBrack(OFF);
            AXTServo.Axt_ServoOnOff(MOTOR_Z, true);
            AXTServo.AxtMotorInit(ServoXDir, ServoYDir, ServoZDir);
#endif
            return;
        }

        private void Z_servo_off()
        {
            //#3 SERVO OFF
#if !DEBUG_MODE
            ZMotorBrack(ON);
            AXTServo.Axt_ServoOnOff(MOTOR_Z, false);
#endif
            return;
        }


        private void SP2_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            SP2.DiscardInBuffer();
            return;
        }

        private void button39_Click(object sender, EventArgs e)
        {
            //X,Y,Z servo on

            if (button39.BackColor == Color.Lime)
            {
                button39.BackColor = SystemColors.Control;
#if !DEBUG_MODE
                //AXTServo.Axt_ServoOnOff(MOTOR_X, false);
                //AXTServo.Axt_ServoOnOff(MOTOR_Y, false);
                //AXTServo.Axt_ServoOnOff(MOTOR_Z, false);
                X_servo_off();
                Y_servo_off();
                Z_servo_off();
                //ZMotorBrack(ON);
#endif
            }
            else
            {
                button39.BackColor = Color.Lime;
#if !DEBUG_MODE
                //AXTServo.Axt_ServoOnOff(MOTOR_X, true);
                //AXTServo.Axt_ServoOnOff(MOTOR_Y, true);
                //AXTServo.Axt_ServoOnOff(MOTOR_Z, true);
                X_servo_on();
                Y_servo_on();
                Z_servo_on();
                //ZMotorBrack(OFF);
#endif
            }
            return;
        }

        private void button53_MouseDown(object sender, MouseEventArgs e)
        {
            //X 원점
#if !DEBUG_MODE
            AXTServo.OriginMove(MOTOR_X);
#endif
            return;
        }

        private void button53_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void button52_MouseDown(object sender, MouseEventArgs e)
        {
            //Y 원점
#if !DEBUG_MODE
            AXTServo.OriginMove(MOTOR_Y);
#endif
            return;
        }

        private void button52_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void button54_MouseDown(object sender, MouseEventArgs e)
        {
            //Z 원점 
#if !DEBUG_MODE
            AXTServo.OriginMove(MOTOR_Z);
#endif
            return;
        }

        private void button54_MouseUp(object sender, MouseEventArgs e)
        {
            //Z 원점 OFF

        }

        private void button31_TextChanged(object sender, EventArgs e)
        {

        }

        private void button32_TextChanged(object sender, EventArgs e)
        {

        }


        private void test_process_RUN()
        {
            if (ProductOutFlag2 == false) ProductOutFlag2 = true;
            if (txt_prd.Value.AsBoolean == false)
            {
                label146.Text = "제품이 안착되어 있지 않습니다.";
                this.Controls.Add(this.panel9);
                panel9.BringToFront();
                panel9.Location = new Point((Screen.PrimaryScreen.Bounds.Width / 2) - (panel9.Width / 2), (Screen.PrimaryScreen.Bounds.Height / 2) - panel9.Height);
                panel9.Visible = true;
                ComF.timedelay(2000);
                panel9.Visible = false;
                ProductOutFlag2 = false;
                PopToSbrOdsSelect = true;
                return;
            }
            if (txt_jig.Value.AsBoolean == false)
            {
                label146.Text = "지그가 상승되어 있지 않습니다.";
                this.Controls.Add(this.panel9);
                panel9.BringToFront();
                panel9.Location = new Point((Screen.PrimaryScreen.Bounds.Width / 2) - (panel9.Width / 2), (Screen.PrimaryScreen.Bounds.Height / 2) - panel9.Height);
                panel9.Visible = true;
                ComF.timedelay(2000);
                panel9.Visible = false;
                ProductOutFlag2 = false;
                PopToSbrOdsSelect = true;
                return;
            }
            if (SbrOrOds == false)
            {
                if (led8.Value.AsBoolean == true)
                {
                    if (led11.Value.AsBoolean == true)
                    {
                        ConnectingMsgDisplay(0, false);
                    }
                    else
                    {
                        label146.Text = "히터 콘넥터가 연결되지 않았습니다.";
                        this.Controls.Add(this.panel9);
                        panel9.BringToFront();
                        panel9.Location = new Point((Screen.PrimaryScreen.Bounds.Width / 2) - (panel9.Width / 2), (Screen.PrimaryScreen.Bounds.Height / 2) - panel9.Height);
                        panel9.Visible = true;
                        ComF.timedelay(2000);
                        panel9.Visible = false;
                        ProductOutFlag2 = false;
                        PopToSbrOdsSelect = true;
                        return;
                    }
                }
                else
                {
                    if (led13.Value.AsBoolean == false)
                    {
                        ConnectingMsgDisplay(1, false);
                    }
                    else
                    {
                        label146.Text = "선택된 모델과 상이 합니다. 희터 사양 제품 입니다.";
                        this.Controls.Add(this.panel9);
                        panel9.BringToFront();
                        panel9.Location = new Point((Screen.PrimaryScreen.Bounds.Width / 2) - (panel9.Width / 2), (Screen.PrimaryScreen.Bounds.Height / 2) - panel9.Height);
                        panel9.Visible = true;
                        ComF.timedelay(2000);
                        panel9.Visible = false;
                        ProductOutFlag2 = false;
                        PopToSbrOdsSelect = true;
                        return;
                    }
                }
            }
            else
            {
                ConnectingMsgDisplay(2, false);
            }


            if (test_process_run == false)
            {
                if (panel17.Visible == false) // pop 가 끊희거나 끊힘을 확인하지 않았을 경우 검사를 진행하지 못하도록 한다.
                {
                    DataCount = 0;
                    RunningFlag = true;
                    test_process_stop = false;
                    test_process_run = true;

                    Thread Test_process = new Thread(new ThreadStart(Test_process_m));
                    Test_process.Start();
                }
            }
            else
            {
                //Debug.WriteLine("Test_process Threed RUNING!!!");
            }
        }

        private void button47_MouseDown(object sender, MouseEventArgs e)
        {
            //X 증가
            button47.BackColor = Color.Lime;
#if !DEBUG_MODE
            AXTServo.AxtJogLeftMove(MOTOR_X, Convert.ToDouble(textBox83.Text));
#endif
            return;
        }

        private void button47_MouseUp(object sender, MouseEventArgs e)
        {
            //X 증가 off
            button47.BackColor = SystemColors.Control;
#if !DEBUG_MODE
            AXTServo.Axt_EStop(MOTOR_X);
#endif
            return;
        }

        private void button56_MouseDown(object sender, MouseEventArgs e)
        {
            //X 감소
            button56.BackColor = Color.Lime;
#if !DEBUG_MODE
            AXTServo.AxtJogRightMove(MOTOR_X, Convert.ToDouble(textBox83.Text));
#endif
            return;
        }

        private void button56_MouseUp(object sender, MouseEventArgs e)
        {
            //X 감소 OFF
            button56.BackColor = SystemColors.Control;
#if !DEBUG_MODE
            AXTServo.Axt_EStop(MOTOR_X);
#endif
            return;
        }

        private void button35_MouseDown(object sender, MouseEventArgs e)
        {
            //Y INC
            button35.BackColor = Color.Lime;
#if !DEBUG_MODE
            AXTServo.AxtJogBackwordMove(MOTOR_Y, Convert.ToDouble(textBox83.Text));
#endif
            return;
        }

        private void button35_MouseUp(object sender, MouseEventArgs e)
        {
            //Y INC OFF            
            button35.BackColor = SystemColors.Control;
#if !DEBUG_MODE
            AXTServo.Axt_EStop(MOTOR_Y);
#endif
            return;
        }

        private void button34_MouseDown(object sender, MouseEventArgs e)
        {
            //Y DEC 
            button34.BackColor = Color.Lime;
#if !DEBUG_MODE
            AXTServo.AxtJogForwordMove(MOTOR_Y, Convert.ToDouble(textBox83.Text));
#endif
            return;
        }

        private void button34_MouseUp(object sender, MouseEventArgs e)
        {
            //Y DEC OFF
            button34.BackColor = SystemColors.Control;
#if !DEBUG_MODE
            AXTServo.Axt_EStop(MOTOR_Y);
#endif
            return;
        }

        private void button37_MouseDown(object sender, MouseEventArgs e)
        {
            //Z INC
            button37.BackColor = Color.Lime;
#if !DEBUG_MODE
            AXTServo.AxtJogDownMove(MOTOR_Z, Convert.ToDouble(textBox83.Text));
#endif
            return;
        }

        private void button37_MouseUp(object sender, MouseEventArgs e)
        {
            //Z INC OFF
            button37.BackColor = SystemColors.Control;
#if !DEBUG_MODE
            AXTServo.Axt_EStop(MOTOR_Z);
#endif
            return;
        }

        private void button36_MouseDown(object sender, MouseEventArgs e)
        {
            //Z DEC
            button36.BackColor = Color.Lime;
#if !DEBUG_MODE
            AXTServo.AxtJogUpMove(MOTOR_Z, Convert.ToDouble(textBox83.Text));
#endif
            return;
        }

        private void button36_MouseUp(object sender, MouseEventArgs e)
        {
            //Z DEC OFF
            button36.BackColor = SystemColors.Control;
#if !DEBUG_MODE
            AXTServo.Axt_EStop(MOTOR_Z);
#endif
            return;
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            //loadcell
            try
            {
#if !DEBUG_MODE
                CheckForIllegalCrossThreadCalls = false;

                this.Invoke(new EventHandler(LoadCellCheck));
                serialPort1.DiscardInBuffer();
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                //Debug.WriteLine(ex.Message);

            }
        }


        //bool Z_moveing_flg = false;    //Z축 이동 검사 진행 중 flg

        private void Z_down_test()
        {
            //Z축 시험
#if !DEBUG_MODE
            bool SetFlag;
            //bool ReadFlag;
            bool Flag;
            //bool mFlag;
            string s;
            string log_data;
            //int over_time_check = 0;
            double First;
            double Last;
            double xTime;
            double sLoad;
            double ReadMin;
            double ReadMax;
            double RealData = 999;
            double Spec;
            double xStroke;

            string DefaultTestPosition;
            UInt16 DefaultSpeed;

            double DefaultLoadMin;
            double DefaultLoadMax;
            double CheckValue = 0.0;

            string CheckPosition;
            double CheckLoadOds;
            double CheckLoadSbr;
            double CheckTimeOds;
            double CheckTimeSbr;

            LoadMode = 0;
            //-------------------------[Speed 및 포지션 값 설정 시작]
            CheckLoadOds = Convert.ToDouble(button61.Text);
            CheckLoadSbr = Convert.ToDouble(button81.Text);

            DefaultLoadMin = Convert.ToDouble(button61.Text);
            DefaultLoadMax = Convert.ToDouble(button81.Text);

            if (SbrOrOds == true)
            {
                //for SBR
                DefaultTestPosition = textBox45.Text;
                CheckPosition = textBox45.Text;
            }
            else
            {
                //for ODS
                DefaultTestPosition = textBox37.Text;
                CheckPosition = textBox37.Text;
            }

            CheckTimeOds = Convert.ToInt16(textBox26.Text) * 1000;
            CheckTimeSbr = Convert.ToInt16(textBox42.Text) * 1000;

            DefaultSpeed = Convert.ToUInt16(textBox70.Text);

            xStroke = Convert.ToDouble(textBox82.Text);
            //-------------------------[Speed 및 포지션 값 설정 끝]


            xTime = 0;
            Flag = false;
            First = ComF.timeGetTimems();
            Last = ComF.timeGetTimems();
            if (SbrOrOds == false) //SBR Test 가 아닐경우
            {
                //over_time_check = 0;
                do
                {
                    //X 축 및 Y 축 모터 이동이 완료될 때까지 대기한다.
                    if (X_move_check() && Y_move_check())
                    {
                        Flag = true;
                        break;
                    }

                    if ((test_process_stop == true) || (test_process_estop == true))
                    {
                        AllMotorStop();
                        return;
                    }
                    if (LadcellOverCheck() == true)
                    {
                        test_process_stop = true;
                        AllMotorStop();
                        return;
                    }
                    Application.DoEvents();
                    Last = ComF.timeGetTimems();
                    xTime = Last - First;
                } while (xTime < 20000);        //wait 20sec
                //while (over_time_check < 200);        //wait 20sec
                if (Flag == false)
                {
                    //타임 아웃에 위의한 리턴
                    AllMotorStop();
                    test_process_stop = true;
                    return;
                }

                Flag = false;


                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    AllMotorStop();
                    return;
                }

                // 이 값 자체를 좀 천천히 움직이게 하면 어덜까??????? 
                //Z_move_set(DefaultTestPosition, DefaultSpeed);      
                Z_move_set(DefaultTestPosition, 90);

                ComF.timedelay(200);
                //over_time_check = 0;

                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    AllMotorStop();
                    return;
                }

                //Z_moveing_flg = true;

                Flag = false;
                SetFlag = false;
                First = ComF.timeGetTimems();
                Last = ComF.timeGetTimems();

                do
                {
                    Last = ComF.timeGetTimems();
                    xTime = Last - First;
                    if (!Z_move_check())
                    {
                        if (3.0 <= kLoadCell)
                        {
                            Z_moter_stop();
                            Z_moter_stop();
                            ToZeroPositionSearch(true);
                            break;  //모터가 이동하면 빠져 나간다.
                        }
                    }

                    if (3.0 <= kLoadCell)
                    {
                        Z_moter_stop();
                        Z_moter_stop();
                        //초기 위치에서 눌렀을때 3 키로 그램 이상일때 빠저나가기전 0점 찾기를 하는데 측정 압력보다 높을 경우에만 진행한다.
                        if ((kLoadCell >= DefaultLoadMin && SbrOrOds == false) || (kLoadCell >= DefaultLoadMax && SbrOrOds == true)) ToZeroPositionSearch(true);
                        break;
                    }

                    if ((test_process_stop == true) || (test_process_estop == true))
                    {
                        AllMotorStop();
                        return;
                    }

                    if ((xStroke + 30) <= kMotorPos[MOTOR_Z])
                    {
                        /*
                        검사 대기 위치보다 30미리 내려같다면 서서히 움직이게 한다.
                        */
                        if (SetFlag == false)
                        {
                            AXTServo.Axt_NStop(MOTOR_Z);
                            ComF.timedelay(300);

                            if (SbrOrOds == true)
                            {
                                //s = textBox45.Text;
                                Z_move_set(DefaultTestPosition, 50);      //for SBR
                                //                    ComF.timedelay(500);
                            }
                            else
                            {
                                //s = textBox37.Text;
                                Z_move_set(DefaultTestPosition, 50);      //for ODS                
                            }

                            ComF.timedelay(200);
                        }
                        SetFlag = true;
                    }

                    Application.DoEvents();
                } while (xTime < 20000);
            }

            float StartStroke = kMotorPos[MOTOR_Z];


            AXTServo.Axt_EStop(MOTOR_Z);
            Z_move_set(CheckPosition, 5);

            ComF.timedelay(200);
            if ((test_process_stop == true) || (test_process_estop == true))
            {
                AllMotorStop();
                return;
            }

            First = ComF.timeGetTimems();
            Last = ComF.timeGetTimems();


            while (true)
            {
                Application.DoEvents();

                if (SbrOrOds == false)
                {
                    //ODS
                    if ((ODS_OffsetLoad == 0) || (ODS_OffsetStroke == 0))
                        ODSOffset = 0;
                    else ODSOffset = (kMotorPos[MOTOR_Z] - StartStroke) * (ODS_OffsetLoad / ODS_OffsetStroke);
                    LoadMode = 1;
                }
                else
                {
                    //SBR
                    if ((SBR_OffsetLoad == 0) || (SBR_OffsetStroke == 0))
                        SBROffset = 0;
                    else SBROffset = (kMotorPos[MOTOR_Z] - StartStroke) * (SBR_OffsetLoad / SBR_OffsetStroke);
                    LoadMode = 2;
                }
                //                button28.Text = string.Format("{0:0.000}", SBROffset);
                if (LadcellOverCheck() == true)
                {
                    AllMotorStop();
                    test_process_stop = true;
                    LoadMode = 0;
                    return;
                }
                else if ((kLoadCell >= CheckLoadOds && SbrOrOds == false) || (kLoadCell >= CheckLoadSbr && SbrOrOds == true))
                {
                    bool sFlag;

                    AXTServo.Axt_EStop(MOTOR_Z);

                    if (SbrOrOds == false)
                    {
                        ReadMin = CheckLoadOds - 3.0;
                        ReadMax = CheckLoadOds + 3.0;
                        Spec = CheckLoadOds + 0.5;
                    }
                    else
                    {
                        ReadMin = CheckLoadSbr - 3.0;
                        ReadMax = CheckLoadSbr + 3.0;
                        Spec = CheckLoadSbr + 0.5;
                    }

                    Z_move_set(textBox13.Text, Convert.ToUInt16(textBox65.Text));      //for SBR 복귀 위치까지                    

                    ComF.timedelay(200);
                    First = ComF.timeGetTimems();
                    Last = ComF.timeGetTimems();
                    sFlag = false;
                    do
                    {
                        if (SbrOrOds == false)
                        {
                            //ODS
                            if ((ODS_OffsetLoad == 0) || (ODS_OffsetStroke == 0))
                                ODSOffset = 0;
                            else ODSOffset = (kMotorPos[MOTOR_Z] - StartStroke) * (ODS_OffsetLoad / ODS_OffsetStroke);
                            LoadMode = 1;
                        }
                        else
                        {
                            //SBR
                            if ((SBR_OffsetLoad == 0) || (SBR_OffsetStroke == 0))
                                SBROffset = 0;
                            else SBROffset = (kMotorPos[MOTOR_Z] - StartStroke) * (SBR_OffsetLoad / SBR_OffsetStroke);
                            LoadMode = 2;
                        }
                        if (LadcellOverCheck() == true)
                        {
                            AllMotorStop();
                            test_process_stop = true;
                            LoadMode = 0;
                            return;
                        }

                        if ((kLoadCell <= (CheckLoadOds - 0.5)) && (SbrOrOds == false))
                        {
                            AXTServo.Axt_EStop(MOTOR_Z);
                            break;
                        }
                        else if ((kLoadCell <= (CheckLoadSbr - 0.5)) && (SbrOrOds == true))
                        {
                            AXTServo.Axt_EStop(MOTOR_Z);
                            break;
                        }

                        if ((ReadMin <= kLoadCell) && (kLoadCell <= ReadMax))
                        {
                            if (button77.Text.Length != 0) CheckValue = Convert.ToDouble(button77.Text);

                            if (CheckValue < RealData) RealData = CheckValue;
                        }

                        if (sFlag == true)
                        {
                            if (Z_move_check())
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (!Z_move_check()) sFlag = true;
                        }

                        if ((test_process_stop == true) || (test_process_estop == true))
                        {
                            AllMotorStop();
                            LoadMode = 0;
                            return;
                        }

                        Application.DoEvents();
                        Last = ComF.timeGetTimems();
                    } while ((Last - First) < 10000);



                    sFlag = false;
                    if (SbrOrOds)
                    {
                        Z_move_set(textBox45.Text, 2);  //for SBR
                    }
                    else
                    {
                        Z_move_set(textBox37.Text, 2);  //for ODS
                    }
                    ComF.timedelay(200);
                    if ((test_process_stop == true) || (test_process_estop == true))
                    {
                        AllMotorStop();
                        LoadMode = 0;
                        return;
                    }

                    //over_time_check = 0;
                    First = ComF.timeGetTimems();
                    Last = ComF.timeGetTimems();
                    do
                    {
                        if (SbrOrOds == false)
                        {
                            //ODS
                            if ((ODS_OffsetLoad == 0) || (ODS_OffsetStroke == 0))
                                ODSOffset = 0;
                            else ODSOffset = (kMotorPos[MOTOR_Z] - StartStroke) * (ODS_OffsetLoad / ODS_OffsetStroke);
                            LoadMode = 1;
                        }
                        else
                        {
                            //SBR
                            if ((SBR_OffsetLoad == 0) || (SBR_OffsetStroke == 0))
                                SBROffset = 0;
                            else SBROffset = (kMotorPos[MOTOR_Z] - StartStroke) * (SBR_OffsetLoad / SBR_OffsetStroke);
                            LoadMode = 2;
                        }
                        if (LadcellOverCheck() == true)
                        {
                            AllMotorStop();
                            test_process_stop = true;
                            return;
                        }
                        //else if (((kLoadCell >= Convert.ToDouble(button61.Text) && SbrOrOds == false)) || ((kLoadCell >= Convert.ToDouble(button81.Text) && SbrOrOds == true)))                        
                        else if (((kLoadCell >= Spec && SbrOrOds == false)) || ((kLoadCell >= Spec && SbrOrOds == true)))
                        {
                            AXTServo.Axt_EStop(MOTOR_Z);
                            break;
                        }
                        if ((ReadMin <= kLoadCell) && (kLoadCell <= ReadMax))
                        {
                            if (button77.Text.Length != 0) CheckValue = Convert.ToDouble(button77.Text);

                            if (CheckValue < RealData) RealData = CheckValue;
                        }

                        if (sFlag == true)
                        {
                            if (Z_move_check())
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (!Z_move_check()) sFlag = true;
                        }
                        if ((test_process_stop == true) || (test_process_estop == true))
                        {
                            AllMotorStop();
                            return;
                        }


                        Application.DoEvents();
                        Last = ComF.timeGetTimems();
                    } while ((Last - First) < 10000);

                    //----------------------------------------------------------
                    //딜레이 역활만 한다. 다른 곳에서 쓰이지 않는다.
                    double Data;

                    Data = 0;
                    for (int i = 0; i < 100; i++)
                    {
                        ComF.timedelay(10);
                        Data += Convert.ToDouble(button60.Text);
                    }

                    Data = Data / 100.0;

                    if (SbrOrOds == false)
                    {
                        //ODS의 경우 딜레이 1초를 더 준다.
                        Data = 0;
                        for (int i = 0; i < 100; i++)
                        {
                            ComF.timedelay(10);
                            Data += Convert.ToDouble(button60.Text);
                        }

                        Data = Data / 100.0;
                    }
                    button60.Text = Data.ToString();

                    button79.Text = button60.Text;
                    //-----------------------------------------------------------
                    if (SbrOrOds)
                    {
                        First = ComF.timeGetTimems();
                        Last = ComF.timeGetTimems();

                        do
                        {
                            if (button77.Text.Length != 0) CheckValue = Convert.ToDouble(button77.Text);

                            if (CheckValue < RealData) RealData = CheckValue;
                            Application.DoEvents();
                            Last = ComF.timeGetTimems();
                        } while ((Last - First) < 1000);

                        button80.Text = RealData.ToString();
                        log_data = "SBR저항값(Ohm)" + button80.Text;
                    }
                    else
                    {
                        T1_T5_TEST(true);

                        if (button48.Text == "0") //무게값이 없다면
                        {
                            T1_T5_TEST(true);

                            if (button48.Text == "0") //무게값이 없다면
                            {
                                T1_T5_TEST(true);

                                if (button48.Text == "0") //무게값이 없다면
                                {
                                    T1_T5_TEST(true);
                                }
                            }
                        }

                        log_data = "";
                    }
                    Log_SAVE(log_data);
                    Flag = true;
                    break;
                }
                Last = ComF.timeGetTimems();


                xTime = Last - First;
                if (((xTime > CheckTimeOds) && SbrOrOds == false) || ((xTime > CheckTimeSbr) && SbrOrOds))
                {
                    Flag = true;
                    break;
                }
                if (Z_move_check())
                {
                    Flag = false;
                    break;
                }

                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    AllMotorStop();
                    LoadMode = 0;
                    return;
                }
                if (LadcellOverCheck() == true)
                {
                    test_process_stop = true;
                    AllMotorStop();
                    LoadMode = 0;
                    return;
                }
            }

            AXTServo.Axt_EStop(MOTOR_Z);

            if ((test_process_stop == true) || (test_process_estop == true))
            {
                LoadMode = 0;
                AllMotorStop();
                return;
            }

            if ((Convert.ToInt16(textBox26.Text) * 1000) <= xTime)
            {
                test_process_stop = true;
                LoadMode = 0;
                return;
            }

            //Z_moveing_flg = false;

            if (Flag == false) //검출이 안되었다면
            {
                bool sFlag;
                double Press;

                LoadMode = 0;
                if (SbrOrOds == false)
                {
                    //for ODS
                    if (15 < CheckLoadOds)
                        Press = 15.0;
                    else Press = 3;
                }
                else
                {
                    //for SBR
                    if (15 < CheckLoadSbr)
                        Press = 15.0;
                    else Press = 3;
                }

                if (SbrOrOds)
                {
                    s = string.Format("{0}", Convert.ToInt16(textBox45.Text));
                    Z_move_set(s, Convert.ToUInt16(textBox65.Text));      //for SBR
                }
                else
                {
                    s = string.Format("{0}", Convert.ToInt16(textBox37.Text));
                    Z_move_set(s, (ushort)5);      //for ODS
                }
                ComF.timedelay(200);
                //Z_moveing_flg = true;

                //over_time_check = 0;
                Flag = false;
                sFlag = false;

                First = ComF.timeGetTimems();
                Last = ComF.timeGetTimems();

                do
                {
                    Last = ComF.timeGetTimems();
                    xTime = Last - First;
                    if (Z_move_check()) break;
                    if (LadcellOverCheck() == true)
                    {
                        AllMotorStop();
                        test_process_stop = true;
                        return;
                    }
                    else
                    {
                        if (SbrOrOds == false)
                        {
                            if ((CheckLoadOds - kLoadCell) <= Press) AXTServo.Axt_EStop(MOTOR_Z);
                        }
                        else
                        {
                            if ((CheckLoadSbr - kLoadCell) <= Press) AXTServo.Axt_EStop(MOTOR_Z);
                        }
                        if ((test_process_stop == true) || (test_process_estop == true))
                        {
                            AllMotorStop();
                            return;
                        }
                    }

                    if ((test_process_stop == true) || (test_process_estop == true))
                    {
                        AllMotorStop();
                        return;
                    }
                    Application.DoEvents();
                } while (xTime < 2000);

                if (SbrOrOds == false)
                    sLoad = Convert.ToDouble(button61.Text) - 0.8;
                else sLoad = Convert.ToDouble(button81.Text);

                while (true)
                {
                    Application.DoEvents();
                    if (sFlag == false)
                    {
                        if (LadcellOverCheck() == true)
                        {
                            AllMotorStop();
                            test_process_stop = true;
                            return;
                        }
                        else
                        {
                            bool eFlag;

                            eFlag = false;
                            if (SbrOrOds == false)
                            {
                                if ((CheckLoadOds - kLoadCell) <= Press) eFlag = true;
                            }
                            else
                            {
                                if ((CheckLoadSbr - kLoadCell) <= Press) eFlag = true;
                            }
                            if (eFlag == true)
                            {
                                sFlag = true;

                                AXTServo.Axt_EStop(MOTOR_Z);

                                if ((test_process_stop == true) || (test_process_estop == true))
                                {
                                    AllMotorStop();
                                    return;
                                }

                                if (SbrOrOds)
                                {
                                    s = string.Format("{0}", Convert.ToInt16(textBox45.Text));
                                    Z_move_set(s, (ushort)70);      //for SBR
                                }
                                else
                                {
                                    s = string.Format("{0}", Convert.ToInt16(textBox37.Text));
                                    Z_move_set(s, (ushort)70);      //for ODS                                
                                }
                                ComF.timedelay(200);
                                //Z_moveing_flg = true;
                            }
                        }
                    }

                    if (LadcellOverCheck() == true)
                    {
                        AllMotorStop();
                        test_process_stop = true;
                        return;
                    }
                    else if ((kLoadCell >= sLoad && SbrOrOds == false) || (kLoadCell >= sLoad && SbrOrOds))
                    {
                        long Pos;

                        Pos = DataCount;

                        AXTServo.Axt_EStop(MOTOR_Z);

                        if ((test_process_stop == true) || (test_process_estop == true))
                        {
                            AllMotorStop();
                            return;
                        }

                        button79.Text = button60.Text;

                        if (SbrOrOds == true)
                        {
                            button80.Text = button77.Text;       //save sbr저항값
                            log_data = "SBR저항값(Ohm)" + button80.Text;

                        }
                        else
                        {
                            T1_T5_TEST(true);
                            log_data = "";
                        }
                        Log_SAVE(log_data);
                        Flag = true;
                        break;
                    }
                    Last = ComF.timeGetTimems();

                    xTime = Last - First;
                    if (((xTime > CheckTimeOds) && SbrOrOds == false) || ((xTime > CheckTimeSbr) && SbrOrOds))
                    {
                        Flag = true;
                        break;
                    }
                    if (!Z_move_check()) break;

                    if ((test_process_stop == true) || (test_process_estop == true))
                    {
                        AllMotorStop();
                        return;
                    }
                    if (LadcellOverCheck() == true)
                    {
                        test_process_stop = true;
                        AllMotorStop();
                        return;
                    }
                    //                    plot1.Channels[0].AddXY(kLoadCell, SBRData);
                }

                if ((Convert.ToInt16(textBox26.Text) * 1000) <= xTime)
                {
                    AllMotorStop();
                    DefaultPositionMove();
                    test_process_stop = true;
                    return;
                }

                AXTServo.Axt_EStop(MOTOR_Z);

                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    AllMotorStop();
                    return;
                }
                //Z_moveing_flg = false;
            }

            if (SbrOrOds == true)
            {
                Z_move_set(textBox46.Text, Convert.ToUInt16(textBox70.Text));  //for SBR
            }
            else
            {
                Z_move_set(textBox38.Text, Convert.ToUInt16(textBox70.Text));  //for ODS
            }
            ComF.timedelay(200);
            if (SbrOrOds == true)
            {
                First = ComF.timeGetTimems();
                Last = ComF.timeGetTimems();

                do
                {
                    if (!Z_move_check()) //모터가 이동을 시작하면 빠져 나간다.
                    {
                        break;
                    }

                    if (LadcellOverCheck() == true)
                    {
                        AllMotorStop();
                        DefaultPositionMove();
                        test_process_stop = true;
                        LoadMode = 0;
                        return;
                    }

                    if ((test_process_stop == true) || (test_process_estop == true))
                    {
                        AllMotorStop();
                        LoadMode = 0;
                        return;
                    }
                    if (LadcellOverCheck() == true)
                    {
                        test_process_stop = true;
                        AllMotorStop();
                        LoadMode = 0;
                        return;
                    }
                    Last = ComF.timeGetTimems();
                    xTime = Last - First;
                    Application.DoEvents();
                } while (xTime < 20000);        //wait 20sec
                do
                {
                    if (Z_move_check())
                    {
                        break;
                    }
                    if (LadcellOverCheck() == true)
                    {
                        AllMotorStop();
                        DefaultPositionMove();
                        test_process_stop = true;
                        LoadMode = 0;
                        return;
                    }
                    Last = ComF.timeGetTimems();
                    xTime = Last - First;
                    Application.DoEvents();
                } while (xTime < 20000);        //wait 20sec


                if (SbrOrOds == true)
                {
                    X_move_set(textBox50.Text, Convert.ToUInt16(textBox70.Text));                  //검사대기위치 이동 for SBR
                }
                else
                {
                    X_move_set(textBox33.Text, Convert.ToUInt16(textBox70.Text));                  //검사대기위치 이동 for ODS
                }

                if (SbrOrOds == true)
                {
                    Y_move_set(textBox48.Text, Convert.ToUInt16(textBox70.Text));                  //검사대기위치 이동 for SBR
                }
                else
                {
                    Y_move_set(textBox36.Text, Convert.ToUInt16(textBox70.Text));                  //검사대기위치 이동 for ODS
                }
            }
            LoadMode = 0;

#endif
            return;
        }

        private void SBR_no_reactive_Z_down_test_a4()
        {
            //Z축 시험  
#if !DEBUG_MODE
            string log_data;
            bool Flag;
            string s;
            double First;
            double Last;
            double xTime;
            double sLoad;


            int over_time_check;

            over_time_check = 0;

            xTime = 0;
            First = ComF.timeGetTimems();
            Last = ComF.timeGetTimems();
            over_time_check = 0;
            do
            {
                if (X_move_check() && Y_move_check()) //x,y 포지션 이동 완료 확인
                {
                    break;
                }
                if (EStopCheck() == true)
                {
                    AllMotorStop();
                    return;

                }
                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    AllMotorStop();
                    return;
                }
                if (LadcellOverCheck() == true)
                {
                    AllMotorStop();
                    DefaultPositionMove();
                    test_process_stop = true;
                    return;
                }
                Last = ComF.timeGetTimems();
                xTime = Last - First;

                Application.DoEvents();
            } while (xTime < 20000);        //wait 20sec

            //이 값을 z_down_test 와 같이 속도값을 낮게 가져간다. (가끔 검사를 못하는 경우 때문에)
            if (SbrOrOds == true)
            {
                s = textBox45.Text;
                //Z_move_set(s,Convert.ToUInt16(textBox70.Text));      //for SBR
                Z_move_set(s, 90);
            }
            else
            {
                s = textBox37.Text;
                //Z_move_set(s,Convert.ToUInt16(textBox70.Text));      //for ODS
                Z_move_set(s, 90);
            }
            ComF.timedelay(200);
            //Z_moveing_flg = true;

            over_time_check = 0;
            Flag = false;
            First = ComF.timeGetTimems();
            Last = ComF.timeGetTimems();

            do
            {
                if (Z_move_check())
                {
                    if (1.0 <= kLoadCell)
                    {
                        ToZeroPositionSearch(false);
                    }
                    break;
                }
                Last = ComF.timeGetTimems();
                xTime = Last - First;
                if (LadcellOverCheck() == true)
                {
                    AllMotorStop();
                    DefaultPositionMove();
                    test_process_stop = true;
                    return;
                }
                else if (1.0 <= kLoadCell)
                {
                    AXTServo.Axt_EStop(MOTOR_Z);
                    ToZeroPositionSearch(false);
                    break;
                }
                Application.DoEvents();
            } while (xTime < 20000); // 이초 안에 동작을 해야 한다.

            First = ComF.timeGetTimems();
            Last = ComF.timeGetTimems();

            xTime = 0;
            do
            {
                if (EStopCheck() == true)
                {
                    AllMotorStop();
                    return;
                }
                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    AllMotorStop();
                    return;
                }
                if (LadcellOverCheck() == true)
                {
                    AllMotorStop();
                    DefaultPositionMove();
                    test_process_stop = true;
                    return;
                }
                if (kLoadCell >= Convert.ToDouble(textBox57.Text))
                {

                    button79.Text = button60.Text;

                    AXTServo.Axt_EStop(MOTOR_Z);

                    Z_move_set(textBox13.Text, Convert.ToUInt16(textBox65.Text));  //for SBR 무반응 검사 복귀위치
                    ComF.timedelay(200);
                    over_time_check = 0; //모터 이동을 기다린다.
                    do
                    {
                        if (Z_move_check()) break;
                        ComF.timedelay(1);
                        over_time_check++;
                        if (EStopCheck() == true)
                        {
                            AllMotorStop();
                            return;
                        }

                        if ((test_process_stop == true) || (test_process_estop == true))
                        {
                            AllMotorStop();
                            return;
                        }

                        if (LadcellOverCheck() == true)
                        {
                            AllMotorStop();
                            DefaultPositionMove();
                            test_process_stop = true;
                            return;
                        }
                        else if (Convert.ToDouble(textBox57.Text) <= kLoadCell)
                        {
                            AXTServo.Axt_EStop(MOTOR_Z);
                            break;
                        }
                        Application.DoEvents();
                    } while ((over_time_check < Convert.ToInt16(textBox42.Text) * 90));

                    over_time_check = 0;
                    do
                    {

                        if (Z_move_check()) break;
                        ComF.timedelay(1);
                        over_time_check++;
                        if (EStopCheck() == true)
                        {
                            AllMotorStop();
                            return;

                        }
                        if ((test_process_stop == true) || (test_process_estop == true))
                        {
                            AllMotorStop();
                            return;
                        }
                        if (LadcellOverCheck() == true)
                        {
                            AllMotorStop();
                            DefaultPositionMove();
                            test_process_stop = true;
                            return;
                        }
                        else if (Convert.ToDouble(textBox57.Text) <= kLoadCell)
                        {
                            AXTServo.Axt_EStop(MOTOR_Z);
                            break;
                        }
                        Application.DoEvents();
                    } while ((over_time_check < Convert.ToInt16(textBox42.Text) * 90));

                    //button79.Text = button60.Text;

                    button80.Text = button77.Text;       //save sbr저항값

                    textBox58.Text = button77.Text;       //save sbr저항값 DB 용

                    //log_data = "Seat loading(Kq)" + button77.Text;
                    log_data = "무반응 SBR저항값(Ohm)" + button80.Text;

                    Log_SAVE(log_data);
                    Flag = true;
                    break;
                }

                if (Z_move_check()) break;
                //ComF.timedelay(1);
                //over_time_check++;
                if (EStopCheck() == true)
                {
                    AllMotorStop();
                    return;

                }
                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    AllMotorStop();
                    return;
                }
                if (LadcellOverCheck() == true)
                {
                    AllMotorStop();
                    DefaultPositionMove();
                    test_process_stop = true;
                    return;
                }
                Last = ComF.timeGetTimems();
                xTime = Last - First;
                Application.DoEvents();
            } while (xTime < (Convert.ToInt16(textBox42.Text) * 1000));
            //while((over_time_check < Convert.ToInt16(textBox42.Text) * 90));

            AXTServo.Axt_EStop(MOTOR_Z);

            if ((Convert.ToInt16(textBox42.Text) * 1000) <= xTime)
            {
                test_process_stop = true;
                return;
            }

            //ComF.timedelay(200);

            if (Flag == false) // 검출이 안 되었다면 일반 속도로보다 낮은 속도로 검사를 진행한다
            {
                int Speed;

                bool sFlag;
                double Press;


                if (15 < Convert.ToDouble(textBox57.Text))
                    Press = 15.0;
                else Press = 3;

                Speed = Convert.ToInt16(textBox65.Text);

                Z_move_set(textBox45.Text, (UInt16)(Speed / 3));      //for SBR                
                ComF.timedelay(200);
                //Z_moveing_flg = true;

                over_time_check = 0;
                Flag = false;
                sFlag = false;
                First = ComF.timeGetTimems();
                Last = ComF.timeGetTimems();

                do
                {
                    if (!Z_move_check()) break;
                    Last = ComF.timeGetTimems();
                    xTime = Last - First;
                    if (LadcellOverCheck() == true)
                    {
                        AllMotorStop();
                        DefaultPositionMove();
                        test_process_stop = true;
                        return;
                    }
                    Application.DoEvents();
                } while (xTime < 2000); // 이초 안에 동작을 해야 한다.

                sLoad = Convert.ToDouble(textBox57.Text) - 0.8;
                do
                {
                    if (sFlag == false)
                    {
                        if (LadcellOverCheck() == true)
                        {
                            AllMotorStop();
                            DefaultPositionMove();
                            test_process_stop = true;
                            return;
                        }
                        else if ((Convert.ToDouble(textBox57.Text) - kLoadCell) <= Press)
                        {
                            sFlag = true;

                            AXTServo.Axt_EStop(MOTOR_Z);
                            if (SbrOrOds)
                            {
                                s = string.Format("{0}", Convert.ToInt16(textBox45.Text));
                                Z_move_set(s, (ushort)70);      //for SBR
                            }
                            else
                            {
                                s = string.Format("{0}", Convert.ToInt16(textBox37.Text));
                                Z_move_set(s, (ushort)70);      //for ODS
                            }
                            ComF.timedelay(200);
                            //Z_moveing_flg = true;
                        }
                    }
                    if (kLoadCell >= sLoad)
                    {
                        button79.Text = button60.Text;

                        AXTServo.Axt_EStop(MOTOR_Z);

                        button80.Text = button77.Text;       //save sbr저항값

                        textBox58.Text = button77.Text;       //save sbr저항값 DB 용

                        log_data = "무반응 SBR저항값(Ohm)" + button80.Text;

                        Log_SAVE(log_data);
                        Flag = true;
                        break;
                    }
                    if (Z_move_check()) break;
                    if (EStopCheck() == true)
                    {
                        AllMotorStop();
                        return;
                    }
                    if ((test_process_stop == true) || (test_process_estop == true))
                    {
                        AllMotorStop();
                        return;
                    }
                    if (LadcellOverCheck() == true)
                    {
                        AllMotorStop();
                        DefaultPositionMove();
                        test_process_stop = true;
                        return;
                    }
                    Last = ComF.timeGetTimems();

                    xTime = Last - First;
                    Application.DoEvents();
                } while (xTime < (Convert.ToInt16(textBox42.Text) * 1000));

                AXTServo.Axt_EStop(MOTOR_Z);

                if ((Convert.ToInt16(textBox42.Text) * 1000) <= xTime)
                {
                    AllMotorStop();
                    DefaultPositionMove();
                    test_process_stop = true;
                    return;
                }
            }

            //Z_moveing_flg = false;
            Z_move_set(textBox13.Text, Convert.ToUInt16(textBox70.Text));  //for SBR 무반응 검사 복귀위치
            over_time_check = 0;
            ComF.timedelay(200);
            do
            {
                if (!Z_move_check()) // 이동을 시작하면 빠져 나간다.
                {
                    break;
                }
                ComF.timedelay(100);
                over_time_check++;
                if (EStopCheck() == true)
                {
                    AllMotorStop();
                    return;
                }
                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    AllMotorStop();
                    return;
                }
                if (LadcellOverCheck() == true)
                {
                    AllMotorStop();
                    DefaultPositionMove();
                    test_process_stop = true;
                    return;
                }
                Application.DoEvents();
            } while (over_time_check < 200);        //wait 20sec

            over_time_check = 0;

            do
            {
                if (Z_move_check())
                {
                    break;
                }
                ComF.timedelay(100);
                over_time_check++;
                if (EStopCheck() == true)
                {
                    AllMotorStop();
                    return;
                }
                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    AllMotorStop();
                    return;
                }
                if (LadcellOverCheck() == true)
                {
                    AllMotorStop();
                    DefaultPositionMove();
                    test_process_stop = true;
                    return;
                }
                Application.DoEvents();
            } while (over_time_check < 200);        //wait 20sec
#endif
        }


        private void SBR_no_reactive_Z_down_testxxxx()
        {
            //Z축 시험
#if !DEBUG_MODE
            string log_data;
            string s;
            bool Flag;
            double xTime;
            double First;
            double Last;
            double sLoad;

            int over_time_check;


            over_time_check = 0;
            First = ComF.timeGetTimems();
            Last = ComF.timeGetTimems();

            do
            {
                if (X_move_check() && Y_move_check())
                {
                    break;
                }
                if (EStopCheck() == true)
                {
                    AllMotorStop();
                    return;

                }

                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    AllMotorStop();
                    return;
                }
                if (LadcellOverCheck() == true)
                {
                    test_process_stop = true;
                    return;
                }
                Last = ComF.timeGetTimems();
                xTime = Last - First;
                Application.DoEvents();
            } while (xTime < 20000);        //wait 20sec

            Flag = false;
            ComF.timedelay(200);
            if (SbrOrOds == true)
            {
                s = textBox45.Text;
                Z_move_set(s, Convert.ToUInt16(textBox70.Text));      //for SBR                
            }
            else
            {
                s = textBox37.Text;
                Z_move_set(s, Convert.ToUInt16(textBox70.Text));      //for ODS                
            }
            ComF.timedelay(200);
            over_time_check = 0;

            //Z_moveing_flg = true;

            Flag = false;

            First = ComF.timeGetTimems();
            Last = ComF.timeGetTimems();

            do
            {
                Last = ComF.timeGetTimems();
                xTime = Last - First;
                if (Z_move_check())
                {
                    break;  //모터가 이동하면 빠져 나간다.
                }

                if (3.0 <= kLoadCell)
                {
                    AXTServo.Axt_EStop(MOTOR_Z);
                    break;
                }
                //}
                Application.DoEvents();
            } while (xTime < 20000);

            if (kLoadCell >= Convert.ToDouble(textBox12.Text))
            {
                bool mFlag;

                Z_move_set(textBox13.Text, (ushort)(Convert.ToUInt16(textBox65.Text) / 3));  //for SBR 무반응 검사 복귀위치
                ComF.timedelay(200);
                over_time_check = 0;
                mFlag = false;
                First = ComF.timeGetTimems();
                Last = ComF.timeGetTimems();
                do
                {
                    if (mFlag == false)
                    {
                        if (Z_move_check())
                        {
                            mFlag = true;
                            break;
                        }
                    }
                    else
                    {
                        if (Z_move_check())
                        {
                            break;
                        }
                    }
                    //                        plot1.Channels[0].AddXY(kLoadCell, SBRData);
                    over_time_check++;
                    if (EStopCheck() == true)
                    {
                        AllMotorStop();
                        return;
                    }
                    if ((test_process_stop == true) || (test_process_estop == true))
                    {
                        AllMotorStop();
                        return;
                    }
                    if (LadcellOverCheck() == true)
                    {
                        test_process_stop = true;
                        return;
                    }
                    else if (kLoadCell <= (Convert.ToDouble(textBox12.Text) + 2))
                    {
                        AXTServo.Axt_EStop(MOTOR_Z);
                        break;
                    }
                    Application.DoEvents();
                    Last = ComF.timeGetTimems();
                } while (xTime < 20000);
                //} while ((over_time_check < Convert.ToInt16(textBox42.Text) * 1000));


                Flag = true;
            }
            else
            {
                if (SbrOrOds == true)
                {
                    s = textBox45.Text;
                    Z_move_set(s, (ushort)(Convert.ToUInt16(textBox65.Text) / 3));      //for SBR                    
                }
                else
                {
                    s = textBox37.Text;
                    Z_move_set(s, (ushort)(Convert.ToUInt16(textBox65.Text) / 3));      //for ODS                    
                }
                ComF.timedelay(200);
                over_time_check = 0;
                First = ComF.timeGetTimems();
                Last = ComF.timeGetTimems();
                do
                {

                    if (Z_move_check()) break;

                    //                        plot1.Channels[0].AddXY(kLoadCell, SBRData);
                    over_time_check++;
                    if (EStopCheck() == true)
                    {
                        AllMotorStop();
                        return;
                    }
                    if ((test_process_stop == true) || (test_process_estop == true))
                    {
                        AllMotorStop();
                        return;
                    }
                    if (LadcellOverCheck() == true)
                    {
                        test_process_stop = true;
                        return;
                    }
                    else if ((Convert.ToDouble(textBox12.Text) + 2) <= kLoadCell)
                    {
                        AXTServo.Axt_EStop(MOTOR_Z);
                        break;
                    }
                    Last = ComF.timeGetTimems();
                    Application.DoEvents();
                } while (xTime < 20000);
                //} while ((over_time_check < Convert.ToInt16(textBox42.Text) * 1000));

                Flag = true;
            }

            AXTServo.Axt_EStop(MOTOR_Z);

            button79.Text = button60.Text;

            button80.Text = button77.Text;       //save sbr저항값

            textBox59.Text = button77.Text;       //save sbr저항값 DB용

            //log_data = "Seat loading(Kq)" + button77.Text;
            log_data = "15Kg부하 SBR저항값(Ohm)" + button80.Text;

            Log_SAVE(log_data);

            //ComF.timedelay(200);
            if ((Convert.ToInt16(textBox42.Text) * 1000) <= xTime)
            {
                test_process_stop = true;
                return;
            }

            if (Flag == false)
            {
                bool sFlag;
                double Press;

                if (15 < Convert.ToDouble(textBox12.Text))
                    Press = 15.0;
                else Press = 3;

                Z_move_set(textBox45.Text, Convert.ToUInt16(textBox65.Text));      //for SBR                
                ComF.timedelay(200);
                //Z_moveing_flg = true;
                //                ComF.timedelay(200);
                over_time_check = 0;
                Flag = false;
                sFlag = false;

                First = ComF.timeGetTimems();
                Last = ComF.timeGetTimems();

                do
                {
                    if (Z_move_check()) break; //모터가 이동을 시작하면 빠져 나간다.
                    Last = ComF.timeGetTimems();
                    xTime = Last - First;
                    if (LadcellOverCheck() == true)
                    {
                        test_process_stop = true;
                        return;
                    }
                    Application.DoEvents();
                } while (xTime < 2000);

                sLoad = Convert.ToDouble(textBox12.Text) - 0.8;
                do
                {
                    if (sFlag == false)
                    {
                        if (LadcellOverCheck() == true)
                        {
                            test_process_stop = true;
                            return;
                        }
                        else if ((Convert.ToDouble(textBox12.Text) - kLoadCell) <= Press)
                        {
                            sFlag = true;

                            AXTServo.Axt_EStop(MOTOR_Z);

                            if (SbrOrOds)
                            {
                                s = string.Format("{0}", Convert.ToInt16(textBox45.Text));
                                Z_move_set(s, (ushort)70);      //for SBR
                            }
                            else
                            {
                                s = string.Format("{0}", Convert.ToInt16(textBox37.Text));
                                Z_move_set(s, (ushort)70);      //for ODS
                            }
                            ComF.timedelay(200);
                            //Z_moveing_flg = true;
                        }
                    }
                    if (LadcellOverCheck() == true)
                    {
                        test_process_stop = true;
                        return;
                    }
                    else if (kLoadCell >= sLoad)
                    {
                        AXTServo.Axt_EStop(MOTOR_Z);

                        button79.Text = "15.6";

                        button80.Text = button77.Text;       //save sbr저항값

                        textBox58.Text = button77.Text;       //save sbr저항값 DB 용

                        log_data = "15Kg부하 SBR저항값(Ohm)" + button80.Text;

                        Log_SAVE(log_data);
                        Flag = true;
                        break;
                    }
                    if (Z_move_check()) break;
                    if (EStopCheck() == true)
                    {
                        AllMotorStop();
                        return;

                    }

                    if ((test_process_stop == true) || (test_process_estop == true))
                    {
                        AllMotorStop();
                        return;
                    }
                    Last = ComF.timeGetTimems();

                    xTime = Last - First;
                    //                    plot1.Channels[0].AddXY(kLoadCell, SBRData);
                    Application.DoEvents();
                } while (xTime < (Convert.ToInt16(textBox42.Text) * 1000));
                AXTServo.Axt_EStop(MOTOR_Z);
                if ((Convert.ToInt16(textBox42.Text) * 1000) <= xTime)
                {
                    test_process_stop = true;
                    return;
                }
            }
#endif
        }
        private void SBR_no_reactive_Z_down_test()
        {
            //Z축 시험
#if !DEBUG_MODE
            string log_data;
            string s;
            bool SetFlag;
            bool Flag;
            bool mFlag;
            double xTime;
            double First;
            double Last;
            double sLoad;

            int over_time_check;

            over_time_check = 0;
            First = ComF.timeGetTimems();
            Last = ComF.timeGetTimems();

            do
            {
                if (X_move_check() && Y_move_check())
                {
                    break;
                }
                if (EStopCheck() == true)
                {
                    AllMotorStop();
                    return;

                }

                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    AllMotorStop();
                    return;
                }
                if (LadcellOverCheck() == true)
                {
                    test_process_stop = true;
                    return;
                }
                Last = ComF.timeGetTimems();
                xTime = Last - First;
                Application.DoEvents();
            } while (xTime < 20000);        //wait 20sec

            Flag = false;
            if (SbrOrOds == true)
            {
                s = textBox45.Text;
                Z_move_set(s, Convert.ToUInt16(textBox70.Text));      //for SBR
                //                    ComF.timedelay(500);
            }
            else
            {
                s = textBox37.Text;
                Z_move_set(s, Convert.ToUInt16(textBox70.Text));      //for ODS                
            }
            ComF.timedelay(200);
            over_time_check = 0;

            //Z_moveing_flg = true;

            Flag = false;

            First = ComF.timeGetTimems();
            Last = ComF.timeGetTimems();
            SetFlag = false;
            do
            {
                if (115 <= kMotorPos[MOTOR_Z])
                {
                    if (SetFlag == false)
                    {
                        AXTServo.Axt_NStop(MOTOR_Z);

                        if (SbrOrOds == true)
                        {
                            s = textBox45.Text;
                            Z_move_set(s, 50);      //for SBR
                            //                    ComF.timedelay(500);
                        }
                        else
                        {
                            s = textBox37.Text;
                            Z_move_set(s, 50);      //for ODS                
                        }
                        ComF.timedelay(200);
                    }
                    SetFlag = true;
                }
                Last = ComF.timeGetTimems();
                xTime = Last - First;
                if (Z_move_check())
                {
                    if (3.0 <= kLoadCell)
                    {
                        ToZeroPositionSearch(false);
                    }
                    break;  //모터가 이동하면 빠져 나간다.
                }

                if (3.0 <= kLoadCell)
                {
                    AXTServo.Axt_EStop(MOTOR_Z);

                    ToZeroPositionSearch(false);
                    break;
                }
                Application.DoEvents();
            } while (xTime < 20000);

            AXTServo.Axt_EStop(MOTOR_Z);
            if (SbrOrOds)
            {
                Z_move_set(textBox45.Text, (ushort)Convert.ToUInt16(textBox65.Text));  //for SBR
            }
            else
            {
                Z_move_set(textBox37.Text, (ushort)Convert.ToUInt16(textBox65.Text));  //for ODS
            }
            mFlag = false;
            ComF.timedelay(500);
            do
            {
                if (kLoadCell >= Convert.ToDouble(textBox12.Text))
                {
                    AXTServo.Axt_EStop(MOTOR_Z);

                    Z_move_set(textBox13.Text, 2);  //for SBR 무반응 검사 복귀위치
                    ComF.timedelay(500);
                    over_time_check = 0;

                    do
                    {
                        if (Z_move_check())
                        {
                            Flag = false;
                            break;
                        }
                        over_time_check++;
                        if (EStopCheck() == true)
                        {
                            AllMotorStop();
                            return;
                        }
                        if ((test_process_stop == true) || (test_process_estop == true))
                        {
                            AllMotorStop();
                            return;
                        }
                        if (LadcellOverCheck() == true)
                        {
                            test_process_stop = true;
                            return;
                        }
                        else if (kLoadCell <= (Convert.ToDouble(textBox12.Text)))
                        {
                            AXTServo.Axt_EStop(MOTOR_Z);
                            break;
                        }
                        Application.DoEvents();
                    } while ((over_time_check < Convert.ToInt16(textBox42.Text) * 1000));

                    button79.Text = button60.Text;

                    button80.Text = button77.Text;       //save sbr저항값

                    textBox59.Text = button77.Text;       //save sbr저항값 DB용

                    log_data = textBox12.Text + "Kg부하 SBR저항값(Ohm)" + button80.Text;

                    Log_SAVE(log_data);
                    Flag = true;
                    break;
                }
                if (mFlag == true)
                {
                    if (Z_move_check())
                    {
                        Flag = false;
                        break;
                    }
                }
                else
                {
                    if (!Z_move_check())
                    {
                        mFlag = true;
                    }
                }
                if (EStopCheck() == true)
                {
                    AllMotorStop();
                    return;
                }
                if ((test_process_stop == true) || (test_process_estop == true))
                {
                    AllMotorStop();
                    return;
                }
                if (LadcellOverCheck() == true)
                {
                    test_process_stop = true;
                    return;
                }
                Last = ComF.timeGetTimems();

                xTime = Last - First;
                Application.DoEvents();
            } while (xTime < (Convert.ToInt16(textBox42.Text) * 1000));

            AXTServo.Axt_EStop(MOTOR_Z);

            if ((Convert.ToInt16(textBox42.Text) * 1000) <= xTime)
            {
                test_process_stop = true;
                return;
            }

            if (Flag == false)
            {
                bool sFlag;
                double Press;

                if (15 < Convert.ToDouble(textBox12.Text))
                    Press = 15.0;
                else Press = 3;

                Z_move_set(textBox45.Text, Convert.ToUInt16(textBox65.Text));      //for SBR                
                //              ComF.timedelay(200);
                //Z_moveing_flg = true;
                over_time_check = 0;
                Flag = false;
                sFlag = false;

                First = ComF.timeGetTimems();
                Last = ComF.timeGetTimems();

                do
                {
                    if (!Z_move_check()) break; //모터가 이동을 시작하면 빠져 나간다.
                    Last = ComF.timeGetTimems();
                    xTime = Last - First;
                    if (LadcellOverCheck() == true)
                    {
                        test_process_stop = true;
                        return;
                    }
                    Application.DoEvents();
                } while (xTime < 2000);

                sLoad = Convert.ToDouble(textBox12.Text) - 0.8;
                do
                {
                    if (sFlag == false)
                    {
                        if (LadcellOverCheck() == true)
                        {
                            test_process_stop = true;
                            return;
                        }
                        else if ((Convert.ToDouble(textBox12.Text) - kLoadCell) <= Press)
                        {
                            sFlag = true;

                            AXTServo.Axt_EStop(MOTOR_Z);

                            if (SbrOrOds)
                            {
                                s = string.Format("{0}", Convert.ToInt16(textBox45.Text));
                                Z_move_set(s, (ushort)70);      //for SBR
                            }
                            else
                            {
                                s = string.Format("{0}", Convert.ToInt16(textBox37.Text));
                                Z_move_set(s, (ushort)70);      //for ODS
                            }
                            ComF.timedelay(200);
                            //Z_moveing_flg = true;
                        }
                    }
                    if (LadcellOverCheck() == true)
                    {
                        test_process_stop = true;
                        return;
                    }
                    else if (kLoadCell >= sLoad)
                    {
                        AXTServo.Axt_EStop(MOTOR_Z);

                        button79.Text = "15.6";

                        button80.Text = button77.Text;       //save sbr저항값

                        textBox58.Text = button77.Text;       //save sbr저항값 DB 용

                        log_data = "15Kg부하 SBR저항값(Ohm)" + button80.Text;

                        Log_SAVE(log_data);
                        Flag = true;
                        break;
                    }
                    if (Z_move_check()) break;

                    if (EStopCheck() == true)
                    {
                        AllMotorStop();
                        return;
                    }

                    if ((test_process_stop == true) || (test_process_estop == true))
                    {
                        AllMotorStop();
                        return;
                    }
                    Last = ComF.timeGetTimems();

                    xTime = Last - First;
                    //                    plot1.Channels[0].AddXY(kLoadCell, SBRData);
                    Application.DoEvents();
                } while (xTime < (Convert.ToInt16(textBox42.Text) * 1000));
                //while ((over_time_check < Convert.ToInt16(textBox42.Text) * 90));

                AXTServo.Axt_EStop(MOTOR_Z);

                if ((Convert.ToInt16(textBox42.Text) * 1000) <= xTime)
                {
                    test_process_stop = true;
                    return;
                }
            }
#endif
        }

        private void button62_Click(object sender, EventArgs e)
        {
            //Z축 시험
#if !DEBUG_MODE
            Z_down_test();
#endif
        }

        private void button31_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void X_moter_stop()
        {
#if !DEBUG_MODE
            AXTServo.Axt_NStop(MOTOR_X);
#endif
            return;
        }

        private void Y_moter_stop()
        {
#if !DEBUG_MODE
            AXTServo.Axt_NStop(MOTOR_Y);
#endif
            return;
        }

        private void Z_moter_stop()
        {
#if !DEBUG_MODE
            AXTServo.Axt_NStop(MOTOR_Z);
#endif
            button63.BackColor = Color.Red;
            return;
        }

        private void button63_Click(object sender, EventArgs e)
        {
            if (button63.BackColor == Color.Red)
            {
                button63.BackColor = SystemColors.Control;
            }
            else
            {
#if !DEBUG_MODE
                X_moter_stop();
                Y_moter_stop();
                Z_moter_stop();
#endif
                button63.BackColor = Color.Red;
            }
        }

        private void button38_Click(object sender, EventArgs e)
        {
            send_alarm_clr();
            return;
        }

        private void button64_Click(object sender, EventArgs e)
        {
            if (button64.BackColor == Color.Lime)
            {
                button64.BackColor = SystemColors.Control;
                IOPort.outportb((short)DO_OUT.ODS_POWER, false);
            }
            else
            {
                button64.BackColor = Color.Lime;
                IOPort.outportb((short)DO_OUT.ODS_POWER, true);

            }
        }

        private void button70_Click(object sender, EventArgs e)
        {
            Diag_start();
        }

        private void button37_Click(object sender, EventArgs e)
        {

        }

        private void button71_Click(object sender, EventArgs e)
        {
            Z_move_set(textBox25.Text, Convert.ToUInt16(textBox84.Text));
            return;
        }

        private void button72_Click(object sender, EventArgs e)
        {
            //X축 이동
            X_move_set(textBox27.Text, Convert.ToUInt16(textBox84.Text));
            return;
        }

        private void button73_Click(object sender, EventArgs e)
        {
            //Y축 이동
            Y_move_set(textBox28.Text, Convert.ToUInt16(textBox84.Text));
            return;
        }

        private void button55_Click(object sender, EventArgs e)
        {
            //heater type
            if (led8.Value.AsBoolean == true)
            {
                led8.Value.AsBoolean = false;
            }
            else
            {
                led8.Value.AsBoolean = false;
                led7.Value.AsBoolean = false;
            }
        }

        private void button57_Click(object sender, EventArgs e)
        {
            //Non heater
            if (led7.Value.AsBoolean == true)
            {
                led7.Value.AsBoolean = false;
            }
            else
            {
                led7.Value.AsBoolean = true;
                led8.Value.AsBoolean = false;
            }
        }

        private void button76_Click(object sender, EventArgs e)
        {
            //ECU RESET
            ECU_RESET();
        }

        private void Server_CloseEvent(object sender, EventArgs e)
        {
            Server.Close();
            Server.LocalPort = Convert.ToInt32(textBox52.Text);
            Server.Listen();

            textBox55.Text = "IP";
            textBox56.Text = "PORT";
            button83.BackColor = SystemColors.Control;

        }

        private void Server_ConnectEvent(object sender, EventArgs e)
        {

        }



        private void Server_DataArrival(object sender, AxMSWinsockLib.DMSWinsockControlEvents_DataArrivalEvent e)
        {

            object dat = (object)data1;
            Server.GetData(ref dat);
            data1 = (String)dat;

            Log_SAVE(data1);     //save rx data logfile

            St_Popdata.Text = data1;
            txt_barcode2.Text = txt_barcode1.Text;
            txt_barcode1.Clear();
            CheckPop();
            SaveToLastBarcode();
            //spec_read();

        }

        private void Server_Error(object sender, AxMSWinsockLib.DMSWinsockControlEvents_ErrorEvent e)
        {
            Server.Close();
            Server.LocalPort = Convert.ToInt32(textBox52.Text);
            Server.Listen();
            textBox55.Text = "IP";
            textBox56.Text = "PORT";
            button83.BackColor = SystemColors.Control;
        }


        //string data_final;
        string send_spec;
        //string send_measue;

        public void Data_Send()
        {
            try
            {

                string DriveType = "";

                if (led4.Value.AsBoolean == true)
                    DriveType = "LHD";
                else if (led3.Value.AsBoolean == true)
                    DriveType = "RHD";

                if (isConnect)
                {
                    //// PoP로 데이터 전송 할 때 여기다 쓰자../
                    //data_final = "";
                    send_spec = "";
                    //send_measue = "";
                    data_sub_routin();


                    if (0 < send_data_bar.Text.Length)
                    {
                        if (send_data_bar.Text.Substring(0, 1) == "")
                        {
                            Client.SendData(send_data_bar.Text + ";" + DriveType + ";" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ";"
                                + linenum + ";" + linecom + ";" + comitAdd.ToString() + ";" + retest + ";" + send_spec + "");
                        }
                        else
                        {
                            Client.SendData("" + send_data_bar.Text + ";" + DriveType + ";" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ";"
                                    + linenum + ";" + linecom + ";" + comitAdd.ToString() + ";" + retest + ";" + send_spec + "");
                        }
                    }
                    else
                    {
                        Client.SendData("" + send_data_bar.Text + ";" + DriveType + ";" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ";"
                                + linenum + ";" + linecom + ";" + comitAdd.ToString() + ";" + retest + ";" + send_spec + "");
                    }
                    Log_SAVE("" + send_data_bar.Text + ";" + DriveType + ";" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ";"
                        + linenum + ";" + linecom + ";" + comitAdd.ToString() + ";" + retest + ";" + send_spec + "");
                    send_data_comit.Text = string.Format("{0:d4}", comitAdd);
                    comitAdd++;
                    if (9999 < comitAdd) comitAdd = 0;
                }
                else
                {
                    string s;

                    //MessageBox.Show("접속 안되있다~");
                    send_data_comit.Text = "";
                    SendFailFlag = true;

                    //// PoP로 데이터 전송 할 때 여기다 쓰자../
                    //data_final = "";
                    send_spec = "";
                    //send_measue = "";
                    data_sub_routin();

                    s = "";



                    //s = send_data_bar.Text + ";" + DriveType + ";" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ";"
                    if (send_data_bar.Text.Substring(0, 1) == "")
                    {
                        s = send_data_bar.Text + ";" + DriveType + ";" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ";"
                            + linenum + ";" + linecom + ";" + comitAdd.ToString() + ";" + retest + ";" + send_spec + "";
                    }
                    else
                    {
                        s = "" + send_data_bar.Text + ";" + DriveType + ";" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ";"
                            + linenum + ";" + linecom + ";" + comitAdd.ToString() + ";" + retest + ";" + send_spec + "";
                    }

                    SendList.Add(s);

                    Log_SAVE(send_data_bar.Text + ";" + DriveType + ";" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ";"
                        + linenum + ";" + linecom + ";" + comitAdd.ToString() + ";" + retest + ";" + send_spec + "");
                    //send_data_comit.Text = string.Format("{0:d4}", comitAdd);
                    //comitAdd++;                    
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

        }
        public void data_sub_routin()
        {
            send_spec += button69.Text + ";";       //1 Vehile ID
            send_spec += "0" + ";";                 //2 None Heater DTC
            send_spec += "0" + ";";                 //3 Heater DTC
            send_spec += textBox78.Text + ";";      //4 Img Noen Heter Unload Max
            send_spec += textBox76.Text + ";";      //5 Img Noen Heter Unload Min
            send_spec += textBox8.Text + ";";       //6 Img Heter Unload Max
            send_spec += textBox62.Text + ";";      //7 Img Heter Unload Min

            send_spec += textBox80.Text + ";";      //8 Img Noen Heter load Max
            send_spec += textBox74.Text + ";";      //9 Img Noen Heter load Min
            send_spec += textBox32.Text + ";";      //10 Img Heter load Max
            send_spec += textBox64.Text + ";";      //11 Img Heter load Min
            send_spec += button90.Text + ";";       //12 Cal Checksum None Heater
            send_spec += button43.Text + ";";       //13 Cal Checksub Heater
            send_spec += button66.Text + ";";       //14 Rom ID
            send_spec += button67.Text + ";";       //15 OPA Number
            send_spec += "0x55" + ";";              //16 LOCK BYTE

            float SBRMax;

            if (float.TryParse(button78.Text, out SBRMax) == false) SBRMax = 0;

            send_spec += "9999;";                    //17 SBR 15kg Max
            send_spec += button78.Text + ";";       //18 SBR 15kg Min
            send_spec += string.Format("{0}", (int)(SBRMax - 1)) + ";";                   //20 SBR 30kg Max
            send_spec += "0;";                       //19 SBR 30kg Min

            send_spec += ";";                       //21 spare
            send_spec += ";";                       //22 spare
            send_spec += ";";                       //23 spare
            send_spec += ";";                       //24 spare
            send_spec += ";";                       //25 spare


            if (SbrOrOds == false)
            {
                //ods                
                send_spec += button40.Text + ";";      //26 Vehicle ID
                send_spec += "0" + ";";                //27 None Heater DTC
                send_spec += "0" + ";";                //28 Heater DTC

                if (led8.Value.AsBoolean == true) // heater
                {
                    send_spec += ";";                  //29 Img unload none heater
                    send_spec += textBox68.Text + ";"; //30 Img unload heater 

                    send_spec += ";";                  //31 Img load none heater
                    send_spec += textBox67.Text + ";"; //32 Img load heater 

                    send_spec += ";";                  //33 Cal CheckSum None Heater
                    send_spec += button50.Text + ";";  //34 Cal CheckSub Heater
                }
                else if (led7.Value.AsBoolean == true) // None heater
                {
                    send_spec += textBox68.Text + ";"; //29 Img unload none heater
                    send_spec += ";";                  //30 Img unload heater 

                    send_spec += textBox67.Text + ";"; //31 Img load none heater
                    send_spec += ";";                  //32 Img load heater 

                    send_spec += button50.Text + ";";  //33 Cal CheckSum None Heater
                    send_spec += ";";                  //34 Cal CheckSub Heater
                }

                send_spec += button44.Text + ";";      //35 ROM ID
                send_spec += button45.Text + ";";      //36 OPA
                send_spec += LockByteData + ";";       //37 Lock Byte
                send_spec += ";";                      //38 SBR 15k
                send_spec += ";";                      //39 SBR 30k
            }
            else
            {
                //sbr
                send_spec += ";";                       //26 Vehicle ID
                send_spec += ";";                       //27 None Heater DTC
                send_spec += ";";                       //28 Heater DTC

                send_spec += ";";                       //29 Img unload none heater
                send_spec += ";";                       //30 Img unload heater 

                send_spec += ";";                       //31 Img load none heater
                send_spec += ";";                       //32 Img load heater 

                send_spec += ";";                       //33 Cal CheckSum None Heater
                send_spec += ";";                       //34 Cal CheckSub Heater

                send_spec += ";";                       //35 ROM ID
                send_spec += ";";                       //36 OPA
                send_spec += ";";                       //37 Lock Byte
                send_spec += NoneLoadData + ";";        //38 SBR 15k
                send_spec += LoadData + ";";            //39 SBR 30k
            }
            if (button51.Text == "O.K") //40
            {
                send_spec += "1;";
            }
            else
            {
                send_spec += "9;";
            }
        }


        private bool PopToSbrOdsSelect = false;

        public void select_popdata()        //검사항목 수신 및 check
        {
            bool Sbr = false;
            bool LH = false;
            bool LHD = false;

            PopToSbrOdsSelect = false;
            if (imageButton8.ButtonColor != Color.Lime) return;

            //// ODS
            if (popdata1[0].ToString() == "0")
            {
                led1.Value.AsBoolean = false;
            }
            else if (popdata1[0].ToString() == "1")
            {
                led1.Value.AsBoolean = true;
                PopToSbrOdsSelect = true;
                SbrOrOds = false;
            }
            else
            {

            }
            // SBR 
            if (popdata1[1].ToString() == "0")
            {
                led2.Value.AsBoolean = false;
                SbrOrOds = false;
                led15.Value.AsBoolean = false;
                led14.Value.AsBoolean = false;
            }
            else if (popdata1[1].ToString() == "1")
            {
                led2.Value.AsBoolean = true;
                PopToSbrOdsSelect = true;
                SbrOrOds = true;
                Sbr = true;
            }
            else
            {

            }
            // 스페어
            if (popdata1[2].ToString() == "0")
            {

            }
            else if (popdata1[2].ToString() == "1")
            {
                //btn_3.BackColor = Color.Lime;

            }
            else
            {

            }
            // 스페어
            if (popdata1[3].ToString() == "0")
            {

            }
            else if (popdata1[3].ToString() == "1")
            {
                //btn_4.BackColor = Color.Lime;
            }
            else
            {

            }
            // 스페어
            if (popdata1[4].ToString() == "0")
            {

            }
            else if (popdata1[4].ToString() == "1")
            {

                //btn_1.BackColor = Color.Lime;
            }
            else
            {

            }
            // 통풍히터
            if (popdata1[5].ToString() == "0")
            {
                VentHeater = false;
                led15.Value.AsBoolean = false;
                led14.Value.AsBoolean = true;
            }
            else if (popdata1[5].ToString() == "1")
            {
                VentHeater = true;
                led15.Value.AsBoolean = true;
                led14.Value.AsBoolean = false;
            }
            else
            {
                VentHeater = false;
                led15.Value.AsBoolean = false;
                led14.Value.AsBoolean = true;
            }

            // ODS 히터 논히터
            if ((led1.Value.AsBoolean == true) || (led2.Value.AsBoolean == true))
            {
                if (popdata1[6].ToString() == "1")
                {
                    led7.Value.AsBoolean = false;
                    led8.Value.AsBoolean = true;
                }
                else if (popdata1[6].ToString() == "0")
                {
                    led8.Value.AsBoolean = false;
                    led7.Value.AsBoolean = true;
                }
            }
            else
            {
                led7.Value.AsBoolean = false;
                led8.Value.AsBoolean = false;
            }


            // LH/RH
            if ((led1.Value.AsBoolean == true) || (led2.Value.AsBoolean == true))
            {
                if (popdata1[7].ToString() == "0")
                {
                    led6.Value.AsBoolean = true;                 //select LH
                    led5.Value.AsBoolean = false;
                    LH = true;
                }
                else
                {
                    led5.Value.AsBoolean = true;                 //select RH
                    led6.Value.AsBoolean = false;
                }
            }
            else
            {
                led5.Value.AsBoolean = false;
                led6.Value.AsBoolean = false;
            }


            // LHD/RHD 선택
            if (popdata1[8].ToString() == "0")
            {
                LHD = true;
                led4.Value.AsBoolean = true;                 //select LHD
                led3.Value.AsBoolean = false;

                /*
                if (led1.Value.AsBoolean == true)
                {
                    comboBox1.Text = "DE_SBR";
                }
                else if (led2.Value.AsBoolean == true)
                {
                    comboBox1.Text = "DE_ODS";
                }
                */
            }
            else if (popdata1[8].ToString() == "1")
            {
                led3.Value.AsBoolean = true;                 //select RHD
                led4.Value.AsBoolean = false;
                /*
                if(led1.Value.AsBoolean == true)
                {
                    comboBox1.Text = "DE_SBR_RHD";
                }
                else if (led2.Value.AsBoolean == true)
                {
                    comboBox1.Text = "DE_ODS_RHD";
                }
                */
            }

            // 차종 0= DE 선택
            if (popdata1[9].ToString() == "0")
            {
                //comboBox1.SelectedIndex = 0;
            }
            else if (popdata1[9].ToString() == "1")
            {
                //comboBox1.SelectedIndex = 1;
            }

            //// ODS
            if (popdata1[0].ToString() == "1")
            {
                SbrOrOds = false;
            }

            // SBR 
            if (popdata1[1].ToString() == "1")
            {
                SbrOrOds = true;

            }
            if ((led1.Value.AsBoolean == true) || (led2.Value.AsBoolean == true))
            {
                string ModelType;
                int i;
                bool Flag;

                ModelType = ModelText;

                if (Sbr == true)
                    ModelType = ModelType + " SBR";
                else ModelType = ModelType + " ODS";

                if (LHD == true)
                    ModelType = ModelType + " LHD";
                else ModelType = ModelType + " RHD";

                if (LH == true)
                    ModelType = ModelType + "-LH";
                else ModelType = ModelType + "-RH";

                Flag = false;
                for (i = 0; i < comboBox1.Items.Count; i++)
                {
                    if (comboBox1.Items[i].ToString() == ModelType)
                    {
                        if (comboBox1.Text != comboBox1.Items[i].ToString())
                        {
                            comboBox1.SelectedIndex = i;
                            Flag = true;
                        }
                        break;
                    }
                }
#if !DEBUG_MODE
                if (Sbr == true)
                {
                    ConnectingMsgDisplay(2, true);
                }
                else
                {
                    if (led8.Value.AsBoolean == true)
                    {
                        ConnectingMsgDisplay(0, true);
                    }
                    else
                    {
                        ConnectingMsgDisplay(1, true);
                    }
                }
#endif
                if (Flag == false) DisplaySpec();
            }
            return;
        }


        private bool isConnect = false;

        private void Client_CloseEvent(object sender, EventArgs e)
        {
            isConnect = false;
            button82.BackColor = SystemColors.Control;
        }

        private void Client_ConnectEvent(object sender, EventArgs e)
        {
            isConnect = true;
            Client.SendData("OK");
            button82.BackColor = Color.Orange;
        }

        private void Client_Error(object sender, AxMSWinsockLib.DMSWinsockControlEvents_ErrorEvent e)
        {
            isConnect = false;
            button82.BackColor = SystemColors.Control;
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (!isConnect)
            {
                Client.Close();
                Client.Connect(textBox53.Text, Convert.ToInt32(textBox54.Text));
            }
            else
            {
                if (SendFailFlag == true)
                {
                    int i;
                    string s;

                    timer3.Enabled = false;

                    for (i = 0; i < SendList.Count; i++)
                    {
                        s = SendList[i].ToString().Trim();
                        Client.SendData(s);
                    }
                    SendList.Clear();

                    DataSendCheck();
                    SendFailFlag = false;
                    timer3.Enabled = true;
                }
            }
            /*
                        string path = "D:\\DB\\" + DateTime.Now.Year + string.Format("{0:d2}", DateTime.Now.Month) + "월";
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                            DB_save_data_NG_table_create();
                            DB_save_data_OK_table_create();
                        }
            */
#if !DEBUG_MODE
            string path = "D:\\DB";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
#endif
        }

        private void Server_ConnectionRequest(object sender, AxMSWinsockLib.DMSWinsockControlEvents_ConnectionRequestEvent e)
        {
            Server.Close();
            Server.Accept(e.requestID);
            Server.SendData("OK");
            textBox55.Text = Server.RemoteHostIP;
            textBox56.Text = Server.LocalPort.ToString();
            button83.BackColor = Color.Orange;
        }

        private void OK_Final()
        {
            //comitAdd = Convert.ToInt32(send_data_comit.Text);
            //send_data_comit.Text = string.Format("{0:d4}", comitAdd++);
            //send_data_comit.Text = string.Format("{0:d4}", comitAdd);
            DB_save_data_OK();
            //Data_Send();

        }
        private void NG_Final()
        {
            //comitAdd = Convert.ToInt32(send_data_comit.Text);
            //send_data_comit.Text = string.Format("{0:d4}", comitAdd++);
            //send_data_comit.Text = string.Format("{0:d4}", comitAdd);
            DB_save_data_NG();
            //Data_Send();
        }

        /// <summary>
        ///  DB 테이블 구성
        /// </summary>
        /// 

        public const string ok_ng_table = "모델," + "Number," + "DataTime," + "검사시간," + "바코드," + "라인번호," + "장비번호," + "재검사 횟수," +
                   "Vehicle ID(설정)," + "Vehicle ID(측정)," +
                   "Img Capacitance unloading(설정)," + "Img Capacitance unloading(측정)," + "real Capacitance unloading(설정)," + "real Capacitance unloading(측정)," +
                   "Img Capacitance Loading(설정)," + "Img Capacitance Loading(측정)," + "real Capacitance Loading(설정)," + "real Capacitance Loading(측정)," +
                   "Cal checksum(설정)," + "Cal checksum(측정)," + "ROM ID(설정)," + "ROM ID(측정)," + "OPR R3 number(측정)," +
                   "Algoparameter selection값," + "EOL byte(측정)," + "Lock byte(측정)," + "SBR설정값A4," + "SBR측정값A4," + "SBR설정값1," + "SBR측정값1," + "SBR설정값2," + "SBR측정값2," + "최종판정,";

        public void DB_save_data_OK_table_create()
        {
            /*
            try
            {
                string path = "D:\\DB\\" + DateTime.Now.Year + string.Format("{0:d2}", DateTime.Now.Month) + "월\\" + "OK_Data.csv";
                FileStream file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);

                StreamWriter sw = new StreamWriter(file, System.Text.Encoding.Default);

                sw.WriteLine(ok_ng_table);
                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);                
            }
            */
        }
        public void DB_save_data_NG_table_create()
        {
            /*
            try
            {
                string path = "D:\\DB\\" + DateTime.Now.Year + string.Format("{0:d2}", DateTime.Now.Month) + "월\\" + "NG_Data.csv";
                FileStream file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);

                StreamWriter sw = new StreamWriter(file, System.Text.Encoding.Default);

                sw.WriteLine(ok_ng_table);
                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
            */
        }

        public void DB_save_data_OK()
        {
            try
            {
                /*
                string path = "D:\\DB\\" + DateTime.Now.Year + string.Format("{0:d2}", DateTime.Now.Month) + "월\\" + "OK_Data.csv";
                FileStream file = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);

                StreamWriter sw = new StreamWriter(file, System.Text.Encoding.Default);
                sw.Write(comboBox1.Text + ","); /// 모델명//
                sw.Write(send_data_comit.Text + ",");
                sw.Write(DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + ",");
                sw.Write(DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ",");
                sw.Write(txt_barcode1.Text + ",");
                sw.Write(linenum + "," + linecom + ",");
                sw.Write(retest + ",");

                sw.Write(textBox8.Text + ",");    //Img Capacitance unloading(설정)
                sw.Write(textBox68.Text + ",");   //Img Capacitance unloading(측정)
                sw.Write(textBox11.Text + ",");   //real Capacitance unloading(설정)
                sw.Write(textBox69.Text + ",");   //real Capacitance unloading(측정)
                sw.Write(textBox32.Text + ",");   //Img Capacitance Loading(설정)
                sw.Write(textBox67.Text + ",");   //Img Capacitance Loading(측정)
                sw.Write(textBox31.Text + ",");   //real Capacitance Loading(설정)
                sw.Write(textBox66.Text + ",");   //real Capacitance Loading(측정)

                if (led8.Indicator.ColorActive == Color.Lime)
                {
                    sw.Write(button43.Text + ",");   //Cal checksum(설정) Heater
                }
                else if (led7.Value.AsBoolean == true)
                {
                    sw.Write(button90.Text + ",");   //Cal checksum(설정)Non-heater
                }

                sw.Write(button50.Text + ",");   //Cal checksum(측정) heater

                sw.Write(button66.Text + ",");   //ROM ID(설정)
                sw.Write(button44.Text + ",");   //ROM ID(측정)

                //sw.Write(button67.Text + ",");   //OPR R3 number(설정)
                sw.Write(button45.Text + ",");   //OPR R3 number(측정)

                sw.Write(textBox30.Text + ",");  //Algoparameter selection값

                sw.Write(button68.Text + ",");   //EOL byte(측정)

                sw.Write(button74.Text + ",");   //Lock byte(측정)

                sw.Write(button78.Text + ",");   //SBR설정값A4 
                sw.Write(textBox58.Text + ",");   //SBR측정값A4

                sw.Write(button78.Text + ",");   //SBR설정값1
                sw.Write(textBox59.Text + ",");   //SBR측정값1

                sw.Write(button78.Text + ",");   //SBR설정값2 
                sw.Write(button80.Text + ",");   //SBR측정값2

                sw.Write(button51.Text + ",");   //최종판정

                sw.WriteLine();

                sw.Flush();
                sw.Close();
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public void DB_save_data_NG()
        {
            try
            {
                /*
                string path = "D:\\DB\\" + DateTime.Now.Year + string.Format("{0:d2}", DateTime.Now.Month) + "월\\" + "NG_Data.csv";
                FileStream file = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);

                StreamWriter sw = new StreamWriter(file, System.Text.Encoding.Default);
                sw.Write(comboBox1.Text + ","); /// 모델명//
                sw.Write(send_data_comit.Text + ",");
                sw.Write(DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + ",");
                sw.Write(DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ",");
                sw.Write(txt_barcode1.Text + ",");
                sw.Write(linenum + "," + linecom + ",");
                sw.Write(retest + ",");


                sw.Write(button69.Text+",");
                sw.Write(button40.Text + ",");

                sw.Write(textBox8.Text + ",");    //Img Capacitance unloading(설정)
                sw.Write(textBox68.Text + ",");   //Img Capacitance unloading(측정)
                sw.Write(textBox11.Text + ",");   //real Capacitance unloading(설정)
                sw.Write(textBox69.Text + ",");   //real Capacitance unloading(측정)
                sw.Write(textBox32.Text + ",");   //Img Capacitance Loading(설정)
                sw.Write(textBox67.Text + ",");   //Img Capacitance Loading(측정)
                sw.Write(textBox31.Text + ",");   //real Capacitance Loading(설정)
                sw.Write(textBox66.Text + ",");   //real Capacitance Loading(측정)

                sw.Write(button43.Text + ",");   //Cal checksum(설정)
                if (led8.Indicator.ColorActive == Color.Lime)
                {
                    sw.Write(button43.Text + ",");   //Cal checksum(설정) Heater
                }
                else if (led7.Value.AsBoolean == true)
                {
                    sw.Write(button90.Text + ",");   //Cal checksum(설정)Non-heater
                }

                sw.Write(button66.Text + ",");   //ROM ID(설정)
                sw.Write(button44.Text + ",");   //ROM ID(측정)

                //sw.Write(button67.Text + ",");   //OPR R3 number(설정)
                sw.Write(button45.Text + ",");   //OPR R3 number(측정)

                sw.Write(textBox30.Text + ",");  //Algoparameter selection값

                sw.Write(button68.Text + ",");   //EOL byte(측정)

                sw.Write(button74.Text + ",");   //Lock byte(측정)

                sw.Write(button78.Text + ",");   //SBR설정값A4 
                sw.Write(textBox58.Text + ",");   //SBR측정값A4

                sw.Write(button78.Text + ",");   //SBR설정값1
                sw.Write(textBox59.Text + ",");   //SBR측정값1

                sw.Write(button78.Text + ",");   //SBR설정값2 
                sw.Write(button80.Text + ",");   //SBR측정값2
                
                sw.Write(button51.Text + ",");   //최종판정

                sw.WriteLine();
                sw.Flush();
                sw.Close();
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void turn()
        {
//#if !YB_OR_NEWDE
//            bool Flag;

//            if (button98.BackColor == Color.Lime)
//            {
//                //turn
//                //button94.BackColor = SystemColors.Control;
//                //button92.BackColor = Color.Lime;
//                Flag = false;                
//                IOPort.outportb((short)DO_OUT.UNLOCK_SOL, true);
//                int wait_ct = 0;
//                do
//                {
//                    ComF.timedelay(10);
//                    wait_ct++;
//                    if (button95.BackColor == Color.Lime)
//                    {
//                        Flag = true;
//                        break;
//                    }
//                    Application.DoEvents();
//                } while (wait_ct < 200); //2sec

//                IOPort.outportb((short)DO_OUT.TURN_SOL, true);
//                wait_ct = 0;
//                do
//                {
//                    ComF.timedelay(10);
//                    wait_ct++;
//                    if (button97.BackColor == Color.Lime)
//                    {
//                        break;
//                    }
//                    Application.DoEvents();
//                } while (wait_ct < 500); //2sec
//                IOPort.outportb((short)DO_OUT.UNLOCK_SOL, false);
//                IOPort.outportb((short)DO_OUT.TURN_SOL, false);
//                if (Flag == false) test_process_stop = true;
//            }
//#endif
        }



        private void button92_Click(object sender, EventArgs e)
        {
            //turn
            turn();
        }

        private void button91_MouseDown(object sender, MouseEventArgs e)
        {
            //unlock
            button91.BackColor = Color.Lime;
//#if !YB_OR_NEWDE
//            IOPort.outportb((short)DO_OUT.UNLOCK_SOL, true);
//#endif
        }

        private void button91_MouseUp(object sender, MouseEventArgs e)
        {
            //lock
            button9.BackColor = SystemColors.Control;
//#if !YB_OR_NEWDE
//            IOPort.outportb((short)DO_OUT.UNLOCK_SOL, false);
//#endif
        }


        private void return_pp()
        {
//#if !YB_OR_NEWDE
//            bool Flag;
//            if (button97.BackColor == Color.Lime)
//            {
//                //return
//                //button92.BackColor = SystemColors.Control;
//                //button94.BackColor = Color.Lime;

//                Flag = false;
//                IOPort.outportb((short)DO_OUT.UNLOCK_SOL, true);
//                int wait_ct = 0;
//                do
//                {
//                    ComF.timedelay(10);
//                    wait_ct++;
//                    if (button95.BackColor == Color.Lime)
//                    {
//                        Flag = true;
//                        break;
//                    }
//                    Application.DoEvents();
//                } while (wait_ct < 200); //2sec

//                IOPort.outportb((short)DO_OUT.RETRUN_SOL, true);
//                wait_ct = 0;
//                do
//                {
//                    ComF.timedelay(10);
//                    wait_ct++;
//                    if (button98.BackColor == Color.Lime)
//                    {
//                        break;
//                    }
//                    Application.DoEvents();
//                } while (wait_ct < 400); //2sec
//                IOPort.outportb((short)DO_OUT.UNLOCK_SOL, false);
//                IOPort.outportb((short)DO_OUT.RETRUN_SOL, false);

//                if (Flag == false)
//                {
//                    test_process_stop = true;
//                }
//            }
//#endif
        }

        private void return_pp2()
        {
            if (button97.BackColor == Color.Lime)
            {
                //return
                //button92.BackColor = SystemColors.Control;
                //button94.BackColor = Color.Lime;
//#if !YB_OR_NEWDE
//                IOPort.outportb((short)DO_OUT.UNLOCK_SOL, true);                
//#endif
            }
        }

        private void button94_Click(object sender, EventArgs e)
        {
            //return
            return_pp();
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

            textBox82.Text = Ods.Read_SET(comboBox1.Text, "TEST_READY_POS");
            return;
        }

        private void ServoMotorOn()
        {
            button39.BackColor = Color.Lime;
#if !DEBUG_MODE
            //AXTServo.Axt_ServoOnOff(MOTOR_X, true);
            //AXTServo.Axt_ServoOnOff(MOTOR_Y, true);
            //AXTServo.Axt_ServoOnOff(MOTOR_Z, true);
            X_servo_on();
            Y_servo_on();
            Z_servo_on();

            //AXTServo.AxtOriginSpeedSet(0, 20);
            //AXTServo.AxtOriginSpeedSet(1, 20);
            AXTServo.AxtOriginSpeedSet(2, 20);
#endif
            return;
        }

        private void SerovoAllOrgMove1()
        {
            //Z 원점 
            //bool Flag;

            if (OrgZSettingFlag == true) return;

            ProgramStartFlagToMotor = false;
#if  !DEBUG_MODE
            AXTServo.OriginMove(MOTOR_Z);
#endif
            IOPort.outportb((short)DO_OUT.LAMP_RED, false);
            IOPort.outportb((short)DO_OUT.LAMP_GREEN, false);
            IOPort.outportb((short)DO_OUT.LAMP_YELLOW, false);
            Z_PosToDefailtOutToPLC(false);
            this.Controls.Add(this.panel5);
            panel5.BringToFront();
            panel5.Location = new Point((Screen.PrimaryScreen.Bounds.Width / 2) - (panel5.Width / 2), (Screen.PrimaryScreen.Bounds.Height / 2) - panel5.Height);
            panel5.Visible = true;
            label126.Text = "Z축 원점 복귀중";

            imageButton6.Enabled = false;
            btn_STOP.Enabled = false;

            OrgFirst = (long)ComF.timeGetTimems();
            OrgLast = (long)ComF.timeGetTimems();
            ComF.timedelay(200);
            //            MotorConfigSetFlag = false;
            return;
        }

        private void SerovoAllOrgMove3()
        {
            //Z 원점 
            //bool Flag;            
            //            MotorConfigSetFlag = true;
#if !DEBUG_MODE
            AXTServo.OriginMove(MOTOR_Z);
#endif
            //MotorConfigSetFlag = false;
            return;
        }

        private void SerovoAllOrgMove2()
        {
            //X 원점

            label126.Text = "X/Y 축 원점 복귀중";
#if !DEBUG_MODE
            send_alarm_clr();
            AXTServo.OriginMove(MOTOR_Y);
            AXTServo.OriginMove(MOTOR_X);
#endif
            OrgMoveMode++;

            return;
        }

        private void button53_Click(object sender, EventArgs e)
        {

        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            timer4.Enabled = false;
            OrgMoveMode = 0;
#if !DEBUG_MODE
            ServoMotorOn();
            ComF.timedelay(500);
            AXTServo.AxtMotorInit(ServoXDir, ServoYDir, ServoZDir);

            SerovoAllOrgMove1();
#endif
            return;
        }

        private void StandbyPosition()
        {
            int over_time_check = 0;
#if !DEBUG_MODE
            Z_move_set(textBox13.Text, Convert.ToUInt16(textBox70.Text));  //for SBR 무반응 검사 복귀위치
            ComF.timedelay(200);
            over_time_check = 0;

            do
            {
                if (Z_move_check())
                {
                    break;
                }
                ComF.timedelay(100);
                over_time_check++;
            } while (over_time_check < 200);        //wait 20sec
#endif
            return;
        }

        private bool EStopCheck()
        {
#if !DEBUG_MODE
            if (checkBox8.Checked) return false;

            if (IOPort.inportb((short)DI_IN.emg_sw_p) == false)
            {
                if (10 <= EStopCount)
                {
                    if (MotorEStopFlag == false)
                    {
                        if (AXTServo.AxtMovingEndCheck(MOTOR_X) == false) AXTServo.Axt_EStop(MOTOR_X);
                        if (AXTServo.AxtMovingEndCheck(MOTOR_Y) == false) AXTServo.Axt_EStop(MOTOR_Y);
                        if (AXTServo.AxtMovingEndCheck(MOTOR_Z) == false) AXTServo.Axt_EStop(MOTOR_Z);
                    }
                    MotorEStopFlag = true;
                    return true;
                }
                else
                {
                    EStopCount++;
                }
            }
            else
            {
                EStopCount = 0;
                MotorEStopFlag = false;
            }
#endif
            return false;
        }

        private void checkBox9_Click(object sender, EventArgs e)
        {

        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            SbrOrOds = checkBox9.Checked;
            return;
        }

        private void checkBox9_CheckStateChanged(object sender, EventArgs e)
        {
            SbrOrOds = checkBox9.Checked;
            return;
        }

        private short ProductResult = RESULT_NONE;
        private void DisplayStatus(short Result)
        {
            switch (Result)
            {
                case RESULT_NONE:
                    button51.Text = "READY";
                    button51.ForeColor = Color.Black;
                    button51.BackColor = SystemColors.WindowFrame;
                    //this.BackColor = Color.Gainsboro;
                    IOPort.outportb((short)DO_OUT.LAMP_RED, false);
                    IOPort.outportb((short)DO_OUT.LAMP_GREEN, false);
                    IOPort.outportb((short)DO_OUT.LAMP_YELLOW, true);
                    break;
                case RESULT_PASS:
                    button51.Text = "O.K";
                    button51.ForeColor = Color.Black;
                    button51.BackColor = Color.Lime;
                    //this.BackColor = Color.Lime;
                    IOPort.outportb((short)DO_OUT.LAMP_RED, false);
                    IOPort.outportb((short)DO_OUT.LAMP_GREEN, true);
                    IOPort.outportb((short)DO_OUT.LAMP_YELLOW, false);
                    ProductResult = RESULT_PASS;
                    break;
                case RESULT_REJECT:
                    button51.Text = "N.G";
                    button51.ForeColor = Color.White;
                    button51.BackColor = Color.Red;
                    //this.BackColor = Color.Red;
                    IOPort.outportb((short)DO_OUT.LAMP_RED, true);
                    IOPort.outportb((short)DO_OUT.LAMP_GREEN, false);
                    IOPort.outportb((short)DO_OUT.LAMP_YELLOW, false);
                    ProductResult = RESULT_REJECT;
                    break;
                case RESULT_TEST:
                    IOPort.outportb((short)DO_OUT.LAMP_RED, false);
                    IOPort.outportb((short)DO_OUT.LAMP_GREEN, false);
                    IOPort.outportb((short)DO_OUT.LAMP_YELLOW, false);
                    button51.Text = "TEST";
                    button51.ForeColor = Color.Black;
                    button51.BackColor = Color.Yellow;
                    //this.BackColor = Color.Gainsboro;                    
                    break;
                default: break;
            }
            return;
        }

        private void DefaultPositionMove()
        {
#if !DEBUG_MODE
            double xTime;
            double First;
            double Last;

            if (SbrOrOds)
            {
                Z_move_set(textBox46.Text, Convert.ToUInt16(textBox70.Text));  //for SBR
            }
            else
            {
                Z_move_set(textBox38.Text, Convert.ToUInt16(textBox70.Text));  //for ODS
            }
            //Z_moveing_flg = true;

            First = ComF.timeGetTimems();
            Last = ComF.timeGetTimems();
            do
            {
                if (Z_move_check())
                {
                    break;
                }
                Last = ComF.timeGetTimems();
                xTime = Last - First;
            } while (xTime < 20000);        //wait 20sec


            if (SbrOrOds)
            {
                X_move_set(textBox50.Text, Convert.ToUInt16(textBox70.Text));                  //검사대기위치 이동 for SBR
            }
            else
            {
                X_move_set(textBox33.Text, Convert.ToUInt16(textBox70.Text));                  //검사대기위치 이동 for ODS
            }

            if (SbrOrOds)
            {
                Y_move_set(textBox48.Text, Convert.ToUInt16(textBox70.Text));                  //검사대기위치 이동 for SBR
            }
            else
            {
                Y_move_set(textBox36.Text, Convert.ToUInt16(textBox70.Text));                  //검사대기위치 이동 for ODS
            }
#endif
            return;
        }

        private bool LadcellOverCheck()
        {
#if !DEBUG_MODE
            if (kLoadCell < 100)
            {
                return false;
            }
            else
            {
                AllMotorStop();
                return true;
            }
#else
            return false;
#endif
        }

        private void AllMotorStop()
        {
#if !DEBUG_MODE
            X_moter_stop();
            Y_moter_stop();
            Z_moter_stop();
#endif
            return;
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //YB 의 경우
            if (IOPort.inportb((short)DI_IN.power_off) == true) //전원이 정상적일 경우
            {
                if ((test_process_run == false) && (panel5.Visible == false))
                {
                    //검사 중이 아니라면                    
                    ExitFlag = true;
#if !DEBUG_MODE
                    AXTServo.AxtClose();
#endif
                    count_SAVE();
                    e.Cancel = false;
                }
                else
                {
                    //검사 중이면
                    e.Cancel = true;
                }
            }
            else
            {
                //전원이 비 정상적인 경우(전원이 꺼져 있을때)                
                ExitFlag = true;
#if !DEBUG_MODE
                AXTServo.AxtClose();
#endif
                count_SAVE();
#if !DEBUG_MODE
                ExitToExit();
                e.Cancel = false;
#endif
            }
            return;
        }

        private void SaveToLastBarcode()
        {
            Ods.Write_BAR("BARCODE", "NOWBARCODE", txt_barcode1.Text);
            Ods.Write_BAR("BARCODE", "OLDBARCODE", txt_barcode2.Text);
            Ods.Write_BAR("BARCODE", "NOW_POPDATA", txt_popdata.Text);
            Ods.Write_BAR("BARCODE", "SOURCEDATA", St_Popdata.Text);
            return;
        }

        private void ReadToLastBarcode()
        {
            txt_barcode1.Text = Ods.Read_BAR("BARCODE", "NOWBARCODE");
            txt_barcode2.Text = Ods.Read_BAR("BARCODE", "OLDBARCODE");
            txt_popdata.Text = Ods.Read_BAR("BARCODE", "NOW_POPDATA");
            St_Popdata.Text = Ods.Read_BAR("BARCODE", "SOURCEDATA");
            data1 = St_Popdata.Text;
            CheckPop();
            return;
        }
        private void CheckPop()
        {
            send_data_bar.Clear();
            string[] pop_data = St_Popdata.Text.Split(';');
            int datalength;
            int Pop_length;

            Pop_length = data1.ToString().Length;

            if (Pop_length == 0) return;
            txt_barcode1.Text = pop_data[3];
            datalength = pop_data[4].ToString().Length;
            if (10 < datalength)
            {
                txt_popdata.Text = pop_data[4].Substring(0, datalength - 1);
                send_data_bar.Text = data1.Substring(0, Pop_length - 1);
            }
            else
            {
                txt_popdata.Text = pop_data[4].Substring(0, 10);
                send_data_bar.Text = data1.Substring(0, Pop_length);
            }

            popdata1 = txt_popdata.Text;
            select_popdata();
            return;
        }

        private void ScreenInit()
        {
            int Count;

            Count = listView1.Items.Count;

            if (Count < 2)
            {
                listView1.BeginUpdate();
                for (i = 0; i < 50; i++)
                {
                    ListViewItem Item = new ListViewItem("");

                    Item.SubItems.Add("");
                    Item.SubItems.Add("");
                    Item.SubItems.Add("");
                    Item.SubItems.Add("");
                    Item.SubItems.Add("");
                    Item.UseItemStyleForSubItems = false;
                    listView1.Items.Add(Item);
                }
                listView1.EndUpdate();
            }
            else
            {
                for (i = 0; i < 50; i++)
                {
                    //listView1.Items[i].SubItems[0].Text = "";
                    //listView1.Items[i].SubItems[1].Text = "";
                    //listView1.Items[i].SubItems[2].Text = "";
                    listView1.Items[i].SubItems[3].Text = "";
                    //listView1.Items[i].SubItems[4].Text = "";
                    listView1.Items[i].SubItems[5].Text = "";
                }
            }
            listView1.FullRowSelect = true;
        }
        private void IndZeroSetting()
        {
            byte[] Data = new byte[20];

#if !DEBUG_MODE

            Data[0] = LOADCELL_STX;
            Data[1] = 0x30;
            Data[2] = 0x31;
            Data[3] = 0x5a;
            Data[4] = LOADCELL_ETX;
            Data[5] = 0x00;

            if (serialPort1.IsOpen) serialPort1.Write(Data, 0, Data.Length);
#endif
            return;
        }

        private void ToZeroPositionSearch(bool Flag)
        {
            double First;
            double Last;
            double xTime;
            double LSpec;

#if !DEBUG_MODE
            if (SbrOrOds)
            {
                Z_move_set(textBox46.Text, (ushort)(Convert.ToUInt16(textBox65.Text) * 1.5));  //for SBR
            }
            else
            {
                Z_move_set(textBox38.Text, (ushort)(Convert.ToUInt16(textBox65.Text) * 1.5));  //for ODS
            }
            ComF.timedelay(300);
            if (SbrOrOds == true)
            {
                if (Flag == true)
                    LSpec = 30.0;
                else LSpec = 1.0;
            }
            else LSpec = 40.0;

            //Z_moveing_flg = true;
            First = ComF.timeGetTimems();
            Last = ComF.timeGetTimems();
            do
            {
                if (Z_move_check()) //모터가 이동을 시작하면 빠져 나간다.
                {
                    break;
                }

                if (kLoadCell < LSpec)
                {
                    AXTServo.Axt_NStop(MOTOR_Z);
                    return;
                }
                Last = ComF.timeGetTimems();
                xTime = Last - First;
                Application.DoEvents();
            } while (xTime < 20000);        //wait 20sec
#endif
            return;
        }

        

        private void StopProcess()
        {
            test_process_estop = false;
            CanClsoeFlag = true;
            RunningFlag = false;
            ComF.timedelay(500);
            //CAN_HIGH_CLOSE();
            CanReWrite.CanClose(CanChannel);
            DisplayStatus(RESULT_REJECT);
            //DB_save_data_NG();
            //NG_COUNT();
            //Data_Send();

            AllMotorStop();
            ComF.timedelay(200);

            if (test_process_stop == true)
            {
                DefaultPositionMove();
            }
            test_process_stop = false;
            test_process_estop = false;

            test_process_run = false;
            checkBox9.Enabled = true;
            return;
        }

        private void ModelChange()
        {
            //            ModelChangeFlag = true;


            if (ModelName != comboBox1.Text)
            {
                read_Position();
                DefaultServoOptOpen();
                if (textBox70.Text == "") textBox70.Text = "0";
                if (textBox33.Text == "") textBox33.Text = "0";
                if (textBox50.Text == "") textBox50.Text = "0";
                if (textBox48.Text == "") textBox48.Text = "0";
                if (textBox46.Text == "") textBox46.Text = "0";
                if (textBox38.Text == "") textBox38.Text = "0";

                if (ProgramStartFlagToMotor == false)
                {
                    ComF.timedelay(100);
                    if (SbrOrOds == false)
                    {
                        //ODS
                        X_move_set(textBox33.Text, Convert.ToUInt16(textBox70.Text));      //복귀위치
                        ComF.timedelay(100);
                        Y_move_set(textBox36.Text, Convert.ToUInt16(textBox70.Text));      //복귀위치
                        ComF.timedelay(100);
                    }
                    else
                    {
                        //SBR
                        X_move_set(textBox50.Text, Convert.ToUInt16(textBox70.Text));      //복귀위치
                        ComF.timedelay(100);
                        Y_move_set(textBox48.Text, Convert.ToUInt16(textBox70.Text));      //복귀위치
                        ComF.timedelay(100);
                    }

                    if (SbrOrOds == true)
                        Z_move_set(textBox46.Text, Convert.ToUInt16(textBox70.Text));  //for SBR        
                    else Z_move_set(textBox38.Text, Convert.ToUInt16(textBox70.Text));  //for ODS
                }
                else
                {
                    Z_servo_on();
                }
                if (SbrOrOds == true)
                    button3.Text = ModelText + " SBR 전장 검사기";
                else button3.Text = ModelText + " ODS 전장 검사기";
            }
            ModelName = comboBox1.Text;
            //            ModelChangeFlag = false;
            return;
        }

        private void button17_Click_1(object sender, EventArgs e)
        {
            
        }

        private void led1_Click(object sender, EventArgs e)
        {
            if (Pop_Selected == false)
            {
                led1.Value.AsBoolean = true;
                led2.Value.AsBoolean = false;
                SbrOrOds = false;
            }
            return;
        }

        private void led2_Click(object sender, EventArgs e)
        {
            if (Pop_Selected == false)
            {
                led1.Value.AsBoolean = false;
                led2.Value.AsBoolean = true;
                SbrOrOds = true;
                return;
            }
        }

        private void led4_Click(object sender, EventArgs e)
        {
            if (Pop_Selected == false)
            {
                led3.Value.AsBoolean = false;
                led4.Value.AsBoolean = true;
            }
            return;
        }

        private void led3_Click(object sender, EventArgs e)
        {
            if (Pop_Selected == false)
            {
                led4.Value.AsBoolean = false;
                led3.Value.AsBoolean = true;
            }
            return;
        }

        private void led6_Click(object sender, EventArgs e)
        {
            if (Pop_Selected == false)
            {
                led5.Value.AsBoolean = false;
                led6.Value.AsBoolean = true;
            }
            return;
        }

        private void led5_Click(object sender, EventArgs e)
        {
            if (Pop_Selected == false)
            {
                led6.Value.AsBoolean = false;
                led5.Value.AsBoolean = true;
            }
            return;
        }



        private void led8_Click(object sender, EventArgs e)
        {
            //heater type
            if (Pop_Selected == false)
            {
                led7.Value.AsBoolean = false;
                led8.Value.AsBoolean = true;
            }
            return;
        }

        private void led7_Click(object sender, EventArgs e)
        {
            if (Pop_Selected == false)
            {
                led8.Value.AsBoolean = false;
                led7.Value.AsBoolean = true;
            }
            return;
        }


        public void SaveData(short Result)
        {
            try
            {
                //short handle = 0;
                //short listCount = 0;
                //bool Flag1;
                bool Flag2;
                //string[] List = new string[5];

                string path1 = "D:\\DB\\" + DateTime.Now.Year + string.Format("{0:d2}", DateTime.Now.Month) + string.Format("{0:d2}", DateTime.Now.Day) + ".xls";
                //string path2 = "System\\Report.xls";

                //Flag1 = false;
                Flag2 = false;

                string ModelInfor = "";

                if (led1.Value.AsBoolean == true)
                    ModelInfor = "ODS";
                else if (led2.Value.AsBoolean == true)
                    ModelInfor = "SBR";

                if (led4.Value.AsBoolean == true)
                    ModelInfor = ModelInfor + ",LHD";
                else if (led3.Value.AsBoolean == true)
                    ModelInfor = ModelInfor + ",RHD";

                if (led6.Value.AsBoolean == true)
                    ModelInfor = ModelInfor + ",LH";
                else if (led5.Value.AsBoolean == true)
                    ModelInfor = ModelInfor + ",RH";

                if (led8.Value.AsBoolean == true)
                    ModelInfor = ModelInfor + ",HEATER";
                else if (led7.Value.AsBoolean == true)
                    ModelInfor = ModelInfor + ",NONE HEATER";

                if (led15.Value.AsBoolean == true)
                    ModelInfor = ModelInfor + ",VENT";
                else if (led15.Value.AsBoolean == true)
                    ModelInfor = ModelInfor + ",NONE VENT";

                if (File.Exists(path1) == false)
                {
                    Flag2 = true;
                    CreateDataFile();
                    ExcelRow = 0;
                }
                else
                {                    
                    if (fpSpread1.IsExcelFile(path1) == true)
                    {
                        if (ExcelFirst == false)
                        {
                            fpSpread1.ActiveSheet = fpSpread1.Sheets[0];
                            Flag2 = true;
                        }
                        else
                        {
                            Flag2 = true;
                        }
                    }
                }


                if (Flag2 == true)
                {
                    if (ExcelFirst == false)
                    {
                        for (i = 5; i < fpSpread1.ActiveSheet.RowCount; i++)
                        {
                            if (fpSpread1.ActiveSheet.Cells[i, 1].Text.Length == 0)
                            {
                                ExcelRow = i;
                                break;
                            }
                        }
                    }


                    if (ExcelRow < 5) ExcelRow = 5;

                    int Col;
                    //모델

                    Col = 1;
                    CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, comboBox1.Text);
                    //넘버
                    CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, send_data_comit.Text);
                    //날짜
                    CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, DateTime.Now.Year.ToString("0000") + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00"));
                    //검사 시간                    
                    CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00"));
                    //바코드
                    CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, txt_barcode1.Text);
                    //사양
                    CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, ModelInfor);
                    //라인번호
                    CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, linenum);
                    //장비번호                    
                    CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, linecom);
                    //재검사 횟수
                    CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, retest);


                    CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox81.Text);


                    if (SbrOrOds == false)
                    {
                        //ODS

                        if (VentHeater == false) //비통풍 
                        {
                            //VehicleID Spec

                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button69.Text);

                            //VehicleID Data
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button40.Text);


                            //Img Capacitance unloading(설정)

                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox8.Text);


                            //Img Capacitance unloading(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox68.Text);

                            //real Capacitance unloading(설정)

                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox11.Text);

                            //real Capacitance unloading(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox69.Text);

                            //Img Capacitance Loading(설정)

                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox32.Text);

                            //Img Capacitance Loading(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox67.Text);
                            //real Capacitance Loading(설정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox31.Text);
                            //real Capacitance Loading(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox66.Text);

                            if (led8.Value.AsBoolean == true)
                            {
                                //Cal checksum(설정) Heater
                                CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button43.Text);
                            }
                            else if (led7.Value.AsBoolean == true)
                            {
                                //Cal checksum(설정)Non-heater
                                CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button90.Text);
                            }

                            //Cal checksum(측정) heater
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button50.Text);

                            //ROM ID(설정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button66.Text);

                            //ROM ID(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button44.Text);

                            //OPA File Number (설정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button67.Text);
                            //OPA File Number (측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button45.Text);
                            //EOL byte(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "0xAA");
                            //EOL byte(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button68.Text);

                            //UNLOCK byte(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "0xAA");
                            //UNLOCK byte(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, UnlockByteData);

                            //LOCK byte(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "0x55");
                            //LOCK byte(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, LockByteData);
                            //Algoparameter selection (설정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox29.Text);
                            //Algoparameter selection (측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox30.Text);

                            //Read Memory
                            if (ReadMemoryResult == 1)
                                CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "OK");
                            else if (ReadMemoryResult == 2)
                                CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "NG");
                            else CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "--");

                            //Read Contents
                            if (ReadContentsResult == 1)
                                CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "OK");
                            else if (ReadContentsResult == 2)
                                CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "NG");
                            else CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "--");
                        }
                        else
                        {
                            //VehicleID Spec
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, SpecList[8]);
                            //VehicleID Data
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button40.Text);


                            //Img Capacitance unloading(설정)
                            if (led15.Value.AsBoolean == true)
                                CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, SpecList[3]);
                            else CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, SpecList[18]);
                            //Img Capacitance unloading(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox68.Text);

                            //real Capacitance unloading(설정)
                            if (led15.Value.AsBoolean == true)
                                CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, SpecList[4]);
                            else CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, SpecList[19]);
                            //real Capacitance unloading(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox69.Text);

                            //Img Capacitance Loading(설정)
                            if (led15.Value.AsBoolean == true)
                                CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, SpecList[0]);
                            else CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, SpecList[20]);
                            //Img Capacitance Loading(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox67.Text);

                            //real Capacitance Loading(설정)
                            if (led15.Value.AsBoolean == true)
                                CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox31.Text);
                            else CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, SpecList[1]);
                            //real Capacitance Loading(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox66.Text);

                            if (led8.Value.AsBoolean == true)
                            {
                                //Cal checksum(설정) Heater                                
                                CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, SpecList[5]);
                            }
                            else if (led7.Value.AsBoolean == true)
                            {
                                //Cal checksum(설정)Non-heater                                
                                CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, SpecList[9]);
                            }

                            //Cal checksum(측정) heater
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button50.Text);

                            //ROM ID(설정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, SpecList[2]);

                            //ROM ID(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button44.Text);

                            //OPA File Number (설정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, SpecList[6]);
                            //OPA File Number (측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button45.Text);

                            //EOL byte(설정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "0xAA");
                            //EOL byte(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button68.Text);

                            //UNLOCK byte(설정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "0xAA");
                            //UNLOCK byte(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, UnlockByteData);

                            //LOCK byte(설정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, SpecList[7]);//"0x55");
                            //LOCK byte(측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, LockByteData);

                            //Algoparameter selection (설정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox29.Text);
                            //Algoparameter selection (측정)
                            CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, textBox30.Text);

                            //Read Memory
                            if (ReadMemoryResult == 1)
                                CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "OK");
                            else if (ReadMemoryResult == 2)
                                CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "NG");
                            else CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "--");

                            //Read Contents
                            if (ReadContentsResult == 1)
                                CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "OK");
                            else if (ReadContentsResult == 2)
                                CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "NG");
                            else CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "--");
                        }

                        //SBR설정값1
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                        //SBR측정값1
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");

                        //SBR설정값2
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");

                        //SBR측정값2
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                    }
                    else
                    {
                        //VehicleID Spec
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                        //VehicleID Data
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");

                        //Img Capacitance unloading(설정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                        //Img Capacitance unloading(측정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");

                        //real Capacitance unloading(설정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                        //real Capacitance unloading(측정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                        //Img Capacitance Loading(설정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                        //Img Capacitance Loading(측정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                        //real Capacitance Loading(설정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                        //real Capacitance Loading(측정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");

                        //Cal checksum(설정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");

                        //Cal checksum(측정) heater
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");

                        //ROM ID(설정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                        //ROM ID(측정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");

                        //OPA File Number (설정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                        //OPA File Number (측정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                        //EOL byte(측정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                        //EOL byte(측정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");

                        //UNLOCK byte(측정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                        //UNLOCK byte(측정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");

                        //LOCK byte(측정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                        //LOCK byte(측정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                        //Algoparameter selection (설정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                        //Algoparameter selection (측정)
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");

                        //Read Memory
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");
                        //Read Contents
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, "");

                        //SBR설정값1
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button78.Text + " Over");
                        //SBR측정값1
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, NoneLoadData);

                        //SBR설정값2
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button78.Text + " Under");

                        //SBR측정값2
                        CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, LoadData);
                    }

                    CellDataSet(ExcelRow, Col++, (button51.Text == "O.K") ? RESULT_PASS : RESULT_REJECT, button51.Text);
                    //axfpSpread1.Col = 37;
                    //axfpSpread1.Text = button51.Text;   //최종판정

                    //SetExcelLayOut();

                    //axfpSpread1.ExportToExcel(path1, List[0], "C:\\ILOGFILE.TXT");
                    fpSpread1.SaveExcel(path1);

                    ExcelRow++;
                }

                ExcelFirst = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void DisplaySpec()
        {
            int i;

            for (i = 0; i < 50; i++)
            {
                listView1.Items[i].SubItems[0].Text = "";
                listView1.Items[i].SubItems[1].Text = "";
                listView1.Items[i].SubItems[2].Text = "";
                listView1.Items[i].SubItems[3].Text = "";
                listView1.Items[i].SubItems[4].Text = "";
                listView1.Items[i].SubItems[5].Text = "";
            }
            if (SbrOrOds == true)
            {
                //SBR
                lv_num = 1;
                list(lv_num, "SBR TEST Start", "", "", "", "");
                lv_num++;

                list(lv_num, textBox12.Text + "Kg부하 검사", button78.Text + "Ω 이상", "", "", "");
                lv_num++;

                list(lv_num, button81.Text + "Kg부하 검사", button78.Text + "Ω 미만", "", "", "");
                lv_num++;

                list(lv_num, "SBR 검사 완료", "", "", "", "");
            }
            else
            {
                //ODS
                lv_num = 1;
                list(lv_num, "BOSE Connected", "", "", "", "");
                lv_num++;

                if (VentHeater == false)
                {
                    //비통풍 히터
                    if (checkBox8.Checked == false)
                    {
                        if (led8.Value.AsBoolean == true)
                        {
                            list(lv_num, "Seat img Unload 값 검사", textBox62.Text, textBox8.Text, "", "");
                            lv_num++;

                            //list(lv_num, "Seat real Unload 값 검사", textBox61.Text,textBox11.Text, "","");
                            list(lv_num, "Seat real Unload 값 검사", "", "", "", "");
                            lv_num++;
                        }
                        else
                        {
                            list(lv_num, "Seat img Unload 값 검사", textBox76.Text, textBox78.Text, "", "");
                            lv_num++;

                            //list(lv_num, "Seat real Unload 값 검사", textBox61.Text,textBox11.Text, "","");
                            list(lv_num, "Seat real Unload 값 검사", "", "", "", "");
                            lv_num++;
                        }
                    }
                    else
                    {
                        //NOPC 검사일때
                        if (NotLoadToTest == true) //무 부하 검사일 경우
                        {
                            if (led8.Value.AsBoolean == true)
                            {
                                list(lv_num, "Seat img Unload 값 검사", textBox62.Text, textBox8.Text, "", "");
                                lv_num++;

                                //list(lv_num, "Seat real Unload 값 검사", textBox61.Text,textBox11.Text, "","");
                                list(lv_num, "Seat real Unload 값 검사", "", "", "", "");
                                lv_num++;
                            }
                            else
                            {
                                list(lv_num, "Seat img Unload 값 검사", textBox76.Text, textBox78.Text, "", "");
                                lv_num++;

                                //list(lv_num, "Seat real Unload 값 검사", textBox61.Text,textBox11.Text, "","");
                                list(lv_num, "Seat real Unload 값 검사", "", "", "", "");
                                lv_num++;
                            }
                        }
                    }

                    list(lv_num, "UNLOCK byte값 검사", "0xAA", "", "", "");
                    lv_num++;
                    list(lv_num, "ROM ID 값 검사", "0x" + button66.Text, "", "", "");
                    lv_num++;

                    //------------------------------------------------------------------------
                    //201060808 추가 
                    if (ReadMemory == true)
                    {
                        list(lv_num, "Read Memory", "", "", "", "");
                        lv_num++;
                    }
                    //------------------------------------------------------------------------
                    list(lv_num, "OPA file Number값 검사", "0x" + button67.Text, "", "", "");
                    lv_num++;
                    if (led8.Value.AsBoolean == true)
                    {
                        list(lv_num, "Seat img Load 값 검사", textBox64.Text, textBox32.Text, "", "");
                        lv_num++;
                        //list(lv_num, "Seat real load 값 검사", textBox63.Text , textBox31.Text, "", "");
                        list(lv_num, "Seat real load 값 검사", "", "", "", "");
                        lv_num++;
                    }
                    else
                    {
                        list(lv_num, "Seat img Load 값 검사", textBox74.Text, textBox80.Text, "", "");
                        lv_num++;
                        //list(lv_num, "Seat real load 값 검사", textBox63.Text , textBox31.Text, "", "");
                        list(lv_num, "Seat real load 값 검사", "", "", "", "");
                        lv_num++;
                    }
                    if (led8.Value.AsBoolean == true) textBox29.Text = "01FF4060";
                    if (led7.Value.AsBoolean == true) textBox29.Text = "00FF41F0";

                    list(lv_num, "Algoparameter값 검사", "0x" + textBox29.Text, "", "", "");
                    lv_num++;

                    if (led8.Value.AsBoolean == true)
                    {
                        //heater type
                        list(lv_num, "Cal Checksum값 검사", "0x" + button43.Text, "", "", "");
                    }
                    else if (led7.Value.AsBoolean == true)
                    {
                        //Non-heater type
                        list(lv_num, "Cal Checksum값 검사", "0x" + button90.Text, "", "", "");
                    }
                    else
                    {
                        list(lv_num, "Cal Checksum값 검사", "0x00", "", "", "");
                    }
                    lv_num++;
                    //------------------------------------------------------------------------
                    //201060808 추가 

                    if (ReadContents == true)
                    {
                        list(lv_num, "Read Contents", "", "", "", "");
                        lv_num++;
                    }
                    //------------------------------------------------------------------------

                    list(lv_num, "EOL byte값 검사", "0xAA", "", "", "");
                    lv_num++;


                    list(lv_num, "LOCK byte값 검사", "0x55", "", "", "");
                    lv_num++;
                    list(lv_num, "Vehicle ID 확인", "0x" + button69.Text, "", "", "");
                    lv_num++;

                    list(lv_num, "DTC 확인", "0", "", "", "");
                    lv_num++;
                    list(lv_num, "DTC Clear", "0", "", "", "");
                    lv_num++;
                    list(lv_num, "BoSe 검사 완료", "", "", "", "");
                    lv_num++;
                    //list(lv_num, "BoSe 검사 완료", "", "", "", "");
                    //lv_num++;
                }
                else
                {
                    //통풍 히터
                    if (checkBox8.Checked == false)
                    {
                        if (led8.Value.AsBoolean == true)
                        {
                            list(lv_num, "Seat img Unload 값 검사", SpecList[10], SpecList[3], "", "");
                            lv_num++;

                            //list(lv_num, "Seat real Unload 값 검사", textBox61.Text,textBox11.Text, "","");
                            list(lv_num, "Seat real Unload 값 검사", "", "", "", "");
                            lv_num++;
                        }
                        else
                        {
                            list(lv_num, "Seat img Unload 값 검사", SpecList[14], SpecList[18], "", "");
                            lv_num++;

                            //list(lv_num, "Seat real Unload 값 검사", textBox61.Text,textBox11.Text, "","");
                            list(lv_num, "Seat real Unload 값 검사", "", "", "", "");
                            lv_num++;
                        }
                    }
                    else
                    {
                        //NOPC 검사일때
                        if (NotLoadToTest == true) //무 부하 검사일 경우
                        {
                            if (led8.Value.AsBoolean == true)
                            {
                                list(lv_num, "Seat img Unload 값 검사", SpecList[10], SpecList[3], "", "");
                                lv_num++;

                                //list(lv_num, "Seat real Unload 값 검사", textBox61.Text,textBox11.Text, "","");
                                list(lv_num, "Seat real Unload 값 검사", "", "", "", "");
                                lv_num++;
                            }
                            else
                            {
                                list(lv_num, "Seat img Unload 값 검사", SpecList[14], SpecList[18], "", "");
                                lv_num++;

                                //list(lv_num, "Seat real Unload 값 검사", textBox61.Text,textBox11.Text, "","");
                                list(lv_num, "Seat real Unload 값 검사", "", "", "", "");
                                lv_num++;
                            }
                        }
                    }

                    list(lv_num, "UNLOCK byte값 검사", "0xAA", "", "", "");
                    lv_num++;
                    list(lv_num, "ROM ID 값 검사", "0x" + SpecList[2], "", "", "");
                    lv_num++;

                    //------------------------------------------------------------------------
                    //201060808 추가 
                    if (ReadMemory == true)
                    {
                        list(lv_num, "Read Memory", "", "", "", "");
                        lv_num++;
                    }
                    //------------------------------------------------------------------------
                    list(lv_num, "OPA file Number값 검사", "0x" + SpecList[6], "", "", "");
                    lv_num++;
                    if (led8.Value.AsBoolean == true)
                    {
                        list(lv_num, "Seat img Load 값 검사", SpecList[12], SpecList[0], "", "");
                        lv_num++;
                        //list(lv_num, "Seat real load 값 검사", textBox63.Text , textBox31.Text, "", "");
                        list(lv_num, "Seat real load 값 검사", "", "", "", "");
                        lv_num++;
                    }
                    else
                    {
                        list(lv_num, "Seat img Load 값 검사", SpecList[16], SpecList[20], "", "");
                        lv_num++;
                        //list(lv_num, "Seat real load 값 검사", textBox63.Text , textBox31.Text, "", "");
                        list(lv_num, "Seat real load 값 검사", "", "", "", "");
                        lv_num++;
                    }
                    if (led8.Value.AsBoolean == true) textBox29.Text = "01FF4060";
                    if (led7.Value.AsBoolean == true) textBox29.Text = "00FF41F0";

                    list(lv_num, "Algoparameter값 검사", "0x" + textBox29.Text, "", "", "");
                    lv_num++;

                    if (led8.Value.AsBoolean == true)
                    {
                        //heater type
                        list(lv_num, "Cal Checksum값 검사", "0x" + SpecList[5], "", "", "");
                    }
                    else if (led7.Value.AsBoolean == true)
                    {
                        //Non-heater type
                        list(lv_num, "Cal Checksum값 검사", "0x" + SpecList[9], "", "", "");
                    }
                    else
                    {
                        list(lv_num, "Cal Checksum값 검사", "0x00", "", "", "");
                    }
                    lv_num++;
                    //------------------------------------------------------------------------
                    //201060808 추가 

                    if (ReadContents == true)
                    {
                        list(lv_num, "Read Contents", "", "", "", "");
                        lv_num++;
                    }
                    //------------------------------------------------------------------------

                    list(lv_num, "EOL byte값 검사", "0xAA", "", "", "");
                    lv_num++;


                    list(lv_num, "LOCK byte값 검사", "0x55", "", "", "");
                    lv_num++;
                    list(lv_num, "Vehicle ID 확인", "0x" + SpecList[8], "", "", "");
                    lv_num++;

                    list(lv_num, "DTC 확인", "0", "", "", "");
                    lv_num++;
                    list(lv_num, "DTC Clear", "0", "", "", "");
                    lv_num++;
                    list(lv_num, "BoSe 검사 완료", "", "", "", "");
                    lv_num++;
                    //list(lv_num, "BoSe 검사 완료", "", "", "", "");
                    //lv_num++;
                }
            }
            return;
        }

        private string ReadMaxSpec(int Pos)
        {
            int i;
            //bool xFlag;
            int xPos = 0;
            string s = "0";
            char[] s1 = new char[100];
            bool Flag = false;

            s = listView1.Items[Pos - 1].SubItems[4].Text;


            Array.Clear(s1, 0, 100);
            Array.Copy(s.ToCharArray(), 0, s1, 0, s.Length);

            for (i = 0; i < s.Length; i++)
            {
                if (ToNumCheck(s1[i]) == false)
                {
                    xPos = i;
                    Flag = true;
                    break;
                }
            }

            /*
            i = 0;
            foreach (char c in s)
            {
                xFlag = false;
                if (char.IsNumber(c) == true) xFlag = true;
                if (c == '-') xFlag = true;
                if ((c == 'x') || ('X' == c)) xFlag = true;
                if (('a' <= c) && (c <= 'f')) xFlag = true;
                if (('A' <= c) && (c <= 'F')) xFlag = true;

                if (xFlag == false)
                {
                    xPos = i;
                    Flag = true;
                    break;
                }
                i++;
            }
            */
            if (Flag == false) xPos = s.Length;
            Array.Clear(s1, xPos, 100 - xPos);
            s = "";
            for (i = 0; i < xPos; i++)
            {
                s = s + s1[i];
            }
            return s;
        }

        private string ReadMinSpec(int Pos)
        {
            //bool xFlag;
            int i;
            int xPos = 0;
            string s = "0";
            bool Flag = false;
            char[] s1 = new char[100];

            s = listView1.Items[Pos - 1].SubItems[2].Text;

            Array.Clear(s1, 0, 100);
            Array.Copy(s.ToCharArray(), 0, s1, 0, s.Length);

            for (i = 0; i < s.Length; i++)
            {
                if (ToNumCheck(s1[i]) == false)
                {
                    xPos = i;
                    Flag = true;
                    break;
                }
            }
            /*
            i = 0;
            foreach (char c in s)
            {
                xFlag = false;
                if (char.IsNumber(c) == true) xFlag = true;
                if (c == '-') xFlag = true;
                if ((c == 'x') || ('X' == c)) xFlag = true;
                if (('a' <= c) && (c <= 'f')) xFlag = true;
                if (('A' <= c) && (c <= 'F')) xFlag = true;

                if(xFlag == false)
                {
                    xPos = i;
                    Flag = true;
                    break;
                }
                i++;
            }
            */
            if (Flag == false) xPos = s.Length;
            Array.Clear(s1, xPos, 100 - xPos);
            s = "";
            for (i = 0; i < xPos; i++)
            {
                s = s + s1[i];
            }
            return s;
        }

        private bool ChecNumber(string s)
        {
            int i;
            //int xPos = 0;

            char[] s1 = new char[100];

            Array.Clear(s1, 0, 100);
            Array.Copy(s.ToCharArray(), 0, s1, 0, s.Length);

            for (i = 0; i < s.Length; i++)
            {
                if (s1[i] != '.' && s1[i] != 'L' && s1[i] != '+' && s1[i] != '-')
                {
                    if (ToNumCheck(s1[i]) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ToNumCheck(char c)
        {
            if (c == '-') return true;
            if (('0' <= c) && (c <= '9')) return true;
            if ((c == 'x') || ('X' == c)) return true;
            if (('a' <= c) && (c <= 'f')) return true;
            if (('A' <= c) && (c <= 'F')) return true;

            return false;
        }

        private void LoadCellCheck(object s, EventArgs e)
        {
            try
            {
                try
                {
#if !DEBUG_MODE
                    double rx_loadcell_data;
                    //bool Flag;

                    if (ConfigSetFlag == true) return;

                    int iRecSize = serialPort1.BytesToRead; // 수신된 데이터 갯수

                    byte[] buff = new byte[iRecSize + 20];
                    serialPort1.Read(buff, 0, iRecSize);

                    //button28.Text = string.Format("{0:X02}   {1:X02}", buff[0], buff[12]);

                    if (buff[0] == LOADCELL_STX && buff[12] == LOADCELL_ETX)
                    {
                        string strRxData = Encoding.Default.GetString(buff);

                        if (0 < strRxData.Substring(3, 8).Length)
                        {
                            /*
                            if (ChecNumber(strRxData.Substring(3, 8)) == true)
                            {
                                //rx_loadcell_data = Convert.ToDouble(strRxData.Substring(3, 8));
                                Flag = double.TryParse(strRxData.Substring(3, 8),out rx_loadcell_data);

                                if (Flag == true)
                                {
                                    if (rx_loadcell_data < 200)
                                    {
                                        kLoadCell = rx_loadcell_data;
                                        button60.Text = rx_loadcell_data.ToString("F1");
                                    }

                                    //if (RunningFlag == true) plot1.Channels[0].AddXY(kLoadCell, SBRData);
                                    if ((RunningFlag == true) && (LiveDisplayFlag == true))
                                    {
                                        plot1.Channels[0].AddXY(DataCount, kLoadCell);
                                        plot1.Channels[1].AddXY(DataCount, (double)SBRData);
                                        plot1.XAxes[0].Tracking.ZoomToFitAll();
                                        plot1.YAxes[0].Tracking.ZoomToFitAll();
                                        plot1.YAxes[1].Tracking.ZoomToFitAll();

                                        DataCount++;
                                    }
                                }
                            }
                            */

                            rx_loadcell_data = GetToNumDouble(strRxData.Substring(3, 8), false, false);
                            if (rx_loadcell_data < 200)
                            {
                                if (LoadMode == 0)
                                {
                                    kLoadCell = rx_loadcell_data;
                                    button60.Text = rx_loadcell_data.ToString("F1");
                                }
                                else if (LoadMode == 1) // ODS
                                {
                                    kLoadCell = rx_loadcell_data - ODSOffset;
                                    if (kLoadCell < 0) kLoadCell = 0;
                                    button60.Text = (rx_loadcell_data - ODSOffset).ToString("F1");
                                }
                                else // SBR
                                {
                                    kLoadCell = rx_loadcell_data - SBROffset;
                                    if (kLoadCell < 0) kLoadCell = 0;
                                    button60.Text = (rx_loadcell_data - SBROffset).ToString("F1");
                                }
                            }

                            //if (RunningFlag == true) plot1.Channels[0].AddXY(kLoadCell, SBRData);
                            if ((RunningFlag == true) && (LiveDisplayFlag == true))
                            {
                                plot1.Channels[0].AddXY(DataCount, kLoadCell);
                                plot1.Channels[1].AddXY(DataCount, (double)SBRData);
                                plot1.XAxes[0].Tracking.ZoomToFitAll();
                                plot1.YAxes[0].Tracking.ZoomToFitAll();
                                plot1.YAxes[1].Tracking.ZoomToFitAll();

                                DataCount++;
                            }
                        }
                        slidingScale1.Value.AsDouble = kLoadCell;
                        slidingScale2.Value.AsInteger = kMotorPos[2];
                        slidingScale3.Value.AsDouble = (double)SBRData;
                    }
#endif
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                }
            }
            finally
            {
            }

            return;
        }

        private void button54_Click(object sender, EventArgs e)
        {

        }


        private void MoveToDefaultZPos()
        {
            double first;
            double last;


            if (SbrOrOds)
            {
                Z_move_set(textBox46.Text, 130);
                first = ComF.timeGetTimems();
                last = ComF.timeGetTimems();
                ComF.timedelay(700);
                do
                {
                    last = ComF.timeGetTimems();
                    Application.DoEvents();
                    if (Z_move_check()) break;
                } while ((last - first) < 5000);
            }
            else
            {
                Z_move_set(textBox38.Text, 130);
                first = ComF.timeGetTimems();
                last = ComF.timeGetTimems();
                ComF.timedelay(700);
                do
                {
                    last = ComF.timeGetTimems();
                    Application.DoEvents();
                    if (Z_move_check()) break;
                } while ((last - first) < 5000);
            }
            return;
        }

        private void button18_Click_1(object sender, EventArgs e)
        {
            
        }


        private void ConnectingMsgDisplay(short Msg, bool OnOff)
        {
            DefaultConnectionMsg = Msg;
            switch (Msg)
            {
                case 0:
                    this.Controls.Add(this.ODS_HeaterPanel);
                    ODS_HeaterPanel.BringToFront();
                    ODS_HeaterPanel.Location = new Point(40 + (Screen.PrimaryScreen.Bounds.Width / 2) - (ODS_HeaterPanel.Width / 2), (Screen.PrimaryScreen.Bounds.Height / 2) - (ODS_HeaterPanel.Height / 2));
                    ODS_HeaterPanel.Visible = OnOff;
                    if (OnOff == true)
                    {
                        //ODS_HeaterPanel.Visible = OnOff;
                        ODS_NoneHeaterPanel.Visible = !OnOff;
                        SBR_Panel.Visible = !OnOff;
                    }
                    if (OnOff == true)
                    {
                        if (File.Exists(ODS_HeaterImageName) == true)
                            pictureBox1.Load(ODS_HeaterImageName);
                        else pictureBox1.Load("Image\\ODS_Heater.jpg");
                    }

                    break;
                case 1:
                    this.Controls.Add(this.ODS_NoneHeaterPanel);
                    ODS_NoneHeaterPanel.BringToFront();
                    ODS_NoneHeaterPanel.Location = new Point(40 + (Screen.PrimaryScreen.Bounds.Width / 2) - (ODS_NoneHeaterPanel.Width / 2), (Screen.PrimaryScreen.Bounds.Height / 2) - (ODS_NoneHeaterPanel.Height / 2));
                    ODS_NoneHeaterPanel.Visible = OnOff;
                    if (OnOff == true)
                    {
                        ODS_HeaterPanel.Visible = !OnOff;
                        //ODS_NoneHeaterPanel.Visible = !OnOff;
                        SBR_Panel.Visible = !OnOff;
                    }
                    if (OnOff == true)
                    {
                        if (File.Exists(ODS_NoneHeaterImageName) == true)
                            pictureBox2.Load(ODS_NoneHeaterImageName);
                        else pictureBox2.Load("Image\\ODS_NoneHeater.jpg");
                    }
                    break;
                case 2:
                    this.Controls.Add(this.SBR_Panel);
                    SBR_Panel.BringToFront();
                    SBR_Panel.Location = new Point(40 + (Screen.PrimaryScreen.Bounds.Width / 2) - (SBR_Panel.Width / 2), (Screen.PrimaryScreen.Bounds.Height / 2) - (SBR_Panel.Height / 2));
                    SBR_Panel.Visible = OnOff;
                    if (OnOff == true)
                    {
                        ODS_HeaterPanel.Visible = !OnOff;
                        ODS_NoneHeaterPanel.Visible = !OnOff;
                        //SBR_Panel.Visible = !OnOff;
                    }
                    if (OnOff == true)
                    {
                        if (File.Exists(SBR_ImageName) == true)
                            pictureBox3.Load(SBR_ImageName);
                        else pictureBox3.Load("Image\\SBR.jpg");
                    }

                    break;
            }

            if (OnOff == false)
            {
                pictureBox1.CancelAsync();
                pictureBox2.CancelAsync();
                pictureBox3.CancelAsync();

                ODS_HeaterPanel.Visible = OnOff;
                ODS_NoneHeaterPanel.Visible = OnOff;
                SBR_Panel.Visible = OnOff;
            }
            return;
        }


        private void StartButtonOnoff(bool OnOff)
        {
            imageButton6.Enabled = OnOff;
            btn_STOP.Enabled = !OnOff;
            imageButton1.Enabled = OnOff;
            imageButton2.Enabled = OnOff;
            imageButton3.Enabled = OnOff;
            imageButton4.Enabled = OnOff;
            imageButton8.Enabled = OnOff;
            return;
        }

        private void DataSendCheck()
        {
            int i;
            int j;
            string s;
            //bool Flag;

            string[] List = new string[5];

            string path1 = "D:\\DB\\" + DateTime.Now.Year + string.Format("{0:d2}", DateTime.Now.Month) + string.Format("{0:d2}", DateTime.Now.Day) + ".xls";
            //string path2 = "System\\Report.xls";


            if (File.Exists(path1) == true)
            {
                if (fpSpread1.IsExcelFile(path1) == true)
                {
                    if (ExcelFirst == true)
                    {
                        j = 0;
                        for (i = 4; i < ExcelRow; i++)
                        {
                            //axfpSpread1.Row = i;
                            //s = axfpSpread1.Text;

                            if (fpSpread1.ActiveSheet.Cells[i, 2].Text == "")
                            {
                                s = Convert.ToString(comitAdd + j);

                                fpSpread1.ActiveSheet.Cells[i, 2].Text = s;
                                j++;
                            }
                        }

                        //axfpSpread1.ExportToExcel(path1, List[0], "C:\\ILOGFILE.TXT");
                        fpSpread1.SaveExcel(path1);
                        comitAdd = comitAdd + j;
                    }
                }
            }

            return;
        }

        private FarPoint.Win.LineBorder LineBorderToHeader = new FarPoint.Win.LineBorder(Color.Black, 1/*RowHeight*/, true, true, true, true);//line color,line style,left,top,right,buttom                       
        private FarPoint.Win.LineBorder LineBorderToData = new FarPoint.Win.LineBorder(Color.Black, 1/*RowHeight*/, true, false, true, true);//line color,line style,left,top,right,buttom                       
        private void CellDataSet(int Row, int Col, short Result, string Data)
        {
            //CellSelect(Col, Row);

            fpSpread1.ActiveSheet.Cells[Row, Col].Border = LineBorderToData;
            //axfpSpread1.CellBorderStyle = FPSpreadADO.CellBorderStyleConstants.CellBorderStyleFineSolid;
            //fpSpread1.ActiveSheet.Cells[Row, Col].CellType = new FarPoint.Win.Spread.CellType.EditBaseCellType();
            fpSpread1.ActiveSheet.Cells[Row, Col].CellType = new FarPoint.Win.Spread.CellType.TextCellType();
            fpSpread1.ActiveSheet.Cells[Row, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread1.ActiveSheet.Cells[Row, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Right;
            /*
            axfpSpread1.SetCellBorder(i + 1, 4, i + 1, 4,
                                    FPSpreadADO.CellBorderIndexConstants.CellBorderIndexLeft |
                                    FPSpreadADO.CellBorderIndexConstants.CellBorderIndexBottom |
                                    FPSpreadADO.CellBorderIndexConstants.CellBorderIndexRight |
                                    FPSpreadADO.CellBorderIndexConstants.CellBorderIndexTop,
                                    1, FPSpreadADO.CellBorderStyleConstants.CellBorderStyleDot);
             */

            if (Result == RESULT_PASS)
                fpSpread1.ActiveSheet.Cells[Row, Col].BackColor = Color.White;
            else fpSpread1.ActiveSheet.Cells[Row, Col].BackColor = Color.Red;

            if (Result == RESULT_PASS)
                fpSpread1.ActiveSheet.Cells[Row, Col].ForeColor = Color.Black;
            else fpSpread1.ActiveSheet.Cells[Row, Col].ForeColor = Color.White;

            //fpSpread1.ActiveSheet.Cells[Row, Col].Se .SetCellBorder(Col, Row, Col, Row, FPSpreadADO.CellBorderIndexConstants.CellBorderIndexOutline, 1, FPSpreadADO.CellBorderStyleConstants.CellBorderStyleFineSolid);
            SetText(Col, Row, Data);
            return;
        }

        private void CheckToNotePC()
        {
            /*
            if (checkBox8.Checked == true)
            {
                //NOTE PC
                comboBox1.Enabled = false;
            }
            else
            {
                //Not Note PC
                comboBox1.Enabled = true;
            }
            */
            comboBox1.Visible = false;
            return;
        }

        private void CreateDataFile()
        {
            fpSpread1.ActiveSheet = fpSpread1.Sheets[0];
            fpSpread1.ActiveSheet.Reset();
            
            fpSpread1.ActiveSheet.RowCount = 3000;
            fpSpread1.ActiveSheet.ColumnCount = 41;

            //용지 방향

            fpSpread1.ActiveSheet.PrintInfo.Orientation = FarPoint.Win.Spread.PrintOrientation.Landscape;

            //프린트할때 컬러 표시할 것인지 여부 선택
            fpSpread1.ActiveSheet.PrintInfo.ShowColor = true;

            //화면에 셀 그리드 표시할 겻인지 선택 (선택되면 내가 원하지 않는 셀 라인까지 표시됨)
            fpSpread1.ActiveSheet.PrintInfo.ShowGrid = false;

            //테두리 및 헤더 표시 여부선택
            fpSpread1.ActiveSheet.PrintInfo.ShowBorder = false;
            fpSpread1.ActiveSheet.PrintInfo.ShowColumnHeader = FarPoint.Win.Spread.PrintHeader.Hide;
            fpSpread1.ActiveSheet.PrintInfo.ShowRowHeader = FarPoint.Win.Spread.PrintHeader.Hide;

            //미리보기 활성화
            fpSpread1.ActiveSheet.PrintInfo.Preview = false;
            
            //여백
            fpSpread1.ActiveSheet.PrintInfo.Margin.Left = 1;
            fpSpread1.ActiveSheet.PrintInfo.Margin.Right = 1;
            fpSpread1.ActiveSheet.PrintInfo.Margin.Top = 1;
            fpSpread1.ActiveSheet.PrintInfo.Margin.Bottom = 1;

            fpSpread1.ActiveSheet.PrintInfo.PageOrder = FarPoint.Win.Spread.PrintPageOrder.Auto;

            //fpSpread1.Sheets[i].PrintInfo.ShowColor = true;
            //fpSpread1.Sheets[i].PrintInfo.ShowGrid = false;
            //fpSpread1.Sheets[i].PrintInfo.Centering = FarPoint.Win.Spread.Centering.Both;

            //fpSpread1.Sheets[i].PrintInfo.PrintShapes = false;

            //fpSpread1.Sheets[i].PrintInfo.ShowSubtitle = FarPoint.Win.Spread.PrintTitle.Hide;
            //fpSpread1.Sheets[i].PrintInfo.ShowTitle = FarPoint.Win.Spread.PrintTitle.Hide;
            //fpSpread1.Sheets[i].PrintInfo.UseSmartPrint = true;

            //fpSpread1.Sheets[i].FrozenColumnCount = 1;
            //fpSpread1.Sheets[i].FrozenRowCount = 3;
            //fpSpread1.Sheets[i].Protect = false;

            //fpSpread1.Sheets[i].VerticalGridLine = new FarPoint.Win.Spread.GridLine(FarPoint.Win.Spread.GridLineType.None);
            //fpSpread1.Sheets[i].HorizontalGridLine = new FarPoint.Win.Spread.GridLine(FarPoint.Win.Spread.GridLineType.None);
            //fpSpread1.Sheets[i].RowHeaderColumnCount = 0;
            //fpSpread1.Sheets[i].ColumnHeaderRowCount = 0;

            
            //axfpSpread1.PrintScalingMethod = FPSpreadADO.PrintScalingMethodConstants.PrintScalingMethodSmartPrint; //용지 넓이에 페이지 맞춤

            //그리드를 표시할 경우 저장할 때나 프린트 할 때 화면에 같이 표시,그리드가 프린트 되기 때문에 지저분해 보인다.
            fpSpread1.ActiveSheet.PrintInfo.ShowColumnHeader = FarPoint.Win.Spread.PrintHeader.Hide;
            fpSpread1.ActiveSheet.PrintInfo.ShowRowHeader = FarPoint.Win.Spread.PrintHeader.Hide;

            //헤더와 밖같 라인이 같이 프린트 되지 않도록 한다.            
            fpSpread1.ActiveSheet.PrintInfo.ShowBorder = false;
            

            fpSpread1.ActiveSheet.PrintInfo.ShowShadows = false;

            fpSpread1.ActiveSheet.PrintInfo.JobName = "레포트 출력";
            

            //시트 보호를 해지 한다.
            
            fpSpread1.ActiveSheet.Protect = false;

            //axfpSpread1.CellBorderColor = Color.Black;
            //axfpSpread1.CellBorderStyle = FPSpreadADO.CellBorderStyleConstants.CellBorderStyleFineSolid;
            //axfpSpread1.CellType = FPSpreadADO.CellTypeConstants.CellTypeEdit;

            //Header
            fpSpread1.ActiveSheet.SetColumnWidth(2, 23);
            fpSpread1.ActiveSheet.SetColumnWidth(3, 23);
            fpSpread1.ActiveSheet.SetColumnWidth(4, 23);

            //틀 고정
            
            fpSpread1.ActiveSheet.FrozenColumnCount = 9;
            fpSpread1.ActiveSheet.FrozenRowCount = 5;


            for (int i = 0; i < 41; i++)
            {
                fpSpread1.ActiveSheet.Cells[2, i + 1].BackColor = Color.Silver;
                fpSpread1.ActiveSheet.Cells[2, i + 3].BackColor = Color.Silver;
                fpSpread1.ActiveSheet.Cells[2, i + 4].BackColor = Color.Silver;

                if ((i < 10) || (i == 40))
                {
                    fpSpread1.ActiveSheet.AddSpanCell(i + 1, 2, 1, 3);
                    fpSpread1.ActiveSheet.Cells[i + 1, 2, i + 1, 4].Border = LineBorderToHeader;
                        
                }
                else
                {
                    if ((i == 10) || (i == 12) || (i == 14) || (i == 16) || (i == 18) || (i == 20) || (i == 22) || (i == 24) || (i == 26) || (i == 28) || (i == 30) || (i == 32) || (i == 36) || (i == 38))
                    {
                        fpSpread1.ActiveSheet.AddSpanCell(i + 1, 2, 1, 3);
                        fpSpread1.ActiveSheet.Cells[i + 1, 2, i + 2, 3].Border = LineBorderToHeader;
                    }

                    fpSpread1.ActiveSheet.AddSpanCell(i + 1, 4, 1, 1);
                    fpSpread1.ActiveSheet.Cells[i + 1, 4, i + 1, 4].Border = LineBorderToHeader;
                }

                SetFont(i + 1, 2);
                SetFont(i + 1, 3);
                SetFont(i + 1, 4);


                switch (i)
                {
                    case 0:
                        fpSpread1.ActiveSheet.SetColumnWidth(1 + i, 13);
                        SetText(i + 1, 2, "모델");
                        break;
                    case 1:
                        fpSpread1.ActiveSheet.SetColumnWidth(1 + i, 8);
                        SetText(i + 1, 2, "POP\nNumber");
                        break;
                    case 2:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 8);
                        SetText(i + 1, 2, "DATE\nTIME");
                        break;
                    case 3:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 8);
                        SetText(i + 1, 2, "검사\n시간");
                        break;
                    case 4:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 23);
                        SetText(i + 1, 2, "바코드");
                        break;
                    case 5:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 2, "사양");
                        break;
                    case 6:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 2, "라인\n번호");
                        break;
                    case 7:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 2, "장비\n번호");
                        break;
                    case 8:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 2, "재검사\n횟수");
                        break;
                    case 9:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 13);
                        SetText(i + 1, 2, "SERIAL\nNO");
                        break;
                    case 10:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 2, "Vehicle ID");
                        SetText(i + 1, 4, "SPEC");
                        break;
                    case 11:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 4, "DATA");
                        break;
                    case 12:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 2, "Img Capacitance\nunloading");
                        SetText(i + 1, 4, "SPEC");
                        break;
                    case 13:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 4, "DATA");
                        break;
                    case 14:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 2, "real Capacitance\nunloading");
                        SetText(i + 1, 4, "SPEC");
                        break;
                    case 15:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 4, "DATA");
                        break;
                    case 16:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 2, "Img Capacitance\nLoading");
                        SetText(i + 1, 4, "SPEC");
                        break;
                    case 17:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 4, "DATA");
                        break;
                    case 18:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 2, "real Capacitance\nLoading");
                        SetText(i + 1, 4, "SPEC");
                        break;
                    case 19:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 4, "DATA");
                        break;
                    case 20:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 6);
                        SetText(i + 1, 2, "Cal checksum");
                        SetText(i + 1, 4, "SPEC");
                        break;
                    case 21:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 4, "DATA");
                        break;
                    case 22:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 2, "ROM ID");
                        SetText(i + 1, 4, "SPEC");
                        break;
                    case 23:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 4, "DATA");
                        break;
                    case 24:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 10);
                        SetText(i + 1, 2, "OPA\nFile Number");
                        SetText(i + 1, 4, "SPEC");
                        break;
                    case 25:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 10);
                        SetText(i + 1, 4, "DATA");
                        break;
                    case 26:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 2, "EOL Byte");
                        SetText(i + 1, 4, "SPEC");
                        break;
                    case 27:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 4, "DATA");
                        break;
                    case 28:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 2, "UNLOCK BYTE");
                        SetText(i + 1, 4, "SPEC");
                        break;
                    case 29:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 4, "DATA");
                        break;
                    case 30:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 2, "LOCK BYTE");
                        SetText(i + 1, 4, "SPEC");
                        break;
                    case 31:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 4, "DATA");
                        break;
                    case 32:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 10);
                        SetText(i + 1, 2, "ALGO\nPARAMETER");
                        SetText(i + 1, 4, "SPEC");
                        break;
                    case 33:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 10);
                        SetText(i + 1, 4, "DATA");
                        break;

                    case 34:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 10);
                        SetText(i + 1, 2, "READ");
                        SetText(i + 1, 4, "MEMORY");
                        break;

                    case 35:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 10);
                        SetText(i + 1, 2, "READ");
                        SetText(i + 1, 4, "CONTENTS");
                        break;

                    case 36:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 8);
                        SetText(i + 1, 2, "SBR\n무부하");
                        SetText(i + 1, 4, "SPEC");
                        break;
                    case 37:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 8);
                        SetText(i + 1, 4, "DATA");
                        break;
                    case 38:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 8);
                        SetText(i + 1, 2, "SBR\n부하");
                        SetText(i + 1, 4, "SPEC");
                        break;
                    case 39:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 8);
                        SetText(i + 1, 4, "DATA");
                        break;
                    case 40:
                        fpSpread1.ActiveSheet.SetColumnWidth(i + 1, 5);
                        SetText(i + 1, 2, "최종\n판정");
                        break;
                }
            }
            //Title
            fpSpread1.ActiveSheet.AddSpanCell(1, 1, 38, 1);
            fpSpread1.ActiveSheet.SetRowHeight(1, 40);
            
            
            fpSpread1.ActiveSheet.Cells[1, 1].ForeColor = Color.Black;

            FontStyle fontStyle = fpSpread1.ActiveSheet.Cells[1, 1].Font.Style;
            if ((fontStyle & FontStyle.Bold) == 0) fontStyle |= FontStyle.Bold;
            if ((fontStyle & FontStyle.Underline) == 0) fontStyle |= FontStyle.Underline;

            fpSpread1.ActiveSheet.Cells[1, 1].Font = new Font(fpSpread1.ActiveSheet.Cells[1, 1].Font.Name, 36, fontStyle);

            fpSpread1.ActiveSheet.Cells[1, 1].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Center;
            fpSpread1.ActiveSheet.Cells[1, 1].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            SetText(1, 1, "검사 레포트");
            return;
        }


        //private void CellSelect(int Col, int Row)
        //{
        //    axfpSpread1.Col = Col;
        //    axfpSpread1.Row = Row;

        //    return;
        //}

        private void SetText(int Col, int Row, string text)
        {
            //CellSelect(Col, Row);
            //axfpSpread1.Text = text;
            fpSpread1.ActiveSheet.Cells[Row, Col].Text = text;
            return;
        }

        private void SetText(int Col, int Row, int text)
        {
            //CellSelect(Col, Row);
            //axfpSpread1.Text = text.ToString();
            fpSpread1.ActiveSheet.Cells[Row, Col].Text = text.ToString();
            return;
        }

        private void SetText(int Col, int Row, short text)
        {
            //CellSelect(Col, Row);
            //axfpSpread1.Text = text.ToString();
            fpSpread1.ActiveSheet.Cells[Row, Col].Text = text.ToString();
            return;
        }

        private void SetText(int Col, int Row, long text)
        {
            //CellSelect(Col, Row);
            //axfpSpread1.Text = text.ToString();
            fpSpread1.ActiveSheet.Cells[Row, Col].Text = text.ToString();
            return;
        }

        private void SetText(int Col, int Row, float text)
        {
            //CellSelect(Col, Row);
            //axfpSpread1.Text = string.Format("{0:0.00}", text);
            fpSpread1.ActiveSheet.Cells[Row, Col].Text = string.Format("{0:0.00}", text);
            return;
        }

        private void SetText(int Col, int Row, double text)
        {
            //CellSelect(Col, Row);
            //axfpSpread1.Text = string.Format("{0:0.00}", text);
            fpSpread1.ActiveSheet.Cells[Row, Col].Text = string.Format("{0:0.00}", text);
            return;
        }

        private void SetFont(int Col, int Row)
        {
            //CellSelect(Col, Row);


            FontStyle fontStyle = fpSpread1.ActiveSheet.Cells[Row, Col].Font.Style;
            if ((fontStyle & FontStyle.Bold) == 0) fontStyle |= FontStyle.Bold;
            if ((fontStyle & FontStyle.Underline) == 0) fontStyle |= FontStyle.Underline;

            fpSpread1.ActiveSheet.Cells[Row, Col].Font = new Font(fpSpread1.ActiveSheet.Cells[Row, Col].Font.Name, 9, fontStyle);

            FarPoint.Win.Spread.CellType.TextCellType Edit = new FarPoint.Win.Spread.CellType.TextCellType(); //(FarPoint.Win.Spread.CellType.TextCellType)fpSpread1.ActiveSheet.Cells[Row, Col].CellType;

            Edit.Multiline = true;

            //axfpSpread1.TypeEditMultiLine = true;
            fpSpread1.ActiveSheet.Cells[Row, Col].CellType = Edit;

            fpSpread1.ActiveSheet.Cells[Row, Col].VerticalAlignment = FarPoint.Win.Spread.CellVerticalAlignment.Center;
            fpSpread1.ActiveSheet.Cells[Row, Col].HorizontalAlignment = FarPoint.Win.Spread.CellHorizontalAlignment.Right;
            return;
        }

        private void button28_Click_1(object sender, EventArgs e)
        {

        }

        private void Z_PosToDefailtOutToPLC(bool Flag)
        {
            IOPort.outportb((short)DO_OUT.Z_MOTOR_ORG, !Flag);
            return;
        }

        private void SetTestEnd(bool OnOff)
        {
            //panel14.Parent = this;
            if (OnOff == true)
            {
                StartButtonOnoff(true);
                Z_PosToDefailtOutToPLCFlag = true;
                test_ing.Value.AsBoolean = false;
                IOPort.outportb((short)DO_OUT.TEST_ING, false);
                //panel14.Visible = true;
                //panel14.Location = new Point((Screen.PrimaryScreen.Bounds.Width / 2) - (panel14.Width / 2), (Screen.PrimaryScreen.Bounds.Height / 2) - panel14.Height);
                //panel14.BringToFront();
                ConnectorOutOnOff(ON);
                IOPort.outportb((short)DO_OUT.BUZZER, true);
                ComF.timedelay(700);
                IOPort.outportb((short)DO_OUT.BUZZER, false);
                IOPort.outportb((short)DO_OUT.LAMP_GREEN, false);
                IOPort.outportb((short)DO_OUT.LAMP_RED, false);
                IOPort.outportb((short)DO_OUT.LAMP_YELLOW, true);
            }
            else
            {
                Z_PosToDefailtOutToPLCFlag = false;
                //panel14.Visible = false;
                ConnectorOutOnOff(OFF);
                test_ing.Value.AsBoolean = true;
                IOPort.outportb((short)DO_OUT.TEST_ING, true);
                Test_OK.Value.AsBoolean = false;
                IOPort.outportb((short)DO_OUT.TEST_END, false);
            }
            return;
        }

        private void ConnectorOutOnOff(bool OnOff)
        {
            if (OnOff == true)
            {
                if (panel14.Visible == true) return;

                xForm = new Form();
                panel14.Visible = true;
                //                this.Controls.Add(this.panel14);
                panel14.BringToFront();
                panel14.Location = new Point(1, 1);

                xForm.Size = new Size(panel14.Size.Width + 1, panel14.Size.Height + 1);
                panel14.Parent = xForm;
                xForm.MinimizeBox = false;
                xForm.MaximizeBox = false;
                xForm.Text = "검사 옵션 설정";
                //xForm.BringToFront();
                xForm.WindowState = FormWindowState.Normal;
                xForm.FormBorderStyle = FormBorderStyle.None;
                xForm.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.xForm_FormClosing);
                xForm.TopMost = true; //C++ StayOnTop 기능과 같은 것으로 다른 폼 위에 내 폼을 표시한다.

                xForm.StartPosition = FormStartPosition.CenterParent;
                /*
                if((xForm.Top != ((Screen.PrimaryScreen.Bounds.Width / 2) - (xForm.Size.Width / 2))) || 
                   (xForm.Left != ((Screen.PrimaryScreen.Bounds.Height / 2) - (xForm.Size.Height / 2))))
                {
                    xForm.Location = new Point((Screen.PrimaryScreen.Bounds.Width / 2) - (xForm.Size.Width / 2), (Screen.PrimaryScreen.Bounds.Height / 2) - (xForm.Size.Height / 2));
                    //xForm.Top = (Screen.PrimaryScreen.Bounds.Height / 2) - (xForm.Size.Height / 2);
                    //xForm.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (xForm.Size.Width / 2);                
                }
                 * */
                ProductOutFlag = true;
                //OptionForm.Top = (Screen.PrimaryScreen.Bounds.Height / 2) - (OptionForm.Size.Height / 2);
                //OptionForm.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (OptionForm.Size.Width / 2);
                xForm.ShowDialog();
            }
            else
            {
                if (panel14.Visible == false) return;
                xForm.Close();
                panel14.Visible = false;
                panel14.Parent = null;
                //xForm.Dispose();
            }
            return;
        }

        private void xForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;
            return;
        }

        public string GetString(string data, bool ReturnToChar, bool ToHexString)
        {
            int i;
            StringBuilder sb = new StringBuilder();

            i = 0;
            foreach (char c in data)
            {
                i++;
                // Check for numeric characters (hex in this case).  Add "." and "e" if float,
                // and remove letters.  Include initial space because it is harmless.
                if (ToHexString == true)
                {
                    if (((c >= '0') && (c <= '9')) || ((c >= 'A') && (c <= 'F')) || ((c >= 'a') && (c <= 'f')) || (c == ' ') || (c == '+') || (c == '-'))
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        if (ReturnToChar == true)
                        {
                            if ((c == 'x') || (c == 'X'))
                            {
                                if (2 < i) break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (((c >= '0') && (c <= '9')) || (c == '.') || (c == ' ') || (c == '+') || (c == '-'))
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return sb.ToString();
        }

        public int GetToNumInt(string s, bool ReturnToChar, bool ToHex) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
        {
            int value;
            string sb;

            sb = GetString(s, ReturnToChar, ToHex);

            if (ToHex == true)
            {
                if (int.TryParse(sb, System.Globalization.NumberStyles.HexNumber, null, out value) == false)
                    return 0;
                else return value;
            }
            else
            {
                if (int.TryParse(sb, System.Globalization.NumberStyles.Number, null, out value) == false)
                    return 0;
                else return value;
            }
        }

        public float GetToNumFloat(string s, bool ReturnToChar, bool ToHex) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
        {
            float value;
            string sb;

            sb = GetString(s, ReturnToChar, ToHex);

            if (ToHex == true)
            {
                if (float.TryParse(sb, System.Globalization.NumberStyles.HexNumber, null, out value) == false)
                    return 0;
                else return value;
            }
            else
            {
                if (float.TryParse(sb, System.Globalization.NumberStyles.Number, null, out value) == false)
                    return 0;
                else return value;
            }
        }

        public short GetToNumShort(string s, bool ReturnToChar, bool ToHex) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
        {
            short value;
            string sb;

            sb = GetString(s, ReturnToChar, ToHex);

            if (ToHex == true)
            {
                if (short.TryParse(sb, System.Globalization.NumberStyles.HexNumber, null, out value) == false)
                    return 0;
                else return value;
            }
            else
            {
                if (short.TryParse(sb, System.Globalization.NumberStyles.Number, null, out value) == false)
                    return 0;
                else return value;
            }
        }

        public long GetToNumLong(string s, bool ReturnToChar, bool ToHex) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
        {
            long value;
            string sb;

            sb = GetString(s, ReturnToChar, ToHex);

            if (ToHex == true)
            {
                if (long.TryParse(sb, System.Globalization.NumberStyles.HexNumber, null, out value) == false)
                    return 0;
                else return value;
            }
            else
            {
                if (long.TryParse(sb, System.Globalization.NumberStyles.Number, null, out value) == false)
                    return 0;
                else return value;
            }
        }

        public double GetToNumDouble(string s, bool ReturnToChar, bool ToHex) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
        {
            double value;
            string sb;

            sb = GetString(s, ReturnToChar, ToHex);

            if (ToHex == true)
            {
                if (double.TryParse(sb, System.Globalization.NumberStyles.HexNumber, null, out value) == false)
                    return 0;
                else return value;
            }
            else
            {
                if (double.TryParse(sb, System.Globalization.NumberStyles.Number, null, out value) == false)
                    return 0;
                else return value;
            }
        }

        public byte GetToNumByte(string s, bool ReturnToChar, bool ToHex) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
        {
            byte value;
            string sb;

            sb = GetString(s, ReturnToChar, ToHex);

            if (ToHex == true)
            {
                if (byte.TryParse(sb, System.Globalization.NumberStyles.HexNumber, null, out value) == false)
                    return 0x00;
                else return value;
            }
            else
            {
                if (byte.TryParse(sb, System.Globalization.NumberStyles.Number, null, out value) == false)
                    return 0x00;
                else return value;
            }
        }

        public string ToString(byte[] buf)
        {
            string s = "";

            foreach (char c in buf)
            {
                s = s + c;
            }
            return s;
        }

        private void ZMotorBrack(bool OnOff)
        {
            IOPort.outportb((short)DO_OUT.SERVO_Z_BRACK, !OnOff);
            return;
        }

        private void ExitTo30SeccondDelay()
        {
            Process.Start("shutdown.exe", "-s");
        }

        private void ExitToExit()
        {
            Process.Start("shutdown.exe", "-s -t 00");
        }
        private void ExitToExitToDelay(int sec)
        {
            string s;

            s = "shutdown.exe" + "," + "-s -t " + sec.ToString();
            Process.Start(s);
        }

        private void ReStart()
        {
            Process.Start("shutdown.exe", "-r");
        }

        private void ReStartToExitToDelay(int sec)
        {
            string s;

            s = "shutdown.exe" + "," + "-r -t " + sec.ToString();
            Process.Start(s);
        }

        private void LogOff()
        {
            Process.Start("shutdown.exe", "-l");
        }

        private int ReadMemoryCheck()
        {
            //int ret;
            int Count;
            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };
            

            data.ID = 0x07c3;
            data.Length = 8;
            
            data.DATA[0] = 0x06;
            data.DATA[1] = 0x23;
            data.DATA[2] = 0x13;
            data.DATA[3] = 0x00;
            data.DATA[4] = 0x04;
            data.DATA[5] = 0x1E;
            data.DATA[6] = 0x02;
            data.DATA[7] = 0x55;

            for (int i = 0; i < 5; i++)
            {
                CanReWrite.WriteCan(CanChannel, data, false);
                ComF.timedelay(30);
            }
            LogSaveToSecond(data, "READ MEMORY");

            ComF.timedelay(400);

            if (textBox85.Text == "FFFF")
            {
                textBox86.Text = "---";
                return 1;
            }
            else if (textBox85.Text == "AA20")
            {
                data.ID = 0x07c3;
                data.Length = 8;
                data.DATA[0] = 0x10;
                data.DATA[1] = 0x23;
                data.DATA[2] = 0x2E;
                data.DATA[3] = 0xFA;
                data.DATA[4] = 0xB1;
                data.DATA[5] = 0x04;
                data.DATA[6] = 0x4E;
                data.DATA[7] = 0xFF;
                CanReWrite.WriteCan(CanChannel, data, false);
                LogSaveToSecond(data, "WR MEMORY REQ");

                Count = 0;
                double xFirst = ComF.timeGetTimems();
                double xLast = ComF.timeGetTimems();

                while ((xLast - xFirst) < 1000)
                {
                    if (textBox86.Text == "300808") break;

                    Application.DoEvents();
                    xLast = ComF.timeGetTimems();
                    Count++;
                    //ComF.timedelay(1);
                }
                if (textBox86.Text == "300808")
                {
                    data.ID = 0x07c3;
                    data.Length = 8;
                    data.DATA[0] = 0x21;
                    data.DATA[1] = 0x01;
                    data.DATA[2] = 0x28;
                    data.DATA[3] = 0x00;
                    data.DATA[4] = 0x28;
                    data.DATA[5] = 0x00;
                    data.DATA[6] = 0xA2;
                    data.DATA[7] = 0x00;
                    CanReWrite.WriteCan(CanChannel, data, false);
                    LogSaveToSecond(data, "WRITE MEMORY1");
                    data.DATA[0] = 0x22;
                    data.DATA[1] = 0xA2;
                    data.DATA[2] = 0x00;
                    data.DATA[3] = 0x01;
                    data.DATA[4] = 0x02;
                    data.DATA[5] = 0xCF;
                    data.DATA[6] = 0xFF;
                    data.DATA[7] = 0xFF;
                    CanReWrite.WriteCan(CanChannel, data, false);
                    LogSaveToSecond(data, "WRITE MEMORY2");

                    data.DATA[0] = 0x23;
                    data.DATA[1] = 0xFF;
                    data.DATA[2] = 0xFF;
                    data.DATA[3] = 0xFF;
                    data.DATA[4] = 0xFF;
                    data.DATA[5] = 0xFF;
                    data.DATA[6] = 0xFF;
                    data.DATA[7] = 0xFF;
                    CanReWrite.WriteCan(CanChannel, data, false);
                    LogSaveToSecond(data, "WRITE MEMORY3");

                    data.DATA[0] = 0x24;
                    data.DATA[1] = 0xFF;
                    data.DATA[2] = 0xFF;
                    data.DATA[3] = 0xFF;
                    data.DATA[4] = 0xFF;
                    data.DATA[5] = 0xFF;
                    data.DATA[6] = 0xFF;
                    data.DATA[7] = 0xFF;
                    CanReWrite.WriteCan(CanChannel, data, false);
                    LogSaveToSecond(data, "WRITE MEMORY4");

                    data.DATA[0] = 0x25;
                    data.DATA[1] = 0xFF;
                    data.DATA[2] = 0x55;
                    data.DATA[3] = 0x55;
                    data.DATA[4] = 0x55;
                    data.DATA[5] = 0x55;
                    data.DATA[6] = 0x55;
                    data.DATA[7] = 0x55;
                    CanReWrite.WriteCan(CanChannel, data, false);
                    LogSaveToSecond(data, "WRITE MEMORY5");
                }
            }

            return 0;
        }

        private void LogSaveToSecond(__CanControl.__CanMsg data, string Title)
        {
            uint ccc = (uint)data.ID;
            string log_data = "";

            log_data += "[SEND]:" + "0x" + ccc.ToString("X4") + " ";
            short temp = data.DATA[0];

            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[1];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[2];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[3];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[4];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[5];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[6];
            log_data += temp.ToString("X2") + " ";
            temp = data.DATA[7];
            log_data += temp.ToString("X2") + " ";
            log_data += Title;

            //textBox9.AppendText(log_data + '\n');
            log_data = log_data + '\n';
            richTextBox1.AppendText(log_data);
            Log_SAVE(log_data);
        }

        private int CheckToWriteMemory()
        {
            //int ret;
            int Count;

            //NCTYPE_CAN_FRAME data = new NCTYPE_CAN_FRAME();
            __CanControl.__CanMsg data = new __CanControl.__CanMsg()
            {
                DATA = new byte[8]
            };


            data.ID = 0x07c3;
            data.Length = 8;
            data.DATA[0] = 0x03;
            data.DATA[1] = 0x22;
            data.DATA[2] = 0xFA;
            data.DATA[3] = 0xB1;
            data.DATA[4] = 0x55;
            data.DATA[5] = 0x55;
            data.DATA[6] = 0x55;
            data.DATA[7] = 0x55;

            CanReWrite.WriteCan(CanChannel, data, false);

            LogSaveToSecond(data, "Check Content");

            Count = 0;
            while (Count < 1000)
            {
                if (textBox87.Text == "FAB1") break;

                Application.DoEvents();
                Count++;
                ComF.timedelay(1);
            }
            if (textBox87.Text == "FAB1")
            {
                data.DATA[0] = 0x30;
                data.DATA[1] = 0x0A;
                data.DATA[2] = 0x01;
                data.DATA[3] = 0x55;
                data.DATA[4] = 0x55;
                data.DATA[5] = 0x55;
                data.DATA[6] = 0x55;
                data.DATA[7] = 0x55;

                CanReWrite.WriteCan(CanChannel, data, false);

                LogSaveToSecond(data, "Memory Read Req");

                Count = 0;

                double xFirst = ComF.timeGetTimems();
                double xLast = ComF.timeGetTimems();

                while ((xLast - xFirst) < 3000)
                {
                    if (((textBox88.Text == "21 01 28 00 28 00 A2 00") || (textBox88.Text != ""))
                        && ((textBox89.Text == "22 A2 00 01 02 CF FF FF") || (textBox89.Text != ""))
                        && ((textBox90.Text == "23 FF FF FF FF FF FF FF") || (textBox90.Text != ""))
                        && ((textBox91.Text == "24 FF FF FF FF FF FF FF") || (textBox91.Text != ""))
                        && ((textBox92.Text == "25 FF AA AA AA AA AA AA") || (textBox92.Text != ""))//25FFAAAAAAAAAAAA
                      ) break;

                    Application.DoEvents();
                    Count++;
                    xLast = ComF.timeGetTimems();
                    //ComF.timedelay(1);
                }

                if ((textBox88.Text == "21 01 28 00 28 00 A2 00")
                        && (textBox89.Text == "22 A2 00 01 02 CF FF FF") //22A2000102CFFFFF
                        && (textBox90.Text == "23 FF FF FF FF FF FF FF")
                        && (textBox91.Text == "24 FF FF FF FF FF FF FF") //24FFFFFFFFFFFFFF
                        && (textBox92.Text == "25 FF AA AA AA AA AA AA")) //25FFAAAAAAAAAAAA
                {
                    //ComF.timedelay(500);
                    return 1;
                }
            }

            return 0;
        }

        private void SelectItem()
        {
            ListViewItem Item = listView1.Items[lv_num];

            if (0 < lv_num)
            {
                listView1.Items[lv_num - 1].Focused = false;
                listView1.Items[lv_num - 1].Selected = false;
            }

            listView1.Items[lv_num].Focused = true;
            listView1.Items[lv_num].Selected = true;
            return;
        }

        private void button25_Click_1(object sender, EventArgs e)
        {
            IndZeroSetting();
            return;
        }

        private void CheckDate()
        {
            string s;

            s = string.Format("{0:0000}{1:00}{2:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            if (s != NowDate)
            {
                if ((DateTime.Now.Hour == 8) && (10 <= DateTime.Now.Minute))
                {
                    if (Ods.Read_CNT("CNT", "DATE FLAG") != "1")
                    {
                        Ods.Write_CNT("CNT", "DATE FLAG", "1");
                        Ods.Write_CNT("CNT", "DATE", s);
                        NowDate = s;
                        TOTAL_CNT.Value.AsInteger = 0;
                        OK_CNT.Value.AsInteger = 0;
                        NG_CNT.Value.AsInteger = 0;
                        Ods.Write_CNT("CNT", "TOTAL COUNT", string.Format("{0}", m_OK_count + m_NG_count));
                        Ods.Write_CNT("CNT", "OK COUNT", m_OK_count.ToString());
                        Ods.Write_CNT("CNT", "NG COUNT", m_NG_count.ToString());
                        Ods.Write_CNT("CNT", "comitNo", comitAdd.ToString());
                    }
                }
                else
                {
                    if (8 < DateTime.Now.Hour)
                    {
                        if (Ods.Read_CNT("CNT", "DATE FLAG") != "1")
                        {
                            Ods.Write_CNT("CNT", "DATE FLAG", "1");

                            Ods.Write_CNT("CNT", "DATE", s);
                            NowDate = s;
                            TOTAL_CNT.Value.AsInteger = 0;
                            OK_CNT.Value.AsInteger = 0;
                            NG_CNT.Value.AsInteger = 0;
                            Ods.Write_CNT("CNT", "TOTAL COUNT", string.Format("{0}", m_OK_count + m_NG_count));
                            Ods.Write_CNT("CNT", "OK COUNT", m_OK_count.ToString());
                            Ods.Write_CNT("CNT", "NG COUNT", m_NG_count.ToString());
                            Ods.Write_CNT("CNT", "comitNo", comitAdd.ToString());
                        }
                    }
                }
                if (DateTime.Now.Hour < 8)
                {
                    if (Ods.Read_CNT("CNT", "DATE FLAG") == "1")
                    {
                        Ods.Write_CNT("CNT", "DATE FLAG", "0");
                    }
                }
            }
            else
            {
                if ((DateTime.Now.Hour == 8) && (10 == DateTime.Now.Minute))
                {
                    if (Ods.Read_CNT("CNT", "DATE FLAG") != "1")
                    {
                        Ods.Write_CNT("CNT", "DATE FLAG", "1");

                        Ods.Write_CNT("CNT", "DATE", s);
                        NowDate = s;
                        TOTAL_CNT.Value.AsInteger = 0;
                        OK_CNT.Value.AsInteger = 0;
                        NG_CNT.Value.AsInteger = 0;
                        Ods.Write_CNT("CNT", "TOTAL COUNT", string.Format("{0}", m_OK_count + m_NG_count));
                        Ods.Write_CNT("CNT", "OK COUNT", m_OK_count.ToString());
                        Ods.Write_CNT("CNT", "NG COUNT", m_NG_count.ToString());
                        Ods.Write_CNT("CNT", "comitNo", comitAdd.ToString());
                    }
                }
            }
            return;
        }

        private void button26_Click_1(object sender, EventArgs e)
        {
            
        }

        private void led15_Click(object sender, EventArgs e)
        {
            if (Pop_Selected == false)
            {
                led15.Value.AsBoolean = true;
                led14.Value.AsBoolean = false;
                VentHeater = true;
            }
            return;
        }

        private void led14_Click(object sender, EventArgs e)
        {
            if (Pop_Selected == false)
            {
                led14.Value.AsBoolean = true;
                led15.Value.AsBoolean = false;
                VentHeater = false;
            }
            return;
        }

        private bool ExitFlag = false;

        public bool isExit { get { return ExitFlag; } }
        public double GetXLimit { get { return 0; } }
        public double GetYLimit { get { return 0; } }
        public double GetZLimit { get { return 0; } }
        public double GetXLoad { get { return 0; } }
        public double GetYLoad { get { return 0; } }

        private void imageButton1_Click(object sender, EventArgs e)
        {
            PasswordCheckForm Pass = new PasswordCheckForm();
            Pass.ShowDialog();
            if (Pass.result == false) return;

            SettingMotorPosFlag = true;
            JogSpeed = textBox83.Text;
            OptSetting set = new OptSetting(this);

            DialogResult result = set.ShowDialog();
            set.Owner = this;
            SettingMotorPosFlag = false;

            if (result == DialogResult.OK)
            {
                read_combo();
                //comboBox1.Text = set.comboBox1.Text; 
                read_Position();
                DefaultServoOptOpen();
            }
            return;
        }

        private void imageButton2_Click(object sender, EventArgs e)
        {
            PasswordCheckForm Pass = new PasswordCheckForm();
            Pass.ShowDialog();
            if (Pass.result == false) return;

            Setting set = new Setting();

            set.ShowDialog();
            set.Owner = this;
            read_model();
            DisplaySpec();
            return;
        }

        private void imageButton3_Click(object sender, EventArgs e)
        {
            PasswordCheckForm Pass = new PasswordCheckForm();
            Pass.ShowDialog();
            if (Pass.result == false) return;

            ConfigSetFlag = true;


            Config set = new Config(this);

            set.ShowDialog();
            set.Owner = this;

            read_Config();
            //CAN_INI_HIGH();

            if (0 <= CanDeivce.Device)
            {
                CanReWrite.OpenCan(0, CanPosition(), (short)CanDeivce.Speed, false);
                if (CanReWrite.isOpen(0) == true)
                {
                    m_Que.Clear();
                    CanClsoeFlag = false;
                    ThreadSetting();
                }
            }
            

            ConfigSetFlag = false;
            return;
        }

        private void imageButton4_Click(object sender, EventArgs e)
        {
            Log_SAVE("Program End!");
            //power_close();
            ConnectorOutOnOff(OFF);
            ZMotorBrack(OFF);
            this.Close();
            return;
        }

        private void imageButton5_Click(object sender, EventArgs e)
        {
            test_process_estop = true;
            return;
        }

        private void imageButton6_Click(object sender, EventArgs e)
        {

            if (test_process_run == false)
            {
                if ((led1.Value.AsBoolean == true) || (led2.Value.AsBoolean == true)) // sbr, ods 중 한 제품일경우
                {
                    test_process_RUN();
                }
            }
            return;
        }

        private void imageButton8_Click(object sender, EventArgs e)
        {
            if (Pop_Selected == false)
            {
                imageButton8.ButtonColor = Color.Lime;
                Pop_Selected = true;
                select_popdata();
                ModelName = "";
                ModelChange();
                imageButton9.Enabled = false;
            }
            else
            {
                PasswordCheckForm Pass = new PasswordCheckForm();
                Pass.ShowDialog();
                if (Pass.result == false) return;

                imageButton8.ButtonColor = Color.Black;
                Pop_Selected = false;
                imageButton9.Enabled = true;
            }
            return;
        }

        private void imageButton9_Click(object sender, EventArgs e)
        {
            bool Sbr = false;
            bool LH = false;
            bool LHD = false;

            string ModelType;
            int i;
            //bool Flag;

            ModelType = ModelText;

            if (led2.Value.AsBoolean == true)
                Sbr = true;
            else Sbr = false;

            if (led6.Value.AsBoolean == true)
                LH = true;
            else LH = false;


            // LHD/RHD 선택
            if (led4.Value.AsBoolean == true)
                LHD = true;
            else LHD = false;

            if (Sbr == true)
                ModelType = ModelType + " SBR";
            else ModelType = ModelType + " ODS";

            if (LHD == true)
                ModelType = ModelType + " LHD";
            else ModelType = ModelType + " RHD";

            if (LH == true)
                ModelType = ModelType + "-LH";
            else ModelType = ModelType + "-RH";

            //Flag = false;
            for (i = 0; i < comboBox1.Items.Count; i++)
            {
                if (comboBox1.Items[i].ToString() == ModelType)
                {
                    if (comboBox1.Text != comboBox1.Items[i].ToString())
                    {
                        comboBox1.SelectedIndex = i;
                    }
                    break;
                }
            }

#if !DEBUG_MODE
            if (Sbr == true)
            {
                ConnectingMsgDisplay(2, true);
            }
            else
            {
                if (led8.Value.AsBoolean == true)
                {
                    ConnectingMsgDisplay(0, true);
                }
                else
                {
                    ConnectingMsgDisplay(1, true);
                }
            }
#endif

            if (Sbr == true)
            {
                VentHeater = false;
            }
            else
            {
                if (led14.Value.AsBoolean == true)
                    VentHeater = false;
                else VentHeater = true;
            }
            DisplaySpec();
            return;
        }

        private void imageButton10_Click(object sender, EventArgs e)
        {
            DialogResult Result;

            Result = MessageBox.Show("카운터 값을 초기화 하시겠습니까?", "카운터 초기화", MessageBoxButtons.OKCancel);
            if (DialogResult.OK == Result)
            {
                PasswordCheckForm Pass = new PasswordCheckForm();
                Pass.ShowDialog();
                if (Pass.result == false) return;

                m_OK_count = 0;
                m_NG_count = 0;
                count_SAVE();
                OK_CNT.Value.AsInteger = m_OK_count;
                NG_CNT.Value.AsInteger = m_NG_count;
                TOTAL_CNT.Value.AsInteger = (m_OK_count + m_NG_count);
            }
            return;
        }

        private void imageButton11_Click(object sender, EventArgs e)
        {
            panel17.Visible = false;
            PopFailFirst = ComF.timeGetTimems();
            PopFailLast = ComF.timeGetTimems();
            label160.BackColor = Color.Red;
            PopFailCheckFlag = true;

            if ((ODS_HeaterPanel.Visible == false) && (ODS_NoneHeaterPanel.Visible == false) && (SBR_Panel.Visible == false))
            {
                ConnectingMsgDisplay(DefaultConnectionMsg, true);
            }
            return;
        }

        public double GetZLoad { get { return 0; } }

        public AxtMotion GetServo { get { return AXTServo; } }
        public COMMON_FUCTION PublicFunction { get { return ComF; } }
        public __CanControl GetCanReWrite { get { return CanReWrite; } }

        private BackgroundWorker backgroundWorker1 = null;
        private void ThreadSetting()
        {
            backgroundWorker1 = new BackgroundWorker();

            //ReportProgress메소드를 호출하기 위해서 반드시 true로 설정, false일 경우 ReportProgress메소드를 호출하면 exception 발생
            backgroundWorker1.WorkerReportsProgress = true;
            //스레드에서 취소 지원 여부
            backgroundWorker1.WorkerSupportsCancellation = true;
            //스레드가 run시에 호출되는 핸들러 등록
            backgroundWorker1.DoWork += new DoWorkEventHandler(BackgroundWorker1_DoWork);
            // ReportProgress메소드 호출시 호출되는 핸들러 등록
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(BackgroundWorker1_ProgressChanged);
            // 스레드 완료(종료)시 호출되는 핸들러 동록
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker1_RunWorkerCompleted);


            // 스레드가 Busy(즉, run)가 아니라면
            if (backgroundWorker1.IsBusy != true)
            {
                // 스레드 작동!! 아래 함수 호출 시 위에서 bw.DoWork += new DoWorkEventHandler(bw_DoWork); 에 등록한 핸들러가
                // 호출 됩니다.

                backgroundWorker1.RunWorkerAsync();
            }
            return;
        }

        private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //바로 위에서 worker.ReportProgress((i * 10));호출 시 
            // bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged); 등록한 핸들러가 호출 된다고
            // 하였는데요.. 이 부분에서는 기존 Thread에서 처럼 Dispatcher를 이용하지 않아도 됩니다. 
            // 즉 아래처럼!!사용이 가능합니다.
            //this.tbProgress.Text = (e.ProgressPercentage.ToString() + "%");

            // 기존의 Thread클래스에서 아래와 같이 UI 엘리먼트를 갱신하려면
            // Dispatcher.BeginInvoke(delegate() 
            // {
            //        this.tbProgress.Text = (e.ProgressPercentage.ToString() + "%");
            // )};
            //처럼 처리해야 할 것입니다. 그러나 바로 UI 엘리먼트를 업데이트 하고 있죠??
        }


        //스레드의 run함수가 종료될 경우 해당 핸들러가 호출됩니다.
        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            //스레드가 종료한 이유(사용자 취소, 완료, 에러)에 맞쳐 처리하면 됩니다.
            if ((e.Cancelled == true))
            {
            }
            else if (!(e.Error == null))
            {

            }
            else
            {

            }
        }

        private bool CanClsoeFlag = false;

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            do
            {
                //CancellationPending 속성이 true로 set되었다면(위에서 CancelAsync 메소드 호출 시 true로 set된다고 하였죠?
                if ((worker.CancellationPending == true))
                {
                    //루프를 break한다.(즉 스레드 run 핸들러를 벗어나겠죠)
                    e.Cancel = true;
                    break;
                }
                else
                {
                    // 이곳에는 스레드에서 처리할 연산을 넣으시면 됩니다.

                    this.Invoke(new EventHandler(Processing));

                    Thread.Sleep(1);
                    // 스레드 진행상태 보고 - 이 메소드를 호출 시 위에서 
                    // bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged); 등록한 핸들러가 호출 됩니다.
                    worker.ReportProgress(5);
                }
                if ((ExitFlag == true) || (CanClsoeFlag == true))
                {
                    worker.CancelAsync();
                }
            } while (true);
            //while (ExitFlag == false);
        }

        private void Processing(object sender, EventArgs e)
        {
#if !DEBUG_MODE
            __CanControl.__CanMsg CanMsg = CanReWrite.ReadCan(CanChannel, false);

            if (CanMsg.ID != -1)
            {
                m_Que.Enqueue(CanMsg);
            }
#endif
            return;
        }
    }
}