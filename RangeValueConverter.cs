using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace trimr
{
    class RangeValueConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(values.Length == 2);
            object startTime = values[0];
            object endTime = values[1];
            Debug.Assert(startTime is double && endTime is double);

            startTime = Utils.MsToTimeString((double) startTime);
            endTime = Utils.MsToTimeString((double) endTime);

            return String.Format("{0} - {1}", startTime.ToString(), endTime.ToString());
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
