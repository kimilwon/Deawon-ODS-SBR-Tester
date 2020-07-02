using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using iniControl;

using System.Drawing;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.RegularExpressions;

public class InputBox : IDisposable
{
    public static DialogResult Show(string title, string promptText, ref string value)
    {
        return Show(title, promptText, ref value, null);
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
        return;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {

        }
        return;
    }

    public static DialogResult Show(string title, string promptText, ref string value,
                                    InputBoxValidation validation)
    {
        Form form = new Form();
        Label label = new Label();
        TextBox textBox = new TextBox();
        Button buttonOk = new Button();
        Button buttonCancel = new Button();

        form.Text = title;
        label.Text = promptText;
        textBox.Text = value;

        buttonOk.Text = "OK";
        buttonCancel.Text = "Cancel";
        buttonOk.DialogResult = DialogResult.OK;
        buttonCancel.DialogResult = DialogResult.Cancel;

        label.SetBounds(9, 20, 372, 13);
        textBox.SetBounds(12, 36, 372, 20);
        buttonOk.SetBounds(228, 72, 75, 23);
        buttonCancel.SetBounds(309, 72, 75, 23);

        label.AutoSize = true;
        textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
        buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

        form.ClientSize = new Size(396, 107);
        form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
        form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.MinimizeBox = false;
        form.MaximizeBox = false;
        form.TopMost = true;
        form.AcceptButton = buttonOk;
        form.CancelButton = buttonCancel;
        if (validation != null)
        {
            form.FormClosing += delegate (object sender, FormClosingEventArgs e)
            {
                if (form.DialogResult == DialogResult.OK)
                {
                    string errorText = validation(textBox.Text);
                    if (e.Cancel = (errorText != ""))
                    {
                        MessageBox.Show(form, errorText, "Validation Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBox.Focus();
                    }
                }
            };
        }
        DialogResult dialogResult = form.ShowDialog();
        value = textBox.Text;
        return dialogResult;
    }
}
public delegate string InputBoxValidation(string errorMessage);


public class uMessageBox
{
    public static void Show(string title = "경고", string promptText = null)
    {
        xShow(title, promptText);
        return;
    }

    public static void xShow(string title, string promptText)
    {
        MessageBox.Show(caption: title, text: promptText, buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Warning, defaultButton: MessageBoxDefaultButton.Button1, options: MessageBoxOptions.ServiceNotification);
    }
}


public class uxMessageBox : IDisposable
{
    static Button[] Buttonx;
    static Form form = new Form();

    public static DialogResult Show(string title, string promptText, string[] sButton, DialogResult[] Result)
    {
        DialogResult sResult = Show(title, promptText, sButton, Result, null);
        return sResult;
    }

    public static DialogResult Show(string title = "경고", string promptText = null)
    {
        DialogResult sResult = Show(title, promptText, null);
        return sResult;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
        return;
    }


    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {

        }
        return;
    }

    public static DialogResult Show(string title, string promptText, string[] sButton, DialogResult[] Result,
                                    uxMessageBoxValidation validation)
    {
        Label label = new Label();
        Buttonx = new Button[sButton.Length];
        PictureBox picture = new PictureBox()
        {
            Parent = form,
            Image = ODS.Properties.Resources.Danger_Shield,

        };
        picture.SetBounds(9, 10, 32, 32);


        int i = 0;
        int bSize;

        for (i = 0; i < Buttonx.Length; i++)
        {
            Buttonx[i] = new Button()
            {
                AutoSize = false
            };
        }

        i = 0;
        foreach (string s in sButton)
        {
            Buttonx[i].Text = s;
            //Buttonx[i].AutoSize = true;
            i++;
        }
        int Max = 0;
        foreach (string s in sButton)
        {
            label.Text = s;
            if (Max < label.Width) Max = label.Right;
        }

        bSize = Max + 10;

        form.Text = title;
        label.Text = promptText;


        int Size = 0;
        for (i = 0; i < sButton.Length; i++)
        {
            Size += bSize + 8;
        }

        //label.SetBounds(9, 20, 372, 13);
        label.SetBounds(50, 20, 372, 13);
        label.Parent = form;
        label.AutoSize = true;

        Max = Math.Max(300, label.Right + 10);
        Max = Math.Max(Size, label.Right + 10);


        form.ClientSize = new Size(396 + 50, 107);
        form.ClientSize = new Size(Math.Max(300, Max) + 50, form.ClientSize.Height);

        int Widgth = (form.Width / 2) - (Size / 2);
        int X = 0;

        X = Widgth;
        for (i = 0; i < sButton.Length; i++)
        {
            Buttonx[i].SetBounds(X, 72, bSize, 23);
            X += bSize + 8;
            Buttonx[i].Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Buttonx[i].Parent = form;
            Buttonx[i].Visible = true;
            Buttonx[i].DialogResult = Result[i];
            Buttonx[i].Click += new EventHandler(ButtonClick);
        }

        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.MinimizeBox = false;
        form.MaximizeBox = false;
        form.ControlBox = false;
        form.TopMost = true;

        if (validation != null)
        {
            form.FormClosing += delegate (object sender, FormClosingEventArgs e)
            {
                if (form.DialogResult == DialogResult.OK)
                {
                }
                else if (form.DialogResult == DialogResult.No)
                {
                }
                else if (form.DialogResult == DialogResult.Cancel)
                {
                }
                form.Dispose();
                form = null;
            };
        }
        DialogResult dialogResult = form.ShowDialog();
        return dialogResult;
    }

