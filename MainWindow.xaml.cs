using EasyPlaylist.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace EasyPlaylist.Views
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel mainViewModel;

        public MainWindow()
        {
            InitializeComponent();

            ExplorerViewModel playlist = null;

            // Récupère la playlist sauvegardée
            if (System.IO.File.Exists(@"Playlist.txt"))
            {
                string json = System.IO.File.ReadAllText(@"Playlist.txt");
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                playlist = JsonConvert.DeserializeObject<ExplorerViewModel>(json, jsonSerializerSettings);
            }

            if(playlist == null)
            {
                playlist = new ExplorerViewModel();
            }

            mainViewModel = new MainViewModel(playlist);

            mainViewModel.Explorer = new ExplorerViewModel();
            string defaultMyMusicFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            // Récupère le dossier et ses sous dossiers et fichiers
            FolderViewModel musicFolder = mainViewModel.GetFolderViewModel(defaultMyMusicFolderPath, "Musiques");
            mainViewModel.Explorer.AddMenuItems(musicFolder.Items.ToList());

            

            DataContext = mainViewModel;
        }
    }
}
