using EasyPlaylist.Events;
using Newtonsoft.Json;
using Prism.Events;
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
        #region Properties 

        [JsonIgnore]
        public override bool IsFolder { get { return true; } }

        /// <summary>
        /// Sous dossiers/fichiers
        /// </summary>
        public ObservableCollection<MenuItemViewModel> Items { get; }

        #endregion

        [JsonConstructor]
        public FolderViewModel(IEventAggregator eventAggregator, string path, string title) : base(eventAggregator, path, title)
        {
            Items = new ObservableCollection<MenuItemViewModel>();
            Items.CollectionChanged += Items_CollectionChanged;
        }

        #region Events

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (EventAggregator != null)
            {
                EventAggregator.GetEvent<MenuItemCollectionChangedEvent>().Publish(this);
            }
        }

        #endregion

        #region Public methods

        public override MenuItemViewModel GetItemCopy()
        {
            FolderViewModel folderCopy = new FolderViewModel(EventAggregator, Path, null);
            if (Items.Any())
            {
                List<MenuItemViewModel> itemsToAdd = Items.Select(x => x.GetItemCopy()).ToList();
                folderCopy.AddItemsCopy(itemsToAdd);
            }

            return folderCopy;
        }

        /// <summary>
        /// Retire un item du dossier.
        /// Fait appel à RemoveItems pour éviter les multiples CollectionChanged
        /// </summary>
        /// <param name="menuItemToRemoveVM"></param>
        public void RemoveItem(MenuItemViewModel menuItemToRemoveVM)
        {
            RemoveItems(new List<MenuItemViewModel>() { menuItemToRemoveVM });
        }

        /// <summary>
        /// Retire des items du dossier
        /// </summary>
        /// <param name="menuItemsToRemoveVM"></param>
        public void RemoveItems(IEnumerable<MenuItemViewModel> menuItemsToRemoveVM)
        {
            foreach (MenuItemViewModel menuItemToRemoveVM in menuItemsToRemoveVM)
            {
                Items.Remove(menuItemToRemoveVM);
            }

            // Un dossier est récent si au moins un de ses item est récent
            IsRecent = Items.Any(x => x.IsRecent);

            MarkParentPlaylistAsChanged();
        }

        /// <summary>
        /// Retire tous les items du dossier
        /// </summary>
        public void RemoveAllItems()
        {
            Items.Clear();
            IsRecent = false;
            MarkParentPlaylistAsChanged();
        }

        /// <summary>
        /// Ajoute une copie de l'item au dossier.
        /// Fait appel à AddItemsCopy pour éviter les multiples CollectionChanged
        /// </summary>
        /// <param name="menuItemVMToAdd"></param>
        public void AddItemCopy(MenuItemViewModel menuItemVMToAdd)
        {
            AddItemsCopy(new List<MenuItemViewModel>() { menuItemVMToAdd });
        }

        /// <summary>
        /// Ajoute une copie des items au dossier
        /// </summary>
        /// <param name="menuItemsVMToAdd"></param>
        public void AddItemsCopy(IEnumerable<MenuItemViewModel> menuItemsVMToAdd)
        {
            // Copie les items
            List<MenuItemViewModel> copiedMenuItemsVM = menuItemsVMToAdd.Select(x => x.GetItemCopy()).ToList();
            AddItems(copiedMenuItemsVM);
        }

        /// <summary>
        /// Ajoute un item au dossier.
        /// Fait appel à AddItems pour éviter les multiples CollectionChanged
        /// </summary>
        /// <param name="menuItemVMToAdd"></param>
        public void AddItem(MenuItemViewModel menuItemVMToAdd)
        {
            AddItems(new List<MenuItemViewModel>() { menuItemVMToAdd });
        }

        /// <summary>
        /// Ajoute des items au dossier
        /// </summary>
        /// <param name="menuItemsVMToAdd"></param>
        public void AddItems(IEnumerable<MenuItemViewModel> menuItemsVMToAdd)
        {
            // Copie les items
            foreach (MenuItemViewModel menuItemVM in menuItemsVMToAdd)
            {
                // Redéfinit l'item parent
                menuItemVM.ParentFolder = this;

                // Si le nom existe déjà, on incrémente un index entre parenthèse
                string nameTmp = menuItemVM.Title;
                int index = 1;
                while (Items.Any(x => x.Title == nameTmp))
                {
                    nameTmp = menuItemVM.Title + $" ({index})";
                    index++;
                }
                menuItemVM.Title = nameTmp;
            }
            Items.AddRange(menuItemsVMToAdd);

            // Un dossier est récent si au moins un de ses item est récent
            IsRecent = Items.Any(x => x.IsRecent);
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
        /// Récupère la liste des fichiers du dossier
        /// </summary>
        /// <param name="recursive">True pour récupérer les fichiers de manière récursive</param>
        /// <returns></returns>
        public List<FileViewModel> GetFiles(bool recursive = false)
        {
            List<FileViewModel> files = Items.OfType<FileViewModel>().ToList();

            if (recursive)
            {
                // Pour tous les dossiers du dossier courant
                foreach (FolderViewModel folder in Items.OfType<FolderViewModel>())
                {
                    // On récupère les fichiers
                    files.AddRange(folder.GetFiles(recursive));
                }
            }

            return files;
        }

        /// <summary>
        /// Récupère la liste des items du dossier
        /// </summary>
        /// <param name="recursive">True pour récupérer les items de manière récursive</param>
        /// <returns></returns>
        public List<MenuItemViewModel> GetItems(bool recursive = false)
        {
            List<MenuItemViewModel> items = Items.ToList();

            if (recursive)
            {
                // Pour tous les dossiers du dossier courant
                foreach (FolderViewModel folder in Items.OfType<FolderViewModel>())
                {
                    // On récupère les fichiers
                    items.AddRange(folder.GetItems(recursive));
                }
            }

            return items;
        }

        /// <summary>
        /// Récupère la liste des dossiers du dossier
        /// </summary>
        /// <param name="recursive">True pour récupérer les dossiers de manière récursive</param>
        /// <returns></returns>
        public List<FolderViewModel> GetFolders(bool recursive = false)
        {
            List<FolderViewModel> folders = Items.OfType<FolderViewModel>().ToList();

            if (recursive)
            {
                // Pour tous les dossiers du dossier courant
                foreach (FolderViewModel folder in Items.OfType<FolderViewModel>())
                {
                    // On récupère les fichiers
                    folders.AddRange(folder.GetFolders(recursive));
                }
            }

            return folders;
        }

        #endregion

        #region Private methods

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

        #endregion
    }
}
