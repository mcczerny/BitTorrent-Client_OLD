using System.Collections.ObjectModel;

using BitTorrent_Client.Models.TorrentModels;

namespace BitTorrent_Client.ViewModels
{
    /// <summary>
    /// This class is responsible for being the view model that will display all
    /// of the torrents that have been added to a client.
    /// </summary>
    public class TorrentViewModel : ObservableCollection<Torrent>
    {
    }
}