using System;
using System.Collections.Generic;
using System.Text;

namespace BitTorrent_Client.Models.Bencoding
{
    /// <summary>
    /// This class is responsible for decoding bencoded byte data.
    /// </summary>
    public class Bencode
    {
        #region Fields

        // Stores current location of a_data.
        private static int m_index;
        // Stores the current character at m_index.
        private static char m_currChar;

        #endregion

        #region Methods

        /// <summary>
        /// Decodes a bencoded byte array.
        /// </summary>
        /// <param name="a_data">Bencoded byte data that will be decoded.</param>
        /// <returns>Returns a list of BDecodedObjects</returns>
        /// <remarks>
        /// BDecode()
        /// 
        /// SYNOPSIS
        /// 
        ///     List<BDecodedObject> BDecode(byte[] a_data);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will decode a bencoded byte array. It will start 
        ///     at the beginning of the array containing the bencoded data and 
        ///     read until the end by using a while loop. Inside the while loop 
        ///     there is a switch statement that will read the next character and
        ///     call the correct function to decode the bencoded type. As it 
        ///     decodes each type it will add the new object to the list of 
        ///     BDecodedObjects to return.
        /// </remarks>
        public static List<BDecodedObject> BDecode(byte[] a_data)
        {
            List<BDecodedObject> decodedObjectList = new List<BDecodedObject>();

            // Set m_index to start of a_data.
            m_index = 0;

            // Read to end of byte array.
            while (m_index < a_data.Length)
            {
                m_currChar = (char)a_data[m_index];

                switch (m_currChar)
                {
                    case 'd':
                        decodedObjectList.Add(DecodeDictionary(a_data));
                        break;
                    case 'i':
                        decodedObjectList.Add(DecodeInteger(a_data));
                        break;
                    case 'l':
                        decodedObjectList.Add(new BDecodedList(DecodeList(a_data)));
                        break;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        decodedObjectList.Add(DecodeString(a_data));
                        break;
                    default:
                        throw new FormatException("Invalid delimiter detected");
                }
                m_index++;
            }
            return decodedObjectList;
        }

        /// <summary>
        /// Decodes a bencoded dictionary.
        /// </summary>
        /// <param name="a_data">Byte array containing bencoded data.</param>
        /// <returns>Returns a BDecodedDictionary containing a decoded bencoded dictionary</returns>
        /// <remarks>
        /// DecodeDictionary()
        /// 
        /// SYNOPSIS
        /// 
        ///     BDecodedDictionary DecodeDictionary(byte[] a_data);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will decode a bencoded dictionary. It will read the
        ///     bytes at current index and first determine the key of the dictionary.
        ///     It then determines the bencode type of the value in the dictionary
        ///     and uses a switch statement to call the appropriate decode 
        ///     function to decode the value of the dictionary.
        ///     
        /// </remarks>
        private static BDecodedDictionary DecodeDictionary(byte[] a_data)
        {
            Dictionary<string, BDecodedObject> decodedDictionary = new Dictionary<string, BDecodedObject>();

            m_index++;
            m_currChar = (char)a_data[m_index];

            while (m_currChar != 'e' && m_index < a_data.Length)
            {
                // They key must be a string so this character must be a number.
                if (!Char.IsDigit(m_currChar))
                {
                    throw new FormatException("The key must be a string so this character must be a number");
                }

                // Generate the key that must be a UTF-8 encoded string.
                string key = DecodeString(a_data).GetUTF8();

                // Find pair value.
                m_index++;
                m_currChar = (char)a_data[m_index];

                switch (m_currChar)
                {
                    case 'd':
                        decodedDictionary.Add(key, DecodeDictionary(a_data));
                        break;
                    case 'i':
                        decodedDictionary.Add(key, DecodeInteger(a_data));
                        break;
                    case 'l':
                        decodedDictionary.Add(key, new BDecodedList(DecodeList(a_data)));
                        break;

                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        decodedDictionary.Add(key, DecodeString(a_data));
                        break;
                }
                m_index++;
                m_currChar = (char)a_data[m_index];
            }
            return new BDecodedDictionary(decodedDictionary);
        }

