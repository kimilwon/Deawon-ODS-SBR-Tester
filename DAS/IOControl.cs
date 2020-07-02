#define PROGRAM_RUNNING

using System;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using UniDAQ_Ns;
//using controlArray; //namespace ControlArray

namespace ODS
{
    public class IOControl
    {
        MyInterface mControl = null;

        public struct __IOData__
        {
            public short Card;
            public short Pos;
            public byte Data;
        }

        private struct P32C32BoardInfor
        {
            public ushort[] Out;
            public ushort[] In;
            public ushort[] InPort;
            public ushort[] OutPort;
            public ushort[] InBoard;
            public ushort[] OutBord;
            public ushort InTotalBoardCount;
            public ushort OutTotalBoardCount;
            public byte[][] OutData;
            public byte[][] InData;
        }

        private struct C64BoardInfor
        {
            public ushort[] Out;
            public ushort[] OutPort;
            public ushort[] OutBord;
            public ushort OutTotalBoardCount;
            public byte[][] OutData;

        }

        private struct ADBoardInfor
        {
            public ushort[] ADPort;
            public ushort[] ADBoard;
            public ushort ADTotalBoardCount;
        }

        private P32C32BoardInfor P32C32 = new P32C32BoardInfor()
        {
            Out = new ushort[3],
            In = new ushort[3],
            InPort = new ushort[3],
            OutPort = new ushort[3],
            InTotalBoardCount = 0,
            OutTotalBoardCount = 0,
            OutData = new byte[3][],
            InData = new byte[3][],
            InBoard = new ushort[3],
            OutBord = new ushort[3],
        };

        private C64BoardInfor C64 = new C64BoardInfor()
        {
            Out = new ushort[3],
            OutPort = new ushort[3],
            OutTotalBoardCount = 0,
            OutData = new byte[3][],
            OutBord = new ushort[3],
        };

        private ADBoardInfor AD813 = new ADBoardInfor()
        {
            ADPort = new ushort[2],
            ADBoard = new ushort[2],
            ADTotalBoardCount = 0
        };

        ushort wInitialCode;



        UniDAQ.IXUD_CARD_INFO[] sCardInfo = new UniDAQ.IXUD_CARD_INFO[UniDAQ.MAX_BOARD_NUMBER];
        UniDAQ.IXUD_DEVICE_INFO[] sDeviceInfo = new UniDAQ.IXUD_DEVICE_INFO[UniDAQ.MAX_BOARD_NUMBER];

        const uint dwDataNum = 2;
        float[] fBuf = new float[dwDataNum];

        ushort wTotalBoard;

        uint[] mDIBase = new uint[4];
        uint[] mBase = new uint[4];
        bool Open = false;
        Timer timer1 = new Timer();

        public enum ADCardType
        {
            /// <summary>
            ///0:Low(  /JPx=20V)Gain 
            /// </summary>
            LOW_JPX_20V_GAIN,
            /// <summary>
            /// 1:High(Fast/JPx= 10V) Gain
            /// </summary>
            HIGH_FAST_JPX_10V_GAIN
        }

        public IOControl()
        {
        }

        /// <summary>
        /// IO 제허 함수를 생성한다.
        /// </summary>
        /// <param name="mControl"></param>
        public IOControl(MyInterface mControl)
        {
            P32C32.OutData[0] = new byte[4];
            P32C32.OutData[1] = new byte[4];
            P32C32.OutData[2] = new byte[4];

            P32C32.InData[0] = new byte[4];
            P32C32.InData[1] = new byte[4];
            P32C32.InData[2] = new byte[4];

            C64.OutData[0] = new byte[8];
            C64.OutData[1] = new byte[8];
            C64.OutData[2] = new byte[8];

            this.mControl = mControl;
            timer1.Interval = 10;
            timer1.Tick += new EventHandler(Timer1_Tick);
            timer1.Enabled = true;
            return;
        }

        public short Ch = 0;
        public void Timer1_Tick(object sender, EventArgs e1)
        {
            try
            {
                timer1.Enabled = false;
                if ((Ch < 0) || (11 < Ch)) Ch = 0;

                if (Open == true)
                {
                    P32C32.InData[Ch / 4][Ch % 4] = inportbdata(Ch);
                }
                Ch++;
            }
            catch
            {
                timer1.Enabled = !mControl.isExit;
            }
            finally
            {
                timer1.Enabled = !mControl.isExit;
            }
        }

        /// <summary>
        /// I/O 제어 보드를 오픈한다.
        /// </summary>
        /// <returns></returns>
        public bool OpenIO()
        {
#if PROGRAM_RUNNING
            try
            {
                try
                {
                    //ushort wIrqNo;
                    //ushort wSubVendor;
                    //ushort wSubDevice;
                    //ushort wSubAux;
                    //ushort wSlotDevice;
                    //ushort WSoltBus;
                    //ushort Result;                    
                    //string s;
                    byte[] szModeName = new byte[20];


                    //Driver Initial
                    wInitialCode = UniDAQ.Ixud_DriverInit(ref wTotalBoard);
                    if (wInitialCode != UniDAQ.Ixud_NoErr)
                    {
                        MessageBox.Show("Driver Initial Error!!Error Code:" + wInitialCode.ToString());
                        return false;
                    }

                    P32C32.InTotalBoardCount = 0;
                    P32C32.OutTotalBoardCount = 0;
                    C64.OutTotalBoardCount = 0;
                    AD813.ADTotalBoardCount = 0;

                    for (ushort wBoardIndex = 0; wBoardIndex < wTotalBoard; wBoardIndex++)
                    {
                        //Get Card Information
                        wInitialCode = UniDAQ.Ixud_GetCardInfo(wBoardIndex, ref sDeviceInfo[wBoardIndex], ref sCardInfo[wBoardIndex], szModeName);

                        string chars = Encoding.Default.GetString(szModeName).Trim();

                        if (chars.Contains("PISO-P32C32") == true)
                        {
                            if (0 < sCardInfo[wBoardIndex].wDIPorts)
                            {
                                P32C32.In[P32C32.InTotalBoardCount] = wBoardIndex;
                                P32C32.InPort[P32C32.InTotalBoardCount] = sCardInfo[wBoardIndex].wDIPorts;
                                P32C32.InTotalBoardCount++;
                            }
                            if (0 < sCardInfo[wBoardIndex].wDOPorts)
                            {
                                P32C32.Out[P32C32.OutTotalBoardCount] = wBoardIndex;
                                P32C32.OutPort[P32C32.OutTotalBoardCount] = sCardInfo[wBoardIndex].wDOPorts;
                                P32C32.OutTotalBoardCount++;
                            }
                        }
                        else if (chars.Contains("PISO-C64") == true)
                        {
                            if (0 < sCardInfo[wBoardIndex].wDOPorts)
                            {
                                C64.Out[C64.OutTotalBoardCount] = wBoardIndex;
                                C64.OutPort[C64.OutTotalBoardCount] = sCardInfo[wBoardIndex].wDOPorts;
                                C64.OutTotalBoardCount++;
                            }
                        }
                        else if (chars.Contains("PISO-813") == true)
                        {
                            if (0 < sCardInfo[wBoardIndex].wAIChannels)
                            {
                                AD813.ADBoard[AD813.ADTotalBoardCount] = wBoardIndex;
                                AD813.ADPort[AD813.ADTotalBoardCount] = sCardInfo[wBoardIndex].wAIChannels;
                                AD813.ADTotalBoardCount++;
                            }
                        }
                    }
                    if (0 < P32C32.OutTotalBoardCount)
                    {
                        for (ushort i = 0; i < P32C32.OutTotalBoardCount; i++)
                        {
                            for (ushort j = 0; j < P32C32.OutPort[i]; j++) wInitialCode = UniDAQ.Ixud_SetDIOModes32(P32C32.Out[i], (uint)(1 << j));
                        }
                    }
                    if (0 < C64.OutTotalBoardCount)
                    {
                        for (ushort i = 0; i < C64.OutTotalBoardCount; i++)
                        {
                            for (ushort j = 0; j < C64.OutPort[i]; j++) wInitialCode = UniDAQ.Ixud_SetDIOModes32(C64.Out[i], (uint)(1 << j));
                        }
                    }
                    if (0 < AD813.ADTotalBoardCount)
                    {
                        ushort wRtn = UniDAQ.Ixud_ConfigAI(AD813.ADBoard[0], 2, 2048, (ushort)ADCardType.HIGH_FAST_JPX_10V_GAIN, 0);
                        if (0 < wRtn)
                        {
                            MessageBox.Show("AI Config Error!!Error Code:" + wRtn.ToString());
                            return false;
                        }

                    }
                    Open = true;
                    if (0 < P32C32.InTotalBoardCount)
                    {
                        for (int i = 0; i < P32C32.InTotalBoardCount; i++) IOInit(i);
                    }

                    if (0 < C64.OutTotalBoardCount)
                    {
                        for (int i = 0; i < C64.OutTotalBoardCount; i++) IOInitC64(i);
                    }
                }
                catch (Exception Msg)
                {
                    MessageBox.Show(Msg.Message + "\n" + Msg.StackTrace);
                }
            }
            finally
            {
            }
#endif
            return true;
        }


        public int GetC64OutPortLength
        {
            get { return C64.OutTotalBoardCount; }
        }


        /// <summary>
        /// I/O 제어 보드를 오픈 했는지 여부를 같는다.
        /// </summary>
        public bool isOpen
        {
            get { return Open; }
        }

        /// <summary>
        /// I/O 제어 보드를 닫는다.
        /// </summary>
        public void CloseIO()
        {
#if PROGRAM_RUNNING


            try
            {
                try
                {
                    //PISODIO.DriverClose();    
                    UniDAQ.Ixud_DriverClose();
                }
                catch (Exception Msg)
                {
                    MessageBox.Show(Msg.Message + "\n" + Msg.StackTrace);
                }
            }
            finally
            {
            }
#endif
            return;
        }

        /// <summary>
        /// I/O 입력 데이타 값을 읽어 온다.
        /// </summary>
        public byte[] GetInData(int Board)
        {
            return P32C32.InData[Board]; 
        }

        /// <summary>
        /// 지정 포트가 동작 되었는지 읽어 온다.
        /// </summary>
        /// <param name="Pos"></param>
        /// <returns></returns>
        public bool inportb(short Pos)
        {
            __IOData__ Value = IOCheck(Pos);

#if PROGRAM_RUNNING         
            bool Data;
            uint DIVal = 0;

            wInitialCode = UniDAQ.Ixud_ReadDI(P32C32.In[Value.Card], (ushort)Value.Pos, ref DIVal);

            Data = false;
            if (((byte)~DIVal & Value.Data) == Value.Data) Data = true;
            return Data;
#else
            return false;
#endif
        }

        public enum AD_RANGE
        {
            /// <summary>
            /// 00:Bipolar +/-   10.0000(V)
            /// </summary>
            Bipolar_P_M_10V,
            /// <summary>
            /// 01:Bipolar +/-     5.0000(V)
            /// </summary>
            Bipolar_P_M_5V,
            /// <summary>
            /// 02:Bipolar +/-     2.5000(V)
            /// </summary>
            Bipolar_P_M_25V,
            /// <summary>
            /// 03:Bipolar +/-     1.2500(V)
            /// </summary>
            Bipolar_P_M_125V,
            /// <summary>
            /// 04:Bipolar +/-     0.6250(V)
            /// </summary>
            Bipolar_P_M_0625V,
            /// <summary>
            /// 05:Bipolar +/-     0.3125(V)
            /// </summary>
            Bipolar_P_M_03125V,
            /// <summary>
            /// 06:Bipolar +/-     0.5000(V)
            /// </summary>
            Bipolar_P_M_05V,
            /// <summary>
            /// 07:Bipolar +/-     0.0500(V)
            /// </summary>
            Bipolar_P_M_005,
            /// <summary>
            /// 08:Bipolar +/-     0.0050(V)
            /// </summary>
            Bipolar_P_M_0005V,
            /// <summary>
            /// 09:Bipolar +/-     1.0000(V)
            /// </summary>
            Bipolar_P_M_1V,
            /// <summary>
            /// 10:Bipolar +/-     0.1000(V)
            /// </summary>
            Bipolar_P_M_01V,
            /// <summary>
            /// 11:Bipolar +/-     0.0100(V)
            /// </summary>
            Bipolar_P_M_001V,
            /// <summary>
            /// 12:Bipolar +/-     0.0010(V)
            /// </summary>
            Bipolar_P_M_0001V,
            /// <summary>
            /// 13:Unipolar 0 ~20.0000(V)
            /// </summary>
            Unipolar_20V,
            /// <summary>
            /// 14:Unipolar 0 ~10.0000(V)
            /// </summary>
            Unipolar_10V,
            /// <summary>
            /// 15:Unipolar 0 ~  5.0000(V)
            /// </summary>
            Unipolar_5V,
            /// <summary>
            /// 16:Unipolar 0 ~  2.5000(V)
            /// </summary>
            Unipolar_25V,
            /// <summary>
            /// 17:Unipolar 0 ~  1.2500(V)
            /// </summary>
            Unipolar_125V,
            /// <summary>
            /// 18:Unipolar 0 ~  0.6250(V)
            /// </summary>
            Unipolar_0625V,
            /// <summary>
            /// 19:Unipolar 0 ~  1.0000(V)
            /// </summary>
            Unipolar_1V,
            /// <summary>
            /// 20:Unipolar 0 ~  0.1000(V)
            /// </summary>
            Unipolar_01V,
            /// <summary>
            /// 21:Unipolar 0 ~  0.0100(V)
            /// </summary>
            Unipolar_001V,
            /// <summary>
            /// 22:Unipolar 0 ~  0.0010(V)
            /// </summary>
            Unipolar_0001V

        }

