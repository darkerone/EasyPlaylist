using EasyPlaylist.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace EasyPlaylist.CustomControls
{
    public class HierarchicalStyleSelector : StyleSelector
    {
        private Style _explorerItem;
        public Style ExplorerItem
        {
            get { return _explorerItem; }
            set { _explorerItem = value; }
        }

        private Style _playlistItem;
        public Style PlaylistItem
        {
            get { return _playlistItem; }
            set { _playlistItem = value; }
        }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            RadTreeView radTreeView = container.GetVisualParent<RadTreeView>();
            HierarchicalTreeViewModel hierarchicalTreeViewModel = radTreeView.DataContext as HierarchicalTreeViewModel;
            
            if (hierarchicalTreeViewModel.IsPlaylist)
            {
                return PlaylistItem;
            }
            else
            {
                return ExplorerItem;
            }
        }
    }
}
