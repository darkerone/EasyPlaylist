using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TagLib;
using TagLib.Id3v2;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Prism.Events;
using EasyPlaylist.Enums;

namespace EasyPlaylist.ViewModels
{
    class FileViewModel : MenuItemViewModel
    {
        private string _fileTagId;
        /// <summary>
        /// Identifiant du fichier (md5 du fichier)
        /// </summary>
        public string FileTagID
        {
            get { return _fileTagId; }
            set
            {
                _fileTagId = value;
                RaisePropertyChanged("FileTagID");
            }
        }

        private ExistsInPlaylistStatusEnum _existsInPlaylistStatus;
        /// <summary>
        /// True si le FileTagID du fichier est aussi dans la playlist (booleen utilisé dans l'explorer)
        /// </summary>
        public ExistsInPlaylistStatusEnum ExistsInPlaylistStatus
        {
            get { return _existsInPlaylistStatus; }
            set
            {
                _existsInPlaylistStatus = value;
                RaisePropertyChanged("ExistsInPlaylistStatus");
            }
        }

        [JsonConstructor]
        public FileViewModel(IEventAggregator eventAggregator, string path, string title) : base(eventAggregator, path, title)
        {
            ExistsInPlaylistStatus = ExistsInPlaylistStatusEnum.Default;

            TagLib.File fileInfos = TagLib.File.Create(path);

            TagLib.Id3v2.Tag tag = (TagLib.Id3v2.Tag)fileInfos.GetTag(TagTypes.Id3v2); // You can add a true parameter to the GetTag function if the file doesn't already have a tag.

            // Lecture du tag EasyPlaylistID
            PrivateFrame readPrivateFrame = PrivateFrame.Get(tag, "EasyPlaylistID", false); // 3ieme paramètre à false pour lire
            

            // Si un tag est déjà présent
            if(readPrivateFrame != null && readPrivateFrame.PrivateData.Data != null)
            {
                FileTagID = Encoding.Unicode.GetString(readPrivateFrame.PrivateData.Data);
            }
            else
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = System.IO.File.OpenRead(path))
                    {
                        FileTagID = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                    }
                }

                // Ecriture du tag EasyPlaylistID
                PrivateFrame writePrivateFrame = PrivateFrame.Get(tag, "EasyPlaylistID", true); // 3ieme paramètre à true pour écrire
                writePrivateFrame.PrivateData = System.Text.Encoding.Unicode.GetBytes(FileTagID);
                fileInfos.Save(); // Enregistre le tag dans le fichier
            }
            
        }

        public override MenuItemViewModel GetItemCopy()
        {
            return new FileViewModel(EventAggregator, Path, null);
        }

        /// <summary>
        /// Copie le fichier dans le répertoire passé en paramètre en utilisant son titre personnalisé
        /// </summary>
        /// <param name="destinationFolder"></param>
        /// <returns></returns>
        public bool WriteFile(string destinationFolder)
        {
            if (!System.IO.File.Exists(Path))
            {
                return false;
            }

            // Copie le fichier dans le répertoire passé en paramètre avec un titre personnalisé et l'extension du fichier d'origine
            System.IO.File.Copy(Path, destinationFolder + "\\" + Title + System.IO.Path.GetExtension(Path));

            return true;
        }
    }
}
