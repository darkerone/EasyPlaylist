using EasyPlaylist.ViewModels.Interfaces;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EasyPlaylist.ViewModels
{
    class ExplorerViewModel : BaseViewModel, IExplorer
    {
        private FolderViewModel _rootFolder;
        private MenuItemViewModel _selectedItem;

        #region Properties

        public FolderViewModel RootFolder
        {
            get { return _rootFolder; }
            set
            {
                _rootFolder = value;
                RaisePropertyChanged("RootFolder");
            }
        }

        public MenuItemViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
            }
        }

        #endregion

        public ExplorerViewModel()
        {
            RootFolder = new FolderViewModel(null, "Root");
        }

        #region Commands

        public ICommand RemoveSelectedItem
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (SelectedItem != null)
                    {
                        RemoveMenuItem(SelectedItem);
                    }
                });
            }
        }

        #endregion

        #region Public methods

        public void AddMenuItem(MenuItemViewModel itemsToAdd)
        {
            AddMenuItems(new List<MenuItemViewModel>() { itemsToAdd });
        }

        public void AddMenuItems(List<MenuItemViewModel> itemsToAdd)
        {
            // Si l'item sélectionné est un dossier
            if (SelectedItem is FolderViewModel)
            {
                // On ajoute les éléments au dossier sélectionné
                FolderViewModel selectedFolder = SelectedItem as FolderViewModel;
                selectedFolder.AddItems(itemsToAdd);
            }
            else
            {
                // On ajoute les éléments au dossier parent de l'élément sélectionné
                FolderViewModel folderWhereAdd = GetFirstParentFolder(SelectedItem);
                folderWhereAdd.AddItems(itemsToAdd);
            }
        }

        public void RemoveMenuItem(MenuItemViewModel itemToRemove)
        {
            RemoveMenuItems(new List<MenuItemViewModel>() { itemToRemove });
        }

        public void RemoveMenuItems(List<MenuItemViewModel> itemsToRemove)
        {
            foreach (MenuItemViewModel itemToRemove in itemsToRemove)
            {
                // On retire les éléments au dossier parent de l'élément sélectionné
                FolderViewModel folderWhereAdd = GetFirstParentFolder(itemToRemove);
                folderWhereAdd.RemoveItem(itemToRemove);
            }
        }

        /// <summary>
        /// Met à jour les références au dossier parents de tous les éléments du dossier root
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized]
        public void UpdateAllRootSubParentFolder(StreamingContext context)
        {
            RootFolder.UpdateAllSubParentFolders();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Renvoie le dossier contenant l'élément passé en paramètre
        /// </summary>
        /// <param name="menuItemVM"></param>
        /// <returns></returns>
        private FolderViewModel GetFirstParentFolder(MenuItemViewModel menuItemVM)
        {
            List<FolderViewModel> allContainerFolders = SearchAllFoldersContainingMenuItem(menuItemVM, RootFolder);

            if (allContainerFolders.Any())
            {
                return allContainerFolders.First();
            }

            return RootFolder;
        }

        /// <summary>
        /// Renvoie tous les dossiers contenant l'élément passé en paramètre
        /// </summary>
        /// <param name="menuItemVM">Eléméent à rechercher</param>
        /// <param name="folderVM">Dossier où l'on fait la recherche</param>
        /// <returns></returns>
        private List<FolderViewModel> SearchAllFoldersContainingMenuItem(MenuItemViewModel menuItemVM, FolderViewModel folderVM)
        {
            List<FolderViewModel> containerFolders = new List<FolderViewModel>();
            if (folderVM.Items.Contains(menuItemVM))
            {
                containerFolders.Add(folderVM);
            }

            foreach (FolderViewModel subFolder in folderVM.Items.OfType<FolderViewModel>())
            {
                containerFolders.AddRange(SearchAllFoldersContainingMenuItem(menuItemVM, subFolder));
            }

            return containerFolders;
        }

        

        #endregion
    }
}
