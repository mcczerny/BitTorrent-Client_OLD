using System.Collections.Generic;

using BitTorrent_Client.Models.TorrentModels;
using System;
using System.Text;

namespace BitTorrent_Client.Models.TrackerModels
{

    public abstract class Tracker 
    {
        #region Fields

        // Stores the last update time for tracker.
        protected DateTime m_lastUpdate;
        // The torrent making request.
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

        #region Events

        public event EventHandler<List<string>> PeerListUpdated;     

        protected virtual void OnPeerListUpdated()
        {
            PeerListUpdated?.Invoke(this, Peers);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get/Protected set the last update time of tracker.
        /// </summary>
        public DateTime LastUpdate
        {
            get { return m_lastUpdate; }
            protected set { m_lastUpdate = value; }
        }

        /// <summary>
        /// Get/Protected set a list of peers received from the tracker.
        /// </summary>
        /// 
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

        /// <summary>
        /// Abstract class that child classes inherit.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Parses peers from raw bytes.
        /// </summary>
        /// <param name="a_rawPeers">Contains peers.</param>
        /// <remarks>
        /// ParsePeers()
        /// 
        /// SYNOPSIS
        /// 
        ///     ParsePeers(byte[] a_rawPeers);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will parse peers from a byte array. It must go
        ///     through the array and form an ip address and port. The first four
        ///     bytes are each segment of the ip where a '.' must be added between
        ///     them. The next 2 bytes make up the port where a ":" must be added
        ///     and the bytes must be converted before the address is added to the
        ///     peers list.
        ///     
        /// </remarks>
        protected void ParsePeers(byte[] a_rawPeers)
        {
            var index = 0;
            while (index < a_rawPeers.Length)
            {
                // Forms ip address.
                StringBuilder address = new StringBuilder();
                for (var i = index; i < (index + 4); i++)
                {
                    address.Append(a_rawPeers[i]);
                    if (i != (index + 3))
                    {
                        address.Append(".");
                    }
                }
                // Start of port.
                address.Append(":");
                index += 4;

                // You must add the high and low bytes to get the port number.
                var highByte = a_rawPeers[index];
                var lowByte = a_rawPeers[index + 1];
                address.Append((highByte * 256 + lowByte));
                index += 2;

                // Add to peers list.
                Peers.Add(address.ToString());
            }
        }

        #endregion
    }
}