using System.Collections.ObjectModel;
using BitTorrent_Client.Models.PeerModels;

namespace BitTorrent_Client.ViewModels
{
    /// <summary>
    /// This class is responsible for being a view model and displaying all the
    /// peers for the selected torrent.
    /// </summary>
    public class SelectedTorrentPeersViewModel : ObservableCollection<Peer>
    {
    }
}