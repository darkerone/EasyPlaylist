using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlaylist.ViewModels
{
    class BaseViewModel : INotifyPropertyChanged
    {
        #region Property Change
        // On créé une méthode pour éviter de recopier a chaque fois le if...
        internal void RaisePropertyChanged(string propertyName)
        {
            // On vérifie qu'il y ai des abonnés à l'evenement
            if (PropertyChanged != null)
            {
                // On dit que la propriété "propertyName" a été changée
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
