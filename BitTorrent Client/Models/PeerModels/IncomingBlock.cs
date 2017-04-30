namespace BitTorrent_Client.Models.PeerModels
{
    /// <summary>
    /// This class is used as a data object for receiving blocks from a peer.
    /// </summary>
    public class IncomingBlock
    {
        #region Constructors

        public IncomingBlock(Peer a_peer, int a_index, int a_begin, byte[] a_block)
        {
            Peer = a_peer;
            Index = a_index;
            Begin = a_begin;
            Block = a_block;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get/Private set the Peer who is sending the block.
        /// </summary>
        public Peer Peer
        {
            get;
            private set;
        }

        /// <summary>
        /// Get/Private set the begining index of the block in the piece.
        /// </summary>
        public int Begin
        {
            get;
            private set;
        }

        /// <summary>
        /// Get/Private set the piece index of the block.
        /// </summary>
        public int Index
        {
            get;
            private set;
        }

        /// <summary>
        /// Get/Private set the actual block data.
        /// </summary>
        public byte[] Block
        {
            get;
            private set;
        }

        #endregion
    }
}