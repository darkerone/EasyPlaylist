using EasyPlaylist.Enums;
using EasyPlaylist.Views;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace EasyPlaylist.ViewModels
{
    abstract class MenuItemViewModel : BaseViewModel
    {
        private string _title;
        /// <summary>
        /// Titre du dossier/fichier
        /// </summary>
        public string Title {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged("Title");
            }
        }

        private IEventAggregator _eventAggregator;
        public IEventAggregator EventAggregator
        {
            get { return _eventAggregator; }
            set
            {
                _eventAggregator = value;
                RaisePropertyChanged("EventAggregator");
            }
        }

        /// <summary>
        /// Chemin du dossier/fichier
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Dossier parent
        /// </summary>
        public FolderViewModel ParentFolder { get; set; }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                RaisePropertyChanged("IsExpanded");
            }
        }
        
        public abstract bool IsFolder { get; }

        private ExistsInPlaylistStatusEnum _existsInPlaylistStatus;
        /// <summary>
        /// True si le FileTagID du fichier est aussi dans la playlist (booleen utilisé dans l'explorer)
        /// </summary>
        public ExistsInPlaylistStatusEnum ExistsInPlaylistStatus
        {
            get { return _existsInPlaylistStatus; }
            set
            {
                _existsInPlaylistStatus = value;
                RaisePropertyChanged("ExistsInPlaylistStatus");
            }
        }

        public MenuItemViewModel(IEventAggregator eventAggregator, string path, string title)
        {
            Path = path;
            if(title != null && title != "")
            {
                Title = title;
            }
            else
            {
                Title = System.IO.Path.GetFileNameWithoutExtension(path);
            }
           
            EventAggregator = eventAggregator;
            IsExpanded = false;
        }

        public ICommand Rename
        {
            get
            {
                return new DelegateCommand((parameter) =>
                {
                    DefineNamePopupView newDefineNamePopupView = new DefineNamePopupView();
                    DefineNamePopupViewModel newDefineNamePopupViewModel = new DefineNamePopupViewModel();
                    newDefineNamePopupViewModel.ItemName = Title;
                    newDefineNamePopupView.DataContext = newDefineNamePopupViewModel;
                    RadWindow radWindow = new RadWindow();
                    radWindow.Header = "Rename item";
                    radWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                    radWindow.Content = newDefineNamePopupView;
                    radWindow.Closed += (object sender, WindowClosedEventArgs e) => {
                        RadWindow popup = sender as RadWindow;
                        DefineNamePopupView defineNamePopupView = popup.Content as DefineNamePopupView;
                        DefineNamePopupViewModel defineNamePopupViewModel = defineNamePopupView.DataContext as DefineNamePopupViewModel;
                        if (e.DialogResult == true)
                        {
                            Title = defineNamePopupViewModel.ItemName;
                        }
                    };
                    radWindow.Show();
                });
            }
        }

        /// <summary>
        /// Renvoie une copie du menu item
        /// </summary>
        /// <returns></returns>
        abstract public MenuItemViewModel GetItemCopy();

        public FolderViewModel GetParentFolder()
        {
            return ParentFolder;
        }
    }
}
