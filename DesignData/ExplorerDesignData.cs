using EasyPlaylist.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyPlaylist.ViewModels;
using System.Collections.ObjectModel;

namespace EasyPlaylist.DesignData
{
    class ExplorerDesignData : IExplorer
    {
        public ObservableCollection<MenuItemViewModel> Items { get; set; }

        public ExplorerDesignData()
        {
            //Items =  new ObservableCollection<MenuItemViewModel>()
            //{
            //    new FolderViewModel()
            //    {
            //        Title = "aaa",
            //        Items = new ObservableCollection<MenuItemViewModel>()
            //        {
            //            new MenuItemViewModel()
            //            {
            //                Title = "aaa1"
            //            },
            //            new MenuItemViewModel()
            //            {
            //                Title = "aaa2"
            //            }
            //        }
            //    },
            //    new MenuItemViewModel()
            //    {
            //        Title = "bbb"
            //    },
            //    new MenuItemViewModel()
            //    {
            //        Title = "ccc"
            //    } 
            //};
        }
    }
}
