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
                RaisePropertyChanged("Playlist");
                CheckIfItemsExistInSelectedPlaylist(Explorer, SelectedPlaylist);
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

        public MainViewModel()
        {
            EventAggregator = new EventAggregator();
            _playlists = new ObservableCollection<HierarchicalTreeViewModel>();

            // =========
            // Playlists
            // =========

            // Récupère les playlists sauvegardées
            if (System.IO.File.Exists(@"Playlists.txt"))
            {
                string json = System.IO.File.ReadAllText(@"Playlists.txt");
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                ObservableCollection<HierarchicalTreeViewModel> deserializedPlaylist = JsonConvert.DeserializeObject<ObservableCollection<HierarchicalTreeViewModel>>(json, jsonSerializerSettings);

                if (deserializedPlaylist != null)
                {
                    foreach (HierarchicalTreeViewModel deserializedHierarchicalTreeVM in deserializedPlaylist)
                    {
                        // Copie toutes les propriétés utiles de la playlist déserialisée
                        HierarchicalTreeViewModel hierarchicalTreeViewModel = new HierarchicalTreeViewModel(EventAggregator);
                        hierarchicalTreeViewModel.AddMenuItems(deserializedHierarchicalTreeVM.RootFolder.Items.ToList());
                        hierarchicalTreeViewModel.Name = deserializedHierarchicalTreeVM.Name;
                        hierarchicalTreeViewModel.CopyItemInEnabled = true;
                        hierarchicalTreeViewModel.CopyItemOutEnabled = false;
                        hierarchicalTreeViewModel.MoveItemEnabled = true;
                        hierarchicalTreeViewModel.IsEditable = true;
                        AddPlaylist(hierarchicalTreeViewModel);
                    }
                }
            }

            if (Playlists.Any())
            {
                SelectedPlaylist = Playlists.First();
            }
            
            // ========
            // Explorer
            // ========
            Explorer = new HierarchicalTreeViewModel(EventAggregator);
            string defaultMyMusicFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            // Récupère le dossier et ses sous dossiers et fichiers
            FolderViewModel musicFolder = GetFolderViewModel(defaultMyMusicFolderPath, "Musiques");
            Explorer.AddMenuItems(musicFolder.Items.ToList());
            Explorer.CopyItemInEnabled = false;
            Explorer.CopyItemOutEnabled = true;
            Explorer.MoveItemEnabled = false;
            Explorer.IsEditable = false;

            CheckIfItemsExistInSelectedPlaylist(Explorer, SelectedPlaylist);
            EventAggregator.GetEvent<MenuItemCollectionChangedEvent>().Subscribe((e) => {
                CheckIfItemsExistInSelectedPlaylist(Explorer, SelectedPlaylist);
            });
        }

        public ICommand Browse
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    FolderBrowserDialog fbd = new FolderBrowserDialog();

                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        Explorer.RootFolder.RemoveAllItems();
                        Explorer.RootFolder.AddItem(GetFolderViewModel(fbd.SelectedPath, Path.GetFileName(fbd.SelectedPath)));
                    }
                });
            }
        }

        public ICommand SavePlaylist
        {
            get
            {
                return new DelegateCommand(() =>
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
                        System.Windows.MessageBox.Show("Playlists saved successfully", "Save playlists", MessageBoxButton.OK, MessageBoxImage.None);
                    }
                    catch
                    {
                        System.Windows.MessageBox.Show("An error occured while saving the playlists", "Save playlists", MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                });
            }
        }

        public ICommand AddNewPlaylist
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    DefineNamePopupView newDefineNamePopupView = new DefineNamePopupView();
                    DefineNamePopupViewModel newDefineNamePopupViewModel = new DefineNamePopupViewModel();
                    newDefineNamePopupViewModel.ItemName = "New playlist";
                    newDefineNamePopupView.DataContext = newDefineNamePopupViewModel;
                    RadWindow radWindow = new RadWindow();
                    radWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                    radWindow.Header = "New playlist";
                    radWindow.Content = newDefineNamePopupView;
                    radWindow.Closed += (object sender, WindowClosedEventArgs e) =>
                    {
                        RadWindow popup = sender as RadWindow;
                        DefineNamePopupView namePopupView = popup.Content as DefineNamePopupView;
                        DefineNamePopupViewModel namePopupViewModel = namePopupView.DataContext as DefineNamePopupViewModel;
                        if (e.DialogResult == true)
                        {
                            HierarchicalTreeViewModel newPlaylist = new HierarchicalTreeViewModel(EventAggregator);
                            newPlaylist.Name = namePopupViewModel.ItemName;
                            newPlaylist.CopyItemInEnabled = true;
                            newPlaylist.CopyItemOutEnabled = false;
                            newPlaylist.MoveItemEnabled = true;
                            newPlaylist.IsEditable = true;
                            AddPlaylist(newPlaylist);
                        }
                    };
                    radWindow.Show();
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
            if(explorerVM != null && playlistVM != null)
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
            string nameTmp = newPlaylist.Name;
            int index = 1;
            while (Playlists.Any(x => x.Name == nameTmp && x != newPlaylist))
            {
                nameTmp = newPlaylist.Name + $" ({index})";
                index++;
            }
            newPlaylist.Name = nameTmp;

            Playlists.Add(newPlaylist);
        }

        /// <summary>
        /// Vérifie et marque les fichiers du dossier passé en paramètre selon s'ils sont présent dans la liste des tag id passée en paramètre
        /// </summary>
        /// <param name="folderVM"></param>
        /// <param name="playlistFileTagIDs"></param>
        private void CheckIfItemsExistInPlaylistRecursively(FolderViewModel folderVM, List<string> playlistFileTagIDs)
        {
            foreach(FolderViewModel subFolderVM in folderVM.Items.OfType<FolderViewModel>())
            {
                CheckIfItemsExistInPlaylistRecursively(subFolderVM, playlistFileTagIDs);
            }

            foreach (FileViewModel fileVM in folderVM.Items.OfType<FileViewModel>())
            {
                if (playlistFileTagIDs.Any(x => x == fileVM.FileTagID))
                {
                    fileVM.ExistsInPlaylistStatus = ExistsInPlaylistStatusEnum.Exists;
                }
                else
                {
                    fileVM.ExistsInPlaylistStatus = ExistsInPlaylistStatusEnum.NotExists;
                }
            }
        }
    }
}
