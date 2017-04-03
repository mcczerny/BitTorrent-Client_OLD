using System.Text;

namespace BitTorrent_Client.Models.Bencoding
{
    public class BDecodedString : BDecodedObject
    {
        public BDecodedString(byte[] decodedString) : base(decodedString)
        {

        }

        public string GetUTF8()
        {
            return Encoding.UTF8.GetString(Value);
        }
    }
}