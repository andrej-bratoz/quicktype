using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using QuickType.Properties;
using QuickType.Services;

namespace QuickType.UI
{
    public enum UIMode
    {
        Insert, 
        Select
    }

    public class QuickTypeViewModel : INotifyPropertyChanged
    {
        private WindowMode _mode;
        private ObservableCollection<QueryResult> _results;
        private string _inputText;
        private QuickTypeCommand _insertCommandConfirm;
        private IntPtr _lastFocusedWindow;
        private QuickTypeCommand _insertCommandEscape;
        private QuickTypeCommand _insertCommandSwitchMode;
        private QuickTypeCommand _selectCommandConfirm;
        private QuickTypeCommand _anyKeyCommand;
        private QueryResult _selectedItem;
        private int _selectedIndex;
        private string _contextWindowTitle;
        private string _contextWindowExe;


        public QuickTypeViewModel()
        {
            _results = new ObservableCollection<QueryResult>();
        }

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged(nameof(InputText));
            }
        }

        public WindowMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                OnPropertyChanged(nameof(Mode));
            }
        }

        public QuickTypeCommand AnyKeyCommand
        {
            get => _anyKeyCommand;
            set
            {
                _anyKeyCommand = value;
                OnPropertyChanged(nameof(AnyKeyCommand));
            }
        }

        public QuickTypeCommand InsertCommandConfirm
        {
            get => _insertCommandConfirm;
            set
            {
                _insertCommandConfirm = value;
                OnPropertyChanged(nameof(InsertCommandConfirm));
            }
        }

        public QuickTypeCommand InsertCommandEscape
        {
            get => _insertCommandEscape;
            set
            {
                _insertCommandEscape = value;
                OnPropertyChanged(nameof(InsertCommandEscape));
            }
        }

        public QuickTypeCommand InsertCommandSwitchMode
        {
            get => _insertCommandSwitchMode;
            set
            {
                _insertCommandSwitchMode = value;
                OnPropertyChanged(nameof(InsertCommandSwitchMode));
            }
        }

        public QuickTypeCommand SelectCommandConfirm
        {
            get => _selectCommandConfirm;
            set
            {
                _selectCommandConfirm = value;
                OnPropertyChanged(nameof(SelectCommandConfirm));
            }
        }

        public ObservableCollection<QueryResult> Results
        {
            get => _results;
            set
            {
                _results = value;
                OnPropertyChanged(nameof(Results));
            }
        }

        public QueryResult SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged(nameof(SelectedIndex));
            }
        }

        public IntPtr LastFocusedWindow
        {
            get => _lastFocusedWindow;
            set
            {
                _lastFocusedWindow = value;
                ContextWindowTitle = WinApiProxy.GetWindowText(value);
                ContextWindowExe = Process.GetProcesses().FirstOrDefault(x => x.MainWindowHandle == value)?.ProcessName ?? "<??>";
                OnPropertyChanged(nameof(LastFocusedWindow));
            }
        }

        public string ContextWindowTitle
        {
            get => _contextWindowTitle;
            set
            {
                _contextWindowTitle = value;
                OnPropertyChanged(nameof(ContextWindowTitle));
            }
        }

        public string ContextWindowExe
        {
            get => _contextWindowExe;
            set
            {
                _contextWindowExe = value;
                OnPropertyChanged(nameof(ContextWindowExe));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}