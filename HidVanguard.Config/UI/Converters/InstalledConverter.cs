using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HidVanguard.Config.UI.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class InstalledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool valbool)
            {
                if (targetType == typeof(Brush))
                    if (valbool)
                        return Brushes.ForestGreen;
                    else
                        return Brushes.IndianRed;
                else
                    if (valbool)
                        return "Installed";
                    else
                        return "Not Installed";
            }

            if (targetType == typeof(Brush))
                return Brushes.Red; 
            else
                return "ERROR";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
