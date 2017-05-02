using System;
using System.Windows.Input;
using System.Linq;
using BitTorrent_Client.Models.TorrentModels;
namespace BitTorrent_Client.ViewModels.Commands
{
    /// <summary>
    /// This class is used for delegating a start download command between the 
    /// view and the view model.
    /// </summary>
    public class StartDownloadCommand : ICommand
    {
        #region Constructors

        public StartDownloadCommand(ViewModelBase a_viewModel, 
            SelectedTorrentInfoViewModel a_selectedTorrentViewModel)
        {
            ViewModel = a_viewModel;
            SelectedTorrentViewModel = a_selectedTorrentViewModel;
        }

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        // Can only execute when the torrent is stopped.
        public bool CanExecute(object parameter)
        {
            if (parameter != null)
            {
                System.Collections.IList items = (System.Collections.IList)parameter;
                var torrent = items.Cast<Torrent>().FirstOrDefault();
                if(torrent != null)
                {
                    if (!torrent.Started)
                    {
                        return true;
                    }
                }
             
            }
            return false;
        }

        // Call StarDownload for selected torrent.
        public void Execute(object parameter)
        {
            System.Collections.IList items = (System.Collections.IList)parameter;
            var torrent = items.Cast<Torrent>().FirstOrDefault();

            ViewModel.StartDownload(torrent);
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
        /// Get/Private set the selected torrent.
        /// </summary>
        public SelectedTorrentInfoViewModel SelectedTorrentViewModel
        {
            get;
            private set;
        }

        #endregion
    }
}