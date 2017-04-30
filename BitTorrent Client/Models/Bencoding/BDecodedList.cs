using System.Collections.Generic;

namespace BitTorrent_Client.Models.Bencoding
{
    /// <summary>
    /// Class is used for storing the value of decoded bencode data type: List.
    /// </summary>
    public class BDecodedList : BDecodedObject
    {
        #region Constructors

        /// <summary>
        /// Calls base class constructor.
        /// </summary>
        /// <param name="a_list">A decoded list.</param>
        public BDecodedList(List<BDecodedObject> a_list) 
            : base(a_list)
        {

        }

        #endregion
    }
}