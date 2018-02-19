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
                folderCopy.AddItems(itemsToAdd);
            }
            
            return folderCopy;
        }

        public void RemoveItem(MenuItemViewModel menuItemToRemoveVM)
        {
            RemoveItems(new List<MenuItemViewModel>() { menuItemToRemoveVM });
        }

        public void RemoveItems(IEnumerable<MenuItemViewModel> menuItemsToRemoveVM)
        {
            foreach (MenuItemViewModel menuItemToRemoveVM in menuItemsToRemoveVM)
            {
                Items.Remove(menuItemToRemoveVM);
            }
        }

        public void RemoveAllItems()
        {
            Items.Clear();
        }

        public void AddItem(MenuItemViewModel menuItemVMToAdd)
        {
            AddItems(new List<MenuItemViewModel>() { menuItemVMToAdd });
        }

        public void AddItems(IEnumerable<MenuItemViewModel> menuItemsVMToAdd)
        {
            Items.AddRange(menuItemsVMToAdd);
            foreach (MenuItemViewModel menuItemVM in menuItemsVMToAdd)
            {
                menuItemVM.ParentFolder = this;
            }
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

        /// <summary>
        /// Met à jour les dossiers parents des éléments du dossier
        /// </summary>
        public void UpdateAllSubParentFolders()
        {
            DefineAllSubParentFoldersIn(this);
        }

        /// <summary>
        /// Met à jour les dossiers parents des éléments du dossier passé en paramètre
        /// </summary>
        /// <param name="folderVM"></param>
        private void DefineAllSubParentFoldersIn(FolderViewModel folderVM)
        {
            // Définit les parents des éléments du dossier
            foreach (MenuItemViewModel menuItemVM in folderVM.Items)
            {
                menuItemVM.ParentFolder = folderVM;
            }

            // Définit les parents des éléments des sous dossiers du dossier
            foreach (FolderViewModel subFolder in folderVM.Items.OfType<FolderViewModel>())
            {
                DefineAllSubParentFoldersIn(subFolder);
            }
        }
    }
}
