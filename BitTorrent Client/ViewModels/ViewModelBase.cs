using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using BitTorrent_Client.Models.TorrentModels;
using BitTorrent_Client.Models.TrackerModels;
using BitTorrent_Client.ViewModels.Commands;
using BitTorrent_Client.Models.Utility_Functions;

namespace BitTorrent_Client.ViewModels
{
    /// <summary>
    /// This class acts as the main view model for the the BitTorrent Client. 
    /// </summary>
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

            OpenFileDialogCommand = new OpenFileDialogCommand(this, new OpenFileDialogViewModel());
            SelectionChangedCommand = new SelectionChangedCommand(this);
            StartDownloadCommand = new StartDownloadCommand(this, SelectedTorrentInfoViewModel);
            PauseDownloadCommand = new PauseDownloadCommand(this, SelectedTorrentInfoViewModel);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get/Set the PauseDownloadCommand object for view.
        /// </summary>
        public PauseDownloadCommand PauseDownloadCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set the OpenFileDialogCommand object for the view.
        /// </summary>
        public OpenFileDialogCommand OpenFileDialogCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set the SeleectionChangedCommand object for the view.
        /// </summary>
        public SelectionChangedCommand SelectionChangedCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set the StartDownloadCommand object for the view.
        /// </summary>
        public StartDownloadCommand StartDownloadCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set the files view model for the selected torrent.
        /// </summary>
        public SelectedTorrentFilesViewModel SelectedTorrentFilesViewModel
        {
            get { return m_selectedTorrentFilesViewModel; }
            set { m_selectedTorrentFilesViewModel = value; }
        }
        
        /// <summary>
        /// Get/Set the the torrent meta-data view model for the selected torrent.
        /// </summary>
        public SelectedTorrentInfoViewModel SelectedTorrentInfoViewModel
        {
            get { return m_selectedTorrentInfoViewModel; }
            set { m_selectedTorrentInfoViewModel = value; }
        }

        /// <summary>
        /// Get/Set the peers view model for the selected torrent.
        /// </summary>
        public SelectedTorrentPeersViewModel SelectedTorrentPeersViewModel
        {
            get { return m_selectedTorrentPeersViewModel; }
            set { m_selectedTorrentPeersViewModel = value; }
        }

        /// <summary>
        /// Get/Set the tracker view model for the selected torrent.
        /// </summary>
        public SelectedTorrentTrackersViewModel SelectedTorrentTrackersViewModel
        {
            get { return m_selectedTorrentTrackersViewModel; }
            set { m_selectedTorrentTrackersViewModel = value; }
        }

        /// <summary>
        /// Get/Sest the torrenet view model for displaying all torrents.
        /// </summary>
        public TorrentViewModel TorrentViewModel
        {
            get { return m_torrentViewModel; }
            set { m_torrentViewModel = value; }
        }
        
        #endregion

        #region Methods

        #region Public Methods
         
        /// <summary>
        /// Allows user to choose save directory for download.
        /// </summary>
        /// <returns>Returns the selected save directory.</returns>
        /// <remarks>
        /// ChooseSaveDirectory()
        /// 
        /// SYNOPSIS
        /// 
        ///     ChooseSaveDirectory()
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will open a folder browser dialog to allow the user
        ///     to choose a save directory for the torrent that is being added
        ///     to the client. It will return the selected save directory.
        ///     
        /// </remarks>
        public string ChooseSaveDirectory()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            var result = folderBrowserDialog.ShowDialog();
            if(result == DialogResult.OK)
            {
                return folderBrowserDialog.SelectedPath;
            }

