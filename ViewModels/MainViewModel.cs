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
                    if(Path.GetExtension(filePath) == ".mp3")
                    {
                        folderViewModel.AddItem(new FileViewModel(filePath));
                    }
                }
            }

            return folderViewModel;
        }
    }
}
