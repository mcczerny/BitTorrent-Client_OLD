namespace BitTorrent_Client.Models.PeerModels
{
    public class OutgoingBlock
    {
        #region Constructors

        public OutgoingBlock(Peer a_peer, int a_index, int a_begin, int a_length)
        {
            Peer = a_peer;
            Index = a_index;
            Begin = a_begin;
            Length = a_length;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get/Private set the peer who the data is being sent to.
        /// </summary>
        public Peer Peer
        {
            get;
            private set;
        }

        /// <summary>
        /// The beginning index of the block.
        /// </summary>
        public int Begin
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Private set the piece index of the block
        /// </summary>
        public int Index
        {
            get;
            private set;
        }

        /// <summary>
        /// Get/Private set the length of the block.
        /// </summary>
        public int Length
        {
            get;
            private set;
        }

        #endregion
    }
}