            return null;
        }

        /// <summary>
        /// Open a file dialog to allow the user to select a torrent to add.
        /// </summary>
        /// <param name="a_torrent">The torrent being added</param>
        /// <param name="a_openFileDialog">The open file dialog.</param>
        /// <remarks>
        /// OpenFileDialog()
        /// 
        /// SYNOPSIS
        /// 
        ///     OpenFileDialog(Torrenet a_torrent, 
        ///         Microsoft.Win32.OpenFileDialog a_openFileDialog);
        ///         
        /// DESCRIPTION
        /// 
        ///     This function will handle after an open file dialog is called. It
        ///     will check if the user choose a file. If no file was chosen, the
        ///     function will return. If not it will initialize a new torrent and
        ///     the user will then choose a save directory. If a save location has
        ///     been chosen. It will attempt to open the file and if the torrent
        ///     can be opened it will add the torrent to the torrent view model.
        ///     It will then start a task for verifying the torrent and starting it.
        ///     
        /// </remarks>
        public void OpenFileDialog(Torrent a_torrent, 
            Microsoft.Win32.OpenFileDialog a_openFileDialog)
        {
            // When user selected no file.
            if (a_openFileDialog == null)
            {
                return;
            }

            // Setup new torrent object and add to the torrent view model.
            a_torrent = new Torrent();
            a_torrent.SaveDirectory = ChooseSaveDirectory();

            // Checks if user selected a save location.
            if(a_torrent.SaveDirectory == null)
            {
                var saveLocationError = MessageBox.Show("No save location chosen", "Error");
                return;
            }

            a_torrent.TorrentName = a_openFileDialog.SafeFileName;

            // Tries to open .torrent file. 
            try
            {
                a_torrent.OpenTorrent(a_openFileDialog.FileName);

            }
            // If torrent is invalidly bencoded. 
            catch (FormatException e)
            {
                var errorMessage = MessageBox.Show(e.Message.ToString(), "Error");
                return;
            }

            TorrentViewModel.Add(a_torrent);

            // Start task verifying torrent allowing GUI to respond to user input.
            Task setupTorrent = Task.Factory.StartNew(() =>
            {
                a_torrent.VerifyTorrent();
            });

            // Get the uiContext allowing the GUI to be later updated through
            // threads that are not the GUI thread.
            uiContext = SynchronizationContext.Current;

            // Wait for the task setupTorrent to run and then start a new task
            // that will start the torrent.
            Task start = Task.Run(() =>
            {
                setupTorrent.Wait();
                Start(a_torrent);
            });         
        }

        /// <summary>
        /// Pauses the current selected torrent.
        /// </summary>
        /// <param name="a_torrent">The torrent to pause.</param>
        /// <remarks>
        /// PauseDownload()
        /// 
        /// SYNOPSIS
        /// 
        ///     void PauseDownload(object a_torrent);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function is used when the pause download commmand is executed.
        ///     It will call the function PausePeers for the torrent stopping 
        ///     network communications.
        ///     
        /// </remarks>
        public void PauseDownload(object a_torrent)
        {
            var torrent = a_torrent as Torrent;
            torrent.PausePeers();
        }

        /// <summary>
        /// Starts the current selected torrent.
        /// </summary>
        /// <param name="a_torrent">The torrent to pause.</param>
        /// <remarks>
        /// StartDownload()
        /// 
        /// SYNOPSIS
        /// 
        ///     void StartDownload(object a_torrent);
        /// 
        /// DESCRIPTION
        /// 
        ///     This function is used when the start download command is executed.
        ///     It will call resume network communications for the selected 
        ///     torrent.
        ///     
        /// </remarks>
        public void StartDownload(object a_torrent)
        {
            var torrent = a_torrent as Torrent;

            Task start = Task.Run(() =>
            {
                torrent.Started = true;
                Start(torrent);
                torrent.ResumeDownloading();
            });
        }

        /// <summary>
        /// Updates the selected torrent views.
        /// </summary>
        /// <param name="a_torrent">The selected torrent.</param>
        /// <remarks>
        /// UpdateSelectedTorrentViews()
        /// 
        /// SYNOPSIS
        /// 
        ///     UpdateSelectedTorrentViews(object a_torrent);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will update the selected torrent views. This 
        ///     includes the files, info, peers, and tracker views.
        ///     
        /// </remarks>
        public void UpdateSelectedTorrentViews(object a_torrent)
        {
            m_selectedTorrent = a_torrent as Torrent;

            // Updates file tab.
            SelectedTorrentFilesViewModel.Clear();
            foreach (FileWrapper file in m_selectedTorrent.Files)
            {
                SelectedTorrentFilesViewModel.Add(file);
            }

            // Updates info tab.
            SelectedTorrentInfoViewModel.Clear();
            SelectedTorrentInfoViewModel.Add(m_selectedTorrent);

            // Updates peers tab.
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
        
        /// <summary>
        /// Starts a torrent.
        /// </summary>
        /// <param name="a_torrent">The torrent to start.</param>
        /// <remarks>
        /// Start()
        /// 
        /// SYNOPSIS
        /// 
        ///     Start(Torrent a_torrent);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will start the torrent by starting a task for
        ///     updating the tracker, processing incoming/outgoing blocks, updating
        ///     peers, and updating the GUI.
        ///     
        /// </remarks>
        public void Start(Torrent a_torrent)
        {
            a_torrent.Status = "Started";

            // Task will update trackers.
            Task UpdateTracker = Task.Run(() =>
            {
                while (a_torrent.Started)
                {
                    a_torrent.UpdateTrackers();
                    Thread.Sleep(60000);
                }
            });

           // Task will check if there are any blocks to process.
           Task ProcessBlocks = Task.Run(() =>
           {
               while (a_torrent.Started)
               {
                   a_torrent.ProcessIncoming();
                   a_torrent.ProcessOutgoing();
                   Thread.Sleep(1000);
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

            // Updates the GUI for the current selected torrent.
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