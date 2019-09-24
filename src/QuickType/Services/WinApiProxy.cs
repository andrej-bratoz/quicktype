using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using QuickType.Enums;

namespace QuickType.Services
{
    public class WinApiProxy
    {
        [DllImport("ole32.dll")]
        public static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseWindow(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int GetClassName(int hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(HookType hookType, CBTProc hookProc, IntPtr instancePtr, uint threadID);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hookPtr);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint GetWindowModuleFileName(IntPtr hwnd, StringBuilder lpszFileName, uint cchFileNameMax);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hookPtr, int nCode, IntPtr wordParam, IntPtr longParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetProcessIdOfThread(IntPtr handle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWinEventHook(int eventMin, int eventMax, IntPtr hmodWinEventProc, HookManager.WinEventProc lpfnWinEventProc, int idProcess, int idThread, int dwflags);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int UnhookWinEvent(IntPtr hWinEventHook);

        // Delegate to filter which windows to include 
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary> Get the text for the window pointed to by hWnd </summary>
        public static string GetWindowText(IntPtr hWnd)
        {
            int size = GetWindowTextLength(hWnd);
            if (size > 0)
            {
                var builder = new StringBuilder(size + 1);
                GetWindowText(hWnd, builder, builder.Capacity);
                return builder.ToString();
            }

            return String.Empty;
        }

        /// <summary> Find all windows that match the given filter </summary>
        /// <param name="filter"> A delegate that returns true for windows
        ///    that should be returned and false for windows that should
        ///    not be returned </param>
        public static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
        {
            IntPtr found = IntPtr.Zero;
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                if (filter(wnd, param))
                {
                    // only add the windows that pass the filter
                    windows.Add(wnd);
                }

                // but return true here so that we iterate all windows
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        public static IEnumerable<IntPtr> FindWindowsWithText(string titleText)
        {
            return FindWindows(delegate (IntPtr wnd, IntPtr param)
            {
                return GetWindowText(wnd).Contains(titleText);
            });
        }

        public static int[] GetZOrder(params IntPtr[] hWnds)
        {
            var z = new int[hWnds.Length];
            for (var i = 0; i < hWnds.Length; i++) z[i] = -1;

            var index = 0;
            var numRemaining = hWnds.Length;
            EnumWindows((wnd, param) =>
            {
                var searchIndex = Array.IndexOf(hWnds, wnd);
                if (searchIndex != -1)
                {
                    z[searchIndex] = index;
                    numRemaining--;
                    if (numRemaining == 0) return false;
                }
                index++;
                return true;
            }, IntPtr.Zero);

            return z;
        }

        public static bool IsExplorer(IntPtr handle)
        {
            const int maxChars = 256;
            StringBuilder className = new StringBuilder(maxChars);


            if (GetClassName(handle.ToInt32(), className, maxChars) > 0)
            {
                string cName = className.ToString();
                if (cName == "Progman" || cName == "WorkerW" || cName == "CabinetWClass"
                    || cName == "Explorer­WClass" || cName == "Cabinet")
                {
                    return true;
                }

                return false;
            }

            return false;
        }
    }
}