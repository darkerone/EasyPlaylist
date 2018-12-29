using EasyPlaylist.Events;
using EasyPlaylist.Views;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace EasyPlaylist.ViewModels
{
    abstract class HierarchicalTreeViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// Liste contenant un seul dossier (le dossier racine). C'est cette liste qui est passée au ItemsSource
        /// du RadTreeView dans la vue. De cette manière, on peut afficher le dossier racine.
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<MenuItemViewModel> RootFolders { get; }

        private RootFolderViewModel _rootFolder;
        public RootFolderViewModel RootFolder
        {
            get { return _rootFolder; }
            set
            {
                _rootFolder = value;
                RaisePropertyChanged("RootFolder");
            }
        }

        private string _searchText;
        /// <summary>
        /// Texte utilisé pour effectuer une recherche
        /// </summary>
        [JsonIgnore]
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                RaisePropertyChanged("SearchText");
                DoTextSearch();
            }
        }

        [JsonIgnore]
        public ObservableCollection<MenuItemViewModel> SelectedItems { get; }

        [JsonIgnore]
        public IEventAggregator EventAggregator { get; set; }

        private HierarchicalTreeSettingsViewModel _settings;
        public HierarchicalTreeSettingsViewModel Settings
        {
            get { return _settings; }
            set
            {
                _settings = value;
                RaisePropertyChanged("Settings");
            }
        }

        [JsonIgnore]
        public abstract bool IsPlaylist { get; }

        #endregion

        [JsonConstructor]
        public HierarchicalTreeViewModel(IEventAggregator eventAggregator, string name = "Unnamed")
        {
            EventAggregator = eventAggregator;
            Settings = new HierarchicalTreeSettingsViewModel()
            {
                Name = name,
                ExportFlatPlaylist = false
            };
            RootFolders = new ObservableCollection<MenuItemViewModel>();
            RootFolder = new RootFolderViewModel(eventAggregator, name, this);
            RootFolders.Add(RootFolder);
            SelectedItems = new ObservableCollection<MenuItemViewModel>();
            //SelectedItems.Add(RootFolder);
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }

        #region Events

        private void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            EventAggregator.GetEvent<SelectedItemsChangedEvent>().Publish(SelectedItems);
        }

        [JsonIgnore]
        public ICommand Search
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    DoTextSearch();
                });
            }
        }

        [JsonIgnore]
        public ICommand SearchDoubles
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    DoSearchDoubles();
                });
            }
        }

        [JsonIgnore]
        public ICommand CollapseAll
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    DoSearch(null);
                });
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Ajoute une copie de l'item à la playlist
        /// </summary>
        /// <param name="itemsToAdd"></param>
        public void AddMenuItemCopy(MenuItemViewModel itemsToAdd)
        {
            AddMenuItemsCopy(new List<MenuItemViewModel>() { itemsToAdd });
        }

        /// <summary>
        /// Ajoute une copie des items à la playlist
        /// </summary>
        /// <param name="itemsToAdd"></param>
        public void AddMenuItemsCopy(List<MenuItemViewModel> itemsToAdd)
        {
            List<MenuItemViewModel> itemsToAddCopies = itemsToAdd.Select(x => x.GetItemCopy()).ToList();
            AddMenuItems(itemsToAddCopies);
        }

        /// <summary>
        /// Ajoute un item à la playlist
        /// </summary>
        /// <param name="itemsToAdd"></param>
        public void AddMenuItem(MenuItemViewModel itemsToAdd)
        {
            AddMenuItems(new List<MenuItemViewModel>() { itemsToAdd });
        }

        /// <summary>
        /// Ajoute des items à la playlist
        /// </summary>
        /// <param name="itemsToAdd"></param>
        public void AddMenuItems(List<MenuItemViewModel> itemsToAdd)
        {
            if(SelectedItems.Count == 0)
            {
                RootFolder.AddItems(itemsToAdd);
            }
            else if (SelectedItems.Count == 1)
            {
                // Si l'item sélectionné est un dossier
                if (SelectedItems.First() is FolderViewModel)
                {
                    // On ajoute les éléments au dossier sélectionné
                    FolderViewModel selectedFolder = SelectedItems.First() as FolderViewModel;
                    selectedFolder.AddItems(itemsToAdd);
                }
                else
                {
                    // On ajoute les éléments au dossier parent de l'élément sélectionné
                    FolderViewModel folderWhereAdd = SelectedItems.First().ParentFolder;
                    folderWhereAdd.AddItems(itemsToAdd);
                }
            }
            else
            {
                // TODO : gérer le cas où plusieurs items sont sélectionnés
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

        /// <summary>
        /// Récupère tous les ID des fichiers du dossier root
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllFileTagIDs()
        {
            return GetAllFileTagIDsRecursively(RootFolder);
        }

        /// <summary>
        /// Effectue une recherche sur les items et les met en avant
        /// </summary>
        /// <param name="predicate">Prédicat pour la recherche. Si null, aucun item ne sera mis en valeur</param>
        public void DoSearch(Func<MenuItemViewModel, bool> predicate)
        {
            List<MenuItemViewModel> items = RootFolder.GetItems(true);
            // Rend tous les fichiers/dossiers non importants
            foreach (MenuItemViewModel item in items)
            {
                item.IsImportant = false;
            }

            if(predicate != null && items.Any(predicate))
            {
                // Pour tous les items vérifiant le prédicat, ils deviennent important
                foreach (MenuItemViewModel item in items.Where(predicate))
                {
                    item.IsImportant = true;
                }
                BringSearchResultToView();
            }
            else
            {
                // Marque tous les items comme trouvés
                foreach (MenuItemViewModel item in items)
                {
                    item.IsExpanded = false;
                    item.IsImportant = true;
                }
            }
        }

        /// <summary>
        /// Recherche les fichiers en double
        /// </summary>
        public void DoSearchDoubles()
        {
            List<MenuItemViewModel> items = RootFolder.GetItems(true);
            List<string> itemsPaths = new List<string>();
            List<string> itemsPathsInDoubles = new List<string>();

            foreach (MenuItemViewModel item in items)
            {
                // Si le chemin de l'item appartient à la liste des chemins
                if (itemsPaths.Contains(item.Path))
                {
                    // Il est considéré comme en double
                    itemsPathsInDoubles.Add(item.Path);
                }
                else
                {
                    itemsPaths.Add(item.Path);
                }
            }

            // Recherche les items dont le chemin est en double
            DoSearch(x => itemsPathsInDoubles.Contains(x.Path));
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Récupère de manière récursive tous les ID des fichiers du dossier passé en paramètre
        /// </summary>
        /// <param name="folderViewModel"></param>
        /// <returns></returns>
        protected List<string> GetAllFileTagIDsRecursively(FolderViewModel folderViewModel)
        {
            List<string> fileTagIDs = new List<string>();
            foreach (FolderViewModel subFolderVM in folderViewModel.Items.OfType<FolderViewModel>())
            {
                fileTagIDs.AddRange(GetAllFileTagIDsRecursively(subFolderVM));
            }

            foreach (FileViewModel fileVM in folderViewModel.Items.OfType<FileViewModel>())
            {
                fileTagIDs.Add(fileVM.FileTagId);
            }

            return fileTagIDs;
        }

        /// <summary>
        /// Effectue une recherche par nom sur l'ensemble de la playlist
        /// </summary>
        protected void DoTextSearch()
        {
            string searchTextString = SearchText == null ? "" : SearchText.ToLower();
            List<MenuItemViewModel> items = RootFolder.GetItems(true);
            // Si aucun recherche n'est effectuée
            if (searchTextString == "")
            {
                DoSearch(null);
            }
            else
            {
                DoSearch(x => x.Title.ToLower().Contains(searchTextString));
            }
        }

        /// <summary>
        /// Met en avant les éléments marqués comme résultat de la recherche
        /// </summary>
        /// <param name="items">Elément qui seront traités (tout l'arbre si null). 
        /// Pour éviter de reparcourir l'arbre si cela a déjà été fait</param>
        protected void BringSearchResultToView(List<MenuItemViewModel> items = null)
        {
            if (items == null)
            {
                items = RootFolder.GetItems(true);
            }

            // Réduit tous les items
            foreach (MenuItemViewModel item in items)
            {
                item.IsExpanded = false;
            }
            // Etend les dossiers jusqu'aux fichiers/dossier trouvés
            foreach (MenuItemViewModel item in items)
            {
                if (item.IsImportant)
                {
                    item.ExpandToHere();
                }
            }
            // Considère les dossiers contenant les fichiers trouvés comme des dossiers trouvés
            foreach (MenuItemViewModel item in items.OfType<FolderViewModel>())
            {
                if (item.IsExpanded)
                {
                    item.IsImportant = true;
                }
            }
        }

        #endregion
    }
}
