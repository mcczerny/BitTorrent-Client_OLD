using System.ComponentModel;

using BitTorrent_Client.Models.TorrentModels;
using System.Collections.ObjectModel;

namespace BitTorrent_Client.ViewModels
{
    /// <summary>
    /// This class will hold a single selected torrent so it's meta-data can be
    /// displayed.
    /// </summary>
    public class SelectedTorrentInfoViewModel : ObservableCollection<Torrent>
    {
    }
}