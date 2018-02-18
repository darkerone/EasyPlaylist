using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlaylist.ViewModels
{
    class FolderViewModel : MenuItemViewModel
    {
        public FolderViewModel(string path) : base(path)
        {
            this.Items = new ObservableCollection<MenuItemViewModel>();
        }

        [JsonConstructor]
        public FolderViewModel(string path, string title) : base(path, title)
        {
            this.Items = new ObservableCollection<MenuItemViewModel>();
        }

        /// <summary>
        /// Sous dossiers/fichiers
        /// </summary>
        public ObservableCollection<MenuItemViewModel> Items { get; }

        public override MenuItemViewModel GetItemCopy()
        {
            FolderViewModel folderCopy = new FolderViewModel(Path);
            if (Items.Any())
            {                
                List<MenuItemViewModel> itemsToAdd = Items.Select(x => x.GetItemCopy()).ToList();
                folderCopy.Items.AddRange(itemsToAdd);
            }
            
            return folderCopy;
        }

        /// <summary>
        /// Créé le dossier au chemin passé en paramètre avec son titre personnalisé
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool WriteFolder(string path)
        {
            // Determine whether the container directory exists.
            if (!Directory.Exists(path))
            {
                return false;
            }

            // Try to create the directory.
            Directory.CreateDirectory(path + "\\" + Title);

            return true;
        }
    }
}
