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
        private IEventAggregator _eventAggregator;

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
        }

        public ICommand Rename
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    FolderNamePopupView folderNamePopupView = new FolderNamePopupView();
                    FolderNamePopupViewModel folderNamePopupViewModel = new FolderNamePopupViewModel();
                    folderNamePopupViewModel.ItemName = Title;
                    folderNamePopupView.DataContext = folderNamePopupViewModel;
                    RadWindow radWindow = new RadWindow();
                    radWindow.Header = "Rename item";
                    radWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                    radWindow.Content = folderNamePopupView;
                    radWindow.Closed += FolderNamePopup_Closed;
                    radWindow.Show();
                });
            }
        }

        private void FolderNamePopup_Closed(object sender, WindowClosedEventArgs e)
        {
            RadWindow popup = sender as RadWindow;
            FolderNamePopupView folderNamePopupView = popup.Content as FolderNamePopupView;
            FolderNamePopupViewModel folderNamePopupViewModel = folderNamePopupView.DataContext as FolderNamePopupViewModel;
            if (e.DialogResult == true)
            {
                Title = folderNamePopupViewModel.ItemName;
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
