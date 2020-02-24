using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace HidVanguard.Config.UI.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class HiddenTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool valbool)
            {
                if (valbool)
                    return "Hidden";
                else
                    return "Visible";
            }

            return "ERROR";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
