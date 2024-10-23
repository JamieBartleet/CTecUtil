using CTecUtil.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CTecUtil.UI.ViewHelpers
{
    [ValueConversion(typeof(Brush), typeof(SerialComms.ConnectionStatus))]
    public sealed class ConnectionStatusToColourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => SerialComms.IsDisconnected ? new SolidColorBrush(Colors.Orange)
                                                                                                                                  : (SerialComms.ConnectionStatus)value switch
                                                                                                                                    {
                                                                                                                                        SerialComms.ConnectionStatus.ConnectedWriteable or
                                                                                                                                        SerialComms.ConnectionStatus.ConnectedReadOnly or
                                                                                                                                        SerialComms.ConnectionStatus.Listening            => new SolidColorBrush(Colors.LimeGreen),
                                                                                                                                        SerialComms.ConnectionStatus.FirmwareNotSupported => new SolidColorBrush(Colors.Black),
                                                                                                                                        _                                                 => new SolidColorBrush(Colors.OrangeRed),
                                                                                                                                    };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
}
