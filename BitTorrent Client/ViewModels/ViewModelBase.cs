using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


using BitTorrent_Client.Models.TorrentModels;
using BitTorrent_Client.Models.PeerModels;
using BitTorrent_Client.Models.TrackerModels;
using BitTorrent_Client.ViewModels.Commands;
using System;
using System.Collections.Generic;

namespace BitTorrent_Client.ViewModels
{
    public class ViewModelBase
    {
        #region Fields

        private SelectedTorrentFilesViewModel m_selectedTorrentFilesViewModel;
        private SelectedTorrentInfoViewModel m_selectedTorrentInfoViewModel;
        private SelectedTorrentPeersViewModel m_selectedTorrentPeersViewModel;
        private SelectedTorrentTrackersViewModel m_selectedTorrentTrackersViewModel;
        private TorrentViewModel m_torrentViewModel;

        #endregion

        #region Constructors 

        public ViewModelBase()
        {
            SelectedTorrentFilesViewModel = new SelectedTorrentFilesViewModel();
            SelectedTorrentInfoViewModel = new SelectedTorrentInfoViewModel();
            SelectedTorrentPeersViewModel = new SelectedTorrentPeersViewModel();
            SelectedTorrentTrackersViewModel = new SelectedTorrentTrackersViewModel();
            TorrentViewModel = new TorrentViewModel();

            this.OpenFileDialogCommand = new OpenFileDialogCommand(this, new OpenFileDialogViewModel());
            this.SelectionChangedCommand = new SelectionChangedCommand(this);

        }

        #endregion

        #region Properties

        public OpenFileDialogCommand OpenFileDialogCommand
        {
            get;
            set;
        }

        public SelectionChangedCommand SelectionChangedCommand
        {
            get;
            set;
        }

        public SelectedTorrentFilesViewModel SelectedTorrentFilesViewModel
        {
            get { return m_selectedTorrentFilesViewModel; }
            set { m_selectedTorrentFilesViewModel = value; }
        }

        public SelectedTorrentInfoViewModel SelectedTorrentInfoViewModel
        {
            get { return m_selectedTorrentInfoViewModel; }
            set { m_selectedTorrentInfoViewModel = value; }
        }

        public SelectedTorrentPeersViewModel SelectedTorrentPeersViewModel
        {
            get { return m_selectedTorrentPeersViewModel; }
            set { m_selectedTorrentPeersViewModel = value; }
        }

        public SelectedTorrentTrackersViewModel SelectedTorrentTrackersViewModel
        {
            get { return m_selectedTorrentTrackersViewModel; }
            set { m_selectedTorrentTrackersViewModel = value; }
        }

        public TorrentViewModel TorrentViewModel
        {
            get { return m_torrentViewModel; }
            set { m_torrentViewModel = value; }
        }

        #endregion

        #region Methods

        #region Public Methods
         
        public string ChooseSaveDirectory()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            folderBrowserDialog.ShowDialog();

            return folderBrowserDialog.SelectedPath;

        }

        public void OpenFileDialog(Torrent a_torrent, Microsoft.Win32.OpenFileDialog a_openFileDialog)
        {
            a_torrent = new Torrent();

            a_torrent.SaveDirectory = ChooseSaveDirectory();
            a_torrent.TorrentName = a_openFileDialog.SafeFileName;
            a_torrent.OpenTorrent(a_openFileDialog.FileName);

            TorrentViewModel.Add(a_torrent);

            //a_torrent.VerifyTorrent();

            Start(a_torrent);

        }

        public void UpdateViews(object parameter)
        {
            System.Collections.IList items = (System.Collections.IList)parameter;
            var collection = items.Cast<Torrent>().FirstOrDefault();

            // Updates file tab.
            SelectedTorrentFilesViewModel.Clear();
            foreach (FileWrapper file in collection.Files)
            {
                SelectedTorrentFilesViewModel.Add(file);
            }

            // Need to update info tab.
            //
            //

            // Need to update peers tab.
            SelectedTorrentPeersViewModel.Clear();
            foreach (var peer in collection.Peers)
            {
                SelectedTorrentPeersViewModel.Add(peer.Value);
            }

            // Updates tracker tab.
            SelectedTorrentTrackersViewModel.Clear();
            foreach (Tracker tracker in collection.Trackers)
            {
                SelectedTorrentTrackersViewModel.Add(tracker);
            }
        }

        #endregion

        #region Private Methods

        private void Start(Torrent a_torrent)
        {
            a_torrent.Started = true;

            // Will update trackers.
            Task UpdateTracker = Task.Run(() =>
            {
                while (a_torrent.Started)
                {
                    a_torrent.UpdateTrackers();
                    Thread.Sleep(5000);
                }
            });

            // Will check if there are any blocks to process.
            Task ProcessBlocks = Task.Run(() =>
            {
                while (a_torrent.Started)
                {
                    a_torrent.ProcessBlocks();
                }
            });

            Task UpdatePeers = Task.Run(() =>
            {
                while (a_torrent.Started)
                {
                    a_torrent.UpdatePeers();
                    Thread.Sleep(5000);
                }
            });

            Task RequestBlocks = Task.Run(() =>
            {
                while (a_torrent.Started)
                {
                    a_torrent.RequestBlocks();
                    //Thread.Sleep(500);
                }
            });


        }

        #endregion


        #endregion
    }
}