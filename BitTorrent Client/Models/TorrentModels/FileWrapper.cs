using BitTorrent_Client.Models.Utility_Functions;

namespace BitTorrent_Client.Models.TorrentModels
{
    /// <summary>
    /// This class acts as a file wrapper for storing information about the files
    /// in the torrent info-dictionary.
    /// </summary>
    public class FileWrapper
    {
        #region Properties

        /// <summary>
        /// Gets/Sets the md5sum of the file.
        /// </summary>
        public byte[] MD5Sum
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the length of the file.
        /// </summary>
        public long Length
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the starting offset of the file.
        /// </summary>
        public long StartOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the name of the file.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets path of the file.
        /// </summary>
        public string Path
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the file size in a readabable format.
        /// </summary>
        public string FileSize
        {
            get
            {
                return Utility.GetBytesReadable(Length);
            }
        }

        #endregion
    }

}