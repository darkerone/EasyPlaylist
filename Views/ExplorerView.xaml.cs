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

            // ===============
            // Eléments dropés
            // ===============
            List<object> draggedItems = options.DraggedItems as List<object>;
            List<MenuItemViewModel> draggedItemVms = draggedItems.OfType<MenuItemViewModel>().ToList();

            // ===========================================
            // Dans le cas d'une copie dans un autre arbre
            // ===========================================
            if (options.DropTargetTree != null)
            {
                // ===================
                // Elément destination
                // ===================
                MenuItemViewModel destinationItemVM;
                if (options.DropTargetItem != null)
                {
                    RadTreeViewItem destinationItem = options.DropTargetItem as RadTreeViewItem;
                    destinationItemVM = destinationItem.DataContext as MenuItemViewModel;
                }
                else
                {
                    // Récupère l'élément root de l'arbre de destination
                    ExplorerViewModel explorerVM = options.DropTargetTree.DataContext as ExplorerViewModel;
                    destinationItemVM = explorerVM.RootFolder;
                }

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

                // =========================
                // Copie des éléments dropés
                // =========================
                if (destinationFolderVM != null)
                {
                    // Copie les éléments dropés 
                    List<MenuItemViewModel> copiedElements = draggedItemVms.Select(x => x.GetItemCopy()).ToList();

                    // Ajoute les éléments copiés au dossier
                    destinationFolderVM.AddItems(copiedElements);
                }
            }
            // ==================================================
            // Dans le cas d'un déplacement au sein du même arbre
            // ==================================================
            else
            {

            }
            
        }

    }
}
