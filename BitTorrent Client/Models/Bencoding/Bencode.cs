using System;
using System.Collections.Generic;
using System.Text;

namespace BitTorrent_Client.Models.Bencoding
{
    public class Bencode
    {
        // Stores current location of a_file.
        private static int m_index;
        // Stores the current character at m_index.
        private static char m_currChar;

        /// <summary>
        /// Decodes a bencoded byte array.
        /// </summary>
        /// <param name="a_file">Bencoded byte data that will be decoded.</param>
        /// <returns>Returns a list of BDecodedObjects</returns>
        /// <remarks>
        /// BDecode()
        /// 
        /// SYNOPSIS
        /// 
        ///     List<BDecodedObject> BDecode(byte[] a_file)
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will decode a bencoded byte array. It will start 
        ///     at the beginning of the array and read until the end by using
        ///     a while loop. Inside the while loop there is a switch statement
        ///     that will read the next character and call the correct function
        ///     to decode the bencoded type. As it decodes each type it will
        ///     add the new object to the list of BDecodedObjects to return.
        /// </remarks>
        public static List<BDecodedObject> BDecode(byte[] a_file)
        {
            List<BDecodedObject> decodedObjectList = new List<BDecodedObject>();

            // Set m_index to start of a_file.
            m_index = 0;

            // Read to end of byte array.
            while (m_index < a_file.Length)
            {
                m_currChar = (char)a_file[m_index];

                switch (m_currChar)
                {
                    case 'd':
                        decodedObjectList.Add(DecodeDictionary(a_file));
                        break;
                    case 'i':
                        decodedObjectList.Add(DecodeInteger(a_file));
                        break;
                    case 'l':
                        decodedObjectList.Add(new BDecodedList(DecodeList(a_file)));
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
                        decodedObjectList.Add(DecodeString(a_file));
                        break;
                }
                m_index++;
            }
            return decodedObjectList;
        }
  
        private static BDecodedInteger DecodeInteger(byte[] a_file)
        {
            bool isNegative = false;
            StringBuilder decodedInt = new StringBuilder("");

            m_index++;
            m_currChar = (char)a_file[m_index];

            while (m_currChar != 'e' && m_index < a_file.Length)
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
                m_currChar = (char)a_file[m_index];
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

        private static BDecodedString DecodeString(byte[] a_file)
        {
            StringBuilder stringLength = new StringBuilder();


            // ':' is the ending delimter for the length of the string.
            while (m_currChar != ':')
            {
                // Append the number to stringLength and read the next byte.
                stringLength.Append(m_currChar);


                // If char is a number then something is wrong.
                if (!Char.IsDigit(m_currChar))
                {

                }

                m_index++;
                m_currChar = (char)a_file[m_index];
            }

            var length = int.Parse(stringLength.ToString());

            byte[] byteString = new byte[length];

            for (var i = 0; i < length; i++)
            {
                m_index++;
                byteString[i] = a_file[m_index];

            }

            return new BDecodedString(byteString);
        }

        private static List<BDecodedObject> DecodeList(byte[] a_file)
        {
            List<BDecodedObject> decodedList = new List<BDecodedObject>();

            m_index++;
            m_currChar = (char)a_file[m_index];

            while (m_currChar != 'e' && m_index < a_file.Length)
            {

                switch (m_currChar)
                {
                    case 'd':
                        decodedList.Add(DecodeDictionary(a_file));
                        break;
                    case 'i':
                        decodedList.Add(DecodeInteger(a_file));
                        break;
                    case 'l':
                        decodedList.Add(new BDecodedList(DecodeList(a_file)));
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
                        decodedList.Add(DecodeString(a_file));
                        break;
                }
                m_index++;
                m_currChar = (char)a_file[m_index];

            }
            return decodedList;
        }

        private static BDecodedDictionary DecodeDictionary(byte[] a_file)
        {
            Dictionary<string, BDecodedObject> decodedDictionary = new Dictionary<string, BDecodedObject>();

            m_index++;
            m_currChar = (char)a_file[m_index];

            while (m_currChar != 'e' && m_index < a_file.Length)
            {
                // They key must be a string so this character must be a number.
                if (!Char.IsDigit(m_currChar))
                {
                    throw new FormatException("The key must be a string so this character must be a number");
                }

                // Generate the key that must be a UTF-8 encoded string.
                string key = DecodeString(a_file).GetUTF8();

                // Find pair value.
                m_index++;
                m_currChar = (char)a_file[m_index];

                switch (m_currChar)
                {
                    case 'd':
                        decodedDictionary.Add(key, DecodeDictionary(a_file));
                        break;
                    case 'i':
                        decodedDictionary.Add(key, DecodeInteger(a_file));
                        break;
                    case 'l':
                        decodedDictionary.Add(key, new BDecodedList(DecodeList(a_file)));
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
                        decodedDictionary.Add(key, DecodeString(a_file));
                        break;

                }
                m_index++;
                m_currChar = (char)a_file[m_index];
            }
            return new BDecodedDictionary(decodedDictionary);
        }

    }
}