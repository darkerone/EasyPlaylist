﻿using EasyPlaylist.ViewModels;
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
    /// Logique d'interaction pour ExplorerView.xaml
    /// </summary>
    public partial class ExplorerView : UserControl
    {
        public ExplorerView()
        {
            InitializeComponent();

            DragDropManager.AddDragOverHandler(ExplorerTreeView, OnDragOver, true);
            DragDropManager.AddDropHandler(ExplorerTreeView, OnDropCompleted, true);
            DragDropManager.AddDragDropCompletedHandler(ExplorerTreeView, OnDragDropCompleted, true);
        }

        private void OnDragOver(object sender, Telerik.Windows.DragDrop.DragEventArgs e)
        {
            var options = DragDropPayloadManager.GetDataFromObject(e.Data, TreeViewDragDropOptions.Key) as TreeViewDragDropOptions;
            if (options != null)
            {
                options.DropAction = DropAction.Copy;
            }
        }

        #region Static methods

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

        // https://www.telerik.com/forums/drag-and-drop----how-to-handle-the-move-copy-myself
        private static void OnDragDropCompleted(object sender, DragDropCompletedEventArgs e)
        {
            var options = DragDropPayloadManager.GetDataFromObject(e.Data, TreeViewDragDropOptions.Key) as TreeViewDragDropOptions;
            if (options == null)
            {
                return;
            }

            // ==============================
            // Récupère le treeview d'origine
            // ==============================
            RadTreeView originRadTreeView = sender as RadTreeView;
            ExplorerViewModel originExplorerVM = originRadTreeView.DataContext as ExplorerViewModel;

            if (originRadTreeView == null || originExplorerVM == null)
            {
                return;
            }

            // ===================================
            // Récupère le treeview de destination
            // ===================================
            RadTreeView destinationRadTreeView = options.DropTargetTree;
            // Si le treeview de destination n'est pas renseigné
            if (destinationRadTreeView == null)
            {
                // Si l'item de destination est renseigné
                if (options.DropTargetItem != null)
                {
                    // On récupère le treeview de destination grâce à l'item de destination
                    destinationRadTreeView = (options.DropTargetItem as FrameworkElement).ParentOfType<RadTreeView>();
                }
            }

            ExplorerViewModel destinationExplorerVM = destinationRadTreeView.DataContext as ExplorerViewModel;

            if (destinationRadTreeView == null || destinationExplorerVM == null)
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
                destinationItemVM = destinationExplorerVM.RootFolder;
            }

            // ===============
            // Eléments dropés
            // ===============
            List<object> draggedItems = options.DraggedItems as List<object>;
            List<MenuItemViewModel> draggedItemVms = draggedItems.OfType<MenuItemViewModel>().ToList();

            // =============================================
            // Dans le cas de 2 arbres différents : on copie
            // =============================================
            if (originRadTreeView != destinationRadTreeView)
            {
                if (destinationExplorerVM != null && destinationExplorerVM.CopyItemInEnabled && originExplorerVM.CopyItemOutEnabled)
                {
                    // Dossier de destination
                    FolderViewModel destinationFolderVM = GetDestinationFolder(destinationItemVM);

                    // Copie des éléments dropés
                    if (destinationFolderVM != null)
                    {
                        // Copie les éléments dropés 
                        List<MenuItemViewModel> copiedElements = draggedItemVms.Select(x => x.GetItemCopy()).ToList();

                        // Ajoute les éléments copiés au dossier
                        destinationFolderVM.AddItems(copiedElements);
                    }
                }
            }
            // ======================================
            // Dans le cas du même arbre : on déplace
            // ======================================
            else
            {
                if (originExplorerVM.MoveItemEnabled)
                {
                    FolderViewModel destinationFolderVM = GetDestinationFolder(destinationItemVM);

                    // Déplacement des éléments dropés
                    if (destinationFolderVM != null)
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

        /// <summary>
        /// Renvoie le dossier de destination du drag and drop.
        /// Si l'élément de destination n'est pas un dossier, le dossier renvoyé sera son parent.
        /// </summary>
        /// <param name="destinationItemVM">Elément de destination (peut être null)</param>
        /// <returns></returns>
        static private FolderViewModel GetDestinationFolder(MenuItemViewModel destinationItemVM)
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

        #endregion
    }
}
