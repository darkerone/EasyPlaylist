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
    class EasyPlaylistSettingsViewModel : BaseViewModel
    {
        public EasyPlaylistSettingsViewModel(EasyPlaylistSettingsViewModel easyPlaylistSettings = null)
        {
            if(easyPlaylistSettings == null)
            {
                AnteriorityYears = 0;
                AnteriorityMonths = 0;
                AnteriorityDays = 1;
                AnteriorityHours = 0;
                ExplorerPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                IsFileWatcherOptionEnabled = true;
            }
            else
            {
                AnteriorityYears = easyPlaylistSettings.AnteriorityYears;
                AnteriorityMonths = easyPlaylistSettings.AnteriorityMonths;
                AnteriorityDays = easyPlaylistSettings.AnteriorityDays;
                AnteriorityHours = easyPlaylistSettings.AnteriorityHours;
                ExplorerPath = easyPlaylistSettings.ExplorerPath;
                IsFileWatcherOptionEnabled = easyPlaylistSettings.IsFileWatcherOptionEnabled;
            }
        }

        private int _anteriorityYears;
        /// <summary>
        /// Définit jusqu'à quand le fichier est considéré comme récent.
        /// On définit ici le nombre d'année qui vont être soustraits à 
        /// la date d'aujourd'hui pour déterminer les fichiers récents
        /// </summary>
        public int AnteriorityYears
        {
            get { return _anteriorityYears; }
            set
            {
                _anteriorityYears = value;
                RaisePropertyChanged("AnteriorityYears");
            }
        }

        private int _anteriorityMonths;
        /// <summary>
        /// Définit jusqu'à quand le fichier est considéré comme récent.
        /// On définit ici le nombre de mois qui vont être soustraits à 
        /// la date d'aujourd'hui pour déterminer les fichiers récents
        /// </summary>
        public int AnteriorityMonths
        {
            get { return _anteriorityMonths; }
            set
            {
                _anteriorityMonths = value;
                RaisePropertyChanged("AnteriorityMonths");
            }
        }

        private int _anteriorityDays;
        /// <summary>
        /// Définit jusqu'à quand le fichier est considéré comme récent.
        /// On définit ici le nombre de jours qui vont être soustraits à 
        /// la date d'aujourd'hui pour déterminer les fichiers récents
        /// </summary>
        public int AnteriorityDays
        {
            get { return _anteriorityDays; }
            set
            {
                _anteriorityDays = value;
                RaisePropertyChanged("AnteriorityDays");
            }
        }

        private int _anteriorityHours;
        /// <summary>
        /// Définit jusqu'à quand le fichier est considéré comme récent.
        /// On définit ici le nombre d'heures qui vont être soustraits à 
        /// la date d'aujourd'hui pour déterminer les fichiers récents
        /// </summary>
        public int AnteriorityHours
        {
            get { return _anteriorityHours; }
            set
            {
                _anteriorityHours = value;
                RaisePropertyChanged("AnteriorityHours");
            }
        }

        private string _explorerPath;
        /// <summary>
        /// Définit le chemin de l'explorer
        /// </summary>
        public string ExplorerPath
        {
            get { return _explorerPath; }
            set
            {
                _explorerPath = value;
                RaisePropertyChanged("ExplorerPath");
            }
        }

        public bool _isFileWatcherOptionEnabled;
        /// <summary>
        /// Définit si l'application va surveiller les modification des fichiers/dossiers Windows
        /// </summary>
        public bool IsFileWatcherOptionEnabled
        {
            get { return _isFileWatcherOptionEnabled; }
            set
            {
                _isFileWatcherOptionEnabled = value;
                RaisePropertyChanged("IsFileWatcherOptionEnabled");
            }
        }
    }
}
