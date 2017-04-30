using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using BitTorrent_Client.Models.TorrentModels;
using BitTorrent_Client.Models.TrackerModels;
using BitTorrent_Client.ViewModels.Commands;
using BitTorrent_Client.Models.Utility_Functions;
namespace BitTorrent_Client.ViewModels
{
    public class ViewModelBase
    {
        #region Fields
        private Torrent m_selectedTorrent;
        private SelectedTorrentFilesViewModel m_selectedTorrentFilesViewModel;
        private SelectedTorrentInfoViewModel m_selectedTorrentInfoViewModel;
        private SelectedTorrentPeersViewModel m_selectedTorrentPeersViewModel;
        private SelectedTorrentTrackersViewModel m_selectedTorrentTrackersViewModel;
        private TorrentViewModel m_torrentViewModel;
        private SynchronizationContext uiContext;

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
            this.StartDownloadCommand = new StartDownloadCommand(this, SelectedTorrentInfoViewModel);
            this.PauseDownloadCommand = new PauseDownloadCommand(this, SelectedTorrentInfoViewModel);
        }

        #endregion

        #region Properties

        public PauseDownloadCommand PauseDownloadCommand
        {
            get;
            set;
        }

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

        public StartDownloadCommand StartDownloadCommand
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
            set {
                m_torrentViewModel = value;
            }
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
            if (a_openFileDialog == null)
            {
                return;
            }

            a_torrent = new Torrent();
            a_torrent.SaveDirectory = ChooseSaveDirectory();
            a_torrent.TorrentName = a_openFileDialog.SafeFileName;

            a_torrent.OpenTorrent(a_openFileDialog.FileName);
            TorrentViewModel.Add(a_torrent);

            Task setupTorrent = Task.Factory.StartNew(() =>
            {
                a_torrent.VerifyTorrent();
            });

            uiContext = SynchronizationContext.Current;

            Task start = Task.Run(() =>
            {
                setupTorrent.Wait();
                Start(a_torrent);
            });         
            //a_torrent = new Torrent();

            //a_torrent.SaveDirectory = ChooseSaveDirectory();
            //a_torrent.TorrentName = a_openFileDialog.SafeFileName;

            ////Task OpenTorrent = Task.Run(() =>
            ////{
            ////    a_torrent.OpenTorrent(a_openFileDialog.FileName);
            ////    a_torrent.VerifyTorrent();
            ////});
            //a_torrent.OpenTorrent(a_openFileDialog.FileName);
            //TorrentViewModel.Add(a_torrent);

            ////OpenTorrent.Wait();
            //Task VerifyTorrent = Task.Run(() =>
            //{
            //    a_torrent.VerifyTorrent();
                
            //});

            //Start(a_torrent); 
            

        }

        public void PauseDownload(object parameter)
        {
            var torrent = parameter as Torrent;

            torrent.PausePeers();
        }

        public void StartDownload(object parameter)
        {
            var torrent = parameter as Torrent;

            Start(torrent);
            torrent.ResumeDownloading();

        }

        public void UpdateSelectedTorrentViews(object parameter)
        {
            m_selectedTorrent = parameter as Torrent;

            // Updates file tab.
            SelectedTorrentFilesViewModel.Clear();
            foreach (FileWrapper file in m_selectedTorrent.Files)
            {
                SelectedTorrentFilesViewModel.Add(file);
            }

            // Need to update info tab.
            //
            //

            // Need to update peers tab.
            SelectedTorrentPeersViewModel.Clear();
            foreach (var peer in m_selectedTorrent.Peers)
            {
                SelectedTorrentPeersViewModel.Add(peer.Value);
            }

            // Updates tracker tab.
            SelectedTorrentTrackersViewModel.Clear();
            foreach (Tracker tracker in m_selectedTorrent.Trackers)
            {
                SelectedTorrentTrackersViewModel.Add(tracker);
            }
        }

        #endregion

        #region Private Methods

        public void Start(Torrent a_torrent)
        {

            a_torrent.Status = "Started";
            // Will update trackers.
            Task UpdateTracker = Task.Run(() =>
            {
                while (a_torrent.Started)
                {
                    a_torrent.UpdateTrackers();
                    Thread.Sleep(60000);
                }
            });

           // Will check if there are any blocks to process.
           Task ProcessBlocks = Task.Run(() =>
           {
               while (a_torrent.Started)
               {
                   if (!a_torrent.Complete)
                   {
                       a_torrent.ProcessBlocks();
                       Thread.Sleep(1000);
                   }
               }
           });

            // Will Update peers.
            Task UpdatePeers = Task.Run(() =>
            {
                while (a_torrent.Started)
                {

                    a_torrent.UpdatePeers();
                    Thread.Sleep(15000);
                }
            });

            // Will request blocks.
            Task RequestBlocks = Task.Run(() =>
            {
                while (a_torrent.Started)
                {
                    if (!a_torrent.Complete)
                    {
                        a_torrent.RequestBlocks();
                    }
                    Thread.Sleep(1000);
                }
            });

            Task UpdateGUI = Task.Run(() =>
            {
                while (a_torrent.Started)
                {
                    if(m_selectedTorrent != null)
                    {
                        uiContext.Send(x => UpdateSelectedTorrentViews(m_selectedTorrent), null);
                    }
                    a_torrent.ComputeDownloadSpeed();
                    Thread.Sleep(2000);
                }
            });
        }


        
        #endregion

        #endregion
    }
}