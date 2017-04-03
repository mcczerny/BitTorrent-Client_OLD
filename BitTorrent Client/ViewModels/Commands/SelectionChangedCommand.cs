using System;
using System.Windows.Input;

namespace BitTorrent_Client.ViewModels.Commands
{
    public class SelectionChangedCommand : ICommand
    {
        public ViewModelBase ViewModel { get; set; }
        public SelectedTorrentInfoViewModel SelectedTorrentViewModel { get; set; }

        public SelectionChangedCommand(ViewModelBase viewModel)
        {
            this.ViewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this.ViewModel.UpdateViews(parameter as object);
        }


    }
}
