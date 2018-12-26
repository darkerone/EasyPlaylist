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
using System.Globalization;
using EasyPlaylist.Models;

namespace EasyPlaylist.ViewModels
{
    class FileViewModel : MenuItemViewModel
    {
        #region Properties

        private string _fileTagId;
        /// <summary>
        /// Identifiant du fichier (md5 du fichier)
        /// </summary>
        public string FileTagId
        {
            get { return _fileTagId; }
            set
            {
                _fileTagId = value;
                RaisePropertyChanged("FileTagId");
            }
        }

        private DateTime _fileTagIdCreationDate;
        /// <summary>
        /// Date à laquelle l'identifiant du fichier a été créé
        /// </summary>
        public DateTime FileTagIdCreationDate
        {
            get { return _fileTagIdCreationDate; }
            set
            {
                _fileTagIdCreationDate = value;
                RaisePropertyChanged("FileTagIdCreationDate");
            }
        }

        public override bool IsFolder { get { return false; } }

        private bool _isFileExisting;
        /// <summary>
        /// Définit si le fichier existe sur le disque dur
        /// </summary>
        public bool IsFileExisting
        {
            get { return _isFileExisting; }
            set
            {
                _isFileExisting = value;
                RaisePropertyChanged("IsFileExisting");
            }
        }

        #endregion

        [JsonConstructor]
        public FileViewModel(IEventAggregator eventAggregator, string path, string title) : base(eventAggregator, path, title)
        {
            ExistsInPlaylistStatus = ExistsInPlaylistStatusEnum.Default;
            TagLib.File fileInfos = null;

            try
            {
                fileInfos = TagLib.File.Create(path);
                IsFileExisting = true;
            }
            catch
            {
                // Dans le cas où le fichier n'a pas été trouvé
                IsFileExisting = false;
                return;
            }

            TagLib.Id3v2.Tag tag = (TagLib.Id3v2.Tag)fileInfos.GetTag(TagTypes.Id3v2); // You can add a true parameter to the GetTag function if the file doesn't already have a tag.

            // Lecture du tag EasyPlaylistID
            PrivateFrame readPrivateFrame = PrivateFrame.Get(tag, "EasyPlaylistID", false); // 3ieme paramètre à false pour lire


            // Si un tag est déjà présent
            if (readPrivateFrame != null && readPrivateFrame.PrivateData.Data != null)
            {
                FileTagId = Encoding.Unicode.GetString(readPrivateFrame.PrivateData.Data);
                FileTagIdCreationDate = GetDateFromEasyPlaylistID(FileTagId);

                // Le fichier est récent si son id a été ajouté récement
                DateTime currentDate = DateTime.Now;
                DateTime recentDate = currentDate.AddYears(-EasyPlaylistStorage.EasyPlaylistSettings.AnteriorityYears)
                                                 .AddMonths(-EasyPlaylistStorage.EasyPlaylistSettings.AnteriorityMonths)
                                                 .AddDays(-EasyPlaylistStorage.EasyPlaylistSettings.AnteriorityDays)
                                                 .AddHours(-EasyPlaylistStorage.EasyPlaylistSettings.AnteriorityHours);
                IsRecent = DateTime.Compare(recentDate, FileTagIdCreationDate) <= 0;
            }
            else
            {
                FileTagId = GenerateNewEasyPlaylistID(path);
                FileTagIdCreationDate = GetDateFromEasyPlaylistID(FileTagId);

                // Ecriture du tag EasyPlaylistID
                PrivateFrame writePrivateFrame = PrivateFrame.Get(tag, "EasyPlaylistID", true); // 3ieme paramètre à true pour écrire
                writePrivateFrame.PrivateData = System.Text.Encoding.Unicode.GetBytes(FileTagId);
                fileInfos.Save(); // Enregistre le tag dans le fichier

                // On considère un fichier récent s'il n'avait pas d'Id auparavant
                IsRecent = true;
            }

        }

        #region Events

        #endregion

        #region Public methods

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

        #endregion

        #region Private methods

        /// <summary>
        /// Génère un Id pour le fichier (avec la date du jour)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GenerateNewEasyPlaylistID(string path)
        {
            string easyPlaylistID;

            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(path))
                {
                    DateTime currentDate = DateTime.Now;
                    string currentDateString = currentDate.ToString("yyyyMMddTHH:mm:ssZ");
                    easyPlaylistID = currentDateString + BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }

            return easyPlaylistID;
        }

        /// <summary>
        /// Récupère la date qui se trouve dans l'Id du fichier
        /// </summary>
        /// <param name="easyPlaylistID"></param>
        /// <returns></returns>
        private DateTime GetDateFromEasyPlaylistID(string easyPlaylistID)
        {
            DateTime date;

            if (!DateTime.TryParseExact(easyPlaylistID.Substring(0, 18),
                                    "yyyyMMddTHH:mm:ssZ",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None,
                                    out date))
            {
                date = new DateTime(1970, 1, 1);
            }

            return date;
        }

        #endregion
    }
}