    public static DialogResult Show(string title, string promptText, uxMessageBoxValidation validation)
    {
        Label label = new Label();
        Buttonx = new Button[1];
        Buttonx[0] = new Button() { AutoSize = false };
        PictureBox picture = new PictureBox()
        {
            Parent = form,
            Image = ODS.Properties.Resources.Danger_Shield

        };
        picture.SetBounds(9, 10, 32, 32);

        System.Drawing.Graphics formGraphics = form.CreateGraphics();

        float FWidth = (form.Font.SizeInPoints / 72) * formGraphics.DpiX;
        int FHeight = form.Font.Height;

        Buttonx[0].Text = "확인";

        int bSize = (int)("확인".Length * FWidth);
        int Size = bSize + 50;

        form.Text = title;
        label.Text = promptText;

        //label.SetBounds(9, 20, 372, 13);
        label.SetBounds(50, 20, 372, 13);
        label.Parent = form;
        label.AutoSize = true;

        int Max = 0;
        Max = Math.Max(300, label.Right + 10);
        Max = Math.Max(Size, label.Right + 10);


        form.ClientSize = new Size(396 + 50, 107);
        form.ClientSize = new Size(Math.Max(300, Max) + 50, form.ClientSize.Height);

        int Widgth = (form.Width / 2) - (Size / 2);
        int X = 0;

        X = Widgth;
        //for (i = 0; i < sButton.Length; i++)
        //{
        //    Buttonx[i].SetBounds(X, 72, bSize, 23);
        //    X += bSize + 8;
        //    Buttonx[i].Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        //    Buttonx[i].Parent = form;
        //    Buttonx[i].Visible = true;
        //    Buttonx[i].DialogResult = Result[i];
        //    Buttonx[i].Click += new EventHandler(ButtonClick);
        //}

        Buttonx[0].SetBounds(X, 72, Size, 23);
        X += bSize + 8;
        Buttonx[0].Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        Buttonx[0].Parent = form;
        Buttonx[0].Visible = true;
        Buttonx[0].DialogResult = DialogResult.OK;
        Buttonx[0].Click += new EventHandler(ButtonClick);

        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.MinimizeBox = false;
        form.MaximizeBox = false;
        form.ControlBox = false;
        form.TopMost = true;

        if (validation != null)
        {
            form.FormClosing += delegate (object sender, FormClosingEventArgs e)
            {
                if (form.DialogResult == DialogResult.OK)
                {
                }
                else if (form.DialogResult == DialogResult.No)
                {
                }
                else if (form.DialogResult == DialogResult.Cancel)
                {
                }
                form.Dispose();
            };
        }
        DialogResult dialogResult = form.ShowDialog();
        return dialogResult;
    }

    ~uxMessageBox()
    {

    }

    private static void ButtonClick(object sender, EventArgs e)
    {
        form.Close();
        return;
    }

    private void FormClosing(object sender, FormClosingEventArgs e)
    {
        form.Dispose();
        form = null;
    }
}
public delegate string uxMessageBoxValidation(string errorMessage);

namespace ODS
{
    public class COMMON_FUCTION : IDisposable
    {
        public Stopwatch STOP_WATCH = new Stopwatch();
        private List<string> FileList = new List<string>();
        private List<string> DirList = new List<string>();


        //        private IntPtr hWnd;
        //uint vncstyles;


        public COMMON_FUCTION()
        {
            STOP_WATCH.Start();
        }
        ~COMMON_FUCTION()
        {
            STOP_WATCH.Stop();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            return;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            return;
        }

        /// <summary>
        /// 화면을 갱신할 때 잔상이 남지 않도록 한다 
        /// 프로그램 스타트가 되면 화면을 갱신할때 잔상이 남을 만한 컴포너트를 이곳으로 보내 속성을 바꾼다.
        /// </summary>
        /// <param name="control"></param>
        public void SetDoubleBuffered(Control control)
        {
            // set instance non-public property with name "DoubleBuffered" to true
            typeof(Control).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, control, new object[] { true });
            return;
        }

        /*
         컴퓨터를 종료/재부팅/로그오프를 하는방법은 다음과 같습니다.

        기본적으로 Using System.Diagnostics을 사용하며,

        예제코드는 다음과 같습니다.

        종료

        Process.Start("shutdown.exe", "-s"); // 기본적으로 30초 후 종료

        Process.Start("shutdown.exe", "-s -t xx") // xx 초 후 종료



        재부팅

        Process.Start("shutdown.exe","-r"); // 종료과 유사하며 커멘드만 "-r"을 사용

        Process.Start("shutdown.exe","-r -t xx");



        로그오프

        Process.Start("shutdown.exe","-l"); // 위 코드와 유사하며 커멘드만 "-l"(숫자 1이 아닌 소문자'l')을 사용

        원하는 시간에 종료하길 원하신다면, 현재시간을 비교해 종료하는 방법이 있음.


        */

