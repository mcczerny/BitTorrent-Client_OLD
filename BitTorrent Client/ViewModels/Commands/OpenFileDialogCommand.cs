using System;
using System.Windows.Input;

using BitTorrent_Client.Models.TorrentModels;

namespace BitTorrent_Client.ViewModels.Commands
{
    public class OpenFileDialogCommand : ICommand
    {
        public ViewModelBase ViewModel { get; set; }
        public OpenFileDialogViewModel OpenFileDialogViewModel { get; set; }

        public OpenFileDialogCommand(ViewModelBase viewModel, OpenFileDialogViewModel openFileDialogModel)
        {
            this.ViewModel = viewModel;
            this.OpenFileDialogViewModel = openFileDialogModel;
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
            this.ViewModel.OpenFileDialog(parameter as Torrent, OpenFileDialogViewModel.OpenFileDialog());
        }

    }
}