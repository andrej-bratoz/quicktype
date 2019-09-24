using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using QuickType.Enums;
using QuickType.UI;

namespace QuickType.Services
{

    public static class HookManager
    {
        private static WinEventCallback _mainWin;
        private static WinEventProc _proc = WindowEventCallback;
        private static IntPtr windowEventHook;
        public delegate void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);


        private const int WINEVENT_INCONTEXT = 4;
        private const int WINEVENT_OUTOFCONTEXT = 0;
        private const int WINEVENT_SKIPOWNPROCESS = 2;
        private const int WINEVENT_SKIPOWNTHREAD = 1;
        private const int EVENT_SYSTEM_FOREGROUND = 3;

        public static void SubscribeToWindowEvents()
        {
            if (windowEventHook == IntPtr.Zero)
            {
                windowEventHook = WinApiProxy.SetWinEventHook(
                    EVENT_SYSTEM_FOREGROUND,    // eventMin
                    EVENT_SYSTEM_FOREGROUND,    // eventMax
              IntPtr.Zero,                // hmodWinEventProc
                    _proc,                             // lpfnWinEventProc
                    0,                         // idProcess
                    0,                          // idThread
                    WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);

                if (windowEventHook == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }

        public static void RegisterCallBack(ref WinEventCallback window)
        {
            _mainWin = window;
        }

        private static void WindowEventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (eventType == EVENT_SYSTEM_FOREGROUND)
            {
                _mainWin?.Invoke(hwnd);
            }
            Debug.WriteLine("Event {0}", hwnd);
        }
    }
}
