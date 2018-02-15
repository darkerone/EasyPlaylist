using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlaylist.ViewModels
{
    class MenuItemViewModel : BaseViewModel
    {
        /// <summary>
        /// Titre du dossier/fichier
        /// </summary>
        public string Title { get; set; }
    }
}
