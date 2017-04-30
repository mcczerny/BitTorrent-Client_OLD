namespace BitTorrent_Client.Models.Bencoding
{
    /// <summary>
    /// Base class for decoded bencode data types.
    /// </summary>
    public abstract class BDecodedObject
    {
        #region Constructors 

        /// <summary>
        /// Constructor sets Value property.
        /// </summary>
        /// <param name="a_value">The value of the decoded bencode data type.</param>
        public BDecodedObject(dynamic a_value)
        {
            Value = a_value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The value of a decoded bencode object. It can be either a dictionary,
        /// long, list, or byte string.
        /// </summary>
        public dynamic Value
        {
            get;
            set;
        }

        #endregion
    }
}