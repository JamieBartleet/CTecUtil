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
    [ValueConversion(typeof(string), typeof(Handshake))]
    public sealed class HandshakeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((Handshake)value).ToString();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
}
