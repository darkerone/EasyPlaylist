using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EasyPlaylist.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        private MusicViewModel _music;

        public ExplorerViewModel Explorer { get; set; }

        public TreeViewItem TreeView { get; set; }

        public MusicViewModel Music
        {
            get { return _music; }
            set
            {
                _music = value;
                RaisePropertyChanged("Music");
            }
        }
    }
}
