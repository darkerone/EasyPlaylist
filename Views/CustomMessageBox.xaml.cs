using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EasyPlaylist.Views
{
    /// <summary>
    /// Logique d'interaction pour CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        private CustomMessageBox()
        {
            InitializeComponent();
        }

        static CustomMessageBox _messageBox;
        static MessageBoxResult _result = MessageBoxResult.No;

        /// <summary>
        /// Affiche une boîte de message qui contient un message et retourne un résultat.
        /// </summary>
        /// <param name="messageBoxText">System.String qui spécifie le texte à afficher</param>
        /// <returns></returns>
        public static MessageBoxResult Show(string messageBoxText)
        {
            return Show(messageBoxText, string.Empty, MessageBoxButton.OK, MessageBoxImage.None);
        }

        /// <summary>
        /// Affiche une boîte de message qui contient un message et retourne un résultat.
        /// </summary>
        /// <param name="messageBoxText">System.String qui spécifie le texte à afficher</param>
        /// <param name="caption">System.String qui spécifie la légende de barre de titre à afficher.</param>
        /// <returns></returns>
        public static MessageBoxResult Show(string messageBoxText, string caption)
        {
            return Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None);
        }

        /// <summary>
        /// Affiche une boîte de message qui contient un message et retourne un résultat.
        /// </summary>
        /// <param name="messageBoxText">System.String qui spécifie le texte à afficher</param>
        /// <param name="caption">System.String qui spécifie la légende de barre de titre à afficher.</param>
        /// <param name="button">System.Windows.MessageBoxButton qui spécifie quel(s) bouton(s) afficher.</param>
        /// <returns></returns>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button)
        {
            return Show(messageBoxText, messageBoxText, button, MessageBoxImage.None);
        }

        /// <summary>
        /// Affiche une boîte de message qui contient un message et retourne un résultat.
        /// </summary>
        /// <param name="messageBoxText">System.String qui spécifie le texte à afficher</param>
        /// <param name="caption">System.String qui spécifie la légende de barre de titre à afficher.</param>
        /// <param name="button">System.Windows.MessageBoxButton qui spécifie quel(s) bouton(s) afficher.</param>
        /// <param name="icon">System.Windows.MessageBoxImage qui spécifie l'icône à afficher.</param>
        /// <returns></returns>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            _messageBox = new CustomMessageBox()
            {
                txtMsg = {
                    Text = messageBoxText
                },
                Title = caption
            };
            SetVisibilityOfButtons(button);
            SetImageOfMessageBox(icon);
            _messageBox.ShowDialog();
            return _result;
        }

        private static void SetVisibilityOfButtons(MessageBoxButton button)
        {
            switch (button)
            {
                case MessageBoxButton.OK:
                    _messageBox.btnCancel.Visibility = Visibility.Collapsed;
                    _messageBox.btnNo.Visibility = Visibility.Collapsed;
                    _messageBox.btnYes.Visibility = Visibility.Collapsed;
                    _messageBox.btnOk.Focus();
                    break;
                case MessageBoxButton.OKCancel:
                    _messageBox.btnNo.Visibility = Visibility.Collapsed;
                    _messageBox.btnYes.Visibility = Visibility.Collapsed;
                    _messageBox.btnOk.Focus();
                    break;
                case MessageBoxButton.YesNo:
                    _messageBox.btnOk.Visibility = Visibility.Collapsed;
                    _messageBox.btnCancel.Visibility = Visibility.Collapsed;
                    _messageBox.btnYes.Focus();
                    break;
                case MessageBoxButton.YesNoCancel:
                    _messageBox.btnOk.Visibility = Visibility.Collapsed;
                    _messageBox.btnYes.Focus();
                    break;
                default:
                    break;
            }
        }

        private static void SetImageOfMessageBox(MessageBoxImage image)
        {
            switch (image)
            {
                case MessageBoxImage.None:
                    _messageBox.img.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxImage.Question:
                    _messageBox.SetImage(SystemIcons.Question);
                    break;
                case MessageBoxImage.Error:
                    _messageBox.SetImage(SystemIcons.Error);
                    break;
                case MessageBoxImage.Warning:
                    _messageBox.SetImage(SystemIcons.Warning);
                    break;
                case MessageBoxImage.Information:
                    _messageBox.SetImage(SystemIcons.Information);
                    break;
                default:
                    _messageBox.img.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnOk)
                _result = MessageBoxResult.OK;
            else if (sender == btnYes)
                _result = MessageBoxResult.Yes;
            else if (sender == btnNo)
                _result = MessageBoxResult.No;
            else if (sender == btnCancel)
                _result = MessageBoxResult.Cancel;
            else
                _result = MessageBoxResult.None;
            _messageBox.Close();
            _messageBox = null;
        }

        private void SetImage(Icon icon)
        {
            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            img.Source = wpfBitmap;
        }
    }
}
