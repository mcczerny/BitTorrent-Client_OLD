using System.Collections.ObjectModel;
using BitTorrent_Client.Models.TorrentModels;

namespace BitTorrent_Client.ViewModels
{
    /// <summary>
    /// This class is used as the view model for displaying all the files in the
    /// selected torrent.
    /// </summary>
    public class SelectedTorrentFilesViewModel : ObservableCollection<FileWrapper>
    {
    }
}