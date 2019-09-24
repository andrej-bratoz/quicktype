using System.Windows;
using System.Windows.Input;

namespace QuickType
{
    public static class UIExtensions
    {
        public static void FocusMe(this IInputElement control)
        {
            control.Focus();
            Keyboard.Focus(control);
        }
    }
}