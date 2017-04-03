namespace BitTorrent_Client.Models.Bencoding
{
    public abstract class BDecodedObject
    {
        /// <summary>
        /// The value of the BDecoded object. It would be a dictionary, list, long
        /// or a byte array.
        /// </summary>
        public dynamic Value
        {
            get;
            set;
        }

        public BDecodedObject(dynamic a_value)
        {
            Value = a_value;
        }
    }
}