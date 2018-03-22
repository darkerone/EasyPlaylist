using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EasyPlaylist.ViewModels
{
    class DefineNamePopupViewModel : BaseViewModel
    {
        private string _itemName;
        public string ItemName
        {
            get { return _itemName; }
            set
            {
                _itemName = value;
                RaisePropertyChanged("ItemName");
            }
        }

        private bool _isOpen;
        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                _isOpen = value;
                RaisePropertyChanged("IsOpen");
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                RaisePropertyChanged("ErrorMessage");
            }
        }

        public DefineNamePopupViewModel()
        {
            IsOpen = false;
            ErrorMessage = "";
        }

        public bool ValidateName()
        {
            // Vérifie que le nom ne soit pas vide
            if (ItemName == null || ItemName == "")
            {
                ErrorMessage = "Name is required";
                return false;
            }

            return true;
        }
    }
}
