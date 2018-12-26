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
    public class HierarchicalTemplateSelector : DataTemplateSelector
    {
        private HierarchicalDataTemplate _explorerItem;
        public HierarchicalDataTemplate ExplorerItem
        {
            get { return _explorerItem; }
            set { _explorerItem = value; }
        }

        private HierarchicalDataTemplate _playlistItem;
        public HierarchicalDataTemplate PlaylistItem
        {
            get { return _playlistItem; }
            set { _playlistItem = value; }
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
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
