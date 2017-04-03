using System.Net.Sockets;

namespace BitTorrent_Client.Models.PeerModels
{
    class AsyncStateObject
    {
        #region Constructors

        public AsyncStateObject()
        {
            ReceiveBuffer = new byte[ReceiveBufferSize];
            WorkSocket = null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get/Set the current working socket.
        /// </summary>
        public Socket WorkSocket
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set the receive buffer.
        /// </summary>
        public byte[] ReceiveBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set the bytes read.
        /// </summary>
        public int BytesRead
        {
            get;
            set;
        }

        /// <summary>
        /// Get the receive buffer size.
        /// </summary>
        public static int ReceiveBufferSize
        {
            get { return 500000; }
        }

        /// <summary>
        /// Get/Set the totalBytes read.
        /// </summary>
        public int TotalBytesRead
        {
            get;
            set;
        }

        #endregion
    }
}