        // 모니터 On/Off 관련
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MONITORPOWER = 0xF170;
        const int MONITOR_ON = -1;
        const int MONITOR_OFF = 2;
        const int MONITOR_STANBY = 1;
        [System.Runtime.InteropServices.DllImport("User32")]
        private static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);
        /// <summary>
        /// this.Handle 을 상수로 주어야 한다.;
        /// </summary>
        /// <param name="Handle"></param>
        public void MonitorOff(IntPtr Handle)
        {
            SendMessage(Handle.ToInt32(), WM_SYSCOMMAND, SC_MONITORPOWER, MONITOR_OFF);
            return;
        }
        /// <summary>
        /// this.Handle 을 상수로 주어야 한다.;
        /// </summary>
        /// <param name="Handle"></param>
        public void MonitorOn(IntPtr Handle)
        {
            SendMessage(Handle.ToInt32(), WM_SYSCOMMAND, SC_MONITORPOWER, MONITOR_ON);
            return;
        }

        public void ExternalProgramRun(string name) //외부 프로그램 실행
        {
            Process.Start(name);
            return;
        }

        public void ExternalProgramExit(string name) //외부 프로그램 종료
        {
            string exitname;

            if (name.IndexOf(".exe") < 0)
            {
                exitname = name;
            }
            else
            {
                exitname = name.Substring(0, name.IndexOf(".exe"));
            }
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName.StartsWith(exitname))
                {
                    process.Kill();
                }
            }
        }

        public void LogOff()
        {
            Process.Start("shutdown.exe", "-l"); // 위 코드와 유사하며 커멘드만 "-l"(숫자 1이 아닌 소문자'l')을 사용
            return;
        }

        public void WindowRestartToDelay(int Sec)
        {
            string s;

            s = "shutdown.exe" + "," + "-r -t " + Sec.ToString();
            Process.Start(s);
            return;
        }

        public void WindowRestartTo30Secconds()
        {
            Process.Start("shutdown.exe", "-r");
            return;
        }

        public void WindowExit()
        {
            Process.Start("shutdown.exe", "-s -t 0"); // xx 초 후 종료
            return;
        }

        public void WindowExitToDelay(int Sec)
        {
            string s;

            s = "shutdown.exe" + "," + "-s -t " + Sec.ToString();
            Process.Start(s);
            return;
        }

        public void WindowExitTo30Secconds()
        {
            Process.Start("shutdown.exe", "-s");
            return;
        }

        /// <summary>
        /// Property 에 변수를 만든다.
        /// Type 은 typeof(string), typeof(int) 와 같이 형을 만들수 있다.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Defalut"></param>
        /// <param name="sType"></param>
        public void SetProperty(string Name, Type sType)
        {
            System.Configuration.SettingsProperty property = new System.Configuration.SettingsProperty(Name);

            //if(sType == typeof(string))
            //    property.DefaultValue = "";
            //else if (sType == typeof(int))
            //    property.DefaultValue = 0;
            //else if (sType == typeof(short))
            //    property.DefaultValue = 0;
            //else if (sType == typeof(long))
            //    property.DefaultValue = 0;

            property.IsReadOnly = false;
            property.PropertyType = sType;
            //property.Provider = Properties.Settings.Default.Providers["LocalFileSettingsProvider"];
            property.Attributes.Add(typeof(System.Configuration.UserScopedSettingAttribute), new System.Configuration.UserScopedSettingAttribute());
            try
            {
                Properties.Settings.Default.Properties.Add(property);
            }
            catch
            {
                Properties.Settings.Default.Properties.Remove(property.Name);
                Properties.Settings.Default.Properties.Add(property);
            }

            return;
        }


        public List<string> GetFileList
        {
            get
            {
                return FileList;
            }
        }

        public List<string> GetDirList
        {
            get
            {
                return DirList;
            }
        }


        [StructLayout(LayoutKind.Explicit)]
        private struct union_r
        {
            [FieldOffset(0)]
            public ushort i;
            [FieldOffset(0)]
            public byte c1;
            [FieldOffset(1)]
            public byte c2;
        };

        public class FileSortMode
        {
            public const short LASTWRITE_ODERBY = 0; //마지막 저장 날짜 기준 내림차순
            public const short LASTWRITE_ORDERBYDESCENDING = 1;//마지막 저장 날짜 기준 오름차순
            public const short FILENAME_ODERBY = 2; //파일명 기준으로 내림차순
            public const short FILENAME_ORDERBYDESCENDIN = 3; //파일명 기준으로 오름 차순
            public const short CREATETIME_ODERBY = 2; //생성날짜 기준으로 내림차순
            public const short CREATETIME_ORDERBYDESCENDIN = 3; //생성날짜 기준으로 오름 차순
            public const short NORMAL = 4;
        }

        public void ReadFileList(string Path, string Ext, short Mode = FileSortMode.NORMAL)
        {
            try
            {
                DirectoryInfo DirInf = new DirectoryInfo(Path);
                if (DirInf.Exists == true) //폴더가 존제하면
                {
                    //FileInfo[] FileInf = DirInf.GetFiles("*.*");

                    if (Mode == FileSortMode.NORMAL)
                    {
                        FileInfo[] FileInf = DirInf.GetFiles(Ext);

                        if (FileInf.Length == 0)
                        {
                            //MessageBox.Show("파일이 존재하지 않습니다.");
                            uMessageBox.Show(title: "경고", promptText: "파일이 존재하지 않습니다.");
                        }
                        else
                        {
                            string s = "";
                            for (int i = 0; i < FileInf.Length; i++)
                            {
                                s += FileInf[i].Name.ToString() + Environment.NewLine;

                                FileList.Add(s);
                                s = "";
                            }
                        }
                    }
                    else
                    {
                        //

                        //방식 1
                        //var Files = new DirectoryInfo(FileDirectory)
                        //.GetFiles()
                        //.OrderBy(f => f.Name.Substring(f.Name.Length - 23, 19)
                        //.ToList();

                        //방식 2
                        //생성 날짜를 기준으로 정렬한다.
                        //내림차순
                        //var FileInf = DirInf.GetFiles(Ext).OrderBy(f => f.LastWriteTime).ToList();
                        //오름차순

                        FileList.Clear();
                        string s = "";
                        if (Mode == FileSortMode.LASTWRITE_ODERBY)//마지막 저장 날짜 기준 오름차순
                        {
                            var FileInf = DirInf.GetFiles(Ext).OrderBy(f => f.LastWriteTime).ToList();
                            foreach (FileInfo fs in FileInf)
                            {
                                s = "";
                                s = fs.Name.ToString() + Environment.NewLine;

                                FileList.Add(s);
                            }
                        }
                        else if (Mode == FileSortMode.LASTWRITE_ORDERBYDESCENDING)//마지막 저장 날짜 기준 내림차순
                        {
                            var FileInf = DirInf.GetFiles(Ext).OrderByDescending(f => f.LastWriteTime).ToList();
                            foreach (FileInfo fs in FileInf)
                            {
                                s = "";
                                s = fs.Name.ToString() + Environment.NewLine;

                                FileList.Add(s);
                            }
                        }
                        else if (Mode == FileSortMode.FILENAME_ODERBY) //파일명을 기준으로 내림차순
                        {
                            var FileInf = DirInf.GetFiles(Ext).OrderBy(f => f.Name).ToList();
                            foreach (FileInfo fs in FileInf)
                            {
                                s = "";
                                s = fs.Name.ToString() + Environment.NewLine;

                                FileList.Add(s);
                            }
                        }
                        else if (Mode == FileSortMode.FILENAME_ORDERBYDESCENDIN)//파일명을 기준으로 오름 차순
                        {
                            var FileInf = DirInf.GetFiles(Ext).OrderByDescending(f => f.Name).ToList();
                            foreach (FileInfo fs in FileInf)
                            {
                                s = "";
                                s = fs.Name.ToString() + Environment.NewLine;

                                FileList.Add(s);
                            }
                        }
                        else if (Mode == FileSortMode.FILENAME_ODERBY) //파일명을 기준으로 내림차순
                        {
                            var FileInf = DirInf.GetFiles(Ext).OrderBy(f => f.CreationTime).ToList();
                            foreach (FileInfo fs in FileInf)
                            {
                                s = "";
                                s = fs.Name.ToString() + Environment.NewLine;

                                FileList.Add(s);
                            }
                        }
                        else if (Mode == FileSortMode.FILENAME_ORDERBYDESCENDIN)//파일명을 기준으로 오름 차순
                        {
                            var FileInf = DirInf.GetFiles(Ext).OrderByDescending(f => f.CreationTime).ToList();
                            foreach (FileInfo fs in FileInf)
                            {
                                s = "";
                                s = fs.Name.ToString() + Environment.NewLine;

                                FileList.Add(s);
                            }
                        }
                    }
                }
                else
                {
                    FileList.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                //uMessageBox.Show(promptText: ex.Message + "\n" + ex.StackTrace);
            }
        }
        public void ReadFileListNotExt(string Path, string Ext, short Mode = FileSortMode.NORMAL)
        {
            try
            {
                DirectoryInfo DirInf = new DirectoryInfo(Path);
                if (DirInf.Exists == true) //폴더가 존제하면
                {
                    //FileInfo[] FileInf = DirInf.GetFiles("*.*");

                    if (Mode == FileSortMode.NORMAL)
                    {
                        FileInfo[] FileInf = DirInf.GetFiles(Ext);

                        FileList.Clear();
                        if (FileInf.Length == 0)
                        {
                            //MessageBox.Show("파일이 존재하지 않습니다.");
                            uMessageBox.Show(promptText: "파일이 존재하지 않습니다.");
                        }
                        else
                        {
                            string s = "";
                            for (int i = 0; i < FileInf.Length; i++)
                            {
                                s += FileInf[i].Name.ToString() + Environment.NewLine;
                                string[] t = s.Split('.');
                                FileList.Add(t[0]);
                                s = "";
                            }
                        }
                    }
                    else
                    {
                        //

                        //방식 1
                        //var Files = new DirectoryInfo(FileDirectory)
                        //.GetFiles()
                        //.OrderBy(f => f.Name.Substring(f.Name.Length - 23, 19)
                        //.ToList();

                        //방식 2
                        //생성 날짜를 기준으로 정렬한다.
                        //내림차순
                        //var FileInf = DirInf.GetFiles(Ext).OrderBy(f => f.LastWriteTime).ToList();
                        //오름차순

                        FileList.Clear();
                        string s = "";
                        if (Mode == FileSortMode.LASTWRITE_ODERBY)//마지막 저장 날짜 기준 오름차순
                        {
                            var FileInf = DirInf.GetFiles(Ext).OrderBy(f => f.LastWriteTime).ToList();
                            foreach (FileInfo fs in FileInf)
                            {
                                s = "";
                                s = fs.Name.ToString() + Environment.NewLine;

                                string[] t = s.Split('.');
                                FileList.Add(t[0]);
                            }
                        }
                        else if (Mode == FileSortMode.LASTWRITE_ORDERBYDESCENDING)//마지막 저장 날짜 기준 내림차순
                        {
                            var FileInf = DirInf.GetFiles(Ext).OrderByDescending(f => f.LastWriteTime).ToList();
                            foreach (FileInfo fs in FileInf)
                            {
                                s = "";
                                s = fs.Name.ToString() + Environment.NewLine;

                                string[] t = s.Split('.');
                                FileList.Add(t[0]);
                            }
                        }
                        else if (Mode == FileSortMode.FILENAME_ODERBY) //파일명을 기준으로 내림차순
                        {
                            var FileInf = DirInf.GetFiles(Ext).OrderBy(f => f.Name).ToList();
                            foreach (FileInfo fs in FileInf)
                            {
                                s = "";
                                s = fs.Name.ToString() + Environment.NewLine;

                                string[] t = s.Split('.');
                                FileList.Add(t[0]);
                            }
                        }
                        else if (Mode == FileSortMode.FILENAME_ORDERBYDESCENDIN)//파일명을 기준으로 오름 차순
                        {
                            var FileInf = DirInf.GetFiles(Ext).OrderByDescending(f => f.Name).ToList();
                            foreach (FileInfo fs in FileInf)
                            {
                                s = "";
                                s = fs.Name.ToString() + Environment.NewLine;

                                string[] t = s.Split('.');
                                FileList.Add(t[0]);
                            }
                        }
                        else if (Mode == FileSortMode.FILENAME_ODERBY) //파일명을 기준으로 내림차순
                        {
                            var FileInf = DirInf.GetFiles(Ext).OrderBy(f => f.CreationTime).ToList();
                            foreach (FileInfo fs in FileInf)
                            {
                                s = "";
                                s = fs.Name.ToString() + Environment.NewLine;

                                string[] t = s.Split('.');
                                FileList.Add(t[0]);
                            }
                        }
                        else if (Mode == FileSortMode.FILENAME_ORDERBYDESCENDIN)//파일명을 기준으로 오름 차순
                        {
                            var FileInf = DirInf.GetFiles(Ext).OrderByDescending(f => f.CreationTime).ToList();
                            foreach (FileInfo fs in FileInf)
                            {
                                s = "";
                                s = fs.Name.ToString() + Environment.NewLine;

                                string[] t = s.Split('.');
                                FileList.Add(t[0]);
                            }
                        }
                    }
                }
                else
                {
                    FileList.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                //uMessageBox.Show(promptText: ex.Message + "\n" + ex.StackTrace);
            }
        }

        public void ReadDirList(string Path)
        {
            try
            {
                DirectoryInfo DirInf = new DirectoryInfo(Path + "\\");
                if (DirInf.Exists == true) //폴더가 존제하면
                {
                    DirectoryInfo[] DirListData = DirInf.GetDirectories();

                    DirList.Clear();
                    if (DirListData.Length == 0)
                    {
                        //MessageBox.Show("폴더가 존재하지 않습니다.");
                        uMessageBox.Show(promptText: "파일이 존재하지 않습니다.");
                    }
                    else
                    {
                        string s = "";
                        for (int i = 0; i < DirListData.Length; i++)
                        {
                            s += DirListData[i].Name.ToString();
                            //string[] t = s.Split('.');
                            //DirList.Add(t[0]);
                            string[] t = s.Split('\r');
                            DirList.Add(t[0]);
                            s = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                //uMessageBox.Show(promptText: ex.Message + "\n" + ex.StackTrace);
            }
        }

        
        public void SaveRunTime<__Struct__>(short Ch, __Struct__ Time)
        {
            try
            {
                string Name = null;

                FileStream DataStream = new FileStream(Name, FileMode.Append, FileAccess.Write);

                byte[] buffer = new byte[Marshal.SizeOf(typeof(__Struct__))];
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                Marshal.StructureToPtr(Time, handle.AddrOfPinnedObject(), false);
                DataStream.Write(buffer, 0, Marshal.SizeOf(typeof(__Struct__)));
                handle.Free();
                DataStream.Close();
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public string[] GetComName(string[] List)
        {
            string[] Name = new string[List.Length];
            int x;

            for (int i = 0; i < Name.Length; i++) Name[i] = "";
            x = 0;
            foreach (string s in List)
            {
                Name[x] = "COM";
                Name[x] = Name[x] + GetNumber(s);
                x++;
            }

            return Name;
        }

        public string GetNumber(string Data)
        {
            string s = "";

            foreach (char c in Data)
            {
                if (isnumeric(c) == true) s = s + c;
            }
            return s;
        }

        public int GetToNumInt(string s, bool ReturnToChar = true, bool ToHex = true) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
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

        public string GetString(string data = "0", bool ReturnToChar = true, bool ToHexString = true)
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

        public void strcpy(out string target, char[] source)
        {
            target = "";

            foreach (char c in source)
            {
                if (c != 0x00)
                    target = target + c;
                else break;
            }
            //target = target1;
            return;
        }

        public void strcpy(out char[] target, char[] source)
        {
            int i;

            target = new char[source.Length];
            i = 0;
            foreach (char c in source)
            {
                //if (c != 0x00)
                target[i++] = c;
                //else break;
            }
            return;
        }

        public void strcpy(out char[] target, string source)
        {
            int i;

            target = new char[source.Length];
            i = 0;
            foreach (char c in source)
            {
                //if (c != 0x00)
                target[i++] = c;
                //else break;
            }
            return;
        }

        public void strcpy(out byte[] target, string source)
        {
            int i;

            target = new byte[source.Length];
            i = 0;
            foreach (char c in source)
            {
                if (c != 0x00)
                    target[i++] = (byte)c;
                else break;
            }
            return;
        }

        public void strncpy(out char[] target, string source, int Length)
        {
            int i;

            i = 0;
            target = new char[Length];
            foreach (char c in source)
            {
                if (c != 0x00)
                    target[i++] = (char)c;
                else break;
            }
            return;
        }

        public void strncpy(out char[] target, byte[] source, int Length)
        {
            int i;

            i = 0;
            target = new char[Length];
            foreach (char c in source)
            {
                if (c != 0x00)
                    target[i++] = (char)c;
                else break;
            }
            return;
        }


        public float GetToNumFloat(string s, bool ReturnToChar = true, bool ToHex = true) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
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

        public short GetToNumShort(string s, bool ReturnToChar = true, bool ToHex = true) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
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

        public long GetToNumLong(string s, bool ReturnToChar = true, bool ToHex = true) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
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

        public double GetToNumDouble(string s, bool ReturnToChar = true, bool ToHex = true) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
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

        public byte GetToNumByte(string s, bool ReturnToChar = true, bool ToHex = true) //ReturnToChar == true 일때 숫자가 아니면 바로 리턴하고 false 이면 숫자값을 모두 읽어 온다.
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

        public void GetSystemDate(out string outTime)
        {
            outTime = DateTime.Now.ToString("yyyy.MM.dd");

            return;
        }

        //---------------------------------------------------------------------------


        public void GetSystemTime(out string outTime)
        {
            outTime = DateTime.Now.ToString("HH:mm:ss");
            return;
        }

        //---------------------------------------------------------------------------
        public void GetSystemDateTime(out string outTime)
        {
            outTime = "[" + DateTime.Now.ToString("yyyy.MM.dd") + "-" + DateTime.Now.ToString("HH:mm:ss") + "]";
            return;
        }

        //---------------------------------------------------------------------------
        public bool isnumeric(char s)
        {
            switch (s)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9': return true;
            }
            return false;
        }
        //---------------------------------------------------------------------------
        public int StringToHex(char[] Data)
        {
            int Value;
            //short i;
            //short Length;

            //Value = 0;

            //Length = (short)Data.Length;

            //for (i = 0; i < Length; i++)
            //{
            //    Value |= CharToHex(Data[i]) << (4 * ((Length - 1) - i));
            //}

            byte[] s = Encoding.UTF8.GetBytes(Data);
            string s2 = Encoding.UTF8.GetString(s).ToUpper();

            var sx = new String(s2.Where(Char.IsLetterOrDigit).ToArray());

            s2 = sx;
            //if (0 <= s2.IndexOf("0x")) s2 = s2.Remove(0, 2);
            if (0 <= s2.IndexOf("0X")) s2 = s2.Remove(0, 2);


            if (int.TryParse(s2, System.Globalization.NumberStyles.HexNumber, null, out Value) == false) Value = 0;
            return Value;
        }

        public int StringToHex(byte[] Data)
        {
            int Value;
            //short i;
            //short Length;

            //Value = 0;

            //Length = (short)Data.Length;

            //for (i = 0; i < Length; i++)
            //{
            //    Value |= CharToHex(Data[i]) << (4 * ((Length - 1) - i));
            //}

            string s2 = Encoding.UTF8.GetString(Data).ToUpper();

            //if (0 <= s2.IndexOf("0x")) s2 = s2.Remove(0, 2);

            var sx = new String(s2.Where(Char.IsLetterOrDigit).ToArray());
            s2 = sx;

            if (0 <= s2.IndexOf("0X")) s2 = s2.Remove(0, 2);

            if (int.TryParse(s2, System.Globalization.NumberStyles.HexNumber, null, out Value) == false) Value = 0;
            return Value;
        }
        //---------------------------------------------------------------------------
        public int StringToHex(string Data)
        {
            int Value;
            //short i;
            //short Length;

            //if(0 <= Data.IndexOf("0x"))
            //{
            //    Data = Data.Substring(2);
            //}
            //else if (0 <= Data.IndexOf("0X"))
            //{
            //    Data = Data.Substring(2);
            //}

            //Value = 0;

            //Length = (short)Data.Length;

            //i = 0;
            //foreach (char c in Data)
            //{
            //    Value |= CharToHex(c) << (4 * ((Length - 1) - i));
            //    i++;
            //}

            string s2 = Data.ToUpper();
            var sx = new String(s2.Where(Char.IsLetterOrDigit).ToArray());
            s2 = sx;
            //if (0 <= s2.IndexOf("0x")) s2 = s2.Remove(0, 2);
            if (0 <= s2.IndexOf("0X")) s2 = s2.Remove(0, 2);

            if (int.TryParse(s2, System.Globalization.NumberStyles.HexNumber, null, out Value) == false) Value = 0;
            return Value;
        }
        //---------------------------------------------------------------------------
        public short CharToHex(char c)
        {
            //    switch(c)
            //    {
            //        case '0' : return 0x00;
            //        case '1' : return 0x01;
            //        case '2' : return 0x02;
            //        case '3' : return 0x03;
            //        case '4' : return 0x04;
            //        case '5' : return 0x05;
            //        case '6' : return 0x06;
            //        case '7' : return 0x07;
            //        case '8' : return 0x08;
            //        case '9' : return 0x09;
            //        case 'A' : return 0x0a;
            //        case 'B' : return 0x0b;
            //        case 'C' : return 0x0c;
            //        case 'D' : return 0x0d;
            //        case 'E' : return 0x0e;
            //        case 'F' : return 0x0f;
            //        default  : return 0x00;
            //    }
            short Value = 0x00;

            if (('0' <= c) && (c <= '9'))
            {
                Value = (short)(c - '0');
            }
            else if (('A' <= c) && (c <= 'F'))
            {
                Value = (short)(c - 'A');
                Value += 10;
            }

            else if (('a' <= c) && (c <= 'f'))
            {
                Value = (short)(c - 'a');
                Value += 10;
            }
            return Value;
        }
        //------------------------------------------------------------------------------
        public string ToString(char[] Data)
        {
            string s;
            //short i;

            if (Data == null) return "";
            if (Data.Length == 0) return "";
            if (Data[0] == 0x00) return "";

            /*
            for (i = 0; i < Data.Length; i++)
            {
                if (Data[i] == 0x0000) break;
            }

            s = new string(Data).Substring(0, i);
            */
            s = "";
            foreach (char c in Data)
            {
                if (c != 0x00)
                    s = s + c;
                else break;
            }
            return s;
        }
        //------------------------------------------------------------------------------
        public string ReturnNumData(string Data)
        {
            string s;

            //if (Data.Length == 0) return "0";
            //s = "";
            //foreach (char c in Data)
            //{
            //    if (char.IsNumber(c))
            //    {
            //        s = s + c;
            //    }
            //    else
            //    {
            //        s = "0";
            //        return s;
            //    }
            //}
            //s = new string(Data).Substring(0, i);
            //s = Data.ToString().Substring(0,i);


            //using System.Text.RegularExpressions;

            //위와 같이 사용하시면 됩니다. "\D" 는 숫자가 아닌 문자열을 뜻하므로 숫자를 제외하고 다 없애라는 뜻이 됩니다.간단하지만 요긴한것 같습니다.
            string strTmp = Regex.Replace(Data, @"\D", "");
            int nTmp = int.Parse(strTmp);

            s = Convert.ToString(nTmp);

            //숫자만 가져오기
            //var onlyLetters = new String(Data.Where(Char.IsNumber).ToArray());
            return s;
        }
        //------------------------------------------------------------------------------
        public string ReturnStringData(string Data)
        {
            string s;

            //if (Data.Length == 0) return "0";
            //s = "";
            //foreach (char c in Data)
            //{
            //    if (char.IsNumber(c))
            //    {
            //        s = s + c;
            //    }
            //    else
            //    {
            //        s = "0";
            //        return s;
            //    }
            //}
            //s = new string(Data).Substring(0, i);
            //s = Data.ToString().Substring(0,i);


            //using System.Text.RegularExpressions;

            //위와 같이 사용하시면 됩니다. "\d" 는 문자가 아닌 숫자열을 뜻하므로 문자를 제외하고 다 없애라는 뜻이 됩니다.간단하지만 요긴한것 같습니다.
            string strTmp = Regex.Replace(Data, @"\d", "");
            int nTmp = int.Parse(strTmp);

            s = Convert.ToString(nTmp);

            //문자열만 축출
            //var onlyLetters = new String(Data.Where(Char.IsLetter).ToArray());
            //대문자만 축출
            //var onlyLetters = new String(Data.Where(c => Char.IsLetter(c) && Char.IsUpper(c)).ToArray());


            return s;
        }
        //------------------------------------------------------------------------------

        /*
        public void SaveIOMap<__IOMap__>(string Name, __IOMap__ Data)
        {
            //string Name;            

            //Path = Application.StartupPath + "\\SYSTEM";
            //Name = Path + "\\IO.dat";
            //File.Delete(Name);

            FileStream DataStream = new FileStream(Name, FileMode.Create, FileAccess.Write);

            byte[] buffer = new byte[Marshal.SizeOf(typeof(__IOMap__))];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            //Marshal.StructureToPtr((object)Data, handle.AddrOfPinnedObject(), false);
            Marshal.StructureToPtr(Data, handle.AddrOfPinnedObject(), false);
            DataStream.Write(buffer, 0, Marshal.SizeOf(typeof(__IOMap__)));
            handle.Free();
            DataStream.Close();
            return;
        }
        
        //------------------------------------------------------------------------------
        public __IOMap__ ReadIOMap<__IOMap__>(string Name, __IOMap__ Data)
        {
            //string Name;            
            int Length;
            FileStream DataStream;

            //Path = Application.StartupPath + "\\SYSTEM";
            //Name = Path + "\\IO.dat";

            byte[] buffer = new byte[Marshal.SizeOf(typeof(__IOMap__))];

            if (File.Exists(Name))
            {
                DataStream = new FileStream(Name, FileMode.Open, FileAccess.Read);
                Length = DataStream.Read(buffer, 0, Marshal.SizeOf(typeof(__IOMap__)));
                DataStream.Close();

                //if (Marshal.SizeOf(typeof(__IOMap__)) == Length) //나중에 함수가 추가 되더라도 데이타를 읽어 오기 위해 이 비교문을 삭제한다.
                {
                    GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    Data = (__IOMap__)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(__IOMap__));
                    handle.Free();
                }
            }
            return Data;
        }
        */
        //------------------------------------------------------------------------------
        public void SaveData<__Struct__>(string Name, __Struct__ Data)
        {
            try
            {
                using (File.Open(Name, FileMode.Append)) { } //파일 엑세스가 가능한지 확인한다.

                if (File.Exists(Name) == false)
                {
                    using (var DataStream = new FileStream(Name, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buffer = new byte[Marshal.SizeOf(typeof(__Struct__))];
                        GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                        //Marshal.StructureToPtr((object)Data, handle.AddrOfPinnedObject(), false);
                        Marshal.StructureToPtr(Data, handle.AddrOfPinnedObject(), false);
                        DataStream.Write(buffer, 0, Marshal.SizeOf(typeof(__Struct__)));
                        handle.Free();
                        DataStream.Close();
                    }
                }
                else
                {
                    using (var DataStream = new FileStream(Name, FileMode.Append, FileAccess.Write))
                    {
                        byte[] buffer = new byte[Marshal.SizeOf(typeof(__Struct__))];
                        GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                        //Marshal.StructureToPtr((object)Data, handle.AddrOfPinnedObject(), false);
                        Marshal.StructureToPtr(Data, handle.AddrOfPinnedObject(), false);
                        DataStream.Write(buffer, 0, Marshal.SizeOf(typeof(__Struct__)));
                        handle.Free();
                        DataStream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }
        //------------------------------------------------------------------------------
        public void SaveBuffer<__Struct__>(string Name, __Struct__ Data)
        {
            try
            {
                //using (File.Open(Name, FileMode.Append)) { } //파일 엑세스가 가능한지 확인한다.

                using (var DataStream = new FileStream(Name, FileMode.Create, FileAccess.Write))
                {
                    //FileStream DataStream = new FileStream(Name, FileMode.Create, FileAccess.Write);

                    byte[] buffer = new byte[Marshal.SizeOf(typeof(__Struct__))];
                    GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    //Marshal.StructureToPtr((object)Data, handle.AddrOfPinnedObject(), false);
                    Marshal.StructureToPtr(Data, handle.AddrOfPinnedObject(), false);
                    DataStream.Write(buffer, 0, Marshal.SizeOf(typeof(__Struct__)));
                    handle.Free();
                    DataStream.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                //uMessageBox.Show(promptText: ex.Message + "\n" + ex.StackTrace);
            }
        }
        //------------------------------------------------------------------------------
        public __Struct__ ReadBuffer<__Struct__>(string Name, __Struct__ Data)
        {
            int Length;
            //FileStream DataStream;

            //using (File.Open(Name, FileMode.Open)) { } //파일 엑세스가 가능한지 확인한다.

            byte[] buffer = new byte[Marshal.SizeOf(typeof(__Struct__))];

            if (File.Exists(Name))
            {
                using (var DataStream = new FileStream(Name, FileMode.Open, FileAccess.Read))
                {
                    //DataStream = new FileStream(Name, FileMode.Open, FileAccess.Read);
                    Length = DataStream.Read(buffer, 0, Marshal.SizeOf(typeof(__Struct__)));
                    DataStream.Close();

                    //if (Marshal.SizeOf(typeof(__Spec__)) == Length) //나중에 함수가 추가 되더라도 데이타를 읽어 오기 위해 이 비교문을 삭제한다.
                    {
                        GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                        Data = (__Struct__)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(__Struct__));
                        handle.Free();
                    }
                }
            }
            return Data;
        }
        

        public long timeGetTimems()
        {
            double ticks = STOP_WATCH.ElapsedTicks;

            //return STOP_WATCH.ElapsedMilliseconds;
            return (long)((ticks / Stopwatch.Frequency) * 1000);
        }

        public long timeGetNanoTimes()
        {
            //1ticks == 100 nanosecond
            //return STOP_WATCH.Elapsed.Ticks;// ElapsedTicks;
            double ticks = STOP_WATCH.ElapsedTicks;
            return (long)((ticks / Stopwatch.Frequency) * 1000000000);
        }


        public void delay(long time)
        {
            long first;
            long last;

            first = timeGetTimems();
            last = timeGetTimems();
            do
            {
                last = timeGetTimems();
            } while ((last - first) < time);
            return;
        }

        //private DelayForm delayForm = null;
        //public void timedelay(long time)
        //{
        //    //long first;
        //    //long last;

        //    //first = timeGetTimems();
        //    //last = timeGetTimems();
        //    //do
        //    //{
        //    //    Application.DoEvents();
        //    //    last = timeGetTimems();
        //    //} while ((last - first) < time);

        //    using (DelayForm delayForm = new DelayForm((int)time) { WindowState = FormWindowState.Minimized })
        //    {
        //        delayForm.FormClosing += delegate (object sender, FormClosingEventArgs e)
        //        {
        //            e.Cancel = false;
        //            delayForm.Dispose();
        //            //delayForm = null;
        //        };

        //        delayForm.ShowDialog();

        //        //delayForm.Dispose();
        //        //delayForm = null;
        //    }
        //    return;
        //}

        public void timedelay(long time)
        {
            long first;
            long last;

            first = timeGetTimems();
            last = timeGetTimems();
            do
            {
                Application.DoEvents();
                last = timeGetTimems();
            } while ((last - first) < time);

            return;
        }

        //private SecurityUnlockForm SecurityForm = null;


        public void nanotimedelay(long time)
        {
            long first;
            long last;

            first = timeGetNanoTimes();
            last = timeGetNanoTimes();
            do
            {
                Application.DoEvents();
                last = timeGetNanoTimes();
            } while ((last - first) < time);
            return;
        }
    }

    public class Password
    {
        private string Master;
        private string pass;
        private string Path;

        TIniFile Ini;

        public Password()
        {
            Path = Application.StartupPath + "\\SYSTEM\\pass.ini";
            Ini = new TIniFile(Path);
            OpenPassword();
        }

        public string SetPassword
        {
            set { pass = value; }
        }

        public string GetPassword
        {
            get {return pass; }
        }

        public string GetMaster
        {
            get {return Master;}
        }

        public void OpenPassword()
        {
            pass = Ini.ReadString("PASS", "PASS");
            Master = Ini.ReadString("PASS", "MASTER");
            if (Master == "") Master = "joeuneng";
            return;
        }

        public void SavePasswird()
        {
            Ini.WriteString("PASS", "PASS", pass);
            Ini.WriteString("PASS", "MASTER", Master);
            return;
        }
    }
}
