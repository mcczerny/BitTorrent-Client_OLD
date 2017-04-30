namespace BitTorrent_Client.Models.Bencoding
{
    /// <summary>
    /// Class is used for storing the value of decoded bencode data type: Byte string.
    /// </summary>
    public class BDecodedString : BDecodedObject
    {
        #region Constructors

        /// <summary>
        /// Calls base class constructor.
        /// </summary>
        /// <param name="decodedString">A decoded byte string.</param>
        public BDecodedString(byte[] decodedString) 
            : base(decodedString)
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets an utf8 encoded string.
        /// </summary>
        /// <returns>Returns the utf8 encoded byte string.</returns>
        /// <remarks>
        /// GetUTF8()
        /// 
        /// SYNOPSIS
        ///     
        ///     string GetUTF8();
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will return a UTF8 encoded string of the property 
        ///     Value, which is a byte array containing a decoded bencode byte string.
        /// </remarks>
        public string GetUTF8()
        {
            return System.Text.Encoding.UTF8.GetString(Value);
        }

        #endregion
    }
}