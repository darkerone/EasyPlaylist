using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlaylist.ViewModels
{
    class FolderViewModel : MenuItemViewModel
    {
        public FolderViewModel()
        {
            this.Items = new ObservableCollection<MenuItemViewModel>();
        }

        /// <summary>
        /// Sous dossiers/fichiers
        /// </summary>
        public ObservableCollection<MenuItemViewModel> Items { get; set; }
    }
}
