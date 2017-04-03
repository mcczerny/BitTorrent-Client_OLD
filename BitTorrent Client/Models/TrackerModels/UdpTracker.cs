using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

using BitTorrent_Client.Models.TorrentModels;

namespace BitTorrent_Client.Models.TrackerModels
{
    public class UdpTracker : Tracker
    {
        private readonly long m_magicNumber = 0x41727101980;
        private readonly int m_announceMessageLength = 109;
        private readonly int m_connectMessageLength = 16;
        private byte[] m_connectionId;
        private byte[] m_transactionId;
        private UdpClient m_client;

        public UdpTracker(Torrent a_torrent, string a_trackerUrl)
            : base(a_torrent, a_trackerUrl)
        {
                 
        }

        private byte[] EncodeAnnouncePacket()
        {
            byte[] announceMessage = new byte[109];
            byte[] action = BitConverter.GetBytes(1).Reverse().ToArray();
            new Random().NextBytes(m_transactionId);
            byte[] infoHash = new byte[] 
            {
                0x22, 0x6c, 0x2f, 0x41, 0x71,
                0xd7, 0x87, 0x2b, 0x51, 0x61,
                0x99, 0xe9, 0xf0, 0x0d, 0xdb,
                0x11, 0x8e, 0xf5, 0x55, 0x39
            };

            Int64 numleft = 136239194;
            byte[] peerID = Encoding.ASCII.GetBytes("-qB33A0-XtNvaJ!5tsqy");
            byte[] left = BitConverter.GetBytes(numleft).Reverse().ToArray();
            byte[] eventType = BitConverter.GetBytes(2).Reverse().ToArray();
            byte[] key = new byte[4];
            new Random().NextBytes(key);
            byte[] numWant = BitConverter.GetBytes(200).Reverse().ToArray();
            Int16 port = 8999;
            Int16 extension = 521;
            byte[] portBytes = BitConverter.GetBytes(port).Reverse().ToArray();
            byte[] extensions = BitConverter.GetBytes(extension).Reverse().ToArray();
            byte[] announce = Encoding.ASCII.GetBytes("/announce");
            
            Buffer.BlockCopy(m_connectionId, 0, announceMessage, 0, 8);
            Buffer.BlockCopy(action, 0, announceMessage, 8, 4);
            Buffer.BlockCopy(m_transactionId, 0, announceMessage, 12, 4);
            Buffer.BlockCopy(infoHash, 0, announceMessage, 16, 20);
            Buffer.BlockCopy(peerID, 0, announceMessage, 36, 20);
            Buffer.BlockCopy(left, 0, announceMessage, 64, 8);
            Buffer.BlockCopy(eventType, 0, announceMessage, 80, 4);
            Buffer.BlockCopy(key, 0, announceMessage, 88, 4);
            Buffer.BlockCopy(numWant, 0, announceMessage, 92, 4);
            Buffer.BlockCopy(portBytes, 0, announceMessage, 96, 2);
            Buffer.BlockCopy(extensions, 0, announceMessage, 98, 2);
            Buffer.BlockCopy(announce, 0, announceMessage, 100, 9);

            return announceMessage;
        }
        private byte[] EncodeConnectPacket()
        {
            byte[] connectMessage = new byte[16];

            m_connectionId = BitConverter.GetBytes(m_magicNumber).Reverse().ToArray();
            m_transactionId = new byte[4];

            new Random().NextBytes(m_transactionId);

            Buffer.BlockCopy(m_connectionId, 0, connectMessage, 0, 8);
            Buffer.BlockCopy(m_transactionId, 0, connectMessage, 12, 4);

            return connectMessage;
        }


    
        override public void Update()
        {
            try
            {
                m_client.Connect("9.rarbg.me", 2710);

                m_client.Send(EncodeConnectPacket(), m_connectMessageLength);

                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("151.80.120.115"), 2710);

                byte[] connectResponse = m_client.Receive(ref remoteEP);

                if(connectResponse.Length < 16)
                {
                    return;
                }

                byte[] receivedTransactionId = new byte[4];
                Buffer.BlockCopy(connectResponse, 4, receivedTransactionId, 0, 4);

                if (!m_transactionId.SequenceEqual(receivedTransactionId))
                {
                    return;
                }

                m_client.Send(EncodeAnnouncePacket(), m_announceMessageLength);

                //byte[] announcepacket =
                //{
                //    0xc4, 0xc5, 0x7b, 0x88, 0x66, 0xf5, 0x0a, 0x9c,
                //    0x00, 0x00, 0x00, 0x01, 0xa4, 0x7c, 0xfe, 0x59,
                //    0x22, 0x6c, 0x2f, 0x51, 0x71, 0xd7, 0x87, 0x2b,
                //    0x51, 0x61, 0x99, 0xe9, 0xf0, 0x0d, 0xdb, 0x11,
                //    0x8e, 0xf5, 0x55, 0x39, 0x2d, 0x71, 0x42, 0x33,
                //    0x33, 0x41, 0x30, 0x2d, 0x6b, 0x61, 0x63, 0x49,
                //    0x47, 0x49, 0x55, 0x79, 0x70, 0x56, 0x4e, 0x2a,
                //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                //    0x00, 0x00, 0x00, 0x00, 0x08, 0x1e, 0xd8, 0x5a,
                //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                //    0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00,
                //    0xde, 0xd3, 0x5b, 0x03, 0x00, 0x00, 0x00, 0xc8,
                //    0x23, 0x27, 0x02, 0x09, 0x2f, 0x61, 0x6e, 0x6e,
                //    0x6f, 0x75, 0x6e, 0x63, 0x65
                //};

                //m_client.Send(announcepacket, announcepacket.Length);
                byte[] announceResponse = m_client.Receive(ref remoteEP);
            }
            catch
            {

            }

        }
    }
}
