using EasyPlaylist.ViewModels;
using EasyPlaylist.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EasyPlaylist.Models
{
    static class EasyPlaylistStorage
    {
        const string EasyPlaylistStorageFilePath = @"EasyPlaylistSettings.acep";

        private static EasyPlaylistSettingsViewModel _easyPlaylistSettings;
        public static EasyPlaylistSettingsViewModel EasyPlaylistSettings
        {
            get
            {
                if(_easyPlaylistSettings == null)
                {
                    Restore();
                }
                return _easyPlaylistSettings;
            }
        }

        /// <summary>
        /// Restaure les settings depuis le fichier de configuration
        /// </summary>
        private static void Restore()
        {
            // Récupère les playlists sauvegardées
            if (System.IO.File.Exists(EasyPlaylistStorageFilePath))
            {
                string json = System.IO.File.ReadAllText(EasyPlaylistStorageFilePath);
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };
                EasyPlaylistSettingsViewModel deserializedEasyPlaylistSettings = JsonConvert.DeserializeObject<EasyPlaylistSettingsViewModel>(json, jsonSerializerSettings);
                _easyPlaylistSettings = deserializedEasyPlaylistSettings;
            }
            else
            {
                _easyPlaylistSettings = new EasyPlaylistSettingsViewModel();
            }
        }

        /// <summary>
        /// Sauvegarde les settings passés en paramètre dans un fichier
        /// </summary>
        /// <param name="easyPlaylistSettings"></param>
        /// <returns></returns>
        public static bool Save(EasyPlaylistSettingsViewModel easyPlaylistSettings)
        {
            try
            {
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                string json = JsonConvert.SerializeObject(easyPlaylistSettings, jsonSerializerSettings);
                System.IO.File.WriteAllText(EasyPlaylistStorageFilePath, json);
                _easyPlaylistSettings = easyPlaylistSettings;
                return true;
            }
            catch
            {
                CustomMessageBox.Show("An error occured while saving settings", "Save settings", MessageBoxButton.OK, MessageBoxImage.Stop);
                return false;
            }
        }
    }
}
