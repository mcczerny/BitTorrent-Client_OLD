using System.Collections.Generic;

namespace BitTorrent_Client.Models.Bencoding
{
    public class BDecodedDictionary : BDecodedObject
    {
        public BDecodedDictionary(Dictionary<string, BDecodedObject> decodedDictionary)
            : base(decodedDictionary)
        {

        }
    }
}