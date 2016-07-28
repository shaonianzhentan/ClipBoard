using GoodHelper.WinAPIHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ClipBoard
{
    static class Program
    {
        //主窗体的标题名称
        static string FormName = "小白剪切板";
        //当前程序的进程
        static Process pro = null;   

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createdNew;
            //系统能够识别有名称的互斥，因此可以使用它禁止应用程序启动两次
            //第二个参数可以设置为产品的名称:Application.ProductName
            Mutex mutexApp = new Mutex(false, Assembly.GetExecutingAssembly().FullName, out createdNew);
            //如果已运行，则在前端显示
            //createdNew == false，说明程序已运行
            if (!createdNew)
            {
                pro = GetExistProcess();                
                if (pro != null)
                {
                    SetForegroud(pro);
                    Application.Exit();
                    return;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        /// <summary>
        /// 查看程序是否已经运行
        /// </summary>
        /// <returns></returns>
        private static Process GetExistProcess()
        {
            Process currentProcess = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(currentProcess.ProcessName))
            {
                if ((process.Id != currentProcess.Id) &&
                    (Assembly.GetExecutingAssembly().Location == currentProcess.MainModule.FileName))
                {
                    return process;
                }
            }
            return null;
        }
        /// <summary>
        /// 使程序前端显示
        /// </summary>
        /// <param name="instance"></param>
        private static void SetForegroud(Process instance)
        {
            IntPtr mainFormHandle = instance.MainWindowHandle;
            //判断程序是否隐藏到托盘中
            if (mainFormHandle != IntPtr.Zero)
            {
                //在界面上显示，则将窗体置顶
                WindowsAPI.ShowWindow(mainFormHandle, WindowsAPI.SW_SHOW);
                WindowsAPI.ShowWindowAsync(mainFormHandle, 1);
                WindowsAPI.SetForegroundWindow(mainFormHandle);
            }
            else {
                //隐藏的情况下，则去遍历所有窗体，去找到对应的窗体，然后显示
                WindowsAPI.EnumWindowsProc myCallBack = new WindowsAPI.EnumWindowsProc(Report);
                WindowsAPI.EnumWindows(myCallBack, 0);
            }
        }     

        private static bool Report(IntPtr hwnd, int lParam)
        {
            //获得窗体标题
            StringBuilder sb = new StringBuilder(100);
            WindowsAPI.GetWindowText(hwnd, sb, sb.Capacity);

            int calcID = 0;
            //获取进程ID   
            WindowsAPI.GetWindowThreadProcessId(hwnd, ref calcID);
            if ((sb.ToString() == FormName) && (pro != null) && (calcID == pro.Id)) //标题栏、进程id符合
            {
                WindowsAPI.ShowWindow(hwnd,WindowsAPI.SW_RESTORE);
                WindowsAPI.SwitchToThisWindow(hwnd, true);

                System.Drawing.Rectangle windowRec = new System.Drawing.Rectangle(0, 0, 0, 0);
                WindowsAPI.GetWindowRect(hwnd, ref windowRec);
                System.Drawing.Rectangle rect = System.Windows.Forms.SystemInformation.VirtualScreen;
                WindowsAPI.SetWindowPos(hwnd, WindowsAPI.HWND_TOP, (rect.Width - (windowRec.Right - windowRec.Left)) / 2,
                    (rect.Height - (windowRec.Bottom - windowRec.Top)) / 2, 0, 0, WindowsAPI.SWP_NOSIZE);
                return true;
            }            
            return true;
        }

    }
}
