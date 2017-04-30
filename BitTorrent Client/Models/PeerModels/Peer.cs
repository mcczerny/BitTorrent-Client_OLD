using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.ComponentModel;

using BitTorrent_Client.Models.TorrentModels;
using BitTorrent_Client.Models.Utility_Functions;

namespace BitTorrent_Client.Models.PeerModels
{
    /// <summary>
    /// This class stores peer data and encodes, sends, decodes, and handles 
    /// incoming and outgoing messages from the peer.
    /// </summary>
    public class Peer : INotifyPropertyChanged
    {
        #region Fields

        private IPEndPoint m_remoteEndPoint;
        private Torrent m_torrent;
        
        private Socket m_client;

        private float m_currentProgress;

        private readonly int m_handshakeSize = 68;

        private string m_downloadSpeed;

        #endregion

        #region Constructors

        public Peer(Torrent a_torrent, string a_address)
        {
            m_torrent = a_torrent;

            HasPiece = new bool[m_torrent.NumberOfPieces];

            var separatedAddress = a_address.Split(':');
            IP = separatedAddress[0];
            Port = Int32.Parse(separatedAddress[1]);

            m_remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), Port);

            AmChoking = true;
            AmInterested = false;
            PeerChoking = true;
            PeerInterested = false;


        }

        #endregion

        #region Events

        // Handles property changed events.
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string a_propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(a_propertyName));
        }

        // Event handlers for peer messages.
        public event EventHandler BitfieldRecieved;
        public event EventHandler Disconnected;
        public event EventHandler<int> HaveRecieved;
        public event EventHandler<IncomingBlock> BlockReceived;
        public event EventHandler<OutgoingBlock> BlockRequested;
        public event EventHandler<OutgoingBlock> BlockCanceled;

        #endregion

        #region Enumerators

        /// <summary>
        /// Enumerator identifies the type of message received.
        /// </summary>
        public enum MessageType
        {
            Choke,
            Unchoke,
            Interested,
            NotInterested,
            Have,
            Bitfield,
            Request,
            Piece,
            Cancel,
            Port,
            Handshake,
            KeepAlive,
            Unknown
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get/Set the last time peer statistics were checked.
        /// </summary>
        public DateTime LastUpdate
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set if client is choking peer.
        /// </summary>
        public bool AmChoking
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set if client is intersted in peer.
        /// </summary>
        public bool AmInterested
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set if the client has received a bitfield message.
        /// </summary>
        public bool BitfieldReceived
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set if the bitfield has been sent to the peer.
        /// </summary>
        public bool BitfieldSent
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set if the peer has all of the pieces.
        /// </summary>
        public bool Complete
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set if client is connected to peer.
        /// </summary>
        public bool Connected
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set if the client has received a handshake message.
        /// </summary>
        public bool HandshakeReceived
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set if the handshake has been sent to the peer.
        /// </summary>
        public bool HandshakeSent
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set if the peer is choking the client.
        /// </summary>
        public bool PeerChoking
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set if the peer is interested in the client.
        /// </summary>
        public bool PeerInterested
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set if peer has piece available.
        /// </summary>
        public bool[] HasPiece
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set The current progress of the peer.
        /// </summary>
        public float CurrentProgress
        {
            get { return m_currentProgress; }
            set {
                if (m_currentProgress != value)
                {
                    m_currentProgress = value;
                    OnPropertyChanged("CurrentProgress");
                }
            }
        }

        /// <summary>
        /// Get/Set the total number of blocks the client is requesting from 
        /// the peer.
        /// </summary>
        public int NumberOfBlocksRequested
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Private set the port of the peer.
        /// </summary>
        public int Port
        {
            get;
            private set;
        }

        /// <summary>
        /// Get/Set how much has been downloaded from peer since last update.
        /// </summary>
        public long DownloadedSince
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set the total amount downloaded from peer.
        /// </summary>
        public long Downloaded
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Private set the IP address of the peer.
        /// </summary>
        public string IP
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets/Set the download speed between the client and peer.
        /// </summary>
        public string DownloadSpeed
        {
            get { return m_downloadSpeed; }
            set
            {
                if (m_downloadSpeed != value)
                {
                    m_downloadSpeed = value;
                    OnPropertyChanged("DownloadSpeed");
                }
            }
        }
        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Connect to peer.
        /// </summary>
        /// <remarks>
        /// Connect()
        /// 
        /// SYNOPSIS
        /// 
        ///     Connect();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will try to connect to a peer. If m_client is null,
        ///     then we try to connect to the peer. If an exception is thrown,
        ///     the Disconnect function is called. If the connection is successful,
        ///     then call the function Receive to start listening for incoming
        ///     messages. A handshake is then sent to the peer and if the Handshake
        ///     is successfully sent then send a Bitfield message.
        /// </remarks>
        public void Connect()
        {
            // If the client is not already connected to the peer.
            if (m_client == null)
            {
                // Creates a new tcp socket.
                m_client = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                // Tries to connect client to the peer.
                try
                {
                    m_client.Connect(m_remoteEndPoint);
                    Connected = true;
                }
                // Catches exception and calls Disconnect()
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Disconnect();
                    return;
                }
            }

            // Start listening from peer.
            Receive(m_client);
            
            // Send handshake and bitfield messages.
            SendHandshake();
            if (HandshakeSent)
            {
                SendBitfield();
            }
        }

        /// <summary>
        /// Disconnect from peer.
        /// </summary>
        /// <remarks>
        /// Disconnect()
        /// 
        /// SYNOPSIS
        /// 
        ///     Disconnect();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will disconnect from the peer if connected and close
        ///     the socket. It will also invoke the Disconnected event handler.
        ///     
        /// </remarks>
        public void Disconnect()
        {
            // If the client was connected to the peer set Connected to false.
            if (Connected)
            {
                Connected = false;
            }

            // If socket is/was connected we close our connection.
            if(m_client != null)
            {
                m_client.Close();
            }

            // Raise event to torrent object.
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        #region Public Send Methods

        /// <summary>
        /// Sends a bifield message to the peer.
        /// </summary>
        /// <remarks>
        /// SendBitfield()
        /// 
        /// SYNOPSIS
        /// 
        ///     void SendBitfield();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will send a bitfield message to the peer. If the
        ///     client has no pieces, then no bitfield message is sent. It calls
        ///     the private send function using the function EncodeBitfieldMessage
        ///     as it's parameter.
        ///     
        /// </remarks>
        public void SendBitfield()
        {    
            // When the client has pieces.
            if(m_torrent.VerifiedPieces.OfType<bool>().Contains(true))
            {
                Send(EncodeBitfieldMessage());
            }
        }

        /// <summary>
        /// Sends a cancel message to the peer.
        /// </summary>
        /// <param name="a_index">The piece index.</param>
        /// <param name="a_begin">The beginning ofset of the block.</param>
        /// <param name="a_length">The length of the block.</param>
        /// <remarks>
        /// SendCancel()
        /// 
        /// SYNOPSIS
        /// 
        ///     void SendCancel(int a_index, int a_begin, int a_length);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will send a cancel message to the peer. It calls 
        ///     the private send function with the parameter being the function
        ///     EncodeCancelMessage, using a_index, a_begin, a_length as it's 
        ///     parameters.
        ///     
        /// </remarks>
        public void SendCancel(int a_index, int a_begin, int a_length)
        {
            Send(EncodeCancelMessage(a_index, a_begin, a_length));
        }

        /// <summary>
        /// Sends a choke message to the peer.
        /// </summary>
        /// <remarks>
        /// SendChoke()
        /// 
        /// SYNOPSIS
        ///     
        ///     void SendChoke();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function sends a choke message to the peer by calling the
        ///     private send function with the parameter being the function
        ///     EncodeChokeMessage. It also sets the AmChoking bool to true.
        /// </remarks>
        public void SendChoke()
        {
            Send(EncodeChokeMessage());
            AmChoking = true;
        }

        /// <summary>
        /// Sends a handshake message to the peer.
        /// </summary>
        /// <remarks>
        /// SendHandshake()
        /// 
        /// SYNOPSIS
        /// 
        ///     void SendHandshake();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function sends a handshake message to the peer. It calls the
        ///     private function Send using the method EncodeHandshake message as
        ///     it's parameter. It also sets the bool HandshakeSent to true.
        ///     
        /// </remarks>
        private void SendHandshake()
        {
            Send(EncodeHandshakeMessage());
            HandshakeSent = true;
        }

        /// <summary>
        /// Sends a have message to the peer.
        /// </summary>
        /// <param name="a_pieceIndex">The piece index to tell peer the client now has.</param>
        /// <remarks>
        /// SendHave()
        /// 
        /// SYNOPSIS
        /// 
        ///     void SendHave(int a_pieceIndex);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will send a have message to the peer. It will call
        ///     the private function Send using the function EncodeHaveMessage as
        ///     it's parameter. EncodeHaveMessage uses the parameter a_pieceIndex 
        ///     for it's parameter.
        ///     
        /// </remarks>
        public void SendHave(int a_pieceIndex)
        {
            Send(EncodeHaveMessage(a_pieceIndex));
        }

        /// <summary>
        /// Sends an interested message to the peer.
        /// </summary>
        /// <remarks>
        /// SendInterested()
        /// 
        /// SYNOPSIS
        /// 
        ///     void SendInterested();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will send an interested message to the peer. It will
        ///     call the private function Send using the function EncodeInterestedMessage
        ///     as it's parameter. It will also set the bool AmInterested to true.
        ///     
        /// </remarks>
        public void SendInterested()
        {
            Send(EncodeInterestedMessage());
            AmInterested = true;
        }

        /// <summary>
        /// Sends a keep alive message to the peer.
        /// </summary>
        /// <remarks>
        /// SendKeepAliveMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     void SendKeepAliveMessage();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will send a keep alive message to the peer by calling
        ///     the private function Send with it's parameter being the function
        ///     EncodeKeepAliveMessage.
        ///     
        /// </remarks>
        public void SendKeepAliveMessage()
        {
            Send(EncodeKeepAliveMessage());
        }

        /// <summary>
        /// Sends a not interested message to the peer.
        /// </summary>
        /// <remarks>
        /// SendNotInterested()
        /// 
        /// SYNOPSIS
        /// 
        ///     void SendNotInterested();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will send a not interested message to the peer by
        ///     calling the private function Send with the using the function
        ///     EncodeNotInterestedMessage as it's function parameter. It will
        ///     also set the bool AmInterested to false.
        ///     
        /// </remarks>
        public void SendNotInterested()
        {
            Send(EncodeNotInterestedMessage());
            AmInterested = false;
        }

        // Not done.
        public void SendPiece()
        {
            Send(EncodePieceMessage());
        }

        /// <summary>
        /// Sends a request message to the peer.
        /// </summary>
        /// <param name="a_pieceIndex">The index of the piece the client wants.</param>
        /// <param name="a_begin">The offset in the piece of the start of the block.</param>
        /// <param name="a_length">The length of the block the client wants.</param>
        /// <remarks>
        /// SendRequest()
        /// 
        /// SYNOPSIS
        /// 
        ///     void SendRequest(int a_pieceIndex, int a_begin, int a_length);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function sends a piece message to the peer. It calls the 
        ///     private function Send using the function EncodeRequestMessage as
        ///     it's parameter. EncodeRequestMessage uses the paramters a_pieceLength,
        ///     a_begin, and a_length as it's paramters.
        ///     
        /// </remarks>
        public void SendRequest(int a_pieceIndex, int a_begin, int a_length)
        {
            Send(EncodeRequestMessage(a_pieceIndex, a_begin, a_length));
        }

        /// <summary>
        /// Sends an unchoke message to the peer.
        /// </summary>
        /// <remarks>
        /// SendUnchoke()
        /// 
        /// DESCRIPTION
        /// 
        ///     This function will send an unchoke message to the peer by using
        ///     the private function Send with it's parameter being the function
        ///     EncodeUnchokeMessage. The function also sets the bool AmChoking to
        ///     false.
        ///     
        /// </remarks>
        public void SendUnchoke()
        {
            Send(EncodeUnchokeMessage());
            AmChoking = false;
        }

        #endregion

        #endregion

        #region Private Methods

        #region Decode Message Methods

        /// <summary>
        /// Decodes a bitfield message.
        /// </summary>
        /// <param name="a_message">The message to be decoded.</param>
        /// <param name="a_hasPiece">Returns the bitfield.</param>
        /// <returns>Returns true if the bitfield was valid and false if not.</returns>
        /// <remarks>
        /// DecodeBitfieldMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     bool DecodeBitfieldMessage(byte[] a_message, out bool[] a_hasPiece);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will decode an incoming bitfield message from a peer.
        ///     It must determine if the message is the correct length. If it is not
        ///     the correct length then false is returned. If it is a valid length,
        ///     then the bitfield is converted to a more readable bool array and 
        ///     stored in a_hasPiece. 
        ///     
        /// </remarks>
        private bool DecodeBitfieldMessage(byte[] a_message, out bool[] a_hasPiece)
        {
            // Initialize out parameter.
            a_hasPiece = new bool[m_torrent.NumberOfPieces];

            int messageSize = (int)(Math.Ceiling(m_torrent.NumberOfPieces / 8.0));

            // If message length is not the same size.
            if (a_message.Length != (messageSize + 5))
            {
                return false;
            }

            var peerBitfield = new BitArray(Utility.SubArray(a_message, 5, messageSize));

            // Convert bitfield into usable bool array.
            for (var i = 0; i < m_torrent.NumberOfPieces; i++)
            {
                a_hasPiece[i] = peerBitfield[peerBitfield.Length - 1 - i];
            }

            return true;
        }

        /// <summary>
        /// Decodes a cancel message.
        /// </summary>
        /// <param name="a_message">The message to be decoded.</param>
        /// <param name="a_index">Returns the piece index of the canceled block.</param>
        /// <param name="a_begin">Returns the beginning ofset of canceled block.</param>
        /// <param name="a_length">Returns the length of canceled block.</param>
        /// <returns>Returns true if it is a valid cancel message and false if not.</returns>
        /// <remarks>
        /// DecodeCancelMessage()
        /// 
        /// SYNOPSIS
        ///     
        ///     DecodeCancelMessage(byte[] a_message, out int a_index, 
        ///         out int a_begin, out int a_length);
        ///         
        /// DESCRIPTION
        /// 
        ///     This function will decode a cancel message. If the message length
        ///     is not correct, then false is returned. If the length is valid,
        ///     the message is split, reversed if needed, and converted to integers.
        /// </remarks>
        private bool DecodeCancelMessage(byte[] a_message, out int a_index,
            out int a_begin, out int a_length)
        {
            // Initialize out parameters.
            a_index = -1;
            a_begin = -1;
            a_length = -1;

            // Cancel message length is not correct length.
            if (a_message.Length != 17 || a_message[3] != 13)
            {
                return false;
            }
            
            // Split up message.
            var indexBytes = Utility.SubArray(a_message, 5, 4);
            var beginBytes = Utility.SubArray(a_message, 9, 4);
            var lengthBytes = Utility.SubArray(a_message, 13, 4);

            // If little endian, reverse the bytes.
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(indexBytes);
                Array.Reverse(beginBytes);
                Array.Reverse(lengthBytes);
            }

            // Convert byte arrays to integers.
            a_index = BitConverter.ToInt32(indexBytes, 0);
            a_begin = BitConverter.ToInt32(beginBytes, 0);
            a_length = BitConverter.ToInt32(lengthBytes, 0);

            return true;
        }

        /// <summary>
        /// Decodes a have message.
        /// </summary>
        /// <param name="a_message">The message to be decoded.</param>
        /// <param name="a_pieceIndex">Returns the piece index the peer has.</param>
        /// <returns>Returns true if message is valid and false if not.</returns>
        /// <remarks>
        /// DecodeHaveMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     bool DecodeHaveMessage(byte[] a_message, out int a_pieceIndex);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will decode a have message. If the message length
        ///     is not correct, then false is returned. If the length is valid,
        ///     the piece index is split from a_message, reversed if needed, and
        ///     converted into an integer.
        /// 
        /// </remarks>
        private bool DecodeHaveMessage(byte[] a_message, out int a_pieceIndex)
        {
            // Initialize out value.
            a_pieceIndex = -1;

            // Message not correct length.
            if (a_message.Length != 9)
            {
                return false;
            }

            var pieceIndex = Utility.SubArray(a_message, 5, 4);

            // If Little endian, reverse the bytes.
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(pieceIndex);
            }

            // Convert byte array to integer.
            a_pieceIndex = BitConverter.ToInt32(pieceIndex, 0);

            return true;
        }

        /// <summary>
        /// Decodes a handshake message.
        /// </summary>
        /// <param name="a_message">The message to be decoded.</param>
        /// <param name="a_infoHash">Returns the info hash sent from peer.</param>
        /// <returns>Returns true if it is a valid message and false if not.</returns>
        /// <remarks>
        /// DecodeHandshakeMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     bool DecodeHandshakeMessage(byte[] a_message, out byte[] a_infoHash);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function decodes a handshake message. It checks if the
        ///     message and protocol string length is valid. It also checks if
        ///     the protocol name is correct.
        ///     
        /// </remarks>
        private bool DecodeHandshakeMessage(byte[] a_message, out byte[] a_infoHash)
        {
            // Initialize out paramter.
            a_infoHash = new byte[20];

            // If the handshake message length or protocol string length is invalid.
            if (a_message.Length != 68 || a_message[0] != 19)
            {
                return false;
            }

            // When the protocol name is not BitTorrent protocol.
            if (Encoding.ASCII.GetString(
                Utility.SubArray(a_message, 1, 19)) != "BitTorrent protocol")
            {
                return false;
            }

            a_infoHash = Utility.SubArray(a_message, 28, 20);

            return true;
        }

        /// <summary>
        /// Decodes a keep alive message.
        /// </summary>
        /// <param name="a_message">The message to be decoded.</param>
        /// <returns>Returns true if it is a valid keep alive message and false if not.</returns>
        /// <remarks>
        /// DecodeKeepAliveMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     bool DecodeKeepAliveMessage(byte[] a_message);
        /// 
        /// DESCRIPTION
        /// 
        ///     This function with decode a keep alive message. It will check if 
        ///     the message is the correct length and if the bytes match a keep 
        ///     alive message.
        ///     
        /// </remarks>
        private bool DecodeKeepAliveMessage(byte[] a_message)
        {
            // If the message length is not 4 bytes long or the bytes are not all 0.
            if (a_message.Length != 4 || !a_message.SequenceEqual(new byte[] { 0, 0, 0, 0 }))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Decodes a peer state message.
        /// </summary>
        /// <param name="a_message">The message to be decoded.</param>
        /// <param name="a_stateID">The state type.</param>
        /// <returns>Returns true if valid state or false if not.</returns>
        /// <remarks>
        /// DecodePeerState()
        /// 
        /// SYNOPSIS
        /// 
        ///     bool DecodePeerState(byte[] a_message, int a_stateID);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will decode a peer state message. It will check if 
        ///     the message is the correct length and if a_stateID is valid.
        ///     
        /// </remarks>
        private bool DecodePeerState(byte[] a_message, int a_stateID)
        {
            if (a_message.Length != 5 || a_stateID < 0 || a_stateID > 3)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Decodes a piece message.
        /// </summary>
        /// <param name="a_message">The message to be decoded.</param>
        /// <param name="a_index">Returns the piece index.</param>
        /// <param name="a_begin">Returns the offset of the beginning of the block.</param>
        /// <param name="a_block">Returns block data.</param>
        /// <returns>Returns true if piece message is valid and false if not.</returns>
        /// <remarks>
        /// DecodePieceMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     bool DecodePieceMessage(byte[] a_message, out int a_index, 
        ///     out int a_begin, out byte[] a_block);
        ///         
        /// DESCRIPTION
        /// 
        ///     This function decodes a piece message. It will check if the message
        ///     is a valid length. It will split the message, reverse them if needed,
        ///     and convert the byte arrays to integers.
        ///     
        /// </remarks>
        private bool DecodePieceMessage(byte[] a_message, out int a_index, 
            out int a_begin, out byte[] a_block)
        {
            // Initialize out parameters.
            a_index = -1;
            a_begin = -1;
            a_block = new byte[0];

            // If the piece message is too short.
            if (a_message.Length < 13)
            {
                Console.WriteLine("Invalid piece message length");
                return false;
            }

            // Split up message.
            var lengthBytes = Utility.SubArray(a_message, 0, 4);
            var indexBytes = Utility.SubArray(a_message, 5, 4);
            var beginBytes = Utility.SubArray(a_message, 9, 4);

            // If little endian, then reverse bytes.
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthBytes);
                Array.Reverse(indexBytes);
                Array.Reverse(beginBytes);
            }

            // The length of the block.
            int blockLength = (BitConverter.ToInt32(lengthBytes, 0) - 9);

            // Convert from byte arrays to integers.
            a_index = BitConverter.ToInt32(indexBytes, 0);
            a_begin = BitConverter.ToInt32(beginBytes, 0);
            a_block = Utility.SubArray(a_message, 13, blockLength);

            return true;
        }

        /// <summary>
        /// Decodes a request message.
        /// </summary>
        /// <param name="a_message">The message to be decoded.</param>
        /// <param name="a_index">Returns the piece index.</param>
        /// <param name="a_begin">Returns the offset of the beginning of the block.</param>
        /// <param name="a_length">Returns block data.</param>
        /// <returns>Returns true if request message if valid and false if not.</returns>
        /// <remarks>
        /// DecodeRequestMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     bool DecodeRequestMessage(byte[] a_message, out int a_index,
        ///         out int a_begin, out int a_length);
        ///         
        /// DESCRIPTION
        /// 
        ///     This function will decode a request message. It will check if the
        ///     message is a valid length. It will split the message, reverse them
        ///     if needed, and convert the byte arrays to integers.
        ///     
        /// </remarks>
        private bool DecodeRequestMessage(byte[] a_message, out int a_index, 
            out int a_begin, out int a_length)
        {
            // Initialize out parameters.
            a_index = -1;
            a_begin = -1;
            a_length = -1;

            // When the requeset message is not the correct length.
            if (a_message.Length != 17 || a_message[3] != 13)
            {
                Console.WriteLine("Invalid request message");
                return false;
            }

            // Split up message.
            byte[] indexBytes = Utility.SubArray(a_message, 5, 4);
            byte[] beginBytes = Utility.SubArray(a_message, 9, 4);
            byte[] lengthBytes = Utility.SubArray(a_message, 13, 4);

            // If little endian, then reverse bytes.
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(indexBytes);
                Array.Reverse(beginBytes);
                Array.Reverse(lengthBytes);
            }

            // Convert from byte arrays to integer.
            a_index = Convert.ToInt32(indexBytes);
            a_begin = Convert.ToInt32(beginBytes);
            a_length = Convert.ToInt32(lengthBytes);

            return true;
        }

        #endregion

        #region Encode Methods

        // Not done
        private byte[] EncodeBitfieldMessage()
        {

            int bitfieldLength = (int)(Math.Ceiling(m_torrent.NumberOfPieces / 8.0));

            byte[] bitfieldMessage = new byte[bitfieldLength + 5];

            bitfieldMessage[4] = 5;
            // Calculate length of of bitfield 


            return bitfieldMessage;
        }

        /// <summary>
        /// Encodes as cancel message. 
        /// </summary>
        /// <param name="a_index">The index of the piece.</param>
        /// <param name="a_begin">The beginning offset of the block in the piece.</param>
        /// <param name="a_length">The length of the block.</param>
        /// <returns>Returns a byte array containing a cancel message to send.</returns>
        /// <remarks>
        /// EncodeCancelMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     byte[] EncodeCancelMessage(int a_index, int a_being, int a_length);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function encodes a cancel message. It first converts the 
        ///     parameters into byte arrays and if neccesary reverses bytes if
        ///     little endian. The byte arrays are copied into cancelMessage and
        ///     it is returned.
        /// </remarks>
        private byte[] EncodeCancelMessage(int a_index, int a_begin, int a_length)
        {
            var cancelMessage = new byte[17];

            // The length of the message.
            cancelMessage[3] = 13;
            // Message ID.
            cancelMessage[4] = 8;

            // Convert integers to byte arrays.
            var pieceIndex = BitConverter.GetBytes(a_index);
            var beginIndex = BitConverter.GetBytes(a_begin);
            var requestLength = BitConverter.GetBytes(a_length);

            // If little endian, then reverse bytes.
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(pieceIndex);
                Array.Reverse(beginIndex);
                Array.Reverse(requestLength);
            }

            // Copy to cancel message.
            Buffer.BlockCopy(pieceIndex, 0, cancelMessage, 5, 4);
            Buffer.BlockCopy(beginIndex, 0, cancelMessage, 9, 4);
            Buffer.BlockCopy(requestLength, 0, cancelMessage, 13, 4);

            return cancelMessage;
        }

        /// <summary>
        /// Encodes a choke message.
        /// </summary>
        /// <returns>Returns a byte array containing a choke message to send.</returns>
        /// <remarks>
        /// EncodeChokeMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     byte[] EncodeChokeMessage();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function encodes a choke message to send to peer.
        ///     
        /// </remarks>
        private byte[] EncodeChokeMessage()
        {
            return new byte[] { 0, 0, 0, 1, 0 };
        }

        /// <summary>
        /// Encodes a handshake message.
        /// </summary>
        /// <returns>Returns a byte array containing a handshake message to send.</returns>
        /// <remarks>
        /// EncodeHandshakeMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     byte[] EncodeHandshakeMessage();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function encodes a handshake message to send to the peer. It
        ///     will set the appropriate protocol along with the protocol string 
        ///     length. It will also copy the hash of the info dictionary of the
        ///     torrent file along with a client ID.
        ///     
        /// </remarks>
        private byte[] EncodeHandshakeMessage()
        {
            StringBuilder stringMessage = new StringBuilder();
            byte[] handshakeMessage = new byte[m_handshakeSize];
            var hash = m_torrent.ByteInfoHash;
            handshakeMessage[0] = 19;

            Encoding.ASCII.GetBytes("BitTorrent protocol", 0, 19, handshakeMessage, 1);
            Buffer.BlockCopy(hash, 0, handshakeMessage, 28, hash.Length);
            Encoding.ASCII.GetBytes("-qB33A0-XtNvaJ!5tsqy", 0, 20, handshakeMessage, 48);

            return handshakeMessage;
        }

        // Not done
        private byte[] EncodeHaveMessage(int a_pieceIndex)
        {
            byte[] haveMessage = new byte[9];

            haveMessage[3] = 5;
            haveMessage[4] = 4;

            byte[] pieceIndex = BitConverter.GetBytes(a_pieceIndex);

            return haveMessage;
        }

        /// <summary>
        /// Encodes an interested message.
        /// </summary>
        /// <returns>Returns a byte array containing an interested message to send.</returns>
        /// <remarks>
        /// EncodeInterestedMessage()
        /// 
        /// SYNOPSIS
        ///     
        ///     byte[] EncodeInterstedMessage();
        ///     
        /// DESCRIPTION
        ///     
        ///     This function will encode an intersted message to send to the 
        ///     peer. 
        /// 
        /// </remarks>
        private byte[] EncodeInterestedMessage()
        {
            return new byte[] { 0, 0, 0, 1, 2 };
        }

        /// <summary>
        /// Encodes a keep alive message.
        /// </summary>
        /// <returns>Returns a byte array containing a keep alive message to send.</returns>
        /// <remarks>
        /// EncodeKeepAliveMessage()
        /// 
        /// SYNOPSIS
        ///     
        ///     byte[] EncodeKeepAliveMessage();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will encode a keep alive message to send to the peer.
        ///     
        /// </remarks>
        private byte[] EncodeKeepAliveMessage()
        {
            return new byte[] { 0, 0, 0, 0 };
        }

        /// <summary>
        /// Encodes a not interested message.
        /// </summary>
        /// <returns>Returns a byte array containing a not interested message to send.</returns>
        /// <remarks>
        /// EncodeNotInterestedMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     byte[] EncodeNotInterestedMessage();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will encode a not interested messge to send to peer.
        ///     
        /// </remarks>
        private byte[] EncodeNotInterestedMessage()
        {
            return new byte[] { 0, 0, 0, 1, 3 };
        }

        // Not done
        private byte[] EncodePieceMessage()
        {
            byte[] pieceMessage = new byte[18];

            return pieceMessage;
        }

        /// <summary>
        /// Encodes a request message.
        /// </summary>
        /// <param name="a_pieceIndex">The piece index of the block.</param>
        /// <param name="a_begin">The beginning offset in the piece.</param>
        /// <param name="a_length">The length of the block requested.</param>
        /// <returns>Returns a byte array contains a request message to send.</returns>
        /// <remarks>
        /// EncodeRequestMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     byte[] EncodeRequestMessage(int a_pieceIndex, int a_begin,
        ///         int a_length);
        ///         
        /// DESCRIPTION.
        /// 
        ///     This function will encode a request message for a block to send
        ///     to the peer. It will convert the parameters into byte arrays, 
        ///     reverse the bytes if little endian and copy into requestMessage
        ///     byte array to be returned.
        ///     
        /// </remarks>
        private byte[] EncodeRequestMessage(int a_pieceIndex, int a_begin,
            int a_length)
        {
            var requestMessage = new byte[17];

            // The length of the message.
            requestMessage[3] = 13;
            // Message ID.
            requestMessage[4] = 6;

            // Convert integers to byte arrays.
            var pieceIndex = BitConverter.GetBytes(a_pieceIndex);
            var beginIndex = BitConverter.GetBytes(a_begin);
            var requestLength = BitConverter.GetBytes(a_length);

            // If little endian, reverse bytes.
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(pieceIndex);
                Array.Reverse(beginIndex);
                Array.Reverse(requestLength);
            }

            // Copy to request message.
            Buffer.BlockCopy(pieceIndex, 0, requestMessage, 5, 4);  
            Buffer.BlockCopy(beginIndex, 0, requestMessage, 9, 4);
            Buffer.BlockCopy(requestLength, 0, requestMessage, 13, 4);

            return requestMessage;
        }
        
        /// <summary>
        /// Encodes an unchoke message.
        /// </summary>
        /// <returns>Returns a byte array that contains an unchoke message to send.</returns>
        /// <remarks>
        /// EncodeUnchokeMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     byte[] EncodeUnchokeMessage();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will encode an unchoke message to send to peer.
        ///     
        /// </remarks>
        private byte[] EncodeUnchokeMessage()
        {
            return new byte[] { 0, 0, 0, 1, 1 };
        }

        #endregion

        /// <summary>
        /// Gets the message length of the next message in the buffer.
        /// </summary>
        /// <param name="a_receivedData">Buffer that has received data.</param>
        /// <returns>Returns the length of the next message in the buffer.</returns>
        /// <remarks>
        /// GetMessageLength()
        /// 
        /// SYNOPSIS
        /// 
        ///     GetMessageLength(byte[] a_receivedData);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will get the length of the next length in the buffer.
        ///     If a handshake message has not been received, then the next message
        ///     must be a handshake message. If not, then the first 4 bytes are
        ///     checked for the length of the next message.
        /// </remarks>
        private int GetMessageLength(byte[] a_receivedData)
        {
            // If handshake has not be received the next message must be a handshake.
            if (!HandshakeReceived)
            {
                return m_handshakeSize;
            }

            // Stores first 4 bytes of buffer, which contains message length.
            byte[] lengthPrefix = Utility.SubArray(a_receivedData, 0, 4);

            // If the byte order of data is stored little endian then the 
            // bytes need to be reversed.
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthPrefix);
            }

            // Convert the byte array to an int and return;
            return BitConverter.ToInt32(lengthPrefix, 0) + 4;
        }
        
        /// <summary>
        /// Gets the next message type.
        /// </summary>
        /// <param name="a_message"> The message to determine the type of.</param>
        /// <returns>Returns a MessageType of the type of message.</returns>
        /// <remarks>
        /// GetMessageType()
        /// 
        /// SYNOPSIS
        /// 
        ///     MessageType GetMessageType(byte[] a_message);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will find the message type of the message. If a 
        ///     handshake has not been received, then the next message must be
        ///     a handshake message. If the message length is 4, then it is a keep
        ///     alive message. If it is neither of those, then it determines the 
        ///     MessageType by checking the 5th byte in the message.
        /// </remarks>
        private MessageType GetMessageType(byte[] a_message)
        {
            // If we haven't receieved our handshake yet, the message will be a handshake message.
            if (!HandshakeReceived)
            {
                return MessageType.Handshake;
            }
            // If the message has a length of 4, then the message is a keep alive message.
            else if (a_message.Length == 4)
            {
                return MessageType.KeepAlive;
            }
            // Check the 5th byte in the array to determine the message type.
            else
            {
                switch (a_message[4])
                {
                    case 0:
                        return MessageType.Choke;
                    case 1:
                        return MessageType.Unchoke;
                    case 2:
                        return MessageType.Interested;
                    case 3:
                        return MessageType.NotInterested;
                    case 4:
                        return MessageType.Have;
                    case 5:
                        return MessageType.Bitfield;
                    case 6:
                        return MessageType.Request;
                    case 7:
                        return MessageType.Piece;
                    case 8:
                        return MessageType.Cancel;
                    case 9:
                        return MessageType.Port;
                    default:
                        return MessageType.Unknown;
                }
            }
        }

        /// <summary>
        /// Handles an incoming message from peer.
        /// </summary>
        /// <param name="a_message">The message to handle.</param>
        /// <remarks>
        /// HandleIncomingMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     HandleIncomingMessage(byte[] a_message);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will handle an incoming message. It first determines
        ///     the type of message. Then, it will Decode the message and handle
        ///     it if needed by calling an event.
        /// </remarks>
        private void HandleIncomingMessage(byte[] a_message)
        {
            MessageType messageType = GetMessageType(a_message);

            switch (messageType)
            {
                case MessageType.Handshake:

                    byte[] infoHash;
                    if (DecodeHandshakeMessage(a_message, out infoHash))
                    {
                        HandleHandshakeMessage(infoHash);
                    }
                    break;
                case MessageType.KeepAlive:

                    if (DecodeKeepAliveMessage(a_message))
                    {

                    }
                    break;
                case MessageType.Choke:

                    if (DecodePeerState(a_message, (int)messageType))
                    {
                        HandleChokeMessage();
                    }
                    break;
                case MessageType.Unchoke:

                    if (DecodePeerState(a_message, (int)messageType))
                    {
                        HandleUnchokeMessage();
                    }
                    break;
                case MessageType.Interested:

                    if (DecodePeerState(a_message, (int)messageType))
                    {
                        HandleInterestedMessage();
                    }
                    break;
                case MessageType.NotInterested:

                    if (DecodePeerState(a_message, (int)messageType))
                    {
                        HandleNotInterestedMessage();
                    }
                    break;
                case MessageType.Have:

                    int haveIndex;
                    if (DecodeHaveMessage(a_message, out haveIndex))
                    {
                        HandleHaveMessage(haveIndex);
                    }
                    break;
                case MessageType.Bitfield:

                    bool[] peerHasPiece;
                    if (!BitfieldReceived)
                    {
                        if (DecodeBitfieldMessage(a_message, out peerHasPiece))
                        {
                            HandleBitfieldMessage(peerHasPiece);
                        }
                    }
                    break;
                case MessageType.Request:

                    int requestIndex;
                    int requestBegin;
                    int requestLength;

                    if (DecodeRequestMessage(a_message, out requestIndex, out requestBegin, out requestLength))
                    {
                        HandleRequestMessage(requestIndex, requestBegin, requestLength);
                    }
                    break;
                case MessageType.Piece:

                    int pieceIndex;
                    int pieceBegin;
                    byte[] pieceBlock;
                    if (DecodePieceMessage(a_message, out pieceIndex, out pieceBegin, out pieceBlock))
                    {
                        HandlePieceMessage(pieceIndex, pieceBegin, pieceBlock);
                    }
                    break;
                case MessageType.Cancel:

                    int cancelIndex;
                    int cancelBegin;
                    int cancelLength;
                    if (DecodeCancelMessage(a_message, out cancelIndex, out cancelBegin, out cancelLength))
                    {
                        HandleCancelMessage(cancelIndex, cancelBegin, cancelLength);
                    }
                    break;
                case MessageType.Port:

                    break;
                default:

                    break;
            }
        }

        #region Handle Methods

        /// <summary>
        /// Handles a bitfield message.
        /// </summary>
        /// <param name="a_hasPiece">Peers available pieces.</param>
        /// <remarks>
        /// HandleBitfieldMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     void HandleBitfieldMessage(bool[] a_hasPiece);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will handle a decoded bitfield message. It will
        ///     set the bool BitfieldReceived to true and copy a_hasPiece to the
        ///     property HasPiece. It will also compute the peers current progress
        ///     and determine if is a seeder. Lastly it will invoke the event.
        ///     
        /// </remarks>
        private void HandleBitfieldMessage(bool[] a_hasPiece)
        {
            BitfieldReceived = true;

            // Copies a_hasPiece to HasPiece property.
            HasPiece = new bool[a_hasPiece.Length];
            Array.Copy(a_hasPiece, HasPiece, a_hasPiece.Length);

            // Computes current progress of peer.
            for(int i = 0; i < HasPiece.Length; i++)
            {
                if (HasPiece[i])
                {
                    CurrentProgress += m_torrent.ComputePieceLength(i);
                }   
            }

            // Checks if peer is a seeder.
            if (!HasPiece.OfType<bool>().Contains(false))
            {
                Complete = true;
            }

            // Sets current progress and invokes event.
            CurrentProgress = CurrentProgress / m_torrent.Length;
            BitfieldRecieved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles a cancel message.
        /// </summary>
        /// <param name="a_index">The piece index of canceled block.</param>
        /// <param name="a_begin">The beginning offset of the cancelled block.</param>
        /// <param name="a_length">The length of the cancelled block.</param>
        /// <remarks>
        /// HandleCancelMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     void HandleCancelMessage(int a_index, int a_begin, int a_length);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will handle a received cancel message after it has
        ///     been decoded. It invokes an event allowing the torrent to handle
        ///     the cancel message.
        ///     
        /// </remarks>
        private void HandleCancelMessage(int a_index, int a_begin, int a_length)
        {
            // Invokes event.
            BlockCanceled?.Invoke(this, new OutgoingBlock(this, a_index, a_begin, a_length));
        }

        /// <summary>
        /// Handles a choke message.
        /// </summary>
        /// <remarks>
        /// HandleChokeMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     void HandleChokeMessage();
        ///   
        /// DESCRIPTION
        /// 
        ///     This function will handle a decoded choke message by setting the
        ///     bool PeerChoking to true.
        /// 
        /// </remarks>
        private void HandleChokeMessage()
        {
            PeerChoking = true;
        }

        /// <summary>
        /// Handles a have message.
        /// </summary>
        /// <param name="a_pieceIndex">The piece index.</param>
        /// <remarks>
        /// HandleHaveMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     void HandleHaveMessage(int a_pieceIndex);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will handle a decoded have message by setting the
        ///     piece index in HasPiece to true marking the piece as available.
        ///     It will also invoke an event.
        ///     
        /// </remarks>
        private void HandleHaveMessage(int a_pieceIndex)
        {
            // Add the piece to available pieces client can download.
            HasPiece[a_pieceIndex] = true;

            // Invokes event.
            HaveRecieved?.Invoke(this, a_pieceIndex);
        }

        /// <summary>
        /// Handles a handshake message.
        /// </summary>
        /// <param name="a_infoHash">Info hash received.</param>
        /// <remarks>
        /// HandleHandshakeMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     void HandleHandshakeMessage(byte[] a_infoHash);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will handle a decoded handshake message. It will 
        ///     compare the received info hash with the one that was sent. If
        ///     the hash values don't match, the client disconnects from the peer.
        ///     It will set the bool HandshakeReceived to true if the hashes match.
        ///     
        /// </remarks>
        private void HandleHandshakeMessage(byte[] a_infoHash)
        {
            // Compares hashes and disconnects if they do not match.
            if (!SameHash(a_infoHash))
            {
                Disconnect();
                return;
            }

            HandshakeReceived = true;
        }

        /// <summary>
        /// Handles an interested message.
        /// </summary>
        /// <remarks>
        /// HandleInterestedMessage()
        /// 
        /// SYNOPSIS
        ///     
        ///     void HandleInterestedMessage();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will handle a decoded interested message by setting
        ///     the bool PeerInterested to true.
        /// 
        /// </remarks>
        private void HandleInterestedMessage()
        {
            PeerInterested = true;
        }

        /// <summary>
        /// Handles a not interested message.
        /// </summary>
        /// <remarks>
        /// HandleNotInterestedMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     void HandleNotInterestedMessage();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will handle a decoded not interested message by 
        ///     setting the bool AmIntersted to false.
        ///     
        /// </remarks>
        private void HandleNotInterestedMessage()
        {
            AmInterested = false;
        }

        /// <summary>
        /// Handles a piece message.
        /// </summary>
        /// <param name="a_index">The index of the piece.</param>
        /// <param name="a_begin">The beginning offset of the block.</param>
        /// <param name="a_block">The length of the block.</param>
        /// <remarks>
        /// HandlePieceMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     void HandlePieceMessage(int a_index, int a_begin, byte[] a_block);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will handle a decoded piece message by invoking the
        ///     event handler.
        ///     
        /// </remarks>
        private void HandlePieceMessage(int a_index, int a_begin, byte[] a_block)
        {
            // Invokes event.
            BlockReceived?.Invoke(this, new IncomingBlock(this, a_index, a_begin, a_block));
        }

        /// <summary>
        /// Handles a request message.
        /// </summary>
        /// <param name="a_index">The index of the piece.</param>
        /// <param name="a_begin">The beginning offset of the block.</param>
        /// <param name="a_length">The length of the block.</param>
        /// <remarks>
        /// HandleRequestMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     void HandleRequestMessage(int a_index, int a_begin, int a_length);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will handle a decoded request message by invoking
        ///     the event handler.
        /// 
        /// </remarks>
        private void HandleRequestMessage(int a_index, int a_begin, int a_length)
        {
            // Invokes event.
            BlockRequested?.Invoke(this, new OutgoingBlock(this, a_index, a_begin, a_length));
        }

        /// <summary>
        /// Handles an unchoke message.
        /// </summary>
        /// <remarks>
        /// HandleUnchokeMessage()
        /// 
        /// SYNOPSIS
        /// 
        ///     void HandleUnchokeMessage();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will handle a decoded unchoke message by setting
        ///     the bool PeerChoking to false.
        ///     
        /// </remarks>
        private void HandleUnchokeMessage()
        {
            PeerChoking = false;
        }

        #endregion

        /// <summary>
        /// Recieves an incoming message.
        /// </summary>
        /// <param name="a_client">The socket to listen on.</param>
        /// <remarks>
        /// Receive()
        /// 
        /// SYNOPSIS
        /// 
        ///     void Receive(Socket a_client);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will start the asynchronous receive call for 
        ///     receiving messages from the peer. If an exception is thrown then
        ///     the peer is disconnected.
        /// 
        /// </remarks>
        private void Receive(Socket a_client)
        {
            try
            {
                var state = new AsyncStateObject();
                state.WorkSocket = a_client;

                a_client.BeginReceive(state.ReceiveBuffer, 0, AsyncStateObject.ReceiveBufferSize / 2, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch 
            {
                Disconnect();
            }
        }

        /// <summary>
        /// The receive asynchronous callback.
        /// </summary>
        /// <param name="a_ar">The IAsyncResult.</param>
        /// <remarks>
        /// ReceiveCallback()
        /// 
        /// SYNOPSIS
        /// 
        ///     void ReceiveCallback(IAsyncResult a_ar);
        ///     
        /// DESCRIPTION 
        /// 
        ///     This function handles the received message from the peer. It 
        ///     computes the number of bytes received from the state object and
        ///     the adds them to the total bytes read. It next computes the length
        ///     of the next message starts continues listening for more incoming
        ///     messages.
        ///     
        /// </remarks>
        private void ReceiveCallback(IAsyncResult a_ar)
        {
            // Gets the state from the async results.
            AsyncStateObject state;
            Socket client;
            try
            {
                state = (AsyncStateObject)a_ar.AsyncState;
                client = state.WorkSocket;
                state.BytesRead = client.EndReceive(a_ar);
                state.TotalBytesRead += state.BytesRead;
            }
            catch
            {
                Disconnect();
                return;
            }
          
            // Computes the message length.
            var messageLength = GetMessageLength(state.ReceiveBuffer);
  
            // Handles messages that are in the buffer.          
            while (state.TotalBytesRead >= messageLength)
            {
                // Gets the first message in the buffer.
                var messageData = Utility.SubArray(state.ReceiveBuffer, 0, messageLength);

                // Handles the message.
                HandleIncomingMessage(messageData);

                // Removes the handled message from the buffer.
                Array.Copy(state.ReceiveBuffer, messageLength, state.ReceiveBuffer,
                    0, state.TotalBytesRead);
                state.TotalBytesRead = state.TotalBytesRead - messageLength;

                // Compute the message length of the next message.
                messageLength = GetMessageLength(state.ReceiveBuffer);
            }
            
            // Starts listening for more incoming messages.
            try
            {
                client.BeginReceive(state.ReceiveBuffer, state.TotalBytesRead,
                    AsyncStateObject.ReceiveBufferSize - state.TotalBytesRead, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch
            {
                Disconnect();
                return;
            }
        }

        /// <summary>
        /// Checks if torrent info hash matches torrent info hash.
        /// </summary>
        /// <param name="a_infoHash">The info hash that peer has sent.</param>
        /// <returns>Returns true if the hashes match and false if not.</returns>
        /// <remarks>
        /// SameHash()
        /// 
        /// SYNOPSIS
        /// 
        ///     SameHash(byte[] a_infoHash);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will check if the torrent info hash matches the info
        ///     hash that the peer sent in their handshake message. If it matches,
        ///     then true is returned and if not false is.
        /// </remarks>
        private bool SameHash(byte[] a_infoHash)
        {
            // Compares info hashes.
            if (a_infoHash.SequenceEqual(m_torrent.ByteInfoHash))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sends a message to the message.
        /// </summary>
        /// <param name="a_message">The message to send.</param>
        /// <remarks>
        /// Send()
        /// 
        /// SYNOPSIS
        /// 
        ///     void Send(byte[] a_message);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function sends a message to the client. The message to send
        ///     is passed in the parameter a_message. It starts an asynchronous
        ///     send call. If an exception is thrown the function Disconnect is
        ///     called.
        ///     
        /// </remarks>
        private void Send(byte[] a_message)
        {
            try
            {
                m_client.BeginSend(a_message, 0, a_message.Length, 0,
             new AsyncCallback(SendCallback), m_client);
            }
            catch
            {
                Disconnect();
            }
        }

        /// <summary>
        /// The asynchronous callback for a send call.
        /// </summary>
        /// <param name="ar">The async result.</param>
        /// <remarks>
        /// SendCallBack()
        /// 
        /// SYNOPSIS
        /// 
        ///     void SendCallback(IAsyncResult a_ar);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function is the asynchronous callback function for a beginSend
        ///     function. If an exception is thrown the function Disconnect is 
        ///     called.
        /// </remarks>
        private void SendCallback(IAsyncResult a_ar)
        {
            try
            {
                var client = a_ar.AsyncState as Socket;
                client.EndSend(a_ar);
            }
            catch
            {
                Disconnect();
            }

        }

        #endregion

        #endregion
    }
}