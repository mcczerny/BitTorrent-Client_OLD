using System;
using System.Windows.Input;
using System.Linq;

using BitTorrent_Client.Models.TorrentModels;
namespace BitTorrent_Client.ViewModels.Commands
{
    /// <summary>
    /// This class is used for delegating a torrent selection changed command 
    /// between the view and the view model.
    /// </summary>
    public class SelectionChangedCommand : ICommand
    {
        #region Constructors

        public SelectionChangedCommand(ViewModelBase a_viewModel)
        {
            ViewModel = a_viewModel;
        }

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        // Can always execute.
        public bool CanExecute(object parameter)
        {
            return true;
        }

        // Gets the selected torrent.
        public void Execute(object parameter)
        {
            System.Collections.IList items = (System.Collections.IList)parameter;
            var torrent = items.Cast<Torrent>().FirstOrDefault();

            ViewModel.UpdateSelectedTorrentViews(torrent);
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