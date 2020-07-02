using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ODS
{
    class ODSPublic
    {       
        [DllImport("kernel32.dll")]
        public static extern long WritePrivateProfileString(
            string section, string key, string val, string filePath);

        [DllImport("Kernel32.dll")]
        public static extern int GetPrivateProfileString(
         string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString,
            int nSize, string lpFileName);


        public void Write_spec(string Section, string Key, string Value)
        {
            string path = "MODEL_SPEC\\" + Section + ".cfg";
            WritePrivateProfileString(Section, Key, Value, path);
        }

        public string Read_spec(string Section, string Key)
        {
            string path = "MODEL_SPEC\\" + Section + ".cfg";
            int a;
            StringBuilder temp = new StringBuilder(1024);
            a = GetPrivateProfileString(Section, Key, string.Empty, temp, 1024, path);
            
            return temp.ToString();
        }

        public void Write_CNT(string Section, string Key, string Value)
        {
            string path = "CNT\\"+ Section + ".cfg";
            WritePrivateProfileString(Section, Key, Value, path);
        }

        public string Read_CNT(string Section, string Key)
        {
            string path = "CNT\\"+ Section + ".cfg";
            int a;
            StringBuilder temp = new StringBuilder(1024);
            a = GetPrivateProfileString(Section, Key, string.Empty, temp, 1024, path);

            return temp.ToString();
        }

        public void Write_SET(string Section, string Key, string Value)
        {
            string Path;

            if(Section == "Config")
                    Path = "SYSTEM\\" + Section + ".cfg";
            else if (Section == "TEST SPEC")
                    Path = "SPEC\\" + Section + ".cfg";
            else    Path = "SETTING\\" + Section + ".cfg";

            WritePrivateProfileString(Section, Key, Value, Path);
            return;
        }

        public string Read_SET(string Section, string Key)
        {
            string Path;
            int ret;
                        
            if (Section == "Config")
                    Path = "SYSTEM\\" + Section + ".cfg";
            else if (Section == "TEST SPEC")
                    Path = "SPEC\\" + Section + ".cfg";
            else    Path = "SETTING\\" + Section + ".cfg";

            StringBuilder temp = new StringBuilder(1024);
            ret = GetPrivateProfileString(Section, Key, string.Empty, temp, 1024, Path);

            if(temp.Length == 0)
            {

                if ((Key == "X_ONECYCELTOPULSE") || (Key == "Y_ONECYCELTOPULSE") || (Key == "Z_ONECYCELTOPULSE"))
                {
                    return "8000.0";
                }
                else if ((Key == "X_DIR") || (Key == "Y_DIR") || (Key == "Z_DIR"))
                {
                    return "4";
                }
                else if (Key == "ConnectorCheck")
                {
                    return "1";
                }
                else
                {
                    return temp.ToString();
                }
            }
            else return temp.ToString();
        }

        public string Read_BAR(string Section, string Key)
        {
            string path = "BARCODE\\" + Section + ".cfg";
            int a;
            StringBuilder temp = new StringBuilder(1024);
            a = GetPrivateProfileString(Section, Key, string.Empty, temp, 1024, path);

            return temp.ToString();
        }

        public void Write_BAR(string Section, string Key, string Value)
        {
            string path = "BARCODE\\" + Section + ".cfg";
            WritePrivateProfileString(Section, Key, Value, path);
            return;
        }
    }
}
