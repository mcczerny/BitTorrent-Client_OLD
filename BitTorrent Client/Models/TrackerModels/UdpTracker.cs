using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

using BitTorrent_Client.Models.TorrentModels;
using BitTorrent_Client.Models.Utility_Functions;

namespace BitTorrent_Client.Models.TrackerModels
{
    public class UdpTracker : Tracker
    {
        private readonly long m_magicNumber = 0x41727101980;
        private readonly int m_announceMessageLength = 109;
        private readonly int m_connectMessageLength = 16;

        private int m_port;
        private string m_address;

        private byte[] m_connectionId;
        private byte[] m_transactionId;
        private UdpClient m_client;

        public UdpTracker(Torrent a_torrent, string a_trackerUrl)
            : base(a_torrent, a_trackerUrl)
        {
            m_client = new UdpClient();
            // Finds the port.
            var separatedAddress = a_trackerUrl.Split(':');
            m_port = Convert.ToInt32(separatedAddress[2]);

            // Finds the address
            separatedAddress = separatedAddress[1].Split(new string[] { "//" }, StringSplitOptions.None);
            m_address = separatedAddress[1];
        }

        private byte[] EncodeAnnouncePacket()
        {
            byte[] announceMessage = new byte[109];
            byte[] action = BitConverter.GetBytes(1).Reverse().ToArray();
            new Random().NextBytes(m_transactionId);
            

            Int64 numleft = m_torrent.Length;
            byte[] peerID = Encoding.ASCII.GetBytes("-qB33A0-XtNvaJ!5tsqy");
            byte[] left = BitConverter.GetBytes(numleft).Reverse().ToArray();
            byte[] eventType = BitConverter.GetBytes(2).Reverse().ToArray();
            byte[] key = new byte[4];
            new Random().NextBytes(key);
            byte[] numWant = BitConverter.GetBytes(200).Reverse().ToArray();
            Int16 port = 1337;
            Int16 extension = 521;
            byte[] portBytes = BitConverter.GetBytes(port).Reverse().ToArray();
            byte[] extensions = BitConverter.GetBytes(extension).Reverse().ToArray();
            byte[] announce = Encoding.ASCII.GetBytes("/announce");
            
            Buffer.BlockCopy(m_connectionId, 0, announceMessage, 0, 8);
            Buffer.BlockCopy(action, 0, announceMessage, 8, 4);
            Buffer.BlockCopy(m_transactionId, 0, announceMessage, 12, 4);
            Buffer.BlockCopy(m_torrent.ByteInfoHash, 0, announceMessage, 16, 20);
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
            TimeSpan timeElasped = DateTime.Now.Subtract(m_lastUpdate);
            if (timeElasped.Seconds > Interval)
            {
                SendRequest();
            }

        }

        private void SendRequest()
        {
            try
            {
                m_client.Connect(m_address, m_port);
                m_client.Send(EncodeConnectPacket(), m_connectMessageLength);
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("151.80.120.115"), 2710);


                byte[] connectResponse = m_client.Receive(ref remoteEP);
                if (connectResponse.Length < 16)
                {
                    return;
                }

                m_connectionId = Utility.SubArray(connectResponse, 8, 8);

                byte[] receivedTransactionId = Utility.SubArray(connectResponse, 4, 4);
                if (!m_transactionId.SequenceEqual(receivedTransactionId))
                {
                    return;
                }

                m_client.Send(EncodeAnnouncePacket(), m_announceMessageLength);
                byte[] announceResponse = m_client.Receive(ref remoteEP);


                byte[] interval = Utility.SubArray(announceResponse, 8, 4).Reverse().ToArray();
                Interval = BitConverter.ToInt32(interval, 0);

                var peerLength = announceResponse.Length - 20;
                byte[] rawPeers = Utility.SubArray(announceResponse, 20, peerLength);
                ParsePeers(rawPeers);

                // Call the base Torrent class invocation method.
                base.OnPeerListUpdated();
            }
            catch
            {

            }

        }
    }
}