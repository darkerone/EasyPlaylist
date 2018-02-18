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

namespace EasyPlaylist.ViewModels
{
    class FileViewModel : MenuItemViewModel
    {
        /// <summary>
        /// Identifiant du fichier (md5 du fichier)
        /// </summary>
        public string FileTagID { get; set; }

        [JsonConstructor]
        public FileViewModel(string path) : base(path)
        {
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
            return new FileViewModel(Path);
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
