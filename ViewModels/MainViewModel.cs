using EasyPlaylist.Enums;
using EasyPlaylist.Events;
using EasyPlaylist.Views;
using Newtonsoft.Json;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace EasyPlaylist.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        #region Properties

        private string _currentFolderPath;
        public string CurrentFolderPath
        {
            get { return _currentFolderPath; }
            set
            {
                _currentFolderPath = value;
                RaisePropertyChanged("CurrentFolderPath");
            }
        }

        private HierarchicalTreeViewModel _explorer;
        public HierarchicalTreeViewModel Explorer
        {
            get { return _explorer; }
            set
            {
                _explorer = value;
                RaisePropertyChanged("Explorer");
            }
        }

        private ObservableCollection<HierarchicalTreeViewModel> _playlists;
        public ObservableCollection<HierarchicalTreeViewModel> Playlists
        {
            get { return _playlists; }
        }

        private HierarchicalTreeViewModel _selectedPlaylist;
        public HierarchicalTreeViewModel SelectedPlaylist
        {
            get { return _selectedPlaylist; }
            set
            {
                _selectedPlaylist = value;
                RaisePropertyChanged("SelectedPlaylist");
                CheckIfItemsExistInSelectedPlaylist(Explorer, SelectedPlaylist);
                SetCanAddSelectedItemToSelectedPlaylist();
            }
        }

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

        private bool _canAddSelectedItemToSelectedPlaylist = false;
        public bool CanAddSelectedItemToSelectedPlaylist
        {
            get { return _canAddSelectedItemToSelectedPlaylist; }
            set
            {
                _canAddSelectedItemToSelectedPlaylist = value;
                RaisePropertyChanged("CanAddSelectedItemToSelectedPlaylist");
            }
        }

        private FileSystemWatcher _watcher;

        private bool _isIhmEnabled = false;
        public bool IsIhmEnabled
        {
            get { return _isIhmEnabled; }
            set
            {
                _isIhmEnabled = value;
                RaisePropertyChanged("IsIhmEnabled");
            }
        }

        #endregion

        public MainViewModel()
        {
            EventAggregator = new EventAggregator();
            _playlists = new ObservableCollection<HierarchicalTreeViewModel>();

            // =========
            // Playlists
            // =========
            RestorePlaylists();

            // ========
            // Explorer
            // ========
            CurrentFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            InitExplorerFolders(CurrentFolderPath);

            // ======
            // Events
            // ======
            // Lorsque les items d'un dossier change
            EventAggregator.GetEvent<MenuItemCollectionChangedEvent>().Subscribe((e) =>
            {
                CheckIfItemsExistInSelectedPlaylist(Explorer, SelectedPlaylist);
            });
            // Lorsque la sélection d'un item change
            EventAggregator.GetEvent<SelectedItemChangedEvent>().Subscribe((e) =>
            {
                SetCanAddSelectedItemToSelectedPlaylist();
            });

            // Effectué après l'initialisation de l'explorer
            SelectedPlaylist = Playlists.FirstOrDefault();

            IsIhmEnabled = true;
        }

        #region Events

        public ICommand Browse
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    FolderBrowserDialog fbd = new FolderBrowserDialog();

                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        InitExplorerFolders(fbd.SelectedPath);
                        //Explorer.RootFolder.RemoveAllItems();
                        //Explorer.RootFolder.AddItem(GetFolderViewModel(fbd.SelectedPath, Path.GetFileName(fbd.SelectedPath)));
                    }
                });
            }
        }

        public ICommand SavePlaylist
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    if (SavePlaylists())
                    {
                        System.Windows.MessageBox.Show("Playlists saved successfully", "Saved playlists", MessageBoxButton.OK, MessageBoxImage.None);
                    }
                });
            }
        }

        public ICommand RemoveSelectedPlaylist
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    if (SavePlaylists())
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show($"Are you sure you want to delete playlist {SelectedPlaylist.Settings.Name} ?", "Remove playlist", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        switch (result)
                        {
                            case MessageBoxResult.Yes:
                                RemovePlaylist(SelectedPlaylist);
                                SelectedPlaylist = Playlists.FirstOrDefault();
                                break;
                        }
                    }
                });
            }
        }

        public ICommand AddNewPlaylist
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    DefineNamePopupView newDefineNamePopupView = new DefineNamePopupView();
                    DefineNamePopupViewModel newDefineNamePopupViewModel = new DefineNamePopupViewModel
                    {
                        ItemName = "New playlist"
                    };
                    newDefineNamePopupView.DataContext = newDefineNamePopupViewModel;
                    RadWindow radWindow = new RadWindow
                    {
                        WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                        Header = "New playlist",
                        Content = newDefineNamePopupView
                    };
                    radWindow.Closed += (object sender, WindowClosedEventArgs e) =>
                    {
                        RadWindow popup = sender as RadWindow;
                        DefineNamePopupView namePopupView = popup.Content as DefineNamePopupView;
                        DefineNamePopupViewModel namePopupViewModel = namePopupView.DataContext as DefineNamePopupViewModel;
                        if (e.DialogResult == true)
                        {
                            HierarchicalTreeViewModel newPlaylist = new HierarchicalTreeViewModel(EventAggregator, namePopupViewModel.ItemName)
                            {
                                CopyItemInEnabled = true,
                                CopyItemOutEnabled = false,
                                MoveItemEnabled = true,
                                IsEditable = true,
                                IsPlaylist = true
                            };
                            AddPlaylist(newPlaylist);
                            SelectedPlaylist = newPlaylist;
                        }
                    };
                    radWindow.Show();
                });
            }
        }

        public ICommand CopySelectedPlaylist
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    DefineNamePopupView newDefineNamePopupView = new DefineNamePopupView();
                    DefineNamePopupViewModel newDefineNamePopupViewModel = new DefineNamePopupViewModel
                    {
                        ItemName = "Copy of " + SelectedPlaylist.Settings.Name
                    };
                    newDefineNamePopupView.DataContext = newDefineNamePopupViewModel;
                    RadWindow radWindow = new RadWindow
                    {
                        WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                        Header = "Copy playlist",
                        Content = newDefineNamePopupView
                    };
                    radWindow.Closed += (object sender, WindowClosedEventArgs e) =>
                    {
                        RadWindow popup = sender as RadWindow;
                        DefineNamePopupView namePopupView = popup.Content as DefineNamePopupView;
                        DefineNamePopupViewModel namePopupViewModel = namePopupView.DataContext as DefineNamePopupViewModel;
                        if (e.DialogResult == true)
                        {
                            HierarchicalTreeViewModel newPlaylist = new HierarchicalTreeViewModel(EventAggregator, namePopupViewModel.ItemName)
                            {
                                CopyItemInEnabled = true,
                                CopyItemOutEnabled = false,
                                MoveItemEnabled = true,
                                IsEditable = true,
                                IsPlaylist = true
                            };
                            newPlaylist.Settings.ExportFlatPlaylist = SelectedPlaylist.Settings.ExportFlatPlaylist;
                            newPlaylist.AddMenuItemsCopy(SelectedPlaylist.RootFolder.Items.ToList());
                            AddPlaylist(newPlaylist);
                            SelectedPlaylist = newPlaylist;
                        }
                    };
                    radWindow.Show();
                });
            }
        }

        /// <summary>
        /// Lorsqu'un dossier windows (surveillé) change
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnExplorerFolderChanged(object source, FileSystemEventArgs e)
        {
            // Le thread du timer n'est pas le même que le thread de l'UI donc on demande à l'UI de faire le travail
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // TODO : effectuer les changement dans l'explorer en fonction de l'évennement
                IsIhmEnabled = false;
                // Force à repeindre l’écran sinon cela est fait trop tard. N'est pas une très bonne pratique.
                System.Windows.Forms.Application.DoEvents();
                RefreshExplorer();
                IsIhmEnabled = true;
            });
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Ajoute l'élément sélectionné dans l'explorer dans l'élément sélectionné de la playlist sélectionnée
        /// </summary>
        public DelegateCommand AddSelectedItemToSelectedPlaylist
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    FolderViewModel folder = null;
                    // S'il y a un élément sélectionné dans la playlist
                    if (SelectedPlaylist.SelectedItem != null)
                    {
                        // Si l'élément sélectionné dans la playlist est un dossier
                        if (SelectedPlaylist.SelectedItem.IsFolder)
                        {
                            folder = SelectedPlaylist.SelectedItem as FolderViewModel;
                        }
                        else
                        {
                            folder = SelectedPlaylist.SelectedItem.GetParentFolder();
                        }
                    }
                    else
                    {
                        folder = SelectedPlaylist.RootFolder;
                    }
                    folder.AddItemCopy(Explorer.SelectedItem);
                    folder.IsExpanded = true;
                });
            }
        }

        /// <summary>
        /// Retire l'élément sélectionné dans la playlist sélectionnée de la playlist sélectionnée
        /// </summary>
        public ICommand RemoveSelectedItemFromSelectedPlaylist
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    SelectedPlaylist.SelectedItem.GetParentFolder().RemoveItem(SelectedPlaylist.SelectedItem);
                });
            }
        }

        /// <summary>
        /// Récupère le modèle de vue du dossier ainsi que de tous ses sous dossiers et fichiers
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="directoryName"></param>
        /// <returns></returns>
        public FolderViewModel GetFolderViewModel(string directoryPath, string directoryName)
        {
            FolderViewModel folderViewModel = new FolderViewModel(EventAggregator, directoryPath, null);

            // Sous dossiers
            string[] subDirectoriesPaths = Directory.GetDirectories(directoryPath);
            if (subDirectoriesPaths.Any())
            {
                foreach (string subDirectoryPath in subDirectoriesPaths)
                {
                    folderViewModel.AddItem(GetFolderViewModel(subDirectoryPath, Path.GetFileName(subDirectoryPath)));
                }
            }

            // Fichiers
            string[] filesPaths = Directory.GetFiles(directoryPath);
            if (filesPaths.Any())
            {
                foreach (string filePath in filesPaths)
                {
                    if (Path.GetExtension(filePath) == ".mp3")
                    {
                        folderViewModel.AddItem(new FileViewModel(EventAggregator, filePath, null));
                    }
                }
            }

            return folderViewModel;
        }

        /// <summary>
        /// Vérifie et marque les fichiers de l'explorer passé en paramètre selon que le tag id est présent dans la playlist passée en paramètre
        /// </summary>
        /// <param name="folderVM"></param>
        /// <param name="playlistFileTagIDs"></param>
        public void CheckIfItemsExistInSelectedPlaylist(HierarchicalTreeViewModel explorerVM, HierarchicalTreeViewModel playlistVM)
        {
            if (explorerVM != null && playlistVM != null)
            {
                List<string> playlistFileTagIDs = playlistVM.GetAllFileTagIDs();

                CheckIfItemsExistInPlaylistRecursively(Explorer.RootFolder, playlistFileTagIDs);
            }
        }

        /// <summary>
        /// Ajoute une playlist à la liste des playlists (ajoute un index au nom si le nom existe déjà)
        /// </summary>
        /// <param name="newPlaylist"></param>
        public void AddPlaylist(HierarchicalTreeViewModel newPlaylist)
        {
            // Si le nom existe déjà, on incrémente un index entre parenthèse
            string nameTmp = newPlaylist.Settings.Name;
            int index = 1;
            while (Playlists.Any(x => x.Settings.Name == nameTmp && x != newPlaylist))
            {
                nameTmp = newPlaylist.Settings.Name + $" ({index})";
                index++;
            }
            newPlaylist.Settings.Name = nameTmp;

            Playlists.Add(newPlaylist);
        }

        /// <summary>
        /// Retire une playlist de la liste des playlists
        /// </summary>
        /// <param name="playlistToRemove"></param>
        public void RemovePlaylist(HierarchicalTreeViewModel playlistToRemove)
        {
            Playlists.Remove(playlistToRemove);
        }

        /// <summary>
        /// Sauvegarde toutes les playlists
        /// </summary>
        /// <returns></returns>
        public bool SavePlaylists()
        {

            try
            {
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                string jsonPlaylist = JsonConvert.SerializeObject(Playlists, jsonSerializerSettings);
                System.IO.File.WriteAllText(@"Playlists.txt", jsonPlaylist);
                return true;
            }
            catch
            {
                System.Windows.MessageBox.Show("An error occured while saving the playlists", "Save playlists", MessageBoxButton.OK, MessageBoxImage.Stop);
                return false;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Vérifie et marque les fichiers du dossier passé en paramètre selon 
        /// s'ils sont présent dans la liste des tag id passée en paramètre.
        /// Retourne : - Exists si le dossier ne contient que des fichiers et dossiers qui existent dans la playlist, 
        ///            - NotExists si le dossier ne contient que des fichiers et dossiers qui n'existent pas dans la playlist, 
        ///            - PartialExists sinon, 
        /// </summary>
        /// <param name="folderVM"></param>
        /// <param name="playlistFileTagIDs"></param>
        /// <returns>aaa</returns>
        private ExistsInPlaylistStatusEnum CheckIfItemsExistInPlaylistRecursively(FolderViewModel folderVM, List<string> playlistFileTagIDs)
        {
            ExistsInPlaylistStatusEnum existsInPlaylistStatusEnum = ExistsInPlaylistStatusEnum.PartialExists;

            // Pour chaque dossier du dossier
            List<FolderViewModel> folders = folderVM.Items.OfType<FolderViewModel>().ToList();
            foreach (FolderViewModel subFolderVM in folders)
            {
                subFolderVM.ExistsInPlaylistStatus = CheckIfItemsExistInPlaylistRecursively(subFolderVM, playlistFileTagIDs);
            }

            // Pour chaque fichier du dossier
            List<FileViewModel> files = folderVM.Items.OfType<FileViewModel>().ToList();
            foreach (FileViewModel fileVM in files)
            {
                // Si la playlist contient déjà le fichier
                if (playlistFileTagIDs.Any(x => x == fileVM.FileTagId))
                {
                    fileVM.ExistsInPlaylistStatus = ExistsInPlaylistStatusEnum.Exists;
                }
                else
                {
                    fileVM.ExistsInPlaylistStatus = ExistsInPlaylistStatusEnum.NotExists;
                }
            }

            // Si aucun fichier/dossier n'existe complètement
            if (!folderVM.Items.Any(x => x.ExistsInPlaylistStatus == ExistsInPlaylistStatusEnum.Exists))
            {
                // Si certains existent 
                if (folderVM.Items.Any(x => x.ExistsInPlaylistStatus == ExistsInPlaylistStatusEnum.PartialExists))
                {
                    existsInPlaylistStatusEnum = ExistsInPlaylistStatusEnum.PartialExists;
                }
                else
                {
                    existsInPlaylistStatusEnum = ExistsInPlaylistStatusEnum.NotExists;
                }
            }
            // Si aucun fichiers/dossier n'existe pas (tous les fichiers/dossiers existent)
            else if (!folderVM.Items.Any(x => x.ExistsInPlaylistStatus == ExistsInPlaylistStatusEnum.NotExists))
            {
                existsInPlaylistStatusEnum = ExistsInPlaylistStatusEnum.Exists;
            }

            return existsInPlaylistStatusEnum;
        }

        /// <summary>
        /// Définit si l'on peut ajouter l'item sélectionné à la playlist sélectionnée
        /// </summary>
        private void SetCanAddSelectedItemToSelectedPlaylist()
        {
            CanAddSelectedItemToSelectedPlaylist = Explorer.SelectedItem != null && SelectedPlaylist != null;
        }

        /// <summary>
        /// Restaure les playlists sauvegardées
        /// </summary>
        private void RestorePlaylists()
        {
            // Récupère les playlists sauvegardées
            if (System.IO.File.Exists(@"Playlists.txt"))
            {
                string json = System.IO.File.ReadAllText(@"Playlists.txt");
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };
                ObservableCollection<HierarchicalTreeViewModel> deserializedPlaylist = JsonConvert.DeserializeObject<ObservableCollection<HierarchicalTreeViewModel>>(json, jsonSerializerSettings);

                string errors = "";
                if (deserializedPlaylist != null)
                {
                    // Pour chaque playlist
                    foreach (HierarchicalTreeViewModel deserializedHierarchicalTreeVM in deserializedPlaylist)
                    {
                        // Copie toutes les propriétés utiles de la playlist déserialisée
                        HierarchicalTreeViewModel hierarchicalTreeViewModel = new HierarchicalTreeViewModel(EventAggregator, deserializedHierarchicalTreeVM.Settings.Name);
                        hierarchicalTreeViewModel.AddMenuItemsCopy(deserializedHierarchicalTreeVM.RootFolder.Items.ToList());
                        hierarchicalTreeViewModel.CopyItemInEnabled = true;
                        hierarchicalTreeViewModel.CopyItemOutEnabled = false;
                        hierarchicalTreeViewModel.MoveItemEnabled = true;
                        hierarchicalTreeViewModel.IsEditable = true;
                        hierarchicalTreeViewModel.IsPlaylist = true;
                        hierarchicalTreeViewModel.Settings = deserializedHierarchicalTreeVM.Settings;
                        AddPlaylist(hierarchicalTreeViewModel);

                        if (hierarchicalTreeViewModel.CheckErrors())
                        {
                            errors += $"- Playlist \"{hierarchicalTreeViewModel.Settings.Name}\" contains errors :\n";
                            foreach (string error in hierarchicalTreeViewModel.Errors)
                            {
                                errors += error + "\n";
                            }
                            errors += "\n";
                        }
                    }
                }

                if (errors != "")
                {
                    errors += "Do you want to show those files ?";
                    MessageBoxResult result = System.Windows.MessageBox.Show(errors, "Errors", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            foreach (HierarchicalTreeViewModel playlist in Playlists)
                            {
                                playlist.DoSearch(x => x is FileViewModel && ((FileViewModel)x).IsFileExisting == false);
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Initialise l'explorer
        /// </summary>
        /// <param name="folderPath"></param>
        private void InitExplorerFolders(string folderPath)
        {
            if (_watcher == null)
            {
                _watcher = new FileSystemWatcher();

                /* Watch for changes in LastAccess and LastWrite times, and the renaming of files or directories. */
                _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Only watch mp3 files.
                _watcher.Filter = "*.mp3";
                _watcher.IncludeSubdirectories = true;
                // Path to watch
                _watcher.Path = folderPath;

                // Add event handlers.
                _watcher.Changed += new FileSystemEventHandler(OnExplorerFolderChanged);
                _watcher.Created += new FileSystemEventHandler(OnExplorerFolderChanged);
                _watcher.Deleted += new FileSystemEventHandler(OnExplorerFolderChanged);
                _watcher.Renamed += new RenamedEventHandler(OnExplorerFolderChanged);

                // Begin watching.
                _watcher.EnableRaisingEvents = true;
            }

            _watcher.Path = folderPath;

            RefreshExplorer();
        }

        /// <summary>
        /// Raffraichi l'explorer
        /// </summary>
        private void RefreshExplorer()
        {
            // Mémorise l'état des dossiers de l'explorer pour le rétablir après la mise à jour
            List<FolderViewModel> oldFolders = null;
            if (Explorer != null)
            {
                oldFolders = Explorer.RootFolder.GetFolders(true);
            }

            // Récupère le dossier et ses sous dossiers et fichiers
            FolderViewModel musicFolder = GetFolderViewModel(CurrentFolderPath, "Musiques");
            Explorer = new HierarchicalTreeViewModel(EventAggregator, musicFolder.Title)
            {
                CopyItemInEnabled = false,
                CopyItemOutEnabled = true,
                MoveItemEnabled = false,
                IsEditable = false,
                IsPlaylist = false
            };
            Explorer.AddMenuItems(musicFolder.Items.ToList());

            CheckIfItemsExistInSelectedPlaylist(Explorer, SelectedPlaylist);

            if (oldFolders != null)
            {
                // Etend les dossiers qui l'étaient
                List<FolderViewModel> newFolders = Explorer.RootFolder.GetFolders(true);
                foreach (FolderViewModel folder in newFolders)
                {
                    folder.IsExpanded = oldFolders.Any(x => x.Path == folder.Path && x.IsExpanded);
                }
            }
        }

        #endregion
    }
}
