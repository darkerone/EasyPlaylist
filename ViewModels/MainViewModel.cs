using Newtonsoft.Json;
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
        private ExplorerViewModel _explorer;
        private ExplorerViewModel _playlist;

        public ExplorerViewModel Explorer
        {
            get { return _explorer; }
            set
            {
                _explorer = value;
                RaisePropertyChanged("Explorer");
            }
        }

        public ExplorerViewModel Playlist
        {
            get { return _playlist; }
            set
            {
                _playlist = value;
                RaisePropertyChanged("Playlist");
            }
        }

        public MainViewModel()
        {
            // Récupère la playlist sauvegardée
            if (System.IO.File.Exists(@"Playlist.txt"))
            {
                string json = System.IO.File.ReadAllText(@"Playlist.txt");
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                Playlist = JsonConvert.DeserializeObject<ExplorerViewModel>(json, jsonSerializerSettings);
            }

            if (Playlist == null)
            {
                Playlist = new ExplorerViewModel();
            }
            Playlist.CopyItemInEnabled = true;
            Playlist.CopyItemOutEnabled = false;
            Playlist.MoveItemEnabled = true;
            Playlist.IsEditable = true;
            Playlist.Name = "Ma playlist";
            
            Explorer = new ExplorerViewModel();
            string defaultMyMusicFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            // Récupère le dossier et ses sous dossiers et fichiers
            FolderViewModel musicFolder = GetFolderViewModel(defaultMyMusicFolderPath, "Musiques");
            Explorer.AddMenuItems(musicFolder.Items.ToList());
            Explorer.CopyItemInEnabled = false;
            Explorer.CopyItemOutEnabled = true;
            Explorer.MoveItemEnabled = false;
            Explorer.IsEditable = false;

            CheckIfItemsExistInPlaylist(Explorer, Playlist);
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
            FolderViewModel folderViewModel = new FolderViewModel(directoryPath);

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
                        folderViewModel.AddItem(new FileViewModel(filePath));
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
        public void CheckIfItemsExistInPlaylist(ExplorerViewModel explorerVM, ExplorerViewModel playlistVM)
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
