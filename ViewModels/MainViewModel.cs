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
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace EasyPlaylist.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        private HierarchicalTreeViewModel _explorer;
        private HierarchicalTreeViewModel _playlist;
        private IEventAggregator _eventAggregator;

        public HierarchicalTreeViewModel Explorer
        {
            get { return _explorer; }
            set
            {
                _explorer = value;
                RaisePropertyChanged("Explorer");
            }
        }

        public HierarchicalTreeViewModel Playlist
        {
            get { return _playlist; }
            set
            {
                _playlist = value;
                RaisePropertyChanged("Playlist");
            }
        }

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

            // ========
            // Playlist
            // ========
            Playlist = new HierarchicalTreeViewModel(EventAggregator);

            // Récupère la playlist sauvegardée
            if (System.IO.File.Exists(@"Playlist.txt"))
            {
                string json = System.IO.File.ReadAllText(@"Playlist.txt");
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                HierarchicalTreeViewModel deserializedPlaylist = JsonConvert.DeserializeObject<HierarchicalTreeViewModel>(json, jsonSerializerSettings);

                // Copie toutes les propriétés utiles de la playlist déserialisée
                Playlist.AddMenuItems(deserializedPlaylist.RootFolder.Items.ToList());
                Playlist.Name = deserializedPlaylist.Name;
            }

            Playlist.CopyItemInEnabled = true;
            Playlist.CopyItemOutEnabled = false;
            Playlist.MoveItemEnabled = true;
            Playlist.IsEditable = true;
            Playlist.Name = "Ma playlist";
            
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

            CheckIfItemsExistInPlaylist(Explorer, Playlist);
            EventAggregator.GetEvent<MenuItemCollectionChangedEvent>().Subscribe((e) => {
                CheckIfItemsExistInPlaylist(Explorer, Playlist);
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
                    var jsonSerializerSettings = new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };
                    string jsonPlaylist = JsonConvert.SerializeObject(Playlist, jsonSerializerSettings);
                    System.IO.File.WriteAllText(@"Playlist.txt", jsonPlaylist);
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
        public void CheckIfItemsExistInPlaylist(HierarchicalTreeViewModel explorerVM, HierarchicalTreeViewModel playlistVM)
        {
            List<string> playlistFileTagIDs = playlistVM.GetAllFileTagIDs();

            CheckIfItemsExistInPlaylistRecursively(Explorer.RootFolder, playlistFileTagIDs);
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

            foreach(FileViewModel fileVM in folderVM.Items.OfType<FileViewModel>())
            {
                if(playlistFileTagIDs.Any(x => x == fileVM.FileTagID))
                {
                    fileVM.ExistsInPlaylist = true;
                }
                else
                {
                    fileVM.ExistsInPlaylist = false;
                }
            }
        }
    }
}
