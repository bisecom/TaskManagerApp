using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Text;
using TaskManager.TabsModels;
using System.Collections.ObjectModel;

namespace TaskManager.OperateDll
{
    public static class user32
    {
        /// <summary>
        /// filter function
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        /// <summary>
        /// check if windows visible
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        /// <summary>
        /// return windows text
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpWindowText"></param>
        /// <param name="nMaxCount"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetWindowText",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        /// <summary>
        /// enumarator on all desktop windows
        /// </summary>
        /// <param name="hDesktop"></param>
        /// <param name="lpEnumCallbackFunction"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        /// <summary>
        /// entry point of the program
        /// </summary>
        /// 
        //----------------------------Closing task
        
        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_CLOSE = 0xF060;

        //---------------------------
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// Find window by Caption only. Note you must pass IntPtr.Zero as the first parameter.
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        const UInt32 WM_CLOSE = 0x0010;

        //---------------------------
        static user32() { }

        public static ObservableCollection<AppsModel> GetHandles()
        {
            var collection = new List<string>();
            ObservableCollection<AppsModel> namesOut = new ObservableCollection<AppsModel>();
            user32.EnumDelegate filter = delegate (IntPtr hWnd, int lParam)
            {
                StringBuilder strbTitle = new StringBuilder(255);
                int nLength = user32.GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
                string strTitle = strbTitle.ToString();

                if (user32.IsWindowVisible(hWnd) && string.IsNullOrEmpty(strTitle) == false)
                {
                    collection.Add(strTitle);
                }
                return true;
            };

            if (user32.EnumDesktopWindows(IntPtr.Zero, filter, IntPtr.Zero))
            {
                foreach (var item in collection)
                {
                    if (item != "Program Manager" && item != "Пуск")
                    {
                        AppsModel temp = new AppsModel();
                        temp.AppsDescription = item;
                        temp.AppsWorkingCondition = "Работает";
                        namesOut.Add(temp);
                    }
                }
            }
            return namesOut;
        }

        //private static void closeWindow()
        //{
        //    // retrieve the handler of the window of Word
        //    // Class name, Window Name
        //    int iHandle = FindWindow("OpusApp", "Document1 - Microsoft Word");
        //    if (iHandle > 0)
        //    {
        //        //close the window using API
        //        SendMessage(iHandle, WM_SYSCOMMAND, SC_CLOSE, 0);
        //    }
        //}

        public static void closeWindow1(string str)
        {
            IntPtr windowPtr = FindWindowByCaption(IntPtr.Zero, str);
            if (windowPtr == IntPtr.Zero)
            {
                return;
            }
            SendMessage(windowPtr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }


    }
}
