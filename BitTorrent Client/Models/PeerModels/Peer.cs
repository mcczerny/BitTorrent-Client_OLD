using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.ComponentModel;
using System.Threading;


using BitTorrent_Client.Models.TorrentModels;
using BitTorrent_Client.Models.Utility_Functions;

namespace BitTorrent_Client.Models.PeerModels
{
    public class Peer 
    {
        #region Fields

        private IPEndPoint m_remoteEndPoint;
        private Torrent m_torrent;

        //private TcpClient m_client;
        //private NetworkStream m_networkStream;
        private Socket m_client;

        private readonly int m_handshakeSize = 68;

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

        private event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string a_propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(a_propertyName));
        }

        public event EventHandler Disconnected;
        public event EventHandler StateChanged;
        public event EventHandler<IncomingBlock> BlockReceived;
        public event EventHandler<OutgoingBlock> BlockRequested;
        public event EventHandler<OutgoingBlock> BlockCanceled;


        #endregion

        #region Enumerators

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

        public bool[] HasPiece
        {
            get;
            set;
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
        /// Get/Private set the IP address of the peer.
        /// </summary>
        public string IP
        {
            get;
            private set;
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
            
            SendHandshake();
            if (HandshakeSent)
            {

            }
        }

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
            Disconnected?.Invoke(this, new EventArgs());
        }

        #region Send Methods

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

        private void SendCallback(IAsyncResult ar)
       { 
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Send {0} bytes to server", bytesSent);
            }
            catch 
            {
                Disconnect();
            }

        }

        // Not done
        public void SendBitfield()
        {
            if (HandshakeSent)
            {
                Console.WriteLine("Sending bitfield message");
                //Send(EncodeBitfieldMessage())
            }
        }


        public void SendCancel(int a_index, int a_begin, int a_length)
        {
            Console.WriteLine("Sending cancel message");
            Send(EncodeCancelMessage(a_index, a_begin, a_length));

        }
        public void SendChoke()
        {
            Console.WriteLine("Sending choke message");
            Send(EncodeChokeMessage());
        }

        private void SendHandshake()
        {
            if (!HandshakeSent)
            {
                Console.WriteLine("Sending handshake message");
                Send(EncodeHandshakeMessage(m_torrent.ByteInfoHash));
                HandshakeSent = true;
            }
        }

        public void SendHave(int a_pieceIndex)
        {
            Console.WriteLine("Sending have message");
            Send(EncodeHaveMessage(a_pieceIndex));
        }

        public void SendInterested()
        {
            //Console.WriteLine("Sending interested message");
            AmInterested = true;
            Send(EncodeInterestedMessage());
        }

        public void SendKeepAliveMessage()
        {
            //Console.WriteLine("Sending keep alive message");
            Send(EncodeKeepAliveMessage());
        }

        public void SendNotInterested()
        {
            //Console.WriteLine("Sending keep alive message");
            AmInterested = false;
            Send(EncodeNotInterestedMessage());
        }

        public void SendPiece()
        {
            Console.WriteLine("Sending piece message");
            Send(EncodePieceMessage());
        }

        // Used for DHT not implemented
        public void SendPort()
        {

        }

        public void SendRequest(int a_pieceIndex, int a_begin, int a_length)
        {
            //Console.WriteLine("{0} Sending request message for piece {1} block {2}", IP, a_pieceIndex, a_begin/16384);
            Send(EncodeRequestMessage(a_pieceIndex, a_begin, a_length));
        }

        public void SendUnchoke()
        {
            Console.WriteLine("Sending unchoke message");
            Send(EncodeUnchokeMessage());
        }

        #endregion

        #endregion

        #region Private Methods

        #region Decode Message Methods

        private bool DecodeBitfieldMessage(byte[] a_message, out bool[] a_hasPiece)
        {
            a_hasPiece = new bool[m_torrent.NumberOfPieces];

            int correctMessageSize = (int)(Math.Ceiling(m_torrent.NumberOfPieces / 8.0));

            // If message length is not the same size.
            if (a_message.Length != (correctMessageSize + 5))
            {
                return false;
            }

            byte[] messagePayload = Utility.SubArray(a_message, 5, correctMessageSize);

            BitArray peerBitfield = new BitArray(messagePayload);

            // Convert bitfield into usable bool array.
            for (int i = 0; i < m_torrent.NumberOfPieces; i++)
            {
                a_hasPiece[i] = peerBitfield[peerBitfield.Length - 1 - i];
            }

            return true;
        }

        private bool DecodeCancelMessage(byte[] a_message, out int a_index,
            out int a_begin, out int a_length)
        {
            // Set base values.
            a_index = -1;
            a_begin = -1;
            a_length = -1;

            // Cancel message length is not correct length.
            if (a_message.Length != 17 || a_message[3] != 13)
            {
                return false;
            }

            byte[] indexBytes = Utility.SubArray(a_message, 5, 4);
            byte[] beginBytes = Utility.SubArray(a_message, 9, 4);
            byte[] lengthBytes = Utility.SubArray(a_message, 13, 4);


            // If the byte order of data is stored little endian then the 
            // bytes need to be reversed.
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

        private bool DecodeHaveMessage(byte[] a_message, out int a_pieceIndex)
        {
            // Set base value.
            a_pieceIndex = -1;

            // Have message is not correct length.
            if (a_message.Length != 9)
            {
                return false;
            }

            byte[] pieceIndex = Utility.SubArray(a_message, 5, 4);

            // If the byte order of data is stored little endian then the 
            // bytes need to be reversed.
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(pieceIndex);
            }

            // Convert from byte array to integer.
            a_pieceIndex = BitConverter.ToInt32(pieceIndex, 0);

            return true;
        }

        private bool DecodeHandshakeMessage(byte[] a_message, out byte[] a_infoHash)
        {
            // Set base value.
            a_infoHash = new byte[20];

            // Handshake message length is not correct length or the protocol
            // string length is not 19.
            if (a_message.Length != 68 || a_message[0] != 19)
            {
                return false;
            }

            // When the protocol name is not BitTorrent protocol.
            if (Encoding.ASCII.GetString(Utility.SubArray(a_message, 1, 19)) != "BitTorrent protocol")
            {
                return false;
            }

            a_infoHash = Utility.SubArray(a_message, 28, 20);

            return true;
        }

        private bool DecodeKeepAliveMessage(byte[] a_message)
        {
            // If the message length is not 4 bytes long or the bytes are not all 0
            if (a_message.Length != 4 || !a_message.SequenceEqual(new byte[] { 0, 0, 0, 0 }))
            {
                return false;
            }

            return true;
        }


        private bool DecodePeerState(byte[] a_message, int a_stateID)
        {
            if (a_message.Length != 5 || a_stateID < 0 || a_stateID > 3)
            {
                Console.WriteLine("Invalid state ID");
                return false;
            }

            return true;
        }

        private bool DecodePieceMessage(byte[] a_message, out int a_index, out int a_begin, out byte[] a_block)
        {
            // Set base values.
            a_index = -1;
            a_begin = -1;
            a_block = new byte[0];

            // If the piece message is too short.
            if (a_message.Length < 13)
            {
                Console.WriteLine("Invalid piece message length");
                return false;
            }


            byte[] lengthBytes = Utility.SubArray(a_message, 0, 4);
            byte[] indexBytes = Utility.SubArray(a_message, 5, 4);
            byte[] beginBytes = Utility.SubArray(a_message, 9, 4);

            // If the byte order of data is stored little endian then the 
            // bytes need to be reversed.
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthBytes);
                Array.Reverse(indexBytes);
                Array.Reverse(beginBytes);
            }

            // The length of the block.
            int blockLength = (BitConverter.ToInt32(lengthBytes, 0) - 9);

            // Convert from byte arrays to integer.
            a_index = BitConverter.ToInt32(indexBytes, 0);
            a_begin = BitConverter.ToInt32(beginBytes, 0);
            a_block = Utility.SubArray(a_message, 13, blockLength);

            return true;
        }

        // Used for DHT not done.
        private void DecodePortMessage()
        {

        }

        private bool DecodeRequestMessage(byte[] a_message, out int a_index, out int a_begin, out int a_length)
        {
            // Set base values.
            a_index = -1;
            a_begin = -1;
            a_length = -1;

            // When the requeset message is not the correct length.
            if (a_message.Length != 17 || a_message[3] != 13)
            {
                Console.WriteLine("Invalid request message");
                return false;
            }

            byte[] indexBytes = Utility.SubArray(a_message, 5, 4);
            byte[] beginBytes = Utility.SubArray(a_message, 9, 4);
            byte[] lengthBytes = Utility.SubArray(a_message, 13, 4);

            // If the byte order of data is stored little endian then the 
            // bytes need to be reversed.
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
        private byte[] EncodeBitfieldMessage(bool[] a_bitfield)
        {

            int bitfieldLength = (int)(Math.Ceiling(m_torrent.NumberOfPieces / 8.0));

            byte[] bitfieldMessage = new byte[bitfieldLength + 5];

            // Calculate length of of bitfield 

            return bitfieldMessage;
        }

        private byte[] EncodeCancelMessage(int a_index, int a_begin, int a_length)
        {
            byte[] cancelMessage = new byte[17];

            // The length of the message.
            cancelMessage[3] = 13;
            // Message ID.
            cancelMessage[4] = 8;

            byte[] pieceIndex = BitConverter.GetBytes(a_index);
            byte[] beginIndex = BitConverter.GetBytes(a_begin);
            byte[] requestLength = BitConverter.GetBytes(a_length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(pieceIndex);
                Array.Reverse(beginIndex);
                Array.Reverse(requestLength);
            }

            Buffer.BlockCopy(pieceIndex, 0, cancelMessage, 5, 4);
            Buffer.BlockCopy(beginIndex, 0, cancelMessage, 9, 4);
            Buffer.BlockCopy(requestLength, 0, cancelMessage, 13, 4);

            return cancelMessage;
        }

        private byte[] EncodeChokeMessage()
        {
            return new byte[] { 0, 0, 0, 1, 0 };
        }

        private byte[] EncodeHandshakeMessage(byte[] a_hash)
        {
            StringBuilder stringMessage = new StringBuilder();

            byte[] handshakeMessage = new byte[m_handshakeSize];

            handshakeMessage[0] = 19;

            Encoding.ASCII.GetBytes("BitTorrent protocol", 0, 19, handshakeMessage, 1);
            Buffer.BlockCopy(a_hash, 0, handshakeMessage, 28, a_hash.Length);
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

        private byte[] EncodeInterestedMessage()
        {
            return new byte[] { 0, 0, 0, 1, 2 };
        }

        private byte[] EncodeKeepAliveMessage()
        {
            return new byte[] { 0, 0, 0, 0 };
        }

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

        private byte[] EncodeRequestMessage(int a_pieceIndex, int a_begin, int a_length)
        {
            byte[] requestMessage = new byte[17];

            // The length of the message.
            requestMessage[3] = 13;
            // Message ID.
            requestMessage[4] = 6;

            byte[] pieceIndex = BitConverter.GetBytes(a_pieceIndex);
            byte[] beginIndex = BitConverter.GetBytes(a_begin);
            byte[] requestLength = BitConverter.GetBytes(a_length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(pieceIndex);
                Array.Reverse(beginIndex);
                Array.Reverse(requestLength);
            }

            Buffer.BlockCopy(pieceIndex, 0, requestMessage, 5, 4);  
            Buffer.BlockCopy(beginIndex, 0, requestMessage, 9, 4);
            Buffer.BlockCopy(requestLength, 0, requestMessage, 13, 4);

            return requestMessage;
        }

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

        private void HandleBitfieldMessage(bool[] a_hasPiece)
        {
            BitfieldReceived = true;

            HasPiece = new bool[a_hasPiece.Length];

            for (int i = 0; i < HasPiece.Length; i++)
            {
                HasPiece[i] = a_hasPiece[i];
            }

            StateChanged?.Invoke(this, new EventArgs());
        }

        private void HandleCancelMessage(int a_index, int a_begin, int a_length)
        {
            BlockCanceled?.Invoke(this, new OutgoingBlock(this, a_index, a_begin, a_length));
        }

        private void HandleChokeMessage()
        {
            PeerChoking = true;

            StateChanged?.Invoke(this, new EventArgs());
        }

        private void HandleHaveMessage(int a_pieceIndex)
        {

            // Add the piece to available pieces client can download.
            HasPiece[a_pieceIndex] = true;

            StateChanged?.Invoke(this, new EventArgs());
        }

        private void HandleHandshakeMessage(byte[] a_infoHash)
        {
            if (!SameHash(a_infoHash))
            {
                //Disconnect();
                return;
            }
            HandshakeReceived = true;
        }

        private void HandleInterestedMessage()
        {
            PeerInterested = true;

            StateChanged?.Invoke(this, new EventArgs());
        }

        private void HandleNotInterestedMessage()
        {
            AmInterested = false;

            StateChanged?.Invoke(this, new EventArgs());
        }

        private void HandlePieceMessage(int a_index, int a_begin, byte[] a_block)
        {
            BlockReceived?.Invoke(this, new IncomingBlock(this, a_index, a_begin, a_block));
        }

        private void HandlePortMessage()
        {

        }

        private void HandleRequestMessage(int a_index, int a_begin, int a_length)
        {
            BlockRequested?.Invoke(this, new OutgoingBlock(this, a_index, a_begin, a_length));
        }

        private void HandleUnchokeMessage()
        {
            PeerChoking = false;

            StateChanged?.Invoke(this, new EventArgs());
        }

        #endregion

        private void Receive(Socket a_client)
        {
            try
            {
                AsyncStateObject state = new AsyncStateObject();
                state.WorkSocket = a_client;

                a_client.BeginReceive(state.ReceiveBuffer, 0, AsyncStateObject.ReceiveBufferSize / 2, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch 
            {
                Disconnect();
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            AsyncStateObject state;
            Socket client;
            try
            {
                state = (AsyncStateObject)ar.AsyncState;
                client = state.WorkSocket;
                state.BytesRead = client.EndReceive(ar);
                state.TotalBytesRead += state.BytesRead;
            }
            catch
            {
                Disconnect();
                return;
            }
        
            int messageLength = GetMessageLength(state.ReceiveBuffer);
            while (state.TotalBytesRead >= messageLength)
            {
                byte[] messageData = Utility.SubArray(state.ReceiveBuffer, 0, messageLength);
                HandleIncomingMessage(messageData);

                Array.Copy(state.ReceiveBuffer, messageLength, state.ReceiveBuffer,
                    0, state.TotalBytesRead);
                state.TotalBytesRead = state.TotalBytesRead - messageLength;

                messageLength = GetMessageLength(state.ReceiveBuffer);
            }

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

        #endregion

        #endregion
    }
}