        public float ReadAd(short Port)
        {
            ushort wRtn = UniDAQ.Ixud_PollingAI((ushort)(AD813.ADBoard[Port / AD813.ADPort[0]]), (ushort)(Port % AD813.ADPort[0]), (ushort)AD_RANGE.Bipolar_P_M_10V, dwDataNum, fBuf);

            float Value;
            if (wRtn == 0)
            {
                Value = (fBuf[0] + fBuf[1]) / 2.0F;
            }
            else
            {
                Value = 0;
            }
            return Value;
        }

        /// <summary>
        /// 인수로 넘어온 데이타 중 지정 포트가 동작 되었는지 알아 낸다.
        /// </summary>
        /// <param name="Pos"></param>
        /// <param name="DIVal"></param>
        /// <returns></returns>
        public bool inportb(short Pos, byte[] DIVal)
        {
            __IOData__ Value = IOCheck(Pos);

#if PROGRAM_RUNNING         
            bool Data;
            //uint DIVal = 0;

            //wInitialCode = UniDAQ.Ixud_ReadDI(P32ToIn[Value.Card], (ushort)Value.Pos, ref DIVal);

            Data = false;
            if (((byte)DIVal[Value.Pos] & Value.Data) == Value.Data) Data = true;
            return Data;
#else
            return false;
#endif

        }
        /// <summary>
        /// 채널별 I/O 입력을 읽어온다.
        /// </summary>
        /// <param name="Ch"></param>
        /// <returns></returns>
        public byte inportbdata(short Ch)
        {
#if PROGRAM_RUNNING
            byte Data;
            uint DIVal = 0;

            wInitialCode = UniDAQ.Ixud_ReadDI(P32C32.In[Ch / 4], (ushort)(Ch % 4), ref DIVal);

            Data = (byte)~DIVal;
            return Data;
#else
            return (byte)0x00;
#endif
        }

        /// <summary>
        /// P32C32 지정된 포트를 On/Off 한다.
        /// </summary>
        /// <param name="Pos"></param>
        /// <param name="OnOff"></param>
        public void outportb(int Pos, bool OnOff)
        {
#if PROGRAM_RUNNING
            __IOData__ Value = IOCheck(Pos);

            if (OnOff == true)
                P32C32.OutData[Value.Card][Value.Pos] |= Value.Data;
            else P32C32.OutData[Value.Card][Value.Pos] &= (byte)~Value.Data;

            outportb(Value.Card, Value.Pos, P32C32.OutData[Value.Card][Value.Pos]);
#endif
            return;
        }



        /// <summary>
        /// P32C32 지정된 포트가 동작하고 있는지 읽어 온다.
        /// </summary>
        /// <param name="Pos"></param>
        /// <returns></returns>
        public bool OutputCheck(short Pos)
        {
            __IOData__ Value = IOCheck(Pos);

            if ((P32C32.OutData[Value.Card][Value.Pos] & Value.Data) == Value.Data)
                return true;
            else return false;
        }
        /// <summary>
        /// P32C32 지정된 카드에 지정된 포트로 출력을 On/Off 한다.
        /// </summary>
        /// <param name="Card"></param>
        /// <param name="Port"></param>
        /// <param name="Value"></param>
        public void outportb(int Card, short Port, byte Value)
        {
#if PROGRAM_RUNNING

            try
            {
                try
                {
                    uint DoVal;

                    DoVal = (uint)Value;
                    wInitialCode = UniDAQ.Ixud_WriteDO(P32C32.Out[Card], (ushort)Port, DoVal);
                }
                catch (Exception Msg)
                {
                    MessageBox.Show(Msg.Message + "\n" + Msg.StackTrace);
                }
            }
            finally
            {
            }
#endif
            return;
        }

