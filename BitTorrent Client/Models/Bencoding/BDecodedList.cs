using System.Collections.Generic;

namespace BitTorrent_Client.Models.Bencoding
{
    public class BDecodedList : BDecodedObject
    {
        public BDecodedList(List<BDecodedObject> decodedList) : base(decodedList)
        {

        }
    }
}