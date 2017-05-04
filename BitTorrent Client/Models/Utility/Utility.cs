using System;
using System.Collections.Concurrent; 

namespace BitTorrent_Client.Models.Utility_Functions
{
    /// <summary>
    /// Class contains utility methods.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Gets a readable byte format.
        /// </summary>
        /// <param name="a_length">Number to convert into readable format.</param>
        /// <returns>Returns a string of a readable number of bytes.</returns>
        /// <remarks>
        /// GetBytesReadable()
        /// 
        /// SYNOPSIS
        /// 
        ///     string GetBytesReadable(long a_length);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will take in a number and convert it into a more
        ///     readable byte format. This is useful for file sizes.
        ///    
        /// </remarks>
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

        /// <summary>
        /// Gets a readable bit format.
        /// </summary>
        /// <param name="a_length">Number to convert into readable format.</param>
        /// <returns>Returns a string of a readable number of bits.</returns>
        /// <remarks>
        /// GetBitsReadable()
        /// 
        /// SYNOPSIS
        ///     
        ///     string GetBitsReadable(long a_length);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will take a number and convert it into a more
        ///     readable bit format. This is useful for computing download speed.
        ///     
        /// </remarks>
        public static string GetBitsReadable(long a_length)
        {
            // Get absolute value
            long absolute_i = (a_length < 0 ? -a_length : a_length) * 8;

            // Determine the suffix and readable value
            string suffix;
            double readable;

            // Terabit.
            if (absolute_i >= 0x10000000000)
            {
                suffix = "Tb";
                readable = (a_length >> 30);
            }
            // Gigabit
            else if (absolute_i >= 0x40000000)
            {
                suffix = "Gb";
                readable = (a_length >> 20);
            }
            // Megabit
            else if (absolute_i >= 0x100000)
            {
                suffix = "Mb";
                readable = (a_length >> 10);
            }
            // Kilobit
            else if (absolute_i >= 0x400)
            {
                suffix = "Kb";
                readable = a_length;
            }
            // Bit
            else
            {
                return a_length.ToString("0 B");
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);
            // Return formatted number with suffix
            return readable.ToString("0.## ") + suffix;
        }

        /// <summary>
        /// Creates a SubArray from given paramters.
        /// </summary>
        /// <param name="a_data">Source array.</param>
        /// <param name="a_index">Start index.</param>
        /// <param name="a_length">Length to read.</param>
        /// <returns>Returns a byte array containing a sub array.</returns>
        /// <remarks>
        /// SubArray()
        /// 
        /// SYNOPSIS
        /// 
        ///     byte[] SubArray(byte[] a_data, int a_index, int a_length);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will return a subarray from the given parameters.
        ///     It will use Buffer.BlockCopy to do this.
        ///     
        /// </remarks>
        public static byte[] SubArray(byte[] a_data, int a_index, int a_length)
        {
            var result = new byte[a_length];
            Buffer.BlockCopy(a_data, a_index, result, 0, a_length);

            return result;
        }

    }

    /// <summary>
    /// This class contains dictionary extension methods.
    /// </summary>
    public static class DictionaryExtension
    {
        /// <summary>
        /// Implements TryRemove function without out parameter.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type.</typeparam>
        /// <typeparam name="TValue">Dictionary value type. </typeparam>
        /// <param name="a_dictionary">The dictionary to run function on.</param>
        /// <param name="a_key">The key value to remove.</param>
        /// <returns>Returns true if removed and false if not.</returns>
        /// <remarks>
        /// TryRemove()
        /// 
        /// SYNOPSIS
        /// 
        ///     bool TryRemove<TKey, TValue>
        ///         (this ConcurrentDictionary<TKey, TValue> a_dictionary, TKey a_key);
        ///         
        /// DESCRIPTION
        /// 
        ///     This function implements a TryRemove function for a concurrent 
        ///     dictionary, so an out value is not needed as a function parameter.
        ///     
        /// </remarks>
        public static bool TryRemove<TKey, TValue>
            (this ConcurrentDictionary<TKey, TValue> a_dictionary, TKey a_key)
        {
            TValue temp;
            return a_dictionary.TryRemove(a_key, out temp);
        }
    }

}