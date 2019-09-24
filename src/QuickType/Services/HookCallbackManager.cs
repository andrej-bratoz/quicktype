using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using QuickType.UI;

namespace QuickType.Services
{
    public static class HookCallbackManager
    {
        public static IView View => System.Windows.Application.Current.MainWindow as IView;
        public static  IntPtr WindowsKeyHookCallback(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case KeyHookConstants.HOTKEY_ID:
                        {
                            if (View.IsVisible)
                            {
                                View.Hide();
                                break;
                            }
                            else
                            {
                                (View.DataContext as QuickTypeViewModel).LastFocusedWindow = WinApiProxy.GetForegroundWindow();
                                View.Show();
                                WinApiProxy.SetForegroundWindow(View.Handle);
                                break;
                            }

                        }
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        public static void WindowsWindowCreatedHook(IntPtr handle)
        {
            var windowName = WinApiProxy.GetWindowText(handle);
            if (string.IsNullOrEmpty(windowName)) return;
            Debug.WriteLine($"Window Created = {windowName}");
            var names = QuickTypeCommandManager.Instance.AutoKillWindowNames();
            if (names.Any(x => x.Item1 == windowName))
            {
                var command = names.First(x => x.Item1 == windowName);
                var keys = command.Item2;
                if (!string.IsNullOrEmpty(keys))
                {
                    WinApiProxy.SetForegroundWindow(handle);
                    SendKeys.SendWait(keys);
                }
                else
                {
                    WinApiProxy.CloseWindow(handle);
                }
            }
        }
    }
}