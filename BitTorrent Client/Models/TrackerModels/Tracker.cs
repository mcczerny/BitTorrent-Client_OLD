using System.Collections.Generic;

using BitTorrent_Client.Models.TorrentModels;
using System;

namespace BitTorrent_Client.Models.TrackerModels
{

    public abstract class Tracker 
    {
        #region Fields

        protected DateTime m_lastUpdate;
        protected Torrent m_torrent;

        #endregion

        #region Constructors

        public Tracker(Torrent a_torrent, string a_trackerUrl)
        {
            Peers = new List<string>();

            m_torrent = a_torrent;
            TrackerUrl = a_trackerUrl;
        }

        #endregion

        public event EventHandler<List<string>> PeerListUpdated;
     
        protected virtual void OnPeerListUpdated()
        {
            PeerListUpdated?.Invoke(this, Peers);
        }

        #region Properties

        /// <summary>
        /// Get/Protected set a list of peers received from the tracker.
        /// </summary>
        public List<string> Peers
        {
            get;
            protected set;
        }

        /// <summary>
        /// Get/Protected set how many peers have all of the files.
        /// </summary>
        public long Complete
        {
            get;
            protected set;
        }

        /// <summary>
        /// Get/Protected set how many peers have not fully downloaded the file yet.
        /// </summary>
        public long Incomplete
        {
            get;
            protected set;
        }

        /// <summary>
        /// Get/Protected set the interval to many another request to the tracker.
        /// </summary>
        public long Interval
        {
            get;
            protected set;
        }

        /// <summary>
        /// Get/Protected set the url of the tracker.
        /// </summary>
        public string TrackerUrl
        {
            get;
            protected set;
        }

        #endregion

        #region Methods

        public abstract void Update();

        #endregion
    }
}
