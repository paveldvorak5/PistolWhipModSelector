using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PistolWhipModSelector.AddNewSongs
{
    class AddNewSongs
    {
        private List<NewSongProperties> SongProperties;
        public bool changed = false;


        public AddNewSongs(string[] fileList)
        {
            if (this.CheckIfFilesAreSupported(fileList))
            {
                this.OpenAddNewSongsForm(fileList);
            }
        }

        private bool CheckIfFilesAreSupported(string[] fileList)
        {
            foreach (string file in fileList)
            {
                if (string.IsNullOrEmpty(file))
                    continue; // skip null/empty entries per chosen behavior
                string ext = Path.GetExtension(file).ToLower();
                if (ext != ".wem" && ext != ".mp3")
                    return false;
            }
            return true;
        }

        private void OpenAddNewSongsForm(string[] fileList)
        {
            if (fileList != null)
            {
                using (var form = new AddNewSongsForm(fileList))
                {
                    var result = form.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        this.SongProperties = form.newSongs;
                        this.DealWithFile();
                    }

                    if (result == DialogResult.Cancel)
                    {
                    }
                }
            }
        }

        private void DealWithFile()
        {
            foreach(NewSongProperties songProperties in this.SongProperties)
            {
                string targetPath = GetTargetPath(songProperties);

                if (songProperties.MoveInstedOfCopy)
                    this.MoveFileToTarget(songProperties.SongDestinationPath, targetPath);
                else
                    this.CopyFileToTarget(songProperties.SongDestinationPath, targetPath);
            }
            this.changed = true;
        }

        private string GetTargetPath(NewSongProperties songProperties)
        {
            string targetFolderPath = GlobalVariables.GetCustomSongFolderPath(GlobalVariables.CurrentID);
            string targetFileName = GlobalVariables.CurrentID + "-" + songProperties.SongTitle + "-" + songProperties.SongAuthor;

            string sourceExt = Path.GetExtension(songProperties.SongDestinationPath);
            if (String.IsNullOrWhiteSpace(sourceExt))
                sourceExt = ".wem";

            return targetFolderPath + "\\" + targetFileName + sourceExt;
        }

        private void CopyFileToTarget(string destinationPath, string targetPath)
        {
            File.Copy(destinationPath, targetPath, true);
        }
        private void MoveFileToTarget(string destinationPath, string targetPath)
        {
            File.Move(destinationPath, targetPath);
        }
    }
}
