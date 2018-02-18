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
        public ExplorerViewModel Explorer { get; set; }
         
        public ExplorerViewModel Playlist { get; set; }

        public MainViewModel(ExplorerViewModel playlist)
        {
            Playlist = playlist;
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
                        Explorer.RootFolder.Items.Clear();
                        Explorer.RootFolder.Items.Add(GetFolderViewModel(fbd.SelectedPath, Path.GetFileName(fbd.SelectedPath)));
                    }
                });
            }
        }

        public ICommand AddToPlaylist
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    // Si l'élément sélectionné dans la playlist est un dossier
                    if(Playlist.SelectedItem != null && Playlist.SelectedItem is FolderViewModel)
                    {
                        // On ajoute l'élément au dossier séléctionné
                        FolderViewModel folderVM = Playlist.SelectedItem as FolderViewModel;
                        folderVM.Items.Add(Explorer.SelectedItem.GetItemCopy());
                    }
                    else
                    {
                        // On ajoute l'élément au dossier parent
                        Playlist.AddMenuItem(Explorer.SelectedItem.GetItemCopy());
                    }
                });
            }
        }

        public ICommand AddFolderToPlaylist
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    FolderViewModel newFolder = new FolderViewModel(null, "Nouveau dossier");
                    Playlist.AddMenuItem(newFolder);
                    Playlist.SelectedItem = newFolder;
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
                        TypeNameHandling = TypeNameHandling.All
                    };
                    string jsonPlaylist = JsonConvert.SerializeObject(Playlist, jsonSerializerSettings);
                    System.IO.File.WriteAllText(@"Playlist.txt", jsonPlaylist);
                });
            }
        }

        public ICommand ExportPlaylist
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    FolderBrowserDialog fbd = new FolderBrowserDialog();

                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        ExportFoldersAndFiles(fbd.SelectedPath, Playlist.RootFolder);
                    }
                });
            }
        }

        /// <summary>
        /// Exporte tout le contenu du dossier passé en paramètre dans le dossier de destination
        /// </summary>
        /// <param name="destinationFolder"></param>
        /// <param name="folderVM"></param>
        private void ExportFoldersAndFiles(string destinationFolder, FolderViewModel folderVM)
        {
            string folderPath = destinationFolder + "\\" + folderVM.Title;

            // Créé le dossier
            bool success = folderVM.WriteFolder(destinationFolder);

            if (success)
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
                    folderViewModel.Items.Add(GetFolderViewModel(subDirectoryPath, Path.GetFileName(subDirectoryPath)));
                }
            }

            // Fichiers
            string[] filesPaths = Directory.GetFiles(directoryPath);
            if (filesPaths.Any())
            {
                foreach (string filePath in filesPaths)
                {
                    if(Path.GetExtension(filePath) == ".mp3")
                    {
                        folderViewModel.Items.Add(new FileViewModel(filePath));
                    }
                }
            }

            return folderViewModel;
        }
    }
}
