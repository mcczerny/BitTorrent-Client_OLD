using System;
using System.Windows.Input;
using System.Linq;
using BitTorrent_Client.Models.TorrentModels;
namespace BitTorrent_Client.ViewModels.Commands
{
    public class StartDownloadCommand : ICommand
    {
        public ViewModelBase ViewModel { get; set; }
        public SelectedTorrentInfoViewModel SelectedTorrentViewModel { get; set; }

        public StartDownloadCommand(ViewModelBase a_viewModel, 
            SelectedTorrentInfoViewModel a_selectedTorrentViewModel)
        {
            this.ViewModel = a_viewModel;
            this.SelectedTorrentViewModel = a_selectedTorrentViewModel;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

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

        public void Execute(object parameter)
        {
            System.Collections.IList items = (System.Collections.IList)parameter;
            var torrent = items.Cast<Torrent>().FirstOrDefault();

            ViewModel.StartDownload(torrent);
        }

    }
}