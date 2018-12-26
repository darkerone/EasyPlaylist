using EasyPlaylist.Models;
using EasyPlaylist.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.TreeView;
using Telerik.Windows.DragDrop;

namespace EasyPlaylist.Views
{
    /// <summary>
    /// Logique d'interaction pour EasyPlaylistSettingsView.xaml
    /// </summary>
    public partial class EasyPlaylistSettingsView : UserControl
    {
        public EasyPlaylistSettingsView()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RadWindow window = this.ParentOfType<RadWindow>();
            window.DialogResult = false;
            window.Close();
        }

        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            EasyPlaylistSettingsView configurationPopupView = (sender as FrameworkElement).ParentOfType<EasyPlaylistSettingsView>(); ;
            EasyPlaylistSettingsViewModel configurationPopupViewModel = configurationPopupView.DataContext as EasyPlaylistSettingsViewModel;
            
            RadWindow window = this.ParentOfType<RadWindow>();
            window.DialogResult = true;
            window.Close();
        }
    }
}
