using System;
using System.Windows.Input;

using BitTorrent_Client.Models.TorrentModels;

namespace BitTorrent_Client.ViewModels.Commands
{
    /// <summary>
    /// This class is used for delegating an open file dialog command between
    /// the view and view model.
    /// </summary>
    public class OpenFileDialogCommand : ICommand
    {
        #region Constructors

        public OpenFileDialogCommand(ViewModelBase a_viewModel, 
            OpenFileDialogViewModel a_openFileDialogModel)
        {
            ViewModel = a_viewModel;
            OpenFileDialogViewModel = a_openFileDialogModel;
        }

        #endregion

        #region Events

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
            this.ViewModel.OpenFileDialog(parameter as Torrent,
                OpenFileDialogViewModel.OpenFileDialog());
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get/Private set the base view model of the client.
        /// </summary>
        public ViewModelBase ViewModel
        {
            get;
            private set;
        }

        /// <summary>
        /// Get/Private set the open file dialog view model to open a file dialog.
        /// </summary>
        public OpenFileDialogViewModel OpenFileDialogViewModel
        {
            get;
            private set;
        }

        #endregion

    }
}