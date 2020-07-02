//#define PROGRAM_TEST

#if !PROGRAM_TEST
#define PROGRAM_RUNNING
#endif

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using System.Windows.Forms;
using System.Threading;

/*
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PISODIO_Ns;
using PISO_Ns;
using NiCAN;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
*/

namespace ODS
{
    public class mStatus
    {
        public const uint STATUS_CW_LIMITE = 0x000001;
        public const uint STATUS_CCW_LIMITE = 0x000002;
        public const uint STATUS_CW_STOP_STATUS = 0x000004;
        public const uint STATUS_CCW_STOP_STATUS = 0x000008;
        public const uint STATUS_ALARM = 0x000010;
        public const uint STATUS_IN_POS = 0x000020;
        public const uint STATUS_ESTOP = 0x000040;
        public const uint STATUS_ORG = 0x000080;
        public const uint STATUS_Z_SIGNAL = 0x000100;
        public const uint STATUS_ECUP_TERMINAL = 0x000200;
        public const uint STATUS_ECDN_TERMINAL = 0x000400;
        public const uint STATUS_EXPP_TERMINAL = 0x000800;
        public const uint STATUS_EXMP_TERMINAL = 0x001000;
        public const uint STATUS_SQSTR1_TERMINAL = 0x002000;
        public const uint STATUS_SQSTR2_TERMINAL = 0x004000;
        public const uint STATUS_SQSTP1_TERMINAL = 0x008000;
        public const uint STATUS_SQSTP2_TERMINAL = 0x010000;
        public const uint STATUS_MODE_TERMINAL = 0x020000;

        public const short X_SELECT = 0;
        public const short Y_SELECT = 1;
        public const short Z_SELECT = 2;
    }

    public class AxtMotion
    {
        private MyInterface mControl;
        private stDeviceInfo m_stDeviceInfo;
        //private short m_nAxis = -1;	// 현재 축번호
        Stopwatch STOP_WATCH;

        private const int SERVO_ON = 0;
        private const int ALARM_RESET_ON = 1;
        private const uint ALARM_RESET_OFF = 1;
        private const uint HIGH_ACTIVE = 0x01;
        private const uint LOW_ACTIVE = 0x00;
        private double XOneCycleToPulse;
        private double YOneCycleToPulse;
        private double ZOneCycleToPulse;
        private double XOneCycleToStroke;
        private double YOneCycleToStroke;
        private double ZOneCycleToStroke;

        private int IAxisCount = 0;
        //private IntPtr handle;

        //private enum DETECT_DESTINATION_SIGNAL
        //{
        //     PElmNegativeEdge    = 0x0,        // +Elm(End limit) 하강 edge
        //     NElmNegativeEdge    = 0x1,        // -Elm(End limit) 하강 edge
        //     PSlmNegativeEdge    = 0x2,        // +Slm(Slowdown limit) 하강 edge
        //     NSlmNegativeEdge    = 0x3,        // -Slm(Slowdown limit) 하강 edge
        //     In0DownEdge         = 0x4,        // IN0(ORG) 하강 edge
        //     In1DownEdge         = 0x5,        // IN1(Z상) 하강 edge
        //     In2DownEdge         = 0x6,        // IN2(범용) 하강 edge
        //     In3DownEdge         = 0x7,        // IN3(범용) 하강 edge
        //     PElmPositiveEdge    = 0x8,        // +Elm(End limit) 상승 edge
        //     NElmPositiveEdge    = 0x9,        // -Elm(End limit) 상승 edge
        //     PSlmPositiveEdge    = 0xa,        // +Slm(Slowdown limit) 상승 edge
        //     NSlmPositiveEdge    = 0xb,        // -Slm(Slowdown limit) 상승 edge
        //     In0UpEdge           = 0xc,        // IN0(ORG) 상승 edge
        //     In1UpEdge           = 0xd,        // IN1(Z상) 상승 edge
        //     In2UpEdge           = 0xe,        // IN2(범용) 상승 edge
        //     In3UpEdge           = 0xf         // IN3(범용) 상승 edge
        //}

        //public enum PulseType
        //{

        //    OneHighLowHigh  = 0,
        //    OneHighHighLow  = 1,
        //    OneLowLowHigh   = 2,
        //    OneLowHighLow   = 3,
        //    TwoCcwCwHigh    = 4,
        //    TwoCcwCwLow     = 5,
        //    TwoCwCcwHigh    = 6,
        //    TwoCwCcwLow     = 7            
        //}

        public struct stDeviceInfo
        {
            public int nAxisCount;
            public int nFirstAxisNo;
            public int nAxtBoardNo;
            public int nModulePos;
            public uint uModuleID;
            public int nAxisPos;
        }

        public AxtMotion()
        {
            STOP_WATCH = new Stopwatch();
            STOP_WATCH.Start();
        }

        public AxtMotion(MyInterface mControl)
        {
            STOP_WATCH = new Stopwatch();
            STOP_WATCH.Start();
            this.mControl = mControl;
        }

        public bool Init(double YOneCycleToPulse, double ZOneCycleToPulse, double XOneCycleToPulse, float YOneCycleToStroke, float ZOneCycleToStroke, float XOneCycleToStroke)
        {
#if PROGRAM_RUNNING
            //++
            // 라이브러리 초기화

            this.YOneCycleToPulse = YOneCycleToPulse;
            this.ZOneCycleToPulse = ZOneCycleToPulse;
            this.XOneCycleToPulse = XOneCycleToPulse;

            this.YOneCycleToStroke = YOneCycleToStroke;
            this.ZOneCycleToStroke = ZOneCycleToStroke;
            this.XOneCycleToStroke = XOneCycleToStroke;

            if (CAXL.AxlOpen(7) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)				// 통합 라이브러리가 사용 가능하지 (초기화가 되었는지)를 확인한다
            {
                //if (CAXL.AxlOpenNoReset(7) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)		// 통합 라이브러리를 초기화 한다
                //{
                //    MessageBox.Show("라이브러리 초기화 실패 입니다. 프로그램을 다시 실행 시켜 주세요");

                //    return false;
                //}
                uint uStatus = 0;

                if (CAXM.AxmInfoIsMotionModule(ref uStatus) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)// 모션 모듈이 있는지 확인한다.
                {
                    if (uStatus != (uint)AXT_EXISTENCE.STATUS_EXIST)
                    {
                        MessageBox.Show("모션 모듈을 찾을수 없습니다.");
                        return false;
                    }

                    if (CAXM.AxmInfoGetAxisCount(ref IAxisCount) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)// 모터가 몇개 연결되어 있는지 읽어 온다.
                    {
                        if (IAxisCount == 0)
                        {
                            MessageBox.Show("연결되어 있는 서보 모터를 찾을 수 없습니다.");
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("연결되어 있는 서보모터 정보를 읽어 오는 동안 오류가 발생 하였습니다.");
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("모션 모듈 정보를 읽어 오는 동안 오류가 발생하였습니다.");
                    return false;
                }
            }
            else
            {
                MessageBox.Show("라이브러리 초기화 실패 입니다. 프로그램을 다시 실행 시켜 주세요");
                return false;
            }


            // 사용하시는 베이스보드에 맞추어 Device를 Open하면 됩니다.
            // BUSTYPE_ISA					:	0
            // BUSTYPE_PCI					:	1
            // BUSTYPE_VME					:	2
            // BUSTYPE_CPCI(Compact PCI)	:	3



            //if (CAXL.AxlIsOpened() != 0)			// 지정한 버스(PCI)가 초기화 되었는지를 확인한다
            //{
            //    uint uStatus = 0;

            //    CAXM.AxmInfoIsMotionModule(ref uStatus);// 모션 모듈이 있는지 확인한다.
            //    if (uStatus != (uint)AXT_EXISTENCE.STATUS_EXIST)			
            //    {
            //        MessageBox.Show("모션 모듀을 찾을수 없습니다.");

            //        return false;
            //    }
            //}
            //CAXM.AxmInfoGetAxisCount(ref IAxisCount);// 모터가 몇개 연결되어 있는지 읽어 온다.
            //if (0 < IAxisCount)				
            //{
            //    if (CAxtCAMCFS20.InitializeCAMCFS20(1) == 0)		// 모듈을 초기화한다. 열려있는 모든베이스보드에서 모듈을 검색하여 초기화한다
            //    {
            //        MessageBox.Show("모듈 초기화 실패 입니다. 확인 후 다시 실행 시켜 주세요");
            //        return false;
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("연결되어 있는 서보 모터를 찾을 수 없습니다.");
            //    return false;
            //}


            m_stDeviceInfo.nFirstAxisNo = 0;
            if (CAXM.AxmInfoGetAxis(
                m_stDeviceInfo.nFirstAxisNo,
                ref m_stDeviceInfo.nAxtBoardNo,
                ref m_stDeviceInfo.nModulePos,
                ref m_stDeviceInfo.uModuleID) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {
                if (CAXM.AxmInfoGetBoardFirstAxisNo(m_stDeviceInfo.nAxtBoardNo, m_stDeviceInfo.nModulePos, ref m_stDeviceInfo.nAxisPos) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                {
                    MessageBox.Show("모션 보드 사용준비중 에러가 발생하였습니다.");
                    return false;
                }
            }

#endif
            return true;
        }

        public void AxtClose()
        {
#if PROGRAM_RUNNING
            CAXL.AxlClose();
#endif
            return;
        }


        public int AxtReadMaxMotor()
        {
#if PROGRAM_RUNNING
            int IAxisCount = 0;
            if (CAXM.AxmInfoGetAxisCount(ref IAxisCount) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)// 모터가 몇개 연결되어 있는지 읽어 온다.
                m_stDeviceInfo.nAxisCount = IAxisCount;
            else m_stDeviceInfo.nAxisCount = 0;

            return m_stDeviceInfo.nAxisCount;
#else
            return 0;
#endif
        }

        private bool GetRunAxis(short m_nAxis)
        {
            return true;
        }

        public void AlarmOperating(short m_nAxis, bool Flag)
        {
#if PROGRAM_RUNNING

            if (GetRunAxis(m_nAxis) == false) return;

            if (Flag == true)
            {
                if (CAXM.AxmSignalServoAlarmReset(m_nAxis, 0x01) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                {
                    MessageBox.Show("No1 Error - 알람 리셋중 에러가 발생하였습니다.");
                };
            }
            else
            {
                if (CAXM.AxmSignalServoAlarmReset(m_nAxis, 0x0) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                {
                    MessageBox.Show("No2 Error - 알람 리셋중 에러가 발생하였습니다.");
                };
            }
#endif
            return;
        }

        public void AxtJogDownMove(short m_nAxis, double Speed)
        {
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return;
            //AxtSpeedSet(m_nAxis, Speed);
            if (CAXM.AxmMoveVel(m_nAxis, -Speed, Speed * 2, Speed * 2) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {
                MessageBox.Show("조그모드에서 하강 동작 설정 중 에러 입니다.");
            }
#endif
            return;
        }

        public void AxtJogUpMove(short m_nAxis, double Speed)
        {
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return;
            //AxtSpeedSet(m_nAxis, Speed);
            if (CAXM.AxmMoveVel(m_nAxis, Speed, Speed * 2, Speed * 2) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {
                MessageBox.Show("조그모드에서 상승 동작 설정 중 에러 입니다.");
            }
#endif
            return;
        }
        public void AxtJogLeftMove(short m_nAxis, double Speed)
        {
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return;
            //AxtSpeedSet(m_nAxis, Speed);
            if (CAXM.AxmMoveVel(m_nAxis, Speed, Speed * 2, Speed * 2) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {
                MessageBox.Show("조그모드에서 좌측 동작 설정 중 에러 입니다.");
            }
#endif
            return;
        }

        public void AxtJogRightMove(short m_nAxis, double Speed)
        {
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return;
            //AxtSpeedSet(m_nAxis, Speed);
            if (CAXM.AxmMoveVel(m_nAxis, -Speed, Speed * 2, Speed * 2) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {
                MessageBox.Show("조그모드에서 우측 동작 설정 중 에러 입니다.");
            }
#endif
            return;
        }
        public void AxtJogForwordMove(short m_nAxis, double Speed)
        {
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return;
            //AxtSpeedSet(m_nAxis, Speed);
            if (CAXM.AxmMoveVel(m_nAxis, Speed, Speed * 2, Speed * 2) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {
                MessageBox.Show("조그모드에서 전진 동작 설정 중 에러 입니다.");
            }
#endif
            return;
        }

        public void AxtJogBackwordMove(short m_nAxis, double Speed)
        {
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return;
            //AxtSpeedSet(m_nAxis, Speed);
            if (CAXM.AxmMoveVel(m_nAxis, -Speed, Speed * 2, Speed * 2) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {
                MessageBox.Show("조그모드에서 훈진 동작 설정 중 에러 입니다.");
            }
#endif
            return;
        }

        public void AxtPositionMove(short m_nAxis, double Speed, double Position)
        {
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return;
            //CAxtCAMCFS20.CFS20start_move(m_nAxis,Position * -1.0, Speed, Speed * 5);

            //if (300 < Speed) Speed = 300;
            //if (300 < Position) Position = 300;
            //AxtSpeedSet(m_nAxis, Speed);
            //CAXM.AxmMoveToAbsPos(m_nAxis, Position, Speed, Speed * 2, Speed * 2);
            CAXM.AxmMoveStartPos(m_nAxis, Position, Speed, Speed * 1, Speed * 4);
#endif
            return;
        }

        //        public void AxtSpeedSet(short m_nAxis, double Speed)
        //        {
        //#if PROGRAM_RUNNING
        //            CAxtCAMCFS20.CFS20set_max_speed(m_nAxis, Speed);            
        //#endif
        //            return;
        //        }

        public uint AxtErrorRead(short m_nAxis)
        {
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return 0;
            return CAXM.AxmInfoGetAxisStatus(m_nAxis);
#else
            return 0;
#endif
        }

        public uint AxtReadStatus(short m_nAxis)
        {
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return 0;
            uint Status = 0;
            if (CAXM.AxmStatusReadMechanical(m_nAxis, ref Status) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) Status = 0;
            return Status;
#else
            return 0;
#endif
        }

        public void Axt_EStop(short m_nAxis)
        {
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return;
            CAXM.AxmMoveEStop(m_nAxis);
#endif
            return;
        }

        public void Axt_NStop(short m_nAxis)
        {
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return;
            CAXM.AxmMoveSStop(m_nAxis);
#endif
            return;
        }


        public void Axt_Alarm_Reset(short m_nAxis)
        {
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return;
            Axt_ServoOnOff(m_nAxis, true);
            delay(100);
            CAXM.AxmSignalServoAlarmReset(m_nAxis, ALARM_RESET_ON);
            delay(500);
            CAXM.AxmSignalServoAlarmReset(m_nAxis, ALARM_RESET_OFF);
#endif
            return;
        }

        public void Axt_OutportBit(short m_nAxis, byte Pos, bool OnOff)
        {
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return;
            if (OnOff == true)
                CAXM.AxmSignalWriteOutputBit(m_nAxis, Pos, 1);
            else CAXM.AxmSignalWriteOutputBit(m_nAxis, Pos, 0);
#endif
            return;
        }

        public void Axt_ServoOnOff(short m_nAxis, bool OnOff)
        {
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return;
            if (OnOff == true)
                CAXM.AxmSignalWriteOutputBit(m_nAxis, SERVO_ON, 1);
            else CAXM.AxmSignalWriteOutputBit(m_nAxis, SERVO_ON, 0);
#endif
            return;
        }

        public bool AxtIsServoOn(short m_nAxis)
        {
            if (GetRunAxis(m_nAxis) == false) return true;
            uint value = 0;

            if (CAXM.AxmSignalIsServoOn(m_nAxis, ref value) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return false;

            if (value == 1)
                return true;
            else return false;
        }


        public void Axt_CWCCW_LimitSet(short m_nAxis, bool CwHighActive, bool CcwHighActive)
        {
#if PROGRAM_RUNNING

            uint Cw;
            uint Ccw;
            uint uStopMode = 0; // 0 - 급정지, 1 - 감속 정지

            if (CwHighActive == true)
                Cw = HIGH_ACTIVE;
            else Cw = LOW_ACTIVE;

            if (CcwHighActive == true)
                Ccw = HIGH_ACTIVE;
            else Ccw = LOW_ACTIVE;

            if (GetRunAxis(m_nAxis) == false)
                return;
            else CAXM.AxmSignalSetLimit(m_nAxis, uStopMode, Cw, Ccw);
#endif
            return;
        }            

        public void AxtMotorInit(int ServoXDir, int ServoYDir, int ServoZDir)
        {
#if PROGRAM_RUNNING
            //각 축에 대한 리미트 센서의 Low/High Active 값을 설정한다.

            Axt_ServoOnOff(mStatus.X_SELECT, true);
            Axt_ServoOnOff(mStatus.Y_SELECT, true);
            Axt_ServoOnOff(mStatus.Z_SELECT, true);

            AlarmOperating(mStatus.X_SELECT, true);
            AlarmOperating(mStatus.Y_SELECT, true);

            delay(500);
            AlarmOperating(mStatus.X_SELECT, false);
            AlarmOperating(mStatus.Y_SELECT, false);
            AlarmOperating(mStatus.Z_SELECT, false);

            Axt_CWCCW_LimitSet(mStatus.X_SELECT, false, false);
            Axt_CWCCW_LimitSet(mStatus.Y_SELECT, false, false);
            Axt_CWCCW_LimitSet(mStatus.Z_SELECT, false, false);


            CAXM.AxmSignalSetServoAlarm(mStatus.X_SELECT, LOW_ACTIVE);
            CAXM.AxmSignalSetServoAlarm(mStatus.Y_SELECT, LOW_ACTIVE);
            CAXM.AxmSignalSetServoAlarm(mStatus.Z_SELECT, LOW_ACTIVE);

            //e stop 신호 입력시 처리
            uint uStopMode = 0; // 0 - emergency stop , 1 - Slow down stop
            CAXM.AxmSignalSetStop(mStatus.X_SELECT, uStopMode, LOW_ACTIVE);
            CAXM.AxmSignalSetStop(mStatus.Y_SELECT, uStopMode, LOW_ACTIVE);
            CAXM.AxmSignalSetStop(mStatus.Z_SELECT, uStopMode, LOW_ACTIVE);

            uint UnitMode = 0; // 0 - unit/sec1 , 1 - sec
            CAXM.AxmMotSetAccelUnit(mStatus.X_SELECT, UnitMode);
            CAXM.AxmMotSetAccelUnit(mStatus.Y_SELECT, UnitMode);
            CAXM.AxmMotSetAccelUnit(mStatus.Z_SELECT, UnitMode);

            SetAbsMode(mStatus.X_SELECT);
            SetAbsMode(mStatus.Y_SELECT);
            SetAbsMode(mStatus.Z_SELECT);

            AxtPulseMethodeSet(mStatus.X_SELECT, (byte)ServoXDir);
            AxtPulseMethodeSet(mStatus.Y_SELECT, (byte)ServoYDir);
            AxtPulseMethodeSet(mStatus.Z_SELECT, (byte)ServoZDir);

            UnitPulse_StartStopSpeed(mStatus.X_SELECT, 0);
            UnitPulse_StartStopSpeed(mStatus.Y_SELECT, 0);
            UnitPulse_StartStopSpeed(mStatus.Z_SELECT, 0);


            AxtEncoderMethod(mStatus.X_SELECT, 3); //4채배
            AxtEncoderMethod(mStatus.Y_SELECT, 3); //4채배
            AxtEncoderMethod(mStatus.Z_SELECT, 3); //4채배

            AxtOriginSpeedSet(mStatus.X_SELECT, 20);
            AxtOriginSpeedSet(mStatus.Y_SELECT, 20);
            AxtOriginSpeedSet(mStatus.Z_SELECT, 20);
#endif
            return;
        }

        public void MoveToPosAndSpeedChange(short m_nAxis, double Pos, double Speed, double TargetPos, double TargetSpeed)
        {
            if (GetRunAxis(m_nAxis) == false) return;

            if (OriginMoveEnd(m_nAxis) == false) //모터가 구동중일때만 속도 변화를 준다.
            {
                CAXM.AxmOverrideVelAtPos(m_nAxis, Pos, Speed, Speed * 4, Speed * 4, TargetPos, TargetSpeed, (int)AXT_MOTION_TRIGGER_MODE.ABS_POS_MODE);
                //if (CAXM.AxmOverridePos(m_nAxis, Pos) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return;
                //if (CAXM.AxmOverrideVel(m_nAxis, Speed) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return;
            }
            return;
        }

        public void AxtPulseMethodeSet(short m_nAxis, byte Value)
        {
            /*
            "OneHighLowHigh",    0
            "OneHighHighLow",    1
            "OneLowLowHigh",     2
            "OneLowHighLow",     3
            "TwoCcwCwHigh",      4
            "TwoCcwCwLow",       5
            "TwoCwCcwHigh",      6 
            "TwoCwCcwLow",       7
            "TwoPhase",          8
            "TwoPhaseReverse     9
             */

            /*
            OneHighLowHigh,                // 1펄스 방식, PULSE(Active High), 정방향(DIR=Low)  / 역방향(DIR=High)
            OneHighHighLow,                 // 1펄스 방식, PULSE(Active High), 정방향(DIR=High) / 역방향(DIR=Low)
            OneLowLowHigh,                  // 1펄스 방식, PULSE(Active Low),  정방향(DIR=Low)  / 역방향(DIR=High)
            OneLowHighLow,                  // 1펄스 방식, PULSE(Active Low),  정방향(DIR=High) / 역방향(DIR=Low)
            TwoCcwCwHigh,                   // 2펄스 방식, PULSE(CCW:역방향),  DIR(CW:정방향),  Active High     
            TwoCcwCwLow,                    // 2펄스 방식, PULSE(CCW:역방향),  DIR(CW:정방향),  Active Low     
            TwoCwCcwHigh,                   // 2펄스 방식, PULSE(CW:정방향),   DIR(CCW:역방향), Active High
            TwoCwCcwLow,                    // 2펄스 방식, PULSE(CW:정방향),   DIR(CCW:역방향), Active Low
            TwoPhase,                       // 2상(90' 위상차),  PULSE lead DIR(CW: 정방향), PULSE lag DIR(CCW:역방향)
            TwoPhaseReverse                 // 2상(90' 위상차),  PULSE lead DIR(CCW: 정방향), PULSE lag DIR(CW:역방향)
            */

#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return;
            CAXM.AxmMotSetPulseOutMethod(m_nAxis, Value);
#endif
            return;
        }

        public byte AxtPulseMethodeGet(short m_nAxis)
        {
            uint value = 0;
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return 0;
            if (CAXM.AxmMotGetPulseOutMethod(m_nAxis, ref value) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                return (byte)value;
            else return 0;
#else
            return 0x00;
#endif
        }

        // unit/pulse, Startstop speed setting
        private void UnitPulse_StartStopSpeed(short m_nAxis, short Speed)
        {
            //
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return;
            if (m_nAxis == mStatus.X_SELECT)
            {
                //CAxtCAMCFS20.AxmMotSetMoveUnitPerPulse(m_nAxis, 5.0 , (XOneCycleToPulse * 1.0)); //볼 스크류 리드(1회전 이동거리) , (모터 1회전 펄스수 * 감속비)
                CAXM.AxmMotSetMoveUnitPerPulse(m_nAxis, YOneCycleToStroke, (int)(YOneCycleToPulse * 1.0)); //볼 스크류 리드(1회전 이동거리) , (모터 1회전 펄스수 * 감속비)
            }
            else if (m_nAxis == mStatus.Y_SELECT)
            {
                CAXM.AxmMotSetMoveUnitPerPulse(m_nAxis, ZOneCycleToStroke, (int)(ZOneCycleToPulse * 1.0)); //볼 스크류 리드(1회전 이동거리) , (모터 1회전 펄스수 * 감속비)
            }
            else if (m_nAxis == mStatus.Z_SELECT)
            {
                CAXM.AxmMotSetMoveUnitPerPulse(m_nAxis, XOneCycleToStroke, (int)(XOneCycleToPulse * 1.0)); //볼 스크류 리드(1회전 이동거리) , (모터 1회전 펄스수 * 감속비)
            }
            //0 축에 초기 속도를 1로 설정한다 설정한다 . Default : 1
            //모션이 시작될 때의 초기 속도를 지정, 초기 속도는 모션 구도의 기준점이 되므로 반드시 설정해야 한다.
            //int lAxisNo = 0;
            //double dMinVelocity = 1;

            if (CAXM.AxmMotSetMinVel(m_nAxis, Speed) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {
                MessageBox.Show("초기 속도 설정 오류 입니다.");
            }
#endif
            return;
        }

        private int[] EncoderMethod = { 0, 0, 0, 0 };

        // Encoder input method setting
        public void AxtEncoderMethod(short m_nAxis, byte Value)
        {
            /*
             * UpDonwMode = 0
             * Sqr1Mode = 1     -> 1채배
             * sqr2Mode = 2     -> 2채배
             * sqr4Mode = 3     -> 4채배
            */

            switch (Value)
            {
                case 0:
                    EncoderMethod[m_nAxis] = 1;
                    break;
                case 1:
                    EncoderMethod[m_nAxis] = 1;
                    break;
                case 2:
                    EncoderMethod[m_nAxis] = 2;
                    break;
                case 3:
                    EncoderMethod[m_nAxis] = 4;
                    break;
            }

#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return;
            CAXM.AxmMotSetEncInputMethod(m_nAxis, Value);
#endif
            return;
        }


        public double AxtCommandPosition(short m_nAxis)
        {
#if PROGRAM_RUNNING
            double Pos = 0;
            if (GetRunAxis(m_nAxis) == false) return 0;
            if (CAXM.AxmStatusGetCmdPos(m_nAxis, ref Pos) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return 0;

            return Pos;
#else
            return 0.0;
#endif
        }


        public double AxtReadPosition(short m_nAxis)
        {
#if PROGRAM_RUNNING
            double Pos = 0;
            //double ErrPos = 0;
            if (GetRunAxis(m_nAxis) == false) return 0;

            if (CAXM.AxmStatusGetActPos(m_nAxis, ref Pos) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return 0;
            //if (CAXM.AxmStatusReadPosError(m_nAxis, ref ErrPos) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return 0;

            //return Pos + ErrPos;
            // 엔코더 채배 값에 따라 카운터 값이 변하므로 채배 값으로 엔코더 값을 나누어 주어야 정상적인 좌표가 나온다.

            return Pos / (double)EncoderMethod[m_nAxis];
#else
            return 0.0;
#endif
        }


        public void AxtClearPosition(short m_nAxis)
        {
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return;
            if (CAXM.AxmStatusSetCmdPos(m_nAxis, 0) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return;
            if (CAXM.AxmStatusSetActPos(m_nAxis, 0) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return;
            if (CAXM.AxmStatusSetPosMatch(m_nAxis, 0) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return;
#endif
            return;
        }



        /// <summary>
        /// return value 1 - cw limite, 2 - ccw limite
        ///  
        /// </summary>
        /// <param name="m_nAxis"></param>
        /// <returns></returns>
        public uint AxtReadLimitIn(short m_nAxis)
        {
            uint In;
            uint Cw = 0;
            uint Ccw = 0;

            if (GetRunAxis(m_nAxis) == false) return 0;
            if (CAXM.AxmSignalReadLimit(m_nAxis, ref Cw, ref Ccw) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {
                In = 0x00;
            }
            else
            {
                if (Cw == 1)
                    In = 0x01;
                else In = 0x00;

                if (Ccw == 1)
                    In |= 0x01 << 1;
                else Cw |= 0x00 << 1;
            }

            return In;
        }


        // Home Search 
        public void OriginMove(short m_nAxis)
        {
#if PROGRAM_RUNNING
            if (GetRunAxis(m_nAxis) == false) return;
            if (CAXM.AxmHomeSetStart(m_nAxis) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return;
#endif
            return;
        }
        // Home Search 

        public bool OriginMoveEnd(short m_nAxis)
        {
#if PROGRAM_RUNNING
            uint Result = 0;

            if (GetRunAxis(m_nAxis) == false) return true;
            if (CAXM.AxmHomeGetResult(m_nAxis, ref Result) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {
                return false;
            }
            if (Result == 1)
                return true;
            else return false;
#else
            return true;
#endif
        }

        private bool CheckOverLoad(short m_nAxis)
        {
            if (m_nAxis == mStatus.X_SELECT)
            {
                if (mControl.GetXLimit <= mControl.GetXLoad)
                {
                    Axt_EStop(m_nAxis);
                    return true;
                }
            }
            else if (m_nAxis == mStatus.Y_SELECT)
            {
                if (mControl.GetYLimit <= mControl.GetYLoad)
                {
                    Axt_EStop(m_nAxis);
                    return true;
                }
            }
            else
            {
                if (mControl.GetZLimit <= mControl.GetZLoad)
                {
                    Axt_EStop(m_nAxis);
                    return true;
                }
            }
            return false;
        }

        public bool OriginSearch1(short m_nAxis)
        {
            //bool Flag = true;
#if PROGRAM_RUNNING
            //double dVelocity = 10;
            //double dAccel = 10 * 2;

            double[] dVelocity = new double[3];
            double[] dAccel = new double[3];


            if(AxtReadAlarm(m_nAxis) == true)
            {
                Axt_Alarm_Reset(m_nAxis);
                delay(500);
            }

            if (m_nAxis == mStatus.X_SELECT)
            {
                dVelocity[0] = 50;
                dVelocity[1] = 10;
                dVelocity[2] = 2;

                dAccel[0] = dVelocity[0] * 2;
                dAccel[1] = dVelocity[1] * 2;
                dAccel[2] = dVelocity[2] * 2;
            }
            else if (m_nAxis == mStatus.Y_SELECT)
            {
                dVelocity[0] = 10;
                dVelocity[1] = 5;
                dVelocity[2] = 2;

                dAccel[0] = dVelocity[0] * 2;
                dAccel[1] = dVelocity[1] * 2;
                dAccel[2] = dVelocity[2] * 2;
            }
            else //가로축
            {
                dVelocity[0] = 10;
                dVelocity[1] = 5;
                dVelocity[2] = 2;

                dAccel[0] = dVelocity[0] * 2;
                dAccel[1] = dVelocity[1] * 2;
                dAccel[2] = dVelocity[2] * 2;

            }

            // UpEdge 와 DownEdge 의 차이는 센서에 따라 LOW 에서 HIGH로 올라가느냐 HIGH 에서 LOW로 떨어지느냐의 차이이다.
            //1 = 급 정지
            //0 -  감속 정지
            int ISignalMethod = 1;

            if (CAXM.AxmMoveSignalSearch(m_nAxis, -dVelocity[0], dAccel[0], (int)AXT_MOTION_HOME_DETECT.HomeSensor, (int)AXT_DIO_EDGE.DOWN_EDGE, ISignalMethod) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return false;
            delay(100);
            // 구동이 완료 될때까지 대기
            if (m_nAxis != mStatus.X_SELECT)
            {
                while (AxtMovingEndCheck(m_nAxis) == false) { delay(1); }
            }
            else
            {
                while (AxtMovingEndCheck(m_nAxis) == false) { delay(1); if (CheckOverLoad(m_nAxis) == true) return false; }
            }

            if (CAXM.AxmMoveSignalSearch(m_nAxis, dVelocity[1], dAccel[1], (int)AXT_MOTION_HOME_DETECT.HomeSensor, (int)AXT_DIO_EDGE.UP_EDGE, ISignalMethod) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return false;
            delay(100);
            // 구동이 완료 될때까지 대기
            while (AxtMovingEndCheck(m_nAxis) == false) { delay(1); if (CheckOverLoad(m_nAxis) == true) return false; }

            if (CAXM.AxmMoveSignalSearch(m_nAxis, -dVelocity[2], dAccel[2], (int)AXT_MOTION_HOME_DETECT.HomeSensor, (int)AXT_DIO_EDGE.DOWN_EDGE, ISignalMethod) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return false;
            delay(100);
            // 구동이 완료 될때까지 대기
            while (AxtMovingEndCheck(m_nAxis) == false) { delay(1); if (CheckOverLoad(m_nAxis) == true) return false; }

            AxtClearPosition(m_nAxis);
#endif
            return true;
        }

        public bool OriginSearch1(short m_nAxis1, short m_nAxis2)
        {
            //bool Flag = true;
#if PROGRAM_RUNNING
            //double dVelocity = 10;
            //double dAccel = 10 * 2;

            double[,] dVelocity = new double[2,3];
            double[,] dAccel = new double[2,3];


            if ((AxtReadAlarm(m_nAxis1) == true) || (AxtReadAlarm(m_nAxis2) == true))
            {
                if (AxtReadAlarm(m_nAxis1) == true) Axt_Alarm_Reset(m_nAxis1);
                if (AxtReadAlarm(m_nAxis2) == true) Axt_Alarm_Reset(m_nAxis2);
                delay(500);
            }

            if (m_nAxis1 == mStatus.X_SELECT)
            {
                dVelocity[0, 0] = 100;
                dVelocity[0, 1] = 50;
                dVelocity[0, 2] = 10;

                dAccel[0, 0] = dVelocity[0, 0] * 2;
                dAccel[0, 1] = dVelocity[0, 1] * 2;
                dAccel[0, 2] = dVelocity[0, 2] * 2;
            }
            else if (m_nAxis1 == mStatus.Y_SELECT)
            {
                dVelocity[0, 0] = 30;
                dVelocity[0, 1] = 20;
                dVelocity[0, 2] = 10;

                dAccel[0, 0] = dVelocity[0, 0] * 2;
                dAccel[0, 1] = dVelocity[0, 1] * 2;
                dAccel[0, 2] = dVelocity[0, 2] * 2;
            }
            else //가로축
            {

                dVelocity[0, 0] = 30;
                dVelocity[0, 1] = 20;
                dVelocity[0, 2] = 10;

                dAccel[0, 0] = dVelocity[0, 0] * 2;
                dAccel[0, 1] = dVelocity[0, 1] * 2;
                dAccel[0, 2] = dVelocity[0, 2] * 2;

            }

            if (m_nAxis2 == mStatus.X_SELECT)
            {
                dVelocity[1, 0] = 50;
                dVelocity[1, 1] = 10;
                dVelocity[1, 2] = 2;

                dAccel[1, 0] = dVelocity[1, 0] * 2;
                dAccel[1, 1] = dVelocity[1, 1] * 2;
                dAccel[1, 2] = dVelocity[1, 2] * 2;
            }
            else if (m_nAxis2 == mStatus.Y_SELECT)
            {
                dVelocity[1, 0] = 20;
                dVelocity[1, 1] = 15;
                dVelocity[1, 2] = 2;

                dAccel[1, 0] = dVelocity[1, 0] * 2;
                dAccel[1, 1] = dVelocity[1, 1] * 2;
                dAccel[1, 2] = dVelocity[1, 2] * 2;
            }
            else //가로축
            {

                dVelocity[1, 0] = 30;
                dVelocity[1, 1] = 15;
                dVelocity[1, 2] = 2;

                dAccel[1, 0] = dVelocity[1, 0] * 2;
                dAccel[1, 1] = dVelocity[1, 1] * 2;
                dAccel[1, 2] = dVelocity[1, 2] * 2;

            }

            // UpEdge 와 DownEdge 의 차이는 센서에 따라 LOW 에서 HIGH로 올라가느냐 HIGH 에서 LOW로 떨어지느냐의 차이이다.
            //1 = 급 정지
            //0 -  감속 정지
            int ISignalMethod = 1;

            if (CAXM.AxmMoveSignalSearch(m_nAxis1, -dVelocity[0, 0], dAccel[0, 0], (int)AXT_MOTION_HOME_DETECT.HomeSensor, (int)AXT_DIO_EDGE.DOWN_EDGE, ISignalMethod) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return false;
            if (CAXM.AxmMoveSignalSearch(m_nAxis2, -dVelocity[1, 0], dAccel[1, 0], (int)AXT_MOTION_HOME_DETECT.HomeSensor, (int)AXT_DIO_EDGE.DOWN_EDGE, ISignalMethod) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return false;
            delay(100);
            // 구동이 완료 될때까지 대기

            bool Flag = true;

            do
            {
                if (m_nAxis1 == mStatus.X_SELECT)
                {
                    if (CheckOverLoad(m_nAxis1) == true) return false;
                }
                if (m_nAxis2 == mStatus.X_SELECT)
                {
                    if (CheckOverLoad(m_nAxis2) == true) return false;
                }
                delay(1);
                if ((AxtMovingEndCheck(m_nAxis1) == true) && (AxtMovingEndCheck(m_nAxis2) == true)) Flag = false;
            }
            while (Flag == true);

            if (CAXM.AxmMoveSignalSearch(m_nAxis1, dVelocity[0, 1], dAccel[0, 1], (int)AXT_MOTION_HOME_DETECT.HomeSensor, (int)AXT_DIO_EDGE.UP_EDGE, ISignalMethod) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return false;
            if (CAXM.AxmMoveSignalSearch(m_nAxis2, dVelocity[1, 1], dAccel[1, 1], (int)AXT_MOTION_HOME_DETECT.HomeSensor, (int)AXT_DIO_EDGE.UP_EDGE, ISignalMethod) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return false;
            delay(100);
            // 구동이 완료 될때까지 대기
            Flag = true;

            do
            {
                if (m_nAxis1 == mStatus.X_SELECT)
                {
                    if (CheckOverLoad(m_nAxis1) == true) return false;
                }
                if (m_nAxis2 == mStatus.X_SELECT)
                {
                    if (CheckOverLoad(m_nAxis2) == true) return false;
                }
                delay(1);
                if ((AxtMovingEndCheck(m_nAxis1) == true) && (AxtMovingEndCheck(m_nAxis2) == true)) Flag = false;
            }
            while (Flag == true);

            if (CAXM.AxmMoveSignalSearch(m_nAxis1, -dVelocity[0, 2], dAccel[0, 2], (int)AXT_MOTION_HOME_DETECT.HomeSensor, (int)AXT_DIO_EDGE.DOWN_EDGE, ISignalMethod) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return false;
            if (CAXM.AxmMoveSignalSearch(m_nAxis2, -dVelocity[1, 2], dAccel[1, 2], (int)AXT_MOTION_HOME_DETECT.HomeSensor, (int)AXT_DIO_EDGE.DOWN_EDGE, ISignalMethod) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return false;
            delay(100);
            // 구동이 완료 될때까지 대기
            Flag = true;

            do
            {
                if (m_nAxis1 == mStatus.X_SELECT)
                {
                    if (CheckOverLoad(m_nAxis1) == true) return false;
                }
                if (m_nAxis2 == mStatus.X_SELECT)
                {
                    if (CheckOverLoad(m_nAxis2) == true) return false;
                }
                delay(1);
                if ((AxtMovingEndCheck(m_nAxis1) == true) && (AxtMovingEndCheck(m_nAxis2) == true)) Flag = false;
            }
            while (Flag == true);

            AxtClearPosition(m_nAxis1);
            AxtClearPosition(m_nAxis2);
#endif
            return true;
        }

        // Home Search CCW 구동 쓰레드 부분
        public bool OriginSearch2(short m_nAxis)
        {
            //bool Flag = true;
#if PROGRAM_RUNNING
            double dVelocity = 10;
            double dAccel = 10 * 2;

            if (GetRunAxis(m_nAxis) == false) return true;

            // UpEdge 와 DownEdge 의 차이는 센서에 따라 LOW 에서 HIGH로 올라가느냐 HIGH 에서 LOW로 떨어지느냐의 차이이다.
            //1 = 급 정지
            //0 -  감속 정지
            int ISignalMethod = 1;

            if (CAXM.AxmMoveSignalSearch(m_nAxis, dVelocity, dAccel, (int)AXT_MOTION_HOME_DETECT.HomeSensor, (int)AXT_DIO_EDGE.DOWN_EDGE, ISignalMethod) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return false;

            // 구동이 완료 될때까지 대기
            while (AxtMovingEndCheck(m_nAxis) == false) { delay(1); if (CheckOverLoad(m_nAxis) == true) return false; }

            if (CAXM.AxmMoveSignalSearch(m_nAxis, -dVelocity, dAccel, (int)AXT_MOTION_HOME_DETECT.HomeSensor, (int)AXT_DIO_EDGE.UP_EDGE, ISignalMethod) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return false;

            // 구동이 완료 될때까지 대기
            while (AxtMovingEndCheck(m_nAxis) == false) { delay(1); if (CheckOverLoad(m_nAxis) == true) return false; }

            if (CAXM.AxmMoveSignalSearch(m_nAxis, dVelocity, dAccel, (int)AXT_MOTION_HOME_DETECT.HomeSensor, (int)AXT_DIO_EDGE.DOWN_EDGE, ISignalMethod) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return false;

            // 구동이 완료 될때까지 대기
            while (AxtMovingEndCheck(m_nAxis) == false) { delay(1); if (CheckOverLoad(m_nAxis) == true) return false; }

            AxtClearPosition(m_nAxis);
#endif
            return true;
        }

        public bool AxtMovingEndCheck(short m_nAxis)
        {
#if PROGRAM_RUNNING
            //return CAxtCAMCFS20.CFS20in_motion(m_nAxis);
            if (GetRunAxis(m_nAxis) == false) return true;

            uint Result = 0;
            if (CAXM.AxmStatusReadInMotion(m_nAxis, ref Result) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return false;

            if (Result == 0)
                return true; //이동 완료면 설정된다.
            else return false;
#else
            return true;
#endif
        }

        public bool AxtReadAlarm(short m_nAxis)
        {
#if PROGRAM_RUNNING
            /*
            if (CAxtCAMCFS20.CFS20get_alarm_switch(m_nAxis) != 0)
                    return true;
            else    return false;
             */
            if (GetRunAxis(m_nAxis) == false) return true;

            uint mStatus = 0;
            if (CAXM.AxmSignalReadServoAlarm(m_nAxis, ref mStatus) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {
                return false;
            }

            if ((mStatus & 0x01) == 0)
                return false;
            else return true;
#else
            return false;
#endif
        }

        public bool AxtReadHome(short m_nAxis)
        {
#if PROGRAM_RUNNING

            uint mStatus = 0;
            if (CAXM.AxmHomeGetResult(m_nAxis, ref mStatus) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {
                return false;
            }

            if ((mStatus & (byte)AXT_MOTION_HOME_RESULT.HOME_SUCCESS) == (byte)AXT_MOTION_HOME_RESULT.HOME_SUCCESS)
                return true;
            else return false;
#else
            return false;
#endif
        }

        public void AxtOriginSpeedSet(short m_nAxis, ushort Speed)
        {
#if PROGRAM_RUNNING

            int nHmDir = 0; //단계 이동 방향   0 - Cw , -0 - ccw 즉 값에 (-) 기호가 있는가 없는가에 따라 방향이 설정됨
            uint uHomeSignal = (int)AXT_MOTION_HOME_DETECT.HomeSensor;
            uint uZphas = 0; //z상 검출 유무 , 1 - 검출 , 0 - 검출 안함
            double uHomeClrTime = 20000.0; //원점 검색 Encoder 값 설정하기 위한 대기 시간
            double uHomeOffet = 0; //원점 이동후 설정한 오프셋 값 만큼킁 이동

            if (GetRunAxis(m_nAxis) == false) return;

            if (CAXM.AxmHomeSetMethod(m_nAxis, nHmDir, uHomeSignal, uZphas, uHomeClrTime, uHomeOffet) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return;

            double[] rSpeed = new double[4];
            double[] Accel = new double[4];
            double[] AccelTime = new double[4];


            Accel[0] = 100;
            Accel[1] = 80;
            Accel[2] = 2;
            Accel[3] = 1;

            rSpeed[0] = (double)Speed;
            rSpeed[1] = (double)Speed;
            rSpeed[2] = 10;
            rSpeed[3] = 5;

            if (CAXM.AxmHomeSetVel(m_nAxis, rSpeed[0], rSpeed[1], rSpeed[2], rSpeed[3], Accel[0], Accel[3]) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS) return;
#endif
            return;
        }

        private long Axt_TimeGetTimemms()
        {
            return STOP_WATCH.ElapsedMilliseconds;
        }

        private void delay(long Time)
        {
            long First;
            long Last;

            First = Axt_TimeGetTimemms();
            Last = Axt_TimeGetTimemms();

            do
            {
                Application.DoEvents();
                Last = Axt_TimeGetTimemms();
            } while ((Last - First) < Time);
        }

        private void SetAbsMode(short m_nAxis) //절대좌표 모드
        {
            if (GetRunAxis(m_nAxis) == false) return;
            if (CAXM.AxmMotSetAbsRelMode(m_nAxis, 0) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {

            }
            return;
        }
        private void SetNotAbsMode(short m_nAxis) //상대 좌표 모드
        {
            if (GetRunAxis(m_nAxis) == false) return;
            if (CAXM.AxmMotSetAbsRelMode(m_nAxis, 1) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {

            }
            return;
        }

        private bool GetIsAbsMode(short m_nAxis) //절대 좌표 모드인지 상대 좌표 모드인지 읽어 온다.
        {
            if (GetRunAxis(m_nAxis) == false) return true;
            uint Value = 0;
            if (CAXM.AxmMotGetAbsRelMode(m_nAxis, ref Value) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {
                return false;
            }
            if (Value == 0)
                return true;
            else return false;
        }

        public void MoveAnglePos(int[] m_nAxis, float Speed, float TargetStroke, float LeverLength)
        {
            // 현재 위치(0,0)에서 (100,100) 위치까지 (0,100)을 중심으로 원호 보간을 수행한다. 
            int[] lAxesNo = { m_nAxis[0], m_nAxis[1] };
            double[] dCenPos = new double[2];
            //double[] dEndPos = new double[2];
            int lCoordinate = 0;
            uint uAbsRelMode = 1;
            //uint uProfileMode = 3;
            uint lSize = 2;

            CAXM.AxmContiWriteClear(lCoordinate);
            //CAXM.AxmContiBeginNode(lCoordinate);
            CAXM.AxmContiSetAxisMap(lCoordinate, lSize, lAxesNo);
            CAXM.AxmContiSetAbsRelMode(lCoordinate, uAbsRelMode); // 상대 위치 구동으로 설정
                                                                  //CAXM.AxmContiEndNode(lCoordinate);
                                                                  //double dCenterXPosition = LeverLength; // 0
                                                                  // dCenterYPosition = LeverLength; // 100
                                                                  //double dEndXPosition = 50;
                                                                  //double dEndYPosition = 50;

            //dCenPos[0] = dCenterXPosition + AxtReadPosition(mStatus.X_SELECT);
            //dCenPos[1] = dCenterYPosition + AxtReadPosition(mStatus.Y_SELECT);
            dCenPos[0] = 0;
            dCenPos[1] = LeverLength;
            //dEndPos[0] = dEndXPosition;
            //dEndPos[1] = dEndYPosition;

            double dMaxVelocity = Speed * 2;
            double dMaxAccel = Speed;
            double dMaxDecel = Speed * 2;
            uint uCWDir = 1; //[0] Ccw방향, [1] Cw방향 

            float dAngle = (float)(TargetStroke / (Math.PI * LeverLength)) * 180F;
            dAngle = 180;
            //CAXM.AxmCircleCenterMove(lCoordinate, lAxesNo, dCenPos, dEndPos, dMaxVelocity, dMaxAccel, dMaxDecel,uCWDir);
            uint Status = CAXM.AxmCircleAngleMove(lCoordinate, lAxesNo, dCenPos, dAngle, dMaxVelocity, dMaxAccel, dMaxDecel, uCWDir);

            return;
        }
        ~AxtMotion()
        {
            STOP_WATCH.Stop();
        }
    }
}