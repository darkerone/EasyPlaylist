using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlaylist.ViewModels
{
    class MusicViewModel : BaseViewModel
    {
        private string _pathNameExtension; // Chemin + nom + extension du fichier
        private string _name;

        public string PathNameExtension
        {
            get { return _pathNameExtension; }
            set
            {
                _pathNameExtension = value;
                RaisePropertyChanged("PathNameExtension");
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
    }
}
