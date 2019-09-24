using System;
using System.Windows.Input;

namespace QuickType.UI
{
    public class QuickTypeCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            ExecuteAction?.Invoke(parameter);
        }

        public Action<object> ExecuteAction { get; set; }

        public event EventHandler CanExecuteChanged;
    }
}