using EasyPlaylist.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlaylist.ViewModels
{
    class ExplorerViewModel : IExplorer
    {
        public ObservableCollection<MenuItemViewModel> Items { get; set; }
    }
}
