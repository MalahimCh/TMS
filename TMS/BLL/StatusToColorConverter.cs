using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TMS.BLL
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string;
            if (status == null) return Brushes.LightGray;

            switch (status)
            {
                case "Assigned":
                    return new SolidColorBrush(Color.FromRgb(173, 216, 230)); // light blue
                case "WaitingSupport":
                    return new SolidColorBrush(Color.FromRgb(255, 200, 0));   // orange
                case "WaitingCustomer":
                    return new SolidColorBrush(Color.FromRgb(255, 255, 153)); // yellow
                case "Resolved":
                    return new SolidColorBrush(Color.FromRgb(144, 238, 144)); // light green
                default:
                    return Brushes.LightGray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
