using System;
using Microsoft.Win32;
namespace BitTorrent_Client.ViewModels
{
    public class OpenFileDialogViewModel
    {
        public OpenFileDialogViewModel()
        {

        }

        public OpenFileDialog OpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.DefaultExt = ".torrent";
            openFileDialog.Filter = "Torrent file (.torrent)|*.torrent";

            Nullable<bool> result = openFileDialog.ShowDialog();

            if (result == true)
            {
                return openFileDialog;
            }
            return null;
        }
    }
}