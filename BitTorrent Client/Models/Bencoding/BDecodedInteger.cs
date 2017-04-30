namespace BitTorrent_Client.Models.Bencoding
{
    /// <summary>
    /// Class is used for storing the value of decoded bencode data type: Integer.
    /// </summary>
    public class BDecodedInteger : BDecodedObject
    {
        #region Constructors

        /// <summary>
        /// Calls base class constructor.
        /// </summary>
        /// <param name="a_integer">A decoded integer.</param>
        public BDecodedInteger(long a_integer) 
            : base(a_integer)
        {

        }

        #endregion
    }
}