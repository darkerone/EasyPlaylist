using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlaylist.ViewModels
{
    abstract class MenuItemViewModel : BaseViewModel
    {
        /// <summary>
        /// Titre du dossier/fichier
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Chemin du dossier/fichier
        /// </summary>
        public string Path { get; set; }

        public MenuItemViewModel(string path)
        {
            Path = path;
            Title = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        public MenuItemViewModel(string path, string title)
        {
            Path = path;
            Title = title;
        }

        /// <summary>
        /// Renvoie une copie du menu item
        /// </summary>
        /// <returns></returns>
        abstract public MenuItemViewModel GetItemCopy();
    }
}
