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

        /// <summary>
        /// Updates the tracker.
        /// </summary>
        /// <remarks>
        /// Update()
        /// 
        /// SYNOPSIS
        /// 
        ///     void Update();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will update a the tracker. It will check if the 
        ///     enough time has passed to send another tracker request. 
        ///     
        /// </remarks>
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

        /// <summary>
        /// Encodes a tracker request URL.
        /// </summary>
        /// <returns>Returns an encoded tracker request message.</returns>
        /// <remarks>
        /// EncodeTrackerRequest()
        /// 
        /// SYNOPSIS
        /// 
        ///     string EncodeTrackerRequest();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will encode a tracker request URL. It will append
        ///     the neccesary parameters needed, such as the tracker url, a URL
        ///     encoded info hash, the peer id, the port and so on.
        ///     
        /// </remarks>
        private string EncodeTrackerRequest()
        {
            StringBuilder requestUrl = new StringBuilder();

            requestUrl.Append(TrackerUrl);
            requestUrl.Append("?");
            requestUrl.Append("info_hash=");
            requestUrl.Append(UrlEncodeHash());
            requestUrl.Append("&peer_id=-qB33A0-XtNvaJ!5tsqy");
            requestUrl.Append("&port=8999");
            requestUrl.Append("&uploaded=0");
            requestUrl.Append("&downloaded=0");
            requestUrl.Append("&left=");
            requestUrl.Append(m_torrent.Length);
            requestUrl.Append("&numwant=100");
            requestUrl.Append("&event=started");
            requestUrl.Append("&compact=1");

            return requestUrl.ToString();
        }

        /// <summary>
        /// Sends a request to tracker.
        /// </summary>
        /// <remarks>
        /// SendTrackerRequest()
        /// 
        /// SYNOPSIS
        /// 
        ///     void SendTrackerRequest()
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will send and receive a tracker response. Once a 
        ///     response is received it will decode the response and if the 
        ///     response is valid, it will call the base class event.
        ///     
        /// </remarks>
        private void SendTrackerRequest()
        {
            try
            {
                // Sends request and gets response.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(EncodeTrackerRequest());
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // Copy response.
                var responseStream = response.GetResponseStream();
                var memoryStream = new MemoryStream();
                responseStream.CopyTo(memoryStream);
                byte[] rawTrackerResponse = memoryStream.ToArray();

                // Decode and parse response.
                var decodedResponse = Bencode.BDecode(rawTrackerResponse).ElementAt(0).Value;
                if(ParseResponse(decodedResponse))
                {
                    // Call the base Torrent class invocation method.
                    base.OnPeerListUpdated();
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Parses tracker response.
        /// </summary>
        /// <param name="a_decodedResponse">Decoded tracker response.</param>
        /// <returns>Returns true if valid response and false if not.</returns>
        /// <remarks>
        /// ParseResponse()
        /// 
        /// SYNOPSIS
        /// 
        ///     bool ParseResponse(Dictionary<string, BDecodedObject> a_decodedResponse);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will parse the tracker response. If the tracker
        ///     responds back with a failure message, then false is returned.
        ///     If not, the function returns true.
        ///     
        /// </remarks>
        private bool ParseResponse(Dictionary<string, BDecodedObject> a_decodedResponse)
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

        /// <summary>
        /// Url encodes a sha1 hash.
        /// </summary>
        /// <returns>Returns a urlencoded hash.</returns>
        /// <remarks>
        /// UrlEncodeHash()
        /// 
        /// SYNOPSIS
        /// 
        ///     string UrlEncodeHash();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will take the torrents byte info hash and url encode
        ///     it to be able to use it in an http request.
        ///     
        /// </remarks>
        private string UrlEncodeHash()
        {
            StringBuilder urlEncodedHash = new StringBuilder();
            // Go through each byte in info hash.
            foreach (byte value in m_torrent.ByteInfoHash)
            {
                // If the characters are numbers or letters.
                if (value < 128 && value > 96 || value < 91 && value > 64 || value < 58 && value > 47)
                {
                    urlEncodedHash.Append((char)value);
                }
                // Non standard character that must be converter for use in URL.
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