using EasyPlaylist.Views;
using Newtonsoft.Json;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace EasyPlaylist.ViewModels
{
    class PlaylistViewModel : HierarchicalTreeViewModel
    {
        #region Properties

        public override bool IsPlaylist { get { return true; } }

        public bool HasBeenModified { get; set; }

        #endregion

        private List<string> _errors = new List<string>();
        public List<string> Errors
        {
            get { return _errors; }
        }

        public PlaylistViewModel(IEventAggregator eventAggregator, string name = "Unnamed") : base(eventAggregator, name)
        {

        }

        #region Events

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
                        if(SelectedItems.First() != RootFolder)
                        {
                            SelectedItems.First().Rename();
                        }
                        else
                        {
                            string message = "Root folder can be renamed by renaming playlist";
                            MessageBoxResult messageBoxResult = CustomMessageBox.Show(message, "Rename", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
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

        #endregion

        #region Public methods

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

        #endregion

        #region Private methods

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

        #endregion
    }
}
