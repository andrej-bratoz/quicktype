using System;

namespace QuickType.UI
{
    public interface IView
    {
        void SetMode(UIMode mode);
        object DataContext { get; set; }
        event Action OnLoaded;
        void Hide();
        void Show();
        IntPtr Handle { get; }
        bool IsVisible { get; }
    }
}