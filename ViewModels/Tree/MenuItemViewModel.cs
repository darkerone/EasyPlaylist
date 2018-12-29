using EasyPlaylist.Enums;
using EasyPlaylist.Views;
using Newtonsoft.Json;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace EasyPlaylist.ViewModels
{
    abstract class MenuItemViewModel : BaseViewModel
    {
        #region Properties

        private string _title;
        /// <summary>
        /// Titre du dossier/fichier
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged("Title");
            }
        }

        [JsonIgnore]
        public IEventAggregator EventAggregator { get; set; }

        /// <summary>
        /// Chemin du dossier/fichier
        /// </summary>
        public string Path { get; set; }

        private FolderViewModel _parentFolder;
        /// <summary>
        /// Dossier parent
        /// </summary>
        [JsonIgnore]
        public FolderViewModel ParentFolder
        {
            get { return _parentFolder; }
            set
            {
                _parentFolder = value;
                //RaisePropertyChanged("ParentFolder"); // Inutil de faire cette notification
                MarkParentPlaylistAsChanged();
            }
        }

        private bool _isExpanded = false;
        [JsonIgnore]
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                RaisePropertyChanged("IsExpanded");
            }
        }

        private bool _isImportant = true;
        /// <summary>
        /// Définit si l'item est important (pour savoir s'il doit être mis en évidence lors de la recherche par exemple)
        /// </summary>
        [JsonIgnore]
        public bool IsImportant
        {
            get { return _isImportant; }
            set
            {
                _isImportant = value;
                RaisePropertyChanged("IsImportant");
            }
        }

        [JsonIgnore]
        public abstract bool IsFolder { get; }

        private ExistsInPlaylistStatusEnum _existsInPlaylistStatus;
        /// <summary>
        /// True si le FileTagID du fichier est aussi dans la playlist (booleen utilisé dans l'explorer)
        /// </summary>
        [JsonIgnore]
        public ExistsInPlaylistStatusEnum ExistsInPlaylistStatus
        {
            get { return _existsInPlaylistStatus; }
            set
            {
                _existsInPlaylistStatus = value;
                RaisePropertyChanged("ExistsInPlaylistStatus");
            }
        }

        private bool _isRecent;
        /// <summary>
        /// Définit si l'item est considéré comme récent ou non.
        /// Un dossier est récent s'il contient au moins un fichier récent
        /// </summary>
        [JsonIgnore]
        public bool IsRecent
        {
            get { return _isRecent; }
            set
            {
                _isRecent = value;
                RaisePropertyChanged("IsRecent");
            }
        }

        #endregion

        public MenuItemViewModel(IEventAggregator eventAggregator, string path, string title)
        {
            Path = path;
            if(title != null && title != "")
            {
                Title = title;
            }
            else
            {
                Title = System.IO.Path.GetFileNameWithoutExtension(path);
            }
           
            EventAggregator = eventAggregator;
        }

        #region Events

        [JsonIgnore]
        public ICommand RenameItem
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    Rename();
                });
            }
        }

        /// <summary>
        /// Retire l'item de la playlist
        /// </summary>
        [JsonIgnore]
        public ICommand RemoveItemFromParent
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    RemoveFromParent();
                });
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Renvoie une copie du menu item
        /// </summary>
        /// <returns></returns>
        abstract public MenuItemViewModel GetItemCopy();

        /// <summary>
        /// Renvoie le dossier parent
        /// </summary>
        /// <returns></returns>
        public FolderViewModel GetParentFolder()
        {
            return ParentFolder;
        }

        /// <summary>
        /// Etend tous les dossiers jusqu'à cet item
        /// </summary>
        public void ExpandToHere()
        {
            if (ParentFolder != null)
            {
                ParentFolder.IsExpanded = true;
                ParentFolder.ExpandToHere();
            }
        }

        /// <summary>
        /// Ouvre la popup de définition du nom pour renommer l'élément
        /// </summary>
        public void Rename()
        {
            DefineNamePopupView newDefineNamePopupView = new DefineNamePopupView();
            DefineNamePopupViewModel newDefineNamePopupViewModel = new DefineNamePopupViewModel();
            newDefineNamePopupViewModel.ItemName = Title;
            newDefineNamePopupView.DataContext = newDefineNamePopupViewModel;
            RadWindow radWindow = new RadWindow();
            radWindow.Header = "Rename item";
            radWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            radWindow.Content = newDefineNamePopupView;
            radWindow.Closed += (object sender, WindowClosedEventArgs e) => {
                RadWindow popup = sender as RadWindow;
                DefineNamePopupView defineNamePopupView = popup.Content as DefineNamePopupView;
                DefineNamePopupViewModel defineNamePopupViewModel = defineNamePopupView.DataContext as DefineNamePopupViewModel;
                if (e.DialogResult == true)
                {
                    Title = defineNamePopupViewModel.ItemName;
                    MarkParentPlaylistAsChanged();
                }
            };
            radWindow.Show();
        }

        /// <summary>
        /// Supprime l'item de la liste des items de son parent
        /// </summary>
        public void RemoveFromParent()
        {
            FolderViewModel parentFolder = GetParentFolder();
            parentFolder.RemoveItem(this);
        }

        /// <summary>
        /// Recherche l'arbre parent
        /// </summary>
        /// <returns></returns>
        public HierarchicalTreeViewModel GetParentHierarchicalTree()
        {
            FolderViewModel folderTmp = ParentFolder;
            // Si l'item a un parent
            if (folderTmp != null)
            {
                // On recherche le premier ancètre n'ayant pas de parent
                while (folderTmp.ParentFolder != null)
                {
                    folderTmp = folderTmp.ParentFolder;
                }
            }
            else
            {
                // Si l'item est l'item racine
                RootFolderViewModel rootFolder = this as RootFolderViewModel;
                if (rootFolder != null)
                {
                    return rootFolder.ParentHierarchicalTree;
                }
            }

            // Si le premier ancetre n'ayant pas de parent est le dossier racine
            RootFolderViewModel rootFolderTmp = folderTmp as RootFolderViewModel;
            if(rootFolderTmp != null)
            {
                return rootFolderTmp.ParentHierarchicalTree;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Marque la playlist parente comme modifiée
        /// </summary>
        protected void MarkParentPlaylistAsChanged()
        {
            HierarchicalTreeViewModel hierarchicalTreeParent = GetParentHierarchicalTree();
            if(hierarchicalTreeParent != null && hierarchicalTreeParent.IsPlaylist)
            {
                ((PlaylistViewModel)hierarchicalTreeParent).HasBeenModified = true;
            }
        }

        #endregion
    }
}
