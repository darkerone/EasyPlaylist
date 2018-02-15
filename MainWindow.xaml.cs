using EasyPlaylist.ViewModels;
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

            mainViewModel = new MainViewModel()
            {
                Music = new MusicViewModel()
                {
                    Name = "baba"
                },
            };

            mainViewModel.Explorer = new ExplorerViewModel()
            {
                Items = new ObservableCollection<MenuItemViewModel>()
                    {
                        new FolderViewModel()
                        {
                            Title = "1",
                            Items = new ObservableCollection<MenuItemViewModel>()
                            {
                                new MenuItemViewModel()
                                {
                                    Title = "11"
                                },
                                new MenuItemViewModel()
                                {
                                    Title = "12"
                                }
                            }
                        },
                        new FolderViewModel()
                        {
                            Title = "2",
                            Items = new ObservableCollection<MenuItemViewModel>()
                            {
                                new FolderViewModel()
                                {
                                    Title = "21",
                                    Items = new ObservableCollection<MenuItemViewModel>
                                    {
                                        new FileViewModel
                                        {
                                            Title = "211"
                                        } 
                                    }
                                },
                                new FileViewModel()
                                {
                                    Title = "22"
                                }
                            }
                        },
                        new MenuItemViewModel()
                        {
                            Title = "3"
                        }
                    }
            };

            DataContext = mainViewModel;
        }
    }
}
