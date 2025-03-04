using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FO.CLS.UTIL
{
    public class VKey
    {
        #region 변수
        // 가상 키보드 경로
        private const string VIRTUAL_KEYBOARD_PATH = "C:\\Program Files\\Common Files\\Microsoft shared\\ink\\TabTip.exe";

        // 가상 키보드 윈도우 상수
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(int hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(String sClassName, String sAppName);
        #endregion

        #region 생성자
        public VKey()
        {

        }

        #endregion

        #region 메서드
        /// <summary>
        /// 가상 키보드 열기
        /// </summary>
        public void OpenVKey()
        {
            ProcessStartInfo process = new ProcessStartInfo(VIRTUAL_KEYBOARD_PATH);
            process.WindowStyle = ProcessWindowStyle.Hidden;

            Process.Start(process);
        }

        /// <summary>
        /// 가상 키보드 닫기
        /// </summary>
        public void CloseVKey()
        {
            uint WM_SYSCOMMAND = 274;
            uint SC_CLOSE = 61536;
            IntPtr KeyboardWnd = FindWindow("IPTip_Main_Window", null);

            PostMessage(KeyboardWnd.ToInt32(), WM_SYSCOMMAND, (int)SC_CLOSE, 0);
        }
        #endregion
    }
}
