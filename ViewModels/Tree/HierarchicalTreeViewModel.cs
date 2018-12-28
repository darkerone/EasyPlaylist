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
    class HierarchicalTreeViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// Liste contenant un seul dossier (le dossier racine). C'est cette liste qui est passée au ItemsSource
        /// du RadTreeView dans la vue. De cette manière, on peut afficher le dossier racine.
        /// </summary>
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

        public ObservableCollection<MenuItemViewModel> SelectedItems { get; }

        private IEventAggregator _eventAggregator;
        public IEventAggregator EventAggregator
        {
            get { return _eventAggregator; }
            set
            {
                _eventAggregator = value;
                RaisePropertyChanged("EventAggregator");
            }
        }

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

        private List<string> _errors = new List<string>();
        public List<string> Errors
        {
            get { return _errors; }
        }

        public bool MoveItemEnabled { get; set; }
        public bool CopyItemInEnabled { get; set; }
        public bool CopyItemOutEnabled { get; set; }
        public bool IsEditable { get; set; }
        public bool IsPlaylist { get; set; }
        public bool HasBeenModified { get; set; }

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
        public ICommand RemoveSelectedItems
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    if (SelectedItems.Any())
                    {
                        List<MenuItemViewModel> selectedItemsTmp = SelectedItems.ToList();
                        foreach (MenuItemViewModel item in selectedItemsTmp)
                        {
                            item.RemoveFromParent();
                        }
                    }
                });
            }
        }

        [JsonIgnore]
        public ICommand RenameSelectedItem
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    if (SelectedItems.Count == 1)
                    {
                        SelectedItems.First().Rename();
                    }
                });
            }
        }

        [JsonIgnore]
        public ICommand OpenAddFolderPopup
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    DefineNamePopupView newDefineNamePopupView = new DefineNamePopupView();
                    DefineNamePopupViewModel newDefineNamePopupViewModel = new DefineNamePopupViewModel();
                    newDefineNamePopupViewModel.ItemName = "New folder";
                    newDefineNamePopupView.DataContext = newDefineNamePopupViewModel;
                    RadWindow radWindow = new RadWindow();
                    radWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                    radWindow.Header = "New folder";
                    radWindow.Content = newDefineNamePopupView;
                    radWindow.Closed += (object sender, WindowClosedEventArgs e) =>
                    {
                        RadWindow popup = sender as RadWindow;
                        DefineNamePopupView defineNamePopupView = popup.Content as DefineNamePopupView;
                        DefineNamePopupViewModel definePopupViewModel = defineNamePopupView.DataContext as DefineNamePopupViewModel;
                        if (e.DialogResult == true)
                        {
                            FolderViewModel newFolder = new FolderViewModel(EventAggregator, definePopupViewModel.ItemName, null);
                            AddMenuItem(newFolder);
                            SelectedItems.Clear();
                            SelectedItems.Add(newFolder);
                        }
                    };
                    radWindow.Show();
                }, (parameter) => { return true; });
            }
        }

        [JsonIgnore]
        public ICommand Export
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    try
                    {
                        FolderBrowserDialog fbd = new FolderBrowserDialog();
                        fbd.Description = "Destination folder (a folder with playlist name will be created)";

                        // Demande à l'utilisateur de choisir le dossier de destination
                        DialogResult result = fbd.ShowDialog();
                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                        {
                            // Si la playlist existe déjà dans le dossier sélectionné
                            if (Directory.Exists(fbd.SelectedPath + "\\" + RootFolder.Title))
                            {
                                MessageBoxResult alreadyExistsMessageBoxResult = CustomMessageBox.Show($"The folder \"{RootFolder.Title}\" already exists in \"{fbd.SelectedPath}\", do you want to replace it ?", "Already exists", MessageBoxButton.YesNo);
                                switch (alreadyExistsMessageBoxResult)
                                {
                                    case MessageBoxResult.Yes:
                                        Directory.Delete(fbd.SelectedPath + "\\" + RootFolder.Title, true);
                                        ExportFoldersAndFiles(fbd.SelectedPath, RootFolder, Settings.ExportFlatPlaylist);
                                        break;
                                }
                            }
                            else
                            {
                                ExportFoldersAndFiles(fbd.SelectedPath, RootFolder, Settings.ExportFlatPlaylist);
                            }
                            MessageBoxResult exportedPlaylistMessageBoxResult = CustomMessageBox.Show($"Playlist exported to \"{fbd.SelectedPath}\". Do you want to open it ?", "Playlist exported successfully", MessageBoxButton.YesNo);
                            switch (exportedPlaylistMessageBoxResult)
                            {
                                case MessageBoxResult.Yes:
                                    Process.Start(fbd.SelectedPath + "\\" + RootFolder.Title);
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show($"An exception occured while writting file :\n- {ex.Message}", "Exception", MessageBoxButton.OK);
                    }
                });
            }
        }

        [JsonIgnore]
        public ICommand OpenPlaylistSettings
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    HierarchicalTreeSettingsView playlistSettingsPopupView = new HierarchicalTreeSettingsView();
                    // Copie les paramètres (pour que le bouton "Cancel" puisse fonctionner)
                    playlistSettingsPopupView.DataContext = new HierarchicalTreeSettingsViewModel()
                    {
                        Name = Settings.Name,
                        ExportFlatPlaylist = Settings.ExportFlatPlaylist
                    };
                    RadWindow radWindow = new RadWindow();
                    radWindow.Header = "Playlist settings";
                    radWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                    radWindow.Content = playlistSettingsPopupView;

                    radWindow.Closed += (object sender, WindowClosedEventArgs e) =>
                    {
                        RadWindow popup = sender as RadWindow;
                        HierarchicalTreeSettingsView hierarchicalTreeSettingsView = popup.Content as HierarchicalTreeSettingsView;
                        HierarchicalTreeSettingsViewModel hierarchicalTreeSettingsViewModel = hierarchicalTreeSettingsView.DataContext as HierarchicalTreeSettingsViewModel;
                        if (e.DialogResult == true)
                        {
                            Settings.Name = hierarchicalTreeSettingsViewModel.Name;
                            RootFolder.Title = hierarchicalTreeSettingsViewModel.Name;
                            Settings.ExportFlatPlaylist = hierarchicalTreeSettingsViewModel.ExportFlatPlaylist;
                        }
                    };

                    radWindow.Show();
                });
            }
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
        public ICommand DisplayFilesInError
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    DoSearch(x => x is FileViewModel && ((FileViewModel)x).IsFileExisting == false);
                });
            }
        }
        
        [JsonIgnore]
        public ICommand DisplayRecentFiles
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    DoSearch(x => x.IsRecent);
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
                    FolderViewModel folderWhereAdd = GetFirstParentFolder(SelectedItems.First());
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
        /// Vérifie si la playlist contient des erreurs
        /// </summary>
        /// <returns></returns>
        public bool CheckErrors()
        {
            Errors.Clear();
            foreach (FileViewModel file in RootFolder.GetFiles(true))
            {
                // Vérifie si le fichier existe sur le disque dur
                if (!file.IsFileExisting)
                {
                    Errors.Add($"\"{file.Path}\" does not exists.");
                }
            }
            return Errors.Any();
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

        /// <summary>
        /// Exporte tout le contenu du dossier passé en paramètre dans le dossier de destination
        /// </summary>
        /// <param name="destinationFolder"></param>
        /// <param name="folderVM"></param>
        private void ExportFoldersAndFiles(string destinationFolder, FolderViewModel folderVM, bool flatedPlaylist = false)
        {
            // Créé le dossier
            bool success = folderVM.WriteFolder(destinationFolder);

            string folderPath = destinationFolder + "\\" + folderVM.Title;

            if (success)
            {
                // Dans le cas où l'on veut que tous les fichiers de la playlist soient exportés dans le dossier racine
                if (flatedPlaylist)
                {
                    // Récupère tous les fichiers
                    List<FileViewModel> filesToExport = folderVM.GetFiles(true);

                    // Ecris les fichiers
                    foreach (FileViewModel file in filesToExport)
                    {
                        file.WriteFile(folderPath);
                    }
                }
                else
                {
                    // Créé les sous dossiers du dossier
                    foreach (FolderViewModel subFolderVM in folderVM.Items.OfType<FolderViewModel>())
                    {
                        ExportFoldersAndFiles(folderPath, subFolderVM);
                    }

                    // Créé les fichiers du dossier
                    foreach (FileViewModel fileVM in folderVM.Items.OfType<FileViewModel>())
                    {
                        fileVM.WriteFile(folderPath);
                    }
                }
            }
        }

        /// <summary>
        /// Récupère de manière récursive tous les ID des fichiers du dossier passé en paramètre
        /// </summary>
        /// <param name="folderViewModel"></param>
        /// <returns></returns>
        private List<string> GetAllFileTagIDsRecursively(FolderViewModel folderViewModel)
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
        private void DoTextSearch()
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
        private void BringSearchResultToView(List<MenuItemViewModel> items = null)
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
