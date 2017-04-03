﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using BitTorrent_Client.Models.PeerModels;
namespace BitTorrent_Client.Models.TorrentModels
{
    static class TorrentIO
    {
        #region Methods

        /// <summary>
        /// Opens the torrent at given path.
        /// </summary>
        /// <param name="a_path">The path of the torrent.</param>
        /// <returns>Returns a byte array with all the bytes from the file.</returns>
        /// <remarks>
        /// OpenTorrent()
        /// 
        /// SYNOPSIS
        /// 
        ///     static byte[] OpenTorrent(string a_path);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function open the torrent at given path. It will return
        ///     File.ReadAllBytes of the path, which is a byte array that contains
        ///     all of the files byte data.
        /// </remarks>
        public static byte[] OpenTorrent(string a_path)
        {
            return File.ReadAllBytes(a_path);
        }

        /// <summary>
        /// Reads in a piece from file.
        /// </summary>
        /// <param name="a_pieceIndex"> The index of the piece.</param>
        /// <param name="a_torrent">The torrent file that contains save directory,
        ///        the file name, and the piece length needed to read the piece.
        /// </param>
        /// <returns>Returns a byte array containing the piece at a_pieceIndex</returns>
        /// <remarks>
        /// ReadPiece()
        /// 
        /// SYNOPSIS
        /// 
        ///     byte[] ReadPiece(int a_pieceIndex, Torrent a_torrent);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will read in the piece at a_pieceIndex for the
        ///     given torrent and file. A FileStream is used to open the file
        ///     and it's position is made equal to the piece index * piece length.
        ///     The piece is read into the piece byte array and then is returned.
        /// </remarks>
        public static byte[] ReadPiece(int a_pieceIndex, Torrent a_torrent)
        {
            string file = a_torrent.SaveDirectory + "\\" + a_torrent.Name;
            byte[] piece = new byte[a_torrent.PieceLength];
            //int bytesRead;

            using (Stream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                fileStream.Position = a_pieceIndex * a_torrent.PieceLength;
                //bytesRead = 0;
                fileStream.Read(piece, 0, (int)a_torrent.PieceLength);
                //do
                //{
                //    bytesRead += fileStream.Read(piece, bytesRead,
                //        (int)a_torrent.PieceLength - bytesRead);
                //} while (bytesRead != a_torrent.PieceLength && 
                //fileStream.Position < fileStream.Length);
            }

            return piece;   
        }

        /// <summary>
        /// Writes a block of data to the file.
        /// </summary>
        /// <param name="a_block"> The recieved block structure that has the data.</param>
        /// <param name="a_torrent"> The torrent file that has the save path and 
        ///        the name of the file along with the length of it.
        /// </param>
        /// <remarks>
        /// WriteBlock()
        /// 
        /// SYNOPSIS
        /// 
        ///     void WriteBlock(IncomingBlock a_block, Torrent a_torrent);
        ///     
        /// DESCRIPTION
        /// 
        ///     This function will write the received block to the file. If the
        ///     file has not been created, then it will create one. 
        /// </remarks>
        public static void WriteBlock(IncomingBlock a_block, Torrent a_torrent)
        {
            var file = a_torrent.SaveDirectory + "\\" + a_torrent.Name;

            if (!File.Exists(file))
            {
                using (Stream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write))
                {
                    fileStream.SetLength(a_torrent.Length);
                }
            }

            using (Stream fileStream = new FileStream(file, FileMode.Open, FileAccess.Write))
            {
                fileStream.Position = a_block.Index * a_torrent.PieceLength + a_block.Begin;
                //fileStream.Seek(pieceIndex + a_block.Begin, SeekOrigin.Begin);
                fileStream.Write(a_block.Block, 0, a_block.Block.Length);
            }
        }

        #endregion
    }
}