using EasyPlaylist.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.TreeView;
using Telerik.Windows.DragDrop;

namespace EasyPlaylist.Views
{
    /// <summary>
    /// Logique d'interaction pour HierarchicalTreeView.xaml
    /// </summary>
    public partial class HierarchicalTreeView : UserControl
    {
        private bool _isSelectedCollectionChanging = false;

        public HierarchicalTreeView()
        {
            InitializeComponent();

            DragDropManager.AddDragOverHandler(HierarchicalTree, OnDragOver, true);
            DragDropManager.AddDropHandler(HierarchicalTree, OnDropCompleted, true);
            DragDropManager.AddDragDropCompletedHandler(HierarchicalTree, OnDragDropCompleted, true);
        }

        /// <summary>
        /// Une fois l'arbre chargé
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ExpandRootFolder();
        }

        private void HierarchicalTree_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            HierarchicalTreeViewModel viewModel = this.DataContext as HierarchicalTreeViewModel;
            if (viewModel != null)
            {
                viewModel.RootFolder.Items.CollectionChanged += RootFolders_CollectionChanged;
                viewModel.SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
            }
        }

        /// <summary>
        /// Avant de réduire un item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HierarchicalTree_PreviewCollapsed(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            HierarchicalTreeViewModel viewModel = this.DataContext as HierarchicalTreeViewModel;
            if (viewModel != null)
            {
                // Si l'item est la racine
                Control itemControl = e.OriginalSource as Control;
                if (itemControl.DataContext == viewModel.RootFolder)
                {
                    // On empêche la réduction pour qu'il reste ouvert
                    e.Handled = true;
                }
            }
        }

        private void RootFolders_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ExpandRootFolder();
        }

        /// <summary>
        /// Au survol (1er évenement appellé)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDragOver(object sender, Telerik.Windows.DragDrop.DragEventArgs e)
        {
            var options = DragDropPayloadManager.GetDataFromObject(e.Data, TreeViewDragDropOptions.Key) as TreeViewDragDropOptions;
            if (options != null)
            {
                HierarchicalTreeViewModel originTreeViewModel = GetOriginTreeViewModel(options);
                HierarchicalTreeViewModel destinationTreeViewModel = GetDestinationTreeViewModel(options);

                if (originTreeViewModel != null)
                {
                    if (destinationTreeViewModel != null)
                    {
                        // Destination : Explorer
                        if (!destinationTreeViewModel.IsPlaylist)
                        {
                            options.DropAction = DropAction.None; // Définit l'action qu'aura le RadTreeView lors du Drop et Visuellement
                        }
                        // Destination : Playlist
                        else
                        {
                            // Origine : Explorer
                            if (!originTreeViewModel.IsPlaylist)
                            {
                                options.DropAction = DropAction.Copy;
                            }
                            // Origine : Playlist
                            else
                            {
                                options.DropAction = DropAction.Move;
                            }

                            // Si un item cible existe
                            if (options.DropTargetItem != null)
                            {
                                // Si cet item cible est un fichier
                                FileViewModel fileTarget = options.DropTargetItem.DataContext as FileViewModel;
                                if (fileTarget != null)
                                {
                                    // On recherche son parent pour le définir comme cible à sa place
                                    options.DropTargetItem = options.DropTargetItem.ParentOfType<RadTreeViewItem>();
                                    options.DropPosition = DropPosition.Inside;
                                }

                                // Si l'item source est le même que l'item de destination
                                // Si l'item de destination est à l'intérieur de l'item source
                                if (options.DragSourceItem == options.DropTargetItem
                                    || options.DropTargetItem.GetParents().Contains(options.DragSourceItem))
                                {
                                    options.DropAction = DropAction.None;
                                }
                            }
                        }
                    }
                    else
                    {
                        options.DropAction = DropAction.None;
                    }
                }

                options.UpdateDragVisual();
            }
        }

        /// <summary>
        /// Quand le drop s'effectue (2ieme évenement appellé)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnDropCompleted(object sender, Telerik.Windows.DragDrop.DragEventArgs e)
        {
            var options = DragDropPayloadManager.GetDataFromObject(e.Data, TreeViewDragDropOptions.Key) as TreeViewDragDropOptions;
            if (options == null)
            {
                return;
            }

            //// Tell RadTreeView to do nothing with the data.
            options.DropAction = DropAction.None;
        }
        
        /// <summary>
        /// Quand le drag and drop s'effectue (3ieme évenement appellé)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDragDropCompleted(object sender, DragDropCompletedEventArgs e)
        {
            var options = DragDropPayloadManager.GetDataFromObject(e.Data, TreeViewDragDropOptions.Key) as TreeViewDragDropOptions;
            if (options == null)
            {
                return;
            }

            // ==============================
            // Récupère le treeview d'origine
            // ==============================
            HierarchicalTreeViewModel originTreeViewModel = GetOriginTreeViewModel(options);
            if (originTreeViewModel == null)
            {
                return;
            }

            // ===================================
            // Récupère le treeview de destination
            // ===================================
            HierarchicalTreeViewModel destinationTreeViewModel = GetDestinationTreeViewModel(options);
            if (destinationTreeViewModel == null)
            {
                return;
            }

            // ==============================
            // Récupère l'item de destination
            // ==============================
            MenuItemViewModel destinationItemVM;
            if (options.DropTargetItem != null)
            {
                destinationItemVM = options.DropTargetItem.DataContext as MenuItemViewModel;
            }
            else
            {
                destinationItemVM = destinationTreeViewModel.RootFolder;
            }

            // ===============
            // Eléments dropés
            // ===============
            List<object> draggedItems = options.DraggedItems as List<object>;
            List<MenuItemViewModel> draggedItemVms = draggedItems.OfType<MenuItemViewModel>().ToList();

            // ====================
            // Copie et Déplacement
            // ====================
            if (originTreeViewModel != null)
            {
                if (destinationTreeViewModel != null)
                {
                    // Destination : Playlist
                    if (destinationTreeViewModel.IsPlaylist)
                    {
                        // Origine : Explorer
                        if (!originTreeViewModel.IsPlaylist)
                        {
                            // Dossier de destination
                            FolderViewModel destinationFolderVM = GetDestinationFolder(destinationItemVM);

                            // Copie des éléments dropés
                            if (destinationFolderVM != null)
                            {
                                // Ajoute les éléments au dossier en les copiant
                                destinationFolderVM.AddItemsCopy(draggedItemVms);
                            }
                        }
                        // Origine : Playlist
                        else
                        {
                            FolderViewModel destinationFolderVM = GetDestinationFolder(destinationItemVM);

                            // Déplacement des éléments dropés
                            // Si l'item source est le même que l'item de destination
                            // Si l'item de destination est à l'intérieur de l'item source
                            if (destinationFolderVM != null
                                && options.DragSourceItem != options.DropTargetItem
                                && !options.DropTargetItem.GetParents().Contains(options.DragSourceItem))
                            {
                                // Retire les éléments déplacés de leur dossier d'origine
                                foreach (MenuItemViewModel draggedMenuItemVM in options.DraggedItems)
                                {
                                    draggedMenuItemVM.ParentFolder.RemoveItem(draggedMenuItemVM);
                                }

                                // Ajoute les éléments déplacés dans leur dossier de destination
                                destinationFolderVM.AddItems(draggedItemVms);
                            }
                        }
                    }
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// A la mise à jour de la collection des items sélectionnés
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Si ce n'est pas la vue qui est en train de faire la modifcation
            if (!_isSelectedCollectionChanging)
            {
                _isSelectedCollectionChanging = true;
                // On met à jour la liste des items sélectionnés du HierarchicalTree
                HierarchicalTree.SelectedItems.Clear();
                if (e.OldItems != null)
                {
                    foreach (MenuItemViewModel item in e.OldItems)
                    {
                        if (HierarchicalTree.SelectedItems.Contains(item))
                        {
                            HierarchicalTree.SelectedItems.Add(item);
                        }
                    }
                }
                if (e.NewItems != null)
                {
                    foreach (MenuItemViewModel item in e.NewItems)
                    {
                        if (HierarchicalTree.SelectedItems.Contains(item))
                        {
                            HierarchicalTree.SelectedItems.Add(item);
                        }
                    }
                }
                _isSelectedCollectionChanging = false;
            }
        }

        /// <summary>
        /// Lors du changement de sélection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HierarchicalTree_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isSelectedCollectionChanging)
            {
                _isSelectedCollectionChanging = true;
                HierarchicalTreeViewModel viewModel = this.DataContext as HierarchicalTreeViewModel;
                // On retire les éléments désélectionné du model de vue
                foreach (MenuItemViewModel item in e.RemovedItems)
                {
                    viewModel.SelectedItems.Remove(item);
                }
                // On ajoute les éléments sélectionné au model de vue
                foreach (MenuItemViewModel item in e.AddedItems)
                {
                    if (!viewModel.SelectedItems.Contains(item))
                    {
                        viewModel.SelectedItems.Add(item);
                    }
                }
                _isSelectedCollectionChanging = false;
            }
        }

        /// <summary>
        /// Au relachement d'une touche
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HierarchicalTree_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                // Récupère le modèle de vue pricinpal
                Window mainWindow = this.ParentOfType<Window>();
                MainViewModel mainViewModel = mainWindow.DataContext as MainViewModel;
                ExplorerViewModel viewModel = this.DataContext as ExplorerViewModel;
                if (viewModel != null)
                {
                    mainViewModel.AddSelectedItemsToSelectedPlaylist.Execute(null);
                }
                e.Handled = true;
            }
            if (e.Key == Key.Delete)
            {
                PlaylistViewModel viewModel = this.DataContext as PlaylistViewModel;
                if (viewModel != null)
                {
                    viewModel.RemoveSelectedItems.Execute(null);
                }
                e.Handled = true;
            }
        }

        #region Explorer

        /// <summary>
        /// Au clic sur le bouton "Add" du menu contextuel d'un item.
        /// On passe par le clic car passer par le DataContext ne semble pas fonctionner.
        /// En effet, nous avons besoin d'une command qui se trouve dans MainViewModel :
        /// DataContext="{Binding Path=PlacementTarget.Tag, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType={x:Type Popup}}}"
        /// De plus, même si celà était possible, il faudrait passer l'item en paramètre de la commande. 
        /// On se retrouverait avec 2 DataContext différents, ce qui n'est pas possible.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadMenuItem_Add_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            RadMenuItem radMenuItem = sender as RadMenuItem;
            // Récupère le modèle de vue de l'item
            MenuItemViewModel itemViewModel = radMenuItem.DataContext as MenuItemViewModel;

            // Récupère le modèle de vue pricinpal
            // Méthode intéressante, à conserver
            //RadContextMenu radContextMenu = radMenuItem.Parent as RadContextMenu;
            //System.Windows.Controls.Primitives.Popup popup = radContextMenu.Parent as System.Windows.Controls.Primitives.Popup;
            //Grid grid = popup.PlacementTarget as Grid;
            //MainViewModel mainViewModel = grid.Tag as MainViewModel;

            Window mainWindow = this.ParentOfType<Window>();
            MainViewModel mainViewModel = mainWindow.DataContext as MainViewModel;

            mainViewModel.AddItemsToSelectedPlaylist(new List<MenuItemViewModel>() { itemViewModel });
        }

        /// <summary>
        /// Au clic sur le bouton "Find in playlist" du menu contextuel d'un item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadMenuItem_Find_In_Playlist_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            RadMenuItem radMenuItem = sender as RadMenuItem;
            // Récupère le modèle de vue de l'item
            MenuItemViewModel itemViewModel = radMenuItem.DataContext as MenuItemViewModel;

            // Récupère le modèle de vue pricinpal
            Window mainWindow = this.ParentOfType<Window>();
            MainViewModel mainViewModel = mainWindow.DataContext as MainViewModel;

            mainViewModel.SearchInSelectedPlaylist(x => x.Path == itemViewModel.Path);
        }

        /// <summary>
        /// Pendant l'ouverture du menu contextuel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadContextMenu_Opening(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            // Récupère le modèle de vue pricinpal
            RadContextMenu radContextMenu = sender as RadContextMenu;

            // Récupère le modèle de vue pricinpal
            Window mainWindow = this.ParentOfType<Window>();
            MainViewModel mainViewModel = mainWindow.DataContext as MainViewModel;

            // Désactive le menu contextuel si aucune playlist n'est sélectionnée
            radContextMenu.IsEnabled = mainViewModel.SelectedPlaylist != null;
        }

        #endregion

        #region Playlist

        /// <summary>
        /// Au clic sur le bouton "Find in explorer" du menu contextuel d'un item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadMenuItem_Find_In_Explorer_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            RadMenuItem radMenuItem = sender as RadMenuItem;
            // Récupère le modèle de vue de l'item
            MenuItemViewModel itemViewModel = radMenuItem.DataContext as MenuItemViewModel;

            // Récupère le modèle de vue pricinpal
            Window mainWindow = this.ParentOfType<Window>();
            MainViewModel mainViewModel = mainWindow.DataContext as MainViewModel;

            mainViewModel.SearchInExplorer(x => x.Path == itemViewModel.Path);
        }

        #endregion

        #region Privates methods
        
        /// <summary>
        /// Ouvre/Etend le dossier racine.
        /// Utilisé car le IsExpanded du dossier racine ne semble pas fonctionner.
        /// </summary>
        private void ExpandRootFolder()
        {
            HierarchicalTreeViewModel viewModel = this.DataContext as HierarchicalTreeViewModel;
            if (viewModel != null)
            {
                try
                {
                    // On étend l'item racine
                    HierarchicalTree.ExpandItemByPath(viewModel.RootFolder.Title);
                }
                catch
                {

                }
            }
        }
        
        /// <summary>
        /// Renvoie le dossier de destination du drag and drop.
        /// Si l'élément de destination n'est pas un dossier, le dossier renvoyé sera son parent.
        /// </summary>
        /// <param name="destinationItemVM">Elément de destination (peut être null)</param>
        /// <returns></returns>
        private FolderViewModel GetDestinationFolder(MenuItemViewModel destinationItemVM)
        {
            // ======================
            // Dossier de destination
            // ======================
            FolderViewModel destinationFolderVM;
            // Si l'élément de destination est un dossier
            if (destinationItemVM is FolderViewModel)
            {
                // Le dossier de destination est l'élément de destination
                destinationFolderVM = destinationItemVM as FolderViewModel;
            }
            else
            {
                FileViewModel destinationFileVM = destinationItemVM as FileViewModel;
                // Le dossier de destination est le dossier parent de l'élément de destination
                destinationFolderVM = destinationFileVM.GetParentFolder();
            }

            return destinationFolderVM;
        }

        /// <summary>
        /// Récupère le modèle de vue du tree view d'origine
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private HierarchicalTreeViewModel GetOriginTreeViewModel(TreeViewDragDropOptions options)
        {
            if (options.DragSourceItem == null)
            {
                return null;
            }

            MenuItemViewModel sourceItem = options.DragSourceItem.DataContext as MenuItemViewModel;
            if (sourceItem == null)
            {
                return null;
            }

            return sourceItem.GetParentHierarchicalTree();
        }

        /// <summary>
        /// Récupère le modèle de vue du tree view de destination
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private HierarchicalTreeViewModel GetDestinationTreeViewModel(TreeViewDragDropOptions options)
        {
            RadTreeView destinationRadTreeView = options.DropTargetTree;
            HierarchicalTreeViewModel destinationTreeViewModel = null;

            // Si le treeview de destination est renseigné
            if (destinationRadTreeView != null)
            {
                // Récupère le modèle de vue du treeview de destination
                destinationTreeViewModel = destinationRadTreeView.DataContext as PlaylistViewModel;
            }
            else
            {
                // Si l'item de destination est renseigné
                if (options.DropTargetItem != null)
                {
                    // On récupère le treeview de destination grâce à l'item de destination
                    MenuItemViewModel droptTargetItem = options.DropTargetItem.DataContext as MenuItemViewModel;
                    destinationTreeViewModel = droptTargetItem.GetParentHierarchicalTree();
                }
                else
                {
                    return null;
                }
            }

            return destinationTreeViewModel;
        }

        #endregion
    }
}
