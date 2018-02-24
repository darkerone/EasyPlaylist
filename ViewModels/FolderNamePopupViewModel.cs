using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EasyPlaylist.ViewModels
{
    class FolderNamePopupViewModel : BaseViewModel
    {
        private bool _isOpen;
        private string _errorMessage;

        public string ItemName { get; set; }

        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                _isOpen = value;
                RaisePropertyChanged("IsOpen");
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                RaisePropertyChanged("ErrorMessage");
            }
        }

        public FolderNamePopupViewModel()
        {
            IsOpen = false;
            ErrorMessage = "";
        }

        public bool ValidateAddFolder()
        {
            // Vérifie que le nom du dossier ne soit pas vide
            if (ItemName == null || ItemName == "")
            {
                ErrorMessage = "Name is required";
                return false;
            }

            return true;
        }
    }
}
