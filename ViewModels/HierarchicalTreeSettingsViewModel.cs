using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EasyPlaylist.ViewModels
{
    class HierarchicalTreeSettingsViewModel : BaseViewModel
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        private bool _exportFlatPlaylist;
        /// <summary>
        /// Définit si la playlist doit être aplatie lors de l'export
        /// </summary>
        public bool ExportFlatPlaylist
        {
            get { return _exportFlatPlaylist; }
            set
            {
                _exportFlatPlaylist = value;
                RaisePropertyChanged("ExportFlatPlaylist");
            }
        }

        private string _errorMessage;
        [JsonIgnore]
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                RaisePropertyChanged("ErrorMessage");
            }
        }

        public bool Validate()
        {
            // Vérifie que le nom ne soit pas vide
            if (Name == null || Name == "")
            {
                ErrorMessage = "Name is required";
                return false;
            }

            return true;
        }
    }
}
