using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlaylist.Enums
{
    /// <summary>
    /// Définit si le fichier/dossier de l'explorer existe ou non dans la playlist sélectionnée
    /// </summary>
    enum ExistsInPlaylistStatusEnum
    {
        Default = 0,

        /// <summary>
        /// Fichier : Si le fichier existe dans la playlist
        /// Dossier : Si tous les fichiers du dossier existent dans la playlist
        /// </summary>
        Exists = 1,

        /// <summary>
        /// Fichier : Si le fichier n'existe pas dans la playlist
        /// Dossier : Si tous les fichiers du dossier n'existent pas dans la playlist
        /// </summary>
        NotExists = 2,

        /// <summary>
        /// Dossier : Si le dossier contient des fichiers qui existent et d'autres qui n'existent pas dans la playlist
        /// </summary>
        PartialExists = 3
    }
}
