﻿using EasyPlaylist.ViewModels;
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
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }

        //private void gif_MediaEnded(object sender, RoutedEventArgs e)
        //{
        //    gif.Position = new TimeSpan(0, 0, 1);
        //    gif.Play();
        //}

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainViewModel mainViewModel = this.DataContext as MainViewModel;
            
            // Si aucune playlist n'a été modifiée
            if (!mainViewModel.Playlists.Any(x => x.HasBeenModified))
            {
                return;
            }

            MessageBoxResult result = CustomMessageBox.Show($"Save playlists ?", "Save playlists", MessageBoxButton.YesNoCancel);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    mainViewModel.SavePlaylists();
                    break;
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
            }
        }
    }
}
