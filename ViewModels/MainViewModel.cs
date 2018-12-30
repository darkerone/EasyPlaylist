using EasyPlaylist.Enums;
using EasyPlaylist.Events;
using EasyPlaylist.Models;
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
        const string PlaylistFilePath = @"Playlists.acep";

        #region Properties

        private ExplorerViewModel _explorer;
        public ExplorerViewModel Explorer
        {
            get { return _explorer; }
            set
            {
                _explorer = value;
                RaisePropertyChanged("Explorer");
            }
        }

        private ObservableCollection<PlaylistViewModel> _playlists;
        public ObservableCollection<PlaylistViewModel> Playlists
        {
            get { return _playlists; }
        }

        private PlaylistViewModel _selectedPlaylist;
        public PlaylistViewModel SelectedPlaylist
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

        private bool _isLoaderEnabled = false;
        public bool IsLoaderEnabled
        {
            get { return _isLoaderEnabled; }
            set
            {
                _isLoaderEnabled = value;
                RaisePropertyChanged("IsLoaderEnabled");
            }
        }

        /// <summary>
        /// Dictionnaire associant les sender de demande de désactivation de l'IHM au nombre de demande (du même sender)
        /// </summary>
        private Dictionary<object, int> _enableLoaderRequests;

        #endregion

        public MainViewModel()
        {
            EventAggregator = new EventAggregator();
            _playlists = new ObservableCollection<PlaylistViewModel>();
            _enableLoaderRequests = new Dictionary<object, int>();

            // =========
            // Playlists
            // =========
            RestorePlaylists();

            // ========
            // Explorer
            // ========
            InitExplorerFolders();

            // ======
            // Events
            // ======
            // Lorsque les items d'un dossier change
            EventAggregator.GetEvent<MenuItemCollectionChangedEvent>().Subscribe((e) =>
            {
                CheckIfItemsExistInSelectedPlaylist(Explorer, SelectedPlaylist);
            });
            // Lorsque la sélection d'un item change
            EventAggregator.GetEvent<SelectedItemsChangedEvent>().Subscribe((e) =>
            {
                SetCanAddSelectedItemToSelectedPlaylist();
            });
            // Lorsqu'une demande d'activation/désactivation de l'IHM est reçu
            EventAggregator.GetEvent<RequestEnableLoaderEvent>().Subscribe((e) =>
            {
                ManageLoader(e.Sender, e.EnableLoader, e.Force);
            });

            // Effectué après l'initialisation de l'explorer
            SelectedPlaylist = Playlists.FirstOrDefault();

            IsLoaderEnabled = false;
        }

        #region Events

        public ICommand Browse
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    fbd.SelectedPath = EasyPlaylistStorage.EasyPlaylistSettings.ExplorerPath;

                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        EasyPlaylistSettingsViewModel settings = EasyPlaylistStorage.EasyPlaylistSettings;
                        settings.ExplorerPath = fbd.SelectedPath;
                        EasyPlaylistStorage.Save(settings);
                        InitExplorerFolders();
                    }
                });
            }
        }

        public ICommand SaveAllPlaylist
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    if (SavePlaylists())
                    {
                        CustomMessageBox.Show("Playlists saved successfully", "Saved playlists", MessageBoxButton.OK, System.Windows.MessageBoxImage.None);
                        foreach (PlaylistViewModel playlist in Playlists)
                        {
                            playlist.HasBeenModified = false;
                        }
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
                        MessageBoxResult result = CustomMessageBox.Show($"Are you sure you want to delete playlist {SelectedPlaylist.Settings.Name} ?", "Remove playlist", MessageBoxButton.YesNo, MessageBoxImage.Warning);
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
                            PlaylistViewModel newPlaylist = new PlaylistViewModel(EventAggregator, namePopupViewModel.ItemName);
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
                            PlaylistViewModel newPlaylist = new PlaylistViewModel(EventAggregator, namePopupViewModel.ItemName);
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
                DoRefreshExplorer();
            });
        }

        public ICommand OpenSettings
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    EasyPlaylistSettingsView settingsPopupView = new EasyPlaylistSettingsView()
                    {
                        // Copie les paramètres (pour que le bouton "Cancel" puisse fonctionner)
                        DataContext = new EasyPlaylistSettingsViewModel(EasyPlaylistStorage.EasyPlaylistSettings)
                    };
                   
                    RadWindow radWindow = new RadWindow()
                    {
                        Header = "Settings",
                        WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                        Content = settingsPopupView
                    };

                    radWindow.Closed += (object sender, WindowClosedEventArgs e) =>
                    {
                        RadWindow popup = sender as RadWindow;
                        EasyPlaylistSettingsView settingsView = popup.Content as EasyPlaylistSettingsView;
                        EasyPlaylistSettingsViewModel settingsViewModel = settingsView.DataContext as EasyPlaylistSettingsViewModel;
                        if (e.DialogResult == true)
                        {
                            EasyPlaylistStorage.Save(settingsViewModel);
                            DoRefreshExplorer();
                        }
                    };

                    radWindow.Show();
                });
            }
        }

        /// <summary>
        /// Ajoute l'élément sélectionné dans l'explorer dans l'élément sélectionné de la playlist sélectionnée
        /// </summary>
        public ICommand AddSelectedItemsToSelectedPlaylist
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    AddItemsToSelectedPlaylist(Explorer.SelectedItems.ToList());
                });
            }
        }

        public ICommand RefreshExplorer
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    DoRefreshExplorer();
                });
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Récupère le modèle de vue du dossier ainsi que de tous ses sous dossiers et fichiers
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public FolderViewModel GetFolderViewModel(string directoryPath)
        {
            FolderViewModel folderViewModel = new FolderViewModel(EventAggregator, directoryPath, null);

            // Sous dossiers
            string[] subDirectoriesPaths = Directory.GetDirectories(directoryPath);
            if (subDirectoriesPaths.Any())
            {
                foreach (string subDirectoryPath in subDirectoriesPaths)
                {
                    folderViewModel.AddItem(GetFolderViewModel(subDirectoryPath));
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
        public void CheckIfItemsExistInSelectedPlaylist(ExplorerViewModel explorerVM, PlaylistViewModel playlistVM)
        {
            if (explorerVM != null)
            {
                if(playlistVM != null)
                {
                    List<string> playlistFileTagIDs = playlistVM.GetAllFileTagIDs();

                    CheckIfItemsExistInPlaylistRecursively(Explorer.RootFolder, playlistFileTagIDs);
                }
                else
                {
                    CheckIfItemsExistInPlaylistRecursively(Explorer.RootFolder, new List<string>());
                }
            }
        }

        /// <summary>
        /// Ajoute une playlist à la liste des playlists (ajoute un index au nom si le nom existe déjà)
        /// </summary>
        /// <param name="newPlaylist"></param>
        public void AddPlaylist(PlaylistViewModel newPlaylist)
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
        public void RemovePlaylist(PlaylistViewModel playlistToRemove)
        {
            Playlists.Remove(playlistToRemove);
        }

        /// <summary>
        /// Sauvegarde toutes les playlists
        /// </summary>
        /// <param name="onlyIfModified">Définit si la sauvegarde sera effectuée même si aucune playlist n'a été modifiée</param>
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
                System.IO.File.WriteAllText(PlaylistFilePath, jsonPlaylist);
                return true;
            }
            catch
            {
                CustomMessageBox.Show("An error occured while saving the playlists", "Save playlists", MessageBoxButton.OK, System.Windows.MessageBoxImage.Stop);
                return false;
            }
        }

        /// <summary>
        /// Ajoute l'item au dossier sélectionné de la playlist sélectionnée
        /// </summary>
        /// <param name="items"></param>
        public void AddItemsToSelectedPlaylist(List<MenuItemViewModel> items)
        {
            AddItemToPlaylist(items , SelectedPlaylist);
        }

        /// <summary>
        /// Ajoute l'item au dossier sélectionné de la playlist
        /// </summary>
        /// <param name="items"></param>
        /// <param name="playlist"></param>
        public void AddItemToPlaylist(List<MenuItemViewModel> items, PlaylistViewModel playlist)
        {
            if(playlist != null)
            {
                playlist.AddMenuItemsCopy(items);
            }
        }

        /// <summary>
        /// Recherche des items dans la playlist sélectionnée
        /// </summary>
        /// <param name="predicate"></param>
        public void SearchInSelectedPlaylist(Func<MenuItemViewModel, bool> predicate)
        {
            SelectedPlaylist.DoSearch(predicate);
        }

        /// <summary>
        /// Recherche des items dans l'explorer
        /// </summary>
        /// <param name="predicate"></param>
        public void SearchInExplorer(Func<MenuItemViewModel, bool> predicate)
        {
            Explorer.DoSearch(predicate);
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
            CanAddSelectedItemToSelectedPlaylist = Explorer.SelectedItems.Any() 
                                                    && SelectedPlaylist != null 
                                                    && SelectedPlaylist.SelectedItems.Count <= 1;
        }

        /// <summary>
        /// Restaure les playlists sauvegardées
        /// </summary>
        private void RestorePlaylists()
        {
            // Récupère les playlists sauvegardées
            if (System.IO.File.Exists(PlaylistFilePath))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(PlaylistFilePath);
                    var jsonSerializerSettings = new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    };
                    ObservableCollection<PlaylistViewModel> deserializedPlaylist = JsonConvert.DeserializeObject<ObservableCollection<PlaylistViewModel>>(json, jsonSerializerSettings);

                    string errors = "";
                    if (deserializedPlaylist != null)
                    {
                        // Pour chaque playlist
                        foreach (PlaylistViewModel deserializedHierarchicalTreeVM in deserializedPlaylist)
                        {
                            // Copie toutes les propriétés utiles de la playlist déserialisée
                            PlaylistViewModel playlistViewModel = new PlaylistViewModel(EventAggregator, deserializedHierarchicalTreeVM.Settings.Name);
                            playlistViewModel.AddMenuItemsCopy(deserializedHierarchicalTreeVM.RootFolder.Items.ToList());
                            playlistViewModel.Settings = deserializedHierarchicalTreeVM.Settings;
                            AddPlaylist(playlistViewModel);

                            if (playlistViewModel.CheckErrors())
                            {
                                errors += $"- Playlist \"{playlistViewModel.Settings.Name}\" contains errors :\n";
                                foreach (string error in playlistViewModel.Errors)
                                {
                                    errors += error + "\n";
                                }
                                errors += "\n";
                            }
                            playlistViewModel.HasBeenModified = false;
                        }
                    }

                    if (errors != "")
                    {
                        errors += "Do you want to show those files ?";
                        MessageBoxResult result = CustomMessageBox.Show(errors, "Errors", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        switch (result)
                        {
                            case MessageBoxResult.Yes:
                                foreach (PlaylistViewModel playlist in Playlists)
                                {
                                    playlist.DoSearch(x => x is FileViewModel && ((FileViewModel)x).IsFileExisting == false);
                                }
                                break;
                        }
                    }
                }
                catch
                {
                    MessageBoxResult result = CustomMessageBox.Show("An error occured when trying to restore playlits", "Errors", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Initialise l'explorer
        /// </summary>
        /// <param name="folderPath"></param>
        private void InitExplorerFolders()
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
                _watcher.Path = EasyPlaylistStorage.EasyPlaylistSettings.ExplorerPath;

                // Add event handlers.
                _watcher.Changed += new FileSystemEventHandler(OnExplorerFolderChanged);
                _watcher.Created += new FileSystemEventHandler(OnExplorerFolderChanged);
                _watcher.Deleted += new FileSystemEventHandler(OnExplorerFolderChanged);
                _watcher.Renamed += new RenamedEventHandler(OnExplorerFolderChanged);
            }

            _watcher.Path = EasyPlaylistStorage.EasyPlaylistSettings.ExplorerPath;

            DoRefreshExplorer();

            // Begin watching.
            EnableFileWatcher(true);
        }

        /// <summary>
        /// Raffraichi l'explorer
        /// </summary>
        private void DoRefreshExplorer()
        {
            // Stop watching.
            EnableFileWatcher(false);
            ManageLoader(this, true);

            // Mémorise l'état des dossiers de l'explorer pour le rétablir après la mise à jour
            List<FolderViewModel> oldFolders = null;
            if (Explorer != null)
            {
                oldFolders = Explorer.RootFolder.GetFolders(true);
            }

            // Récupère le dossier et ses sous dossiers et fichiers
            FolderViewModel musicFolder = GetFolderViewModel(EasyPlaylistStorage.EasyPlaylistSettings.ExplorerPath);
            Explorer = new ExplorerViewModel(EventAggregator, musicFolder.Title);
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

            // Begin watching.
            EnableFileWatcher(true);
            ManageLoader(this, false);
        }

        /// <summary>
        /// Active ou désactive le file watcher (attention, si l'utilisateur a désactivé l'option, il ne s'activera jamais)
        /// </summary>
        /// <param name="enable"></param>
        private void EnableFileWatcher(bool enable)
        {
            _watcher.EnableRaisingEvents = enable && EasyPlaylistStorage.EasyPlaylistSettings.IsFileWatcherOptionEnabled;
        }

        /// <summary>
        /// Gère l'activation et la désactivation du loader en fonction des demandes.
        /// Gère une liste de demande. Tant que des demandes d'activation existent, le loader s'affiche.
        /// </summary>
        /// <param name="sender">Celui qui demande l'activation du lolader</param>
        /// <param name="enableLoader">True pour activer, False pour désactiver</param>
        /// <param name="force">Supprime les demandes précédentes et ne tient compte que de celle ci</param>
        private void ManageLoader(object sender, bool enableLoader, bool force = false)
        {
            if (force)
            {
                IsLoaderEnabled = enableLoader;
                _enableLoaderRequests.Clear();
            }
            else
            {
                // Demande activation
                if (enableLoader)
                {
                    // Si une demande d'activation existe
                    if (_enableLoaderRequests.ContainsKey(sender))
                    {
                        // On incrémente son compteur
                        _enableLoaderRequests[sender]++;
                    }
                    else
                    {
                        // On créé une demande d'activation
                        _enableLoaderRequests.Add(sender, 1);
                    }
                }
                // Demande désactivation
                else
                {
                    // Si une demande d'activation existe
                    if (_enableLoaderRequests.ContainsKey(sender))
                    {
                        // Retire une demande d'activation
                        _enableLoaderRequests[sender]--;
                    }
                    // Si le compteur du sender est nul
                    if (_enableLoaderRequests[sender] == 0)
                    {
                        // On le retire
                        _enableLoaderRequests.Remove(sender);
                    }
                }
            }

            // S'il reste des demandes d'activation
            if (_enableLoaderRequests.Any())
            {
                // On affiche le loader
                IsLoaderEnabled = true;
            }
            else
            {
                // On masque le loader
                IsLoaderEnabled = false;
            }

            // Force à repeindre l’écran sinon cela est fait trop tard. N'est pas une très bonne pratique.
            System.Windows.Forms.Application.DoEvents();
        }

        #endregion
    }
}
