using System.Collections.ObjectModel;

using BitTorrent_Client.Models.TrackerModels;

namespace BitTorrent_Client.ViewModels
{
    /// <summary>
    /// This class is responsible for being a view model that will display all 
    /// of the trackers for the selected torrent.
    /// </summary>
    public class SelectedTorrentTrackersViewModel : ObservableCollection<Tracker>
    {
    }
}