using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace EasyPlaylist.Converters
{
    /// <summary>
    /// Ajoute la valeur passé en paramètre
    /// </summary>
    public class AddValueConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double currentValue = (double)value;
            double valueToAdd = double.Parse(parameter.ToString());
            return currentValue + valueToAdd;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
