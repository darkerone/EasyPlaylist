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
    /// Retourne true si value (un entier) vaut la même valeur que le paramètre
    /// </summary>
    public class IntParameterValueToTrueConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int valueInt = (int)value;
            int parameterInt = Int32.Parse((string)parameter);
            return valueInt == parameterInt;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
