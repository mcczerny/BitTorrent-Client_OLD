using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrent_Client.Models.Utility_Functions
{
    public static class Utility
    {
        public static string GetBytesReadable(long a_length)
        {
            // Get absolute value
            long absolute_i = (a_length < 0 ? -a_length : a_length);
            // Determine the suffix and readable value
            string suffix;
            double readable;

            // Terabyte.
            if (absolute_i >= 0x10000000000)
            {
                suffix = "TB";
                readable = (a_length >> 30);
            }
            // Gigabyte
            else if (absolute_i >= 0x40000000)
            {
                suffix = "GB";
                readable = (a_length >> 20);
            }
            // Megabyte
            else if (absolute_i >= 0x100000)
            {
                suffix = "MB";
                readable = (a_length >> 10);
            }
            // Kilobyte
            else if (absolute_i >= 0x400)
            {
                suffix = "KB";
                readable = a_length;
            }
            // Byte
            else
            {
                return a_length.ToString("0 B");
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);
            // Return formatted number with suffix
            return readable.ToString("0.## ") + suffix;
        }

        public static byte[] SubArray(byte[] a_data, int a_index, int a_length)
        {
            byte[] result = new byte[a_length];

            Buffer.BlockCopy(a_data, a_index, result, 0, a_length);
            return result;
        }
    }
}
