using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CTecUtil.UI.ViewHelpers
{
    [ValueConversion(typeof(string), typeof(StopBits))]
    public sealed class StopBitsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (StopBits)value switch { StopBits.None => "None", StopBits.One => "1", StopBits.OnePointFive => "1.5", StopBits.Two => "2", _ => "" };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
}
