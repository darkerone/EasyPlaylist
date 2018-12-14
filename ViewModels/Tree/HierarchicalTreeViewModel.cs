using EasyPlaylist.Events;
using EasyPlaylist.Views;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        private FolderViewModel _rootFolder;
        public FolderViewModel RootFolder
        {
            get { return _rootFolder; }
            set
            {
                _rootFolder = value;
                RaisePropertyChanged("RootFolder");
            }
        }

        /// <summary>
        /// Liste contenant un seul dossier (le dossier racine). C'est cette liste qui est passée au ItemsSource
        /// du RadTreeView dans la vue. De cette manière, on peut afficher le dossier racine.
        /// </summary>
        public ObservableCollection<MenuItemViewModel> RootFolders { get; }

        private MenuItemViewModel _selectedItem;
        public MenuItemViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
                EventAggregator.GetEvent<SelectedItemChangedEvent>().Publish(_selectedItem);
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

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        public bool MoveItemEnabled { get; set; }
        public bool CopyItemInEnabled { get; set; }
        public bool CopyItemOutEnabled { get; set; }
        public bool IsEditable { get; set; }

        #endregion

        /// <summary>
        /// Contructeur pour désérialization
        /// </summary>
        [JsonConstructor]
        public HierarchicalTreeViewModel()
        {
        }

        public HierarchicalTreeViewModel(IEventAggregator eventAggregator, string name = "Unnamed")
        {
            EventAggregator = eventAggregator;
            Name = name;
            RootFolders = new ObservableCollection<MenuItemViewModel>();
            RootFolder = new FolderViewModel(eventAggregator, name, null);
            RootFolders.Add(RootFolder);
            RootFolder.IsExpanded = true;
            SelectedItem = RootFolder;
        }

        #region Commands

        [JsonIgnore]
        public ICommand RemoveSelectedItem
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    if (SelectedItem != null)
                    {
                        FolderViewModel parentFolder = SelectedItem.GetParentFolder();
                        RemoveMenuItem(SelectedItem);
                        SelectedItem = parentFolder;
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
                    radWindow.Closed += (object sender, WindowClosedEventArgs e) => {
                        RadWindow popup = sender as RadWindow;
                        DefineNamePopupView defineNamePopupView = popup.Content as DefineNamePopupView;
                        DefineNamePopupViewModel definePopupViewModel = defineNamePopupView.DataContext as DefineNamePopupViewModel;
                        if (e.DialogResult == true)
                        {
                            FolderViewModel newFolder = new FolderViewModel(EventAggregator, definePopupViewModel.ItemName, null);
                            AddMenuItem(newFolder);
                            SelectedItem = newFolder;
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
                    FolderBrowserDialog fbd = new FolderBrowserDialog();

                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        ExportFoldersAndFiles(fbd.SelectedPath, RootFolder);
                        RadWindow.Alert(new DialogParameters()
                        {
                            Content = $"Playlist exported to \"{fbd.SelectedPath}\"",
                            Header = "Success"
                        });
                    }
                });
            }
        }

        [JsonIgnore]
        public ICommand RenamePlaylist
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    DefineNamePopupView newDefineNamePopupView = new DefineNamePopupView();
                    DefineNamePopupViewModel newDefineNamePopupViewModel = new DefineNamePopupViewModel();
                    newDefineNamePopupViewModel.ItemName = Name;
                    newDefineNamePopupView.DataContext = newDefineNamePopupViewModel;
                    RadWindow radWindow = new RadWindow();
                    radWindow.Header = "Rename playlist";
                    radWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                    radWindow.Content = newDefineNamePopupView;

                    radWindow.Closed += (object sender, WindowClosedEventArgs e) => {
                        RadWindow popup = sender as RadWindow;
                        DefineNamePopupView defineNamePopupView = popup.Content as DefineNamePopupView;
                        DefineNamePopupViewModel defineNamePopupViewModel = defineNamePopupView.DataContext as DefineNamePopupViewModel;
                        if (e.DialogResult == true)
                        {
                            Name = defineNamePopupViewModel.ItemName;
                        }
                    };

                    radWindow.Show();
                });
            }
        }

        #endregion

        #region Public methods

        public void AddMenuItem(MenuItemViewModel itemsToAdd)
        {
            AddMenuItems(new List<MenuItemViewModel>() { itemsToAdd });
        }

        public void AddMenuItems(List<MenuItemViewModel> itemsToAdd)
        {
            // Si l'item sélectionné est un dossier
            if (SelectedItem is FolderViewModel)
            {
                // On ajoute les éléments au dossier sélectionné
                FolderViewModel selectedFolder = SelectedItem as FolderViewModel;
                selectedFolder.AddItems(itemsToAdd);
            }
            else
            {
                // On ajoute les éléments au dossier parent de l'élément sélectionné
                FolderViewModel folderWhereAdd = GetFirstParentFolder(SelectedItem);
                folderWhereAdd.AddItems(itemsToAdd);
            }
        }

        public void RemoveMenuItem(MenuItemViewModel itemToRemove)
        {
            RemoveMenuItems(new List<MenuItemViewModel>() { itemToRemove });
        }

        public void RemoveMenuItems(List<MenuItemViewModel> itemsToRemove)
        {
            foreach (MenuItemViewModel itemToRemove in itemsToRemove)
            {
                // On retire les éléments au dossier parent de l'élément sélectionné
                FolderViewModel folderWhereAdd = GetFirstParentFolder(itemToRemove);
                folderWhereAdd.RemoveItem(itemToRemove);
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

            foreach(FileViewModel fileVM in folderViewModel.Items.OfType<FileViewModel>())
            {
                fileTagIDs.Add(fileVM.FileTagID);
            }

            return fileTagIDs;
        }

        

        #endregion
    }
}
