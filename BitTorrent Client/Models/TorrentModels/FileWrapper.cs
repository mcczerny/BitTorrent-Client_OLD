using BitTorrent_Client.Models.Utility_Functions;

namespace BitTorrent_Client.Models.TorrentModels
{
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

        public long EndOffset
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