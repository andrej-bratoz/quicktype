using System;

namespace QuickType.Enums
{
    public delegate IntPtr CBTProc(int code, IntPtr wParam, IntPtr lParam);

    public delegate void WinEventCallback(IntPtr hwand);
}