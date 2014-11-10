using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Custom.Converters
{
    class MinutesToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string result = "00:00";
            int total = (int)value;
            int hours = 0;
            int minutes = 0;

            if (total > 0)
            {
                hours = total / 60;
                minutes = total % 60;
                
                result = hours < 10 ? "0" + hours.ToString() : hours.ToString();
                result += ":";
                result += minutes < 10 ? "0" + minutes.ToString() : minutes.ToString(); 
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
