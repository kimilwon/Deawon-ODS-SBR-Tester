using System;
using System.Runtime.InteropServices;	// Add
using System.Text;

/*
 * using System.Runtime.InteropServices를 선언하지 않을 경우
 * 밑에 보이는 1, 2번 문장 중에 1번으로 사용해야 하며
 * 위와 같이 using문으로 선언이 된 경우 2번 문장을 사용해야 한다.
 * using문을 선언하고 1번 문장을 사용해도 상관은 없다.
 * 단 using문을 선언하지 않고 2번 문장과 같이 사용해서는 안된다.
 */

// 1 [System.Runtime.InteropServices.DllImport("kernel32")]
// 2 [DllImport("kernel32")]

namespace iniControl
{
	/// <summary>
	/// Create a New INI file to store or load data
	/// </summary>
	public class TIniFile
	{
        string iniPath;

		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section,
			string key, string def, StringBuilder retVal, int size, string filePath);       
        
		[DllImport("kernel32")]
		private static extern long WritePrivateProfileString(string section,
			string key, string val, string filePath);

        public TIniFile(string sName)
        {
            iniPath = sName;
        }

		// INI 값 읽기
        public String ReadString(String Section, String Key)
		{            
			StringBuilder temp = new StringBuilder(255);
			int i = GetPrivateProfileString(Section, Key, "", temp, 255, iniPath);
			return temp.ToString();
		}

        public bool ReadBool(String Section, String Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, iniPath);

            if (temp.ToString() == "1")
                return true;
            else if (temp.ToString() == "True")
                return true;
            else return false;
        }

        public int ReadInteger(String Section, String Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, iniPath);

            if (temp.Length == 0)
                return 0;
            else return Convert.ToInt32(temp.ToString());
        }

        public long ReadLong(String Section, String Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, iniPath);

            if (temp.Length == 0)
                return 0;
            else return Convert.ToInt64(temp.ToString());
        }

        public short ReadShort(String Section, String Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, iniPath);

            if (temp.Length == 0)
                return 0;
            else return Convert.ToInt16(temp.ToString());
        }

        public float ReadFloat(String Section, String Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, iniPath);

            if (temp.Length == 0)
                return 0;
            else return float.Parse(temp.ToString());
        }

        public double ReadDouble(String Section, String Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, iniPath);

            if (temp.Length == 0)
                return 0;
            else return Convert.ToDouble(temp.ToString());
        }

        public string ReadDateTime(String Section, String Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, iniPath);

            if (temp.Length == 0)
                return "";
            else return temp.ToString();            
        }

		// INI 값 설정
        public void WriteString(String Section, String Key, String Value)
		{
			WritePrivateProfileString(Section, Key, Value, iniPath);
            return;
		}

        public void WriteInteger(String Section, String Key, long Value)
        {
            WritePrivateProfileString(Section, Key, Value.ToString(), iniPath);
            return;
        }

        public void WriteInteger(String Section, String Key, int Value)
        {
            WritePrivateProfileString(Section, Key, Value.ToString(), iniPath);
            return;
        }

        public void WriteInteger(String Section, String Key, short Value)
        {
            WritePrivateProfileString(Section, Key, Value.ToString(), iniPath);
            return;
        }

        public void WriteFloat(String Section, String Key, double Value)
        {
            WritePrivateProfileString(Section, Key, Value.ToString(), iniPath);
            return;
        }

        public void WriteFloat(String Section, String Key, float Value)
        {
            WritePrivateProfileString(Section, Key, Value.ToString(), iniPath);
            return;
        }

        public void WriteBool(String Section, String Key, bool Value)
        {
            WritePrivateProfileString(Section, Key, Value.ToString(), iniPath);
            return;
        }

        public void WriteDateTime(String Section, String Key, DateTime Value)
        {
            WritePrivateProfileString(Section, Key, Value.ToString(), iniPath);
            return;
        }
	}
}
