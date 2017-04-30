using System.Collections.Generic;

namespace BitTorrent_Client.Models.Bencoding
{
    /// <summary>
    /// Class is used for storing the value of decoded bencode data type: Dictionary.
    /// </summary>
    public class BDecodedDictionary : BDecodedObject
    {
        #region Constructors

        /// <summary>
        /// Calls base class constructor.
        /// </summary>
        /// <param name="a_dictionary">A decoded dictionary.</param>
        public BDecodedDictionary(Dictionary<string, BDecodedObject> a_dictionary)
            : base(a_dictionary)
        {

        }

        #endregion
    }
}