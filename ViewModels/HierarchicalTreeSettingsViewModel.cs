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
        #region Properties

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

        private bool _exportRandomOrderPlaylist;
        /// <summary>
        /// Définit si la playlist doit être mélangée lors de l'export
        /// </summary>
        public bool ExportRandomOrderPlaylist
        {
            get { return _exportRandomOrderPlaylist; }
            set
            {
                _exportRandomOrderPlaylist = value;
                RaisePropertyChanged("ExportRandomOrderPlaylist");
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

        #endregion

        public HierarchicalTreeSettingsViewModel(HierarchicalTreeSettingsViewModel hierarchicalTreeSettings = null)
        {
            if(hierarchicalTreeSettings == null)
            {
                Name = "Unnamed";
                ExportFlatPlaylist = false;
                ExportRandomOrderPlaylist = false;
            }
            else
            {
                Name = hierarchicalTreeSettings.Name;
                ExportFlatPlaylist = hierarchicalTreeSettings.ExportFlatPlaylist;
                ExportRandomOrderPlaylist = hierarchicalTreeSettings.ExportRandomOrderPlaylist;
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
