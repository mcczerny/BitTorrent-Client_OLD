using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;

using BitTorrent_Client.Models.Bencoding;
using BitTorrent_Client.Models.TorrentModels;
namespace BitTorrent_Client.Models.TrackerModels
{
    public class HttpTracker : Tracker
    {
        #region Fields

        private long m_minInterval;
        private string m_failureReason;
        private string m_warningMessage;
        private string m_trackerId;

        #endregion

        #region Constructors

        public HttpTracker(Torrent a_torrent, string a_trackerUrl) 
            : base(a_torrent, a_trackerUrl)
        {
         
        }

        #endregion

        #region Events


        #endregion

        #region Methods

        #region Public Methods

        override public void Update()
        {
            TimeSpan timeElasped = DateTime.Now.Subtract(m_lastUpdate);
            if(timeElasped.Seconds > Interval)
            {
                SendTrackerRequest();

            }
        }

        #endregion

        #region Private Methods

        private bool DecodeResponse(Dictionary<string, BDecodedObject> a_decodedResponse)
        {
            // If failure reason exist then no other values will be present.
            if (a_decodedResponse.ContainsKey("failure reason"))
            {
                m_failureReason = Encoding.UTF8.GetString(a_decodedResponse["failure reason"].Value);
                Console.WriteLine(m_failureReason);
                return false;
            }

            // No failure response, check other tracker values to see if they exist.

            // Optional message.
            if (a_decodedResponse.ContainsKey("warning message"))
            {
                m_warningMessage = a_decodedResponse["warning message"].Value;
            }

            Interval = a_decodedResponse["interval"].Value;

            // Optional message.
            if (a_decodedResponse.ContainsKey("min interval"))
            {
                m_minInterval = a_decodedResponse["min interval"].Value;
            }

            // Optional message.
            if (a_decodedResponse.ContainsKey("tracker id"))
            {
                m_trackerId = a_decodedResponse["tracker id"].Value;
            }

            Complete = a_decodedResponse["complete"].Value;
            Incomplete = a_decodedResponse["incomplete"].Value;

            ParsePeers(a_decodedResponse["peers"].Value);

            return true;
        }

        private string EncodeTrackerRequest()
        {
            StringBuilder requestUrl = new StringBuilder();

            requestUrl.Append(TrackerUrl);
            requestUrl.Append("?");
            requestUrl.Append("info_hash=");
            requestUrl.Append(UrlEncodeHash(m_torrent.ByteInfoHash));
            requestUrl.Append("&peer_id=-qB33A0-XtNvaJ!5tsqy");
            requestUrl.Append("&port=8999");
            requestUrl.Append("&uploaded=0");
            requestUrl.Append("&downloaded=0");
            requestUrl.Append("&left=1593835520");
            requestUrl.Append("&numwant=5");
            requestUrl.Append("&event=started");
            requestUrl.Append("&compact=1");

            return requestUrl.ToString();
        }

        private void ParsePeers(byte[] a_rawPeers)
        {
            var index = 0;
            while(index < a_rawPeers.Length)
            {
                StringBuilder address = new StringBuilder();
                for(var i = index; i < (index + 4); i++)
                {
                    address.Append(a_rawPeers[i]);
                    if(i != (index + 3))
                    {
                        address.Append(".");
                    }
                }
                address.Append(":");
                index += 4;

                // You must add the high and low bytes to get the port number.
                var highByte = a_rawPeers[index];
                var lowByte = a_rawPeers[index + 1];
                address.Append((highByte * 256 + lowByte));

                index += 2;

                Peers.Add(address.ToString());
            }
        }

        private void SendTrackerRequest()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(EncodeTrackerRequest());
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var responseStream = response.GetResponseStream();
            var memoryStream = new MemoryStream();

            responseStream.CopyTo(memoryStream);
            byte[] rawTrackerResponse = memoryStream.ToArray();

            var decodedResponse = Bencode.BDecode(rawTrackerResponse).ElementAt(0).Value;

            DecodeResponse(decodedResponse);

            // Call the base Torrent class invocation method.
            base.OnPeerListUpdated();
        }

        private string UrlEncodeHash(byte[] a_hash)
        {
            StringBuilder urlEncodedHash = new StringBuilder();
            foreach (byte value in a_hash)
            {
                if (value < 128 && value > 96 || value < 91 && value > 64 || value < 58 && value > 47)
                {
                    urlEncodedHash.Append((char)value);
                }
                else
                {
                    urlEncodedHash.Append("%" + value.ToString("x2"));
                }
            }

            return urlEncodedHash.ToString();
        }

        #endregion

        #endregion
    }
}