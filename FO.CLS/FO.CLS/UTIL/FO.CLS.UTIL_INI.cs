using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FO.CLS.UTIL
{
    public class INI
    {
        [System.Runtime.InteropServices.DllImport("kernel32")]
        public static extern int WritePrivateProfileString(string section, string key, string val, string filePath);

        [System.Runtime.InteropServices.DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        [System.Runtime.InteropServices.DllImport("kernel32")]
        public static extern uint GetPrivateProfileSectionNames(IntPtr pszReturnBuffer, uint nSize, string lpFileName);

        [DllImport("kernel32.dll")]
        public static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);


        uint MAX_BUFFER = 65535;

        string iniPath;
        StringBuilder sb = new StringBuilder();

        public void setPath(string _path)
        {
            iniPath = _path;
        }

        public List<string> getSectionNames()
        {
            List<string> result = new List<string>();

            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER);
            uint bytesReturned = GetPrivateProfileSectionNames(pReturnedString, MAX_BUFFER, iniPath);

            if(bytesReturned != 0)
            {
                string local = Marshal.PtrToStringAnsi(pReturnedString, (int)bytesReturned).ToString();
                string[] r = local.Substring(0, local.Length - 1).Split('\0');

                result = r.ToList();
            }

            Marshal.FreeCoTaskMem(pReturnedString);
            return result;
        }
        public List<string> getKeys(string section)
        {

            byte[] buffer = new byte[2048];

            GetPrivateProfileSection(section, buffer, 2048, iniPath);
            String[] tmp = Encoding.ASCII.GetString(buffer).Trim('\0').Split('\0');

            List<string> result = new List<string>();

            foreach(String entry in tmp)
            {
                result.Add(entry.Substring(0, entry.IndexOf("=")));
            }

            return result;
        }

        public string readString(string section, string key, string def = "")
        {
            string result = string.Empty;

            sb.Length = 10000;

            GetPrivateProfileString(section, key, def, sb, sb.Length, iniPath);
            if(sb.ToString() != null) result = sb.ToString();

            if(result == string.Empty)
            {
                result = def;
            }

            //byte[] bytes = Encoding.Default.GetBytes(result);
            //result = Encoding.UTF8.GetString(bytes);
            //result = Encoding.ASCII.GetString(bytes);
            //result = Encoding.Unicode.GetString(bytes); // 나중에 Qtx파일 encoding 확인 후 소스사용 * ANCI사용시 소스없어도됨 UTF-8일경우 변경시 깨지는것있음..ANCI권장

            return result;
        }

        public double readDouble(string section, string key, double def = 0)
        {
            string result = string.Empty;

            GetPrivateProfileString(section, key, null, sb, 1024, iniPath);
            if(sb.ToString() != null) result = sb.ToString();

            if(result == string.Empty)
                result = def.ToString();

            return Convert.ToDouble(result);
        }

        public int readInt(string section, string key, int def = 0)
        {
            string result = string.Empty;

            GetPrivateProfileString(section, key, null, sb, 1024, iniPath);
            if(sb.ToString() != null) result = sb.ToString();

            if(result == string.Empty)
                result = def.ToString();

            return Convert.ToInt32(result);
        }

        public void writeString(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, iniPath);
        }
        public void writeDouble(string section, string key, double value)
        {
            WritePrivateProfileString(section, key, value.ToString(), iniPath);
        }
        public void writeInt(string section, string key, int value)
        {
            WritePrivateProfileString(section, key, value.ToString(), iniPath);
        }
    }
}