        /// <summary>
        /// C64 지정된 카드에 지정된 포트로 출력을 On/Off 한다.
        /// </summary>
        /// <param name="Card"></param>
        /// <param name="Port"></param>
        /// <param name="Value"></param>
        public void C64outportb(int Card, short Port, byte Value)
        {
#if PROGRAM_RUNNING

            try
            {
                try
                {
                    uint DoVal;

                    DoVal = (uint)Value;
                    wInitialCode = UniDAQ.Ixud_WriteDO(C64.Out[Card], (ushort)Port, DoVal);
                }
                catch (Exception Msg)
                {
                    MessageBox.Show(Msg.Message + "\n" + Msg.StackTrace);
                }
            }
            finally
            {
            }
#endif
            return;
        }
        

        /// <summary>
        /// P32C32 지정된 포트의 I/O 위치를 알아낸다.
        /// </summary>
        /// <param name="Pos"></param>
        /// <returns></returns>
        public __IOData__ IOCheck(int Pos)
        {
            __IOData__ value = new __IOData__();

            int OPos = Pos / 8;
            byte Data = (byte)(0x01 << (Pos % 8));

            value.Card = (short)(OPos / 4);
            value.Pos = (short)(OPos % 4);
            value.Data = Data;

            return value;
        }

        /// <summary>
        /// P32C32 출력 초기화
        /// </summary>
        public void IOInit(int Board)
        {
            //outportb(IO_OUT.특성_UIP_SERVO_SELECT, false);

            P32C32.OutData[Board][0] = 0x00;
            P32C32.OutData[Board][1] = 0x00;
            P32C32.OutData[Board][2] = 0x00;
            P32C32.OutData[Board][3] = 0x00;

            outportb(Board, 0, P32C32.OutData[Board][0]);
            outportb(Board, 1, P32C32.OutData[Board][1]);
            outportb(Board, 2, P32C32.OutData[Board][2]);
            outportb(Board, 3, P32C32.OutData[Board][3]);
            return;
        }

        public void IOInitC64(int Board)
        {
            //outportb(IO_OUT.특성_UIP_SERVO_SELECT, false);

            C64.OutData[Board][0] = 0x00;
            C64.OutData[Board][1] = 0x00;
            C64.OutData[Board][2] = 0x00;
            C64.OutData[Board][3] = 0x00;
            C64.OutData[Board][4] = 0x00;
            C64.OutData[Board][5] = 0x00;
            C64.OutData[Board][6] = 0x00;
            C64.OutData[Board][7] = 0x00;

            C64outportb(Board, 0, C64.OutData[Board][0]);
            C64outportb(Board, 1, C64.OutData[Board][1]);
            C64outportb(Board, 2, C64.OutData[Board][2]);
            C64outportb(Board, 3, C64.OutData[Board][3]);
            C64outportb(Board, 4, C64.OutData[Board][4]);
            C64outportb(Board, 5, C64.OutData[Board][5]);
            C64outportb(Board, 6, C64.OutData[Board][6]);
            C64outportb(Board, 7, C64.OutData[Board][7]);
            return;
        }

        /// <summary>
        /// P32C32 출력 초기화
        /// </summary>
        //public void C64IOInit()
        //{
        //    //outportb(IO_OUT.특성_UIP_SERVO_SELECT, false);

        //    C64.OutData[0] = 0x00;
        //    C64.OutData[1] = 0x00;
        //    C64.OutData[2] = 0x00;
        //    C64.OutData[3] = 0x00;
        //    C64.OutData[4] = 0x00;
        //    C64.OutData[5] = 0x00;
        //    C64.OutData[6] = 0x00;
        //    C64.OutData[7] = 0x00;

        //    C64outportb(0, 0, 0x00);
        //    C64outportb(0, 1, 0x00);
        //    C64outportb(0, 2, 0x00);
        //    C64outportb(0, 3, 0x00);
        //    C64outportb(0, 4, 0x00);
        //    C64outportb(0, 5, 0x00);
        //    C64outportb(0, 6, 0x00);
        //    C64outportb(0, 7, 0x00);
        //    return;
        //}

        

        ~IOControl()
        {
        }
    }
}