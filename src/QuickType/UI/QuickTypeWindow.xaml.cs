using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using QuickType.Enums;
using QuickType.Services;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace QuickType.UI
{
    /// <summary>
    /// Interaction logic for QuickTypeWindow.xaml
    /// </summary>
    public partial class QuickTypeWindow : Window, IView
    {
        private HwndSource _source;
        private IntPtr _windowHandle;
        private static WinEventCallback _windowEventCallback;

        public event Action OnLoaded;
        public IntPtr Handle { get; private set; }
        public void ExecuteOnView(Action a)
        {
            Dispatcher.Invoke(a);
        }

        public QuickTypeWindow()
        {
            InitializeComponent();
            Loaded += (sender, args) => OnLoaded?.Invoke();
            Deactivated += (sender, args) => Hide();
            Bootstrapper.Start<QuickTypeViewModel,QuickTypePresenter>(this);
        }
       
        private void FocusSelectionBox()
        {
            Dispatcher?.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                additionalGrid.FocusMe();
                if (additionalGrid.Items.Count > 0) additionalGrid.SelectedIndex = 0;
            }));
        }

        private void FocusInputBox()
        {
            Dispatcher?.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                input.FocusMe();
                input.SelectAll();
            }));
        }

        public void SetMode(UIMode mode)
        {
            if(mode == UIMode.Insert) FocusInputBox();
            else FocusSelectionBox();
        }

      

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HookCallbackManager.WindowsKeyHookCallback);
            WinApiProxy.UnregisterHotKey(_windowHandle, KeyHookConstants.HOTKEY_ID);
            base.OnClosed(e);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _windowHandle = new WindowInteropHelper(this).Handle;
            Handle = _windowHandle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HookCallbackManager.WindowsKeyHookCallback);

            WinApiProxy.RegisterHotKey(_windowHandle, KeyHookConstants.HOTKEY_ID, KeyHookConstants.MOD_ALT, KeyHookConstants.VK_OEM_COMMA); //ALT + ,
            _windowEventCallback = HookCallbackManager.WindowsWindowCreatedHook;
            HookManager.RegisterCallBack(ref _windowEventCallback);
            HookManager.SubscribeToWindowEvents();
        }
    }
}