        /// <summary>
        /// Decodes a bencoded integer.
        /// </summary>
        /// <param name="a_data">Byte array containing bencoded data.</param>
        /// <returns>Returns a BDecodedInteger containing a decoded bencoded integer</returns>
        /// <remarks>
        /// DecodeInteger()
        /// 
        /// SYNOPSIS
        /// 
        ///     BDecodedInteger DecodeInteger(byte[] a_data);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will decode a bencoded integer. It will read the
        ///     bytes at current index and keep reading until the delimiter for
        ///     a bencoded integer has been reached.
        ///     
        /// </remarks>
        private static BDecodedInteger DecodeInteger(byte[] a_data)
        {
            bool isNegative = false;
            StringBuilder decodedInt = new StringBuilder("");

            m_index++;
            m_currChar = (char)a_data[m_index];

            // Reads until it it reaches the delimter or the end of byte array.
            while (m_currChar != 'e' && m_index < a_data.Length)
            {
                // If char is not a number then something is wrong.
                if (!Char.IsDigit(m_currChar) && m_currChar != '-')
                {
                    throw new FormatException("The character must be a number.");
                }

                // If there are multiple '-' characters then something is wrong.
                if (m_currChar == '-' && isNegative)
                {
                    throw new FormatException("There can only be one '-' in a number.");
                }

                // To mark that the number is negative.
                if (m_currChar == '-')
                {
                    isNegative = true;
                }
                // Append the char that is a number and move to the next byte.
                decodedInt.Append(m_currChar);
                m_index++;
                m_currChar = (char)a_data[m_index];
            }

            // Cannot contain leading zero(s) according to Bencode specifications.
            if (decodedInt.ToString()[0] == '0' && decodedInt.Length > 1)
            {
                throw new FormatException("Cannot contain leading zero(s)");
            }

            // -0 is invalid according to Bencode specifications.
            if (decodedInt.Equals("-0"))
            {
                throw new FormatException("'-0' is an invalid format");
            }

            // If decodedInt is "" then that means there was no integer between the i and e.
            if (decodedInt.Equals(""))
            {
                throw new FormatException("Integer is formated incorrectly: Missing number.");
            }
            return new BDecodedInteger(long.Parse(decodedInt.ToString()));
        }

        /// <summary>
        /// Decodes a bencoded byte string.
        /// </summary>
        /// <param name="a_data">Byte array containing bencoded data.</param>
        /// <returns>Returns a BDecoded string containing a decoded byte string.</returns>
        /// <remarks>
        /// DecodeString()
        /// 
        /// SYNOPSIS
        /// 
        ///     BDecodedString DecodeString(byte[] a_data);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will decode a bencoded byte string. It will determine
        ///     the length of the byte string and then read until the end of the
        ///     bencoded byte string.
        ///     
        /// </remarks>
        private static BDecodedString DecodeString(byte[] a_data)
        {
            StringBuilder stringLength = new StringBuilder();

            // ':' is the ending delimter for the length of the string.
            while (m_currChar != ':')
            {
                // Append the number to stringLength and read the next byte.
                stringLength.Append(m_currChar);

                // If char is not a number then something is wrong.
                if (!Char.IsDigit(m_currChar))
                {
                    throw new FormatException("Invalid byte string length");
                }

                m_index++;
                m_currChar = (char)a_data[m_index];
            }

            var length = int.Parse(stringLength.ToString());

            byte[] byteString = new byte[length];

            for (var i = 0; i < length; i++)
            {
                m_index++;
                byteString[i] = a_data[m_index];

            }

            return new BDecodedString(byteString);
        }

        /// <summary>
        /// Decodes a bencoded list.
        /// </summary>
        /// <param name="a_data">Byte array containing bencoded data.</param>
        /// <returns>
        /// Returns a list of BDecodedObject objects containing a decoded bencoded integer.
        /// </returns>
        /// <remarks>
        /// DecodeList()
        /// 
        /// SYNOPSIS
        /// 
        ///     List<BDecodedOBject> DecodeList(byte[] a_data);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will decode a bencoded list. It will read the
        ///     bytes at current index and using a switch statement inside a while
        ///     loop will decode the bencoded data types that are in the list.
        ///     
        /// </remarks>
        private static List<BDecodedObject> DecodeList(byte[] a_data)
        {
            List<BDecodedObject> decodedList = new List<BDecodedObject>();

            m_index++;
            m_currChar = (char)a_data[m_index];

            while (m_currChar != 'e' && m_index < a_data.Length)
            {

                switch (m_currChar)
                {
                    case 'd':
                        decodedList.Add(DecodeDictionary(a_data));
                        break;
                    case 'i':
                        decodedList.Add(DecodeInteger(a_data));
                        break;
                    case 'l':
                        decodedList.Add(new BDecodedList(DecodeList(a_data)));
                        break;

                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        decodedList.Add(DecodeString(a_data));
                        break;
                }
                m_index++;
                m_currChar = (char)a_data[m_index];

            }
            return decodedList;
        }

        #endregion
    }
}