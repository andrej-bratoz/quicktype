using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using QuickType.Services;

namespace QuickType.UI
{
    public class PresenterBase<TView, TViewModel>
    {
        public TView View { get; }
        public TViewModel ViewModel { get; }

        public PresenterBase(TView view, TViewModel viewModel)
        {
            View = view;
            ViewModel = viewModel;
        }

        public virtual void OnInitialize()
        {
            OnInitializeCommands();
        }
        protected virtual void OnInitializeCommands() { }
    }

    public class QuickTypePresenter : PresenterBase<IView, QuickTypeViewModel>
    {
        private ICollectionView _collectionView;
        public QuickTypePresenter(IView view, QuickTypeViewModel viewModel) : 
            base(view, viewModel)
        {
            CommandFactory.InitializeFromFile();
            ViewModel.PropertyChanged += async (sender, args) =>
            {
                if (args.PropertyName == nameof(QuickTypeViewModel.LastFocusedWindow))
                {
                    await OnLastFocusedWindowChanged();
                }
                else if (args.PropertyName == nameof(QuickTypeViewModel.Mode))
                {
                    if (ViewModel.Mode == WindowMode.InputCommand)
                    {
                        View.SetMode(UIMode.Insert);
                        _collectionView.Filter = o => true;
                    }
                    else
                    {
                        View.SetMode(UIMode.Insert);
                        _collectionView.Filter = Filter;
                    }
                }
            };
        }

        private bool Filter(object obj)
        {
            var item = obj as QueryResult;
            if (item == null) return true;
            return item.Name.Contains(ViewModel.InputText);
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
            ViewModel.InputText = string.Empty;
            ViewModel.Results = new ObservableCollection<QueryResult>();
            _collectionView = CollectionViewSource.GetDefaultView(ViewModel.Results);
            ViewModel.Mode = WindowMode.InputCommand;

        }

        protected override void OnInitializeCommands()
        {
            ViewModel.InsertCommandConfirm = new QuickTypeCommand();
            ViewModel.InsertCommandConfirm.ExecuteAction = InsertCommandConfirmExecute;

            ViewModel.InsertCommandEscape = new QuickTypeCommand();
            ViewModel.InsertCommandEscape.ExecuteAction = InsertCommandEscapeExecute;

            ViewModel.InsertCommandSwitchMode = new QuickTypeCommand();
            ViewModel.InsertCommandSwitchMode.ExecuteAction = InsertCommandSwitchModeExecute;

            ViewModel.SelectCommandConfirm = new QuickTypeCommand();
            ViewModel.SelectCommandConfirm.ExecuteAction = SelectCommandConfirmExecute;

            ViewModel.AnyKeyCommand = new QuickTypeCommand();
            ViewModel.AnyKeyCommand.ExecuteAction = AnyKeyCommandExecute;
        }

        

        private async Task OnLastFocusedWindowChanged()
        {
            QuickTypeCommandManager.Instance.WindowFocus = ViewModel.LastFocusedWindow;
            ViewModel.Results.Clear();
            List<QueryResult> commands = new List<QueryResult>();
            await Task.Run(() => { commands = QuickTypeCommandManager.Instance.GetContextCommands(); });
            commands.ForEach(x => ViewModel.Results.Add(x));
            ViewModel.Mode = WindowMode.Filter;
            View.SetMode(UIMode.Insert);
        }


        // F1
        private void InsertCommandSwitchModeExecute(object obj)
        {
            if(ViewModel.Mode == WindowMode.Filter)
            {
                ViewModel.Mode = WindowMode.InputCommand;
            }
            else
            {
                ViewModel.Mode = WindowMode.Filter;
            }
        }

        // ESC
        private void InsertCommandEscapeExecute(object obj)
        {
            ViewModel.InputText = string.Empty;
            View.Hide();
        }

        // Enter
        private void InsertCommandConfirmExecute(object obj)
        {
            if (ViewModel.Mode == WindowMode.Filter)
            {
                if (((CollectionView)_collectionView).Count == 1)
                {
                    var data = (QueryResult)((CollectionView) _collectionView).GetItemAt(0);
                    data.Data?.Invoke();
                    return;
                }
                View.SetMode(UIMode.Select);
            }
            else if(ViewModel.Mode == WindowMode.InputCommand)
            {
                var availableCommands = QuickTypeCommandManager.Instance.ExecuteQuery(ViewModel.InputText);
                ViewModel.Results.Clear();
                availableCommands.ForEach(x => ViewModel.Results.Add(x));
                ViewModel.InputText = string.Empty;
                ViewModel.Mode = WindowMode.Filter;
                View.SetMode(UIMode.Insert);
            }
        }

        // Enter in grid
        private void SelectCommandConfirmExecute(object obj)
        {
            if (ViewModel.Results.Any())
            {
                ViewModel.SelectedItem?.Data?.Invoke();
            }
            else
            {
                View.SetMode(UIMode.Insert);
            }
        }

        private void AnyKeyCommandExecute(object obj)
        {
            _collectionView.Refresh();
        }
    }
}
