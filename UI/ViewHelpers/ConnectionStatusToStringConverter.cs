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
    [ValueConversion(typeof(string), typeof(SerialComms.ConnectionStatus))]
    public sealed class ConnectionStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (SerialComms.ConnectionStatus)value switch
                                                                                                        {
                                                                                                            SerialComms.ConnectionStatus.ConnectedWriteable    => Cultures.Resources.Comms_Connected_Writeable,
                                                                                                            SerialComms.ConnectionStatus.ConnectedReadOnly    => Cultures.Resources.Comms_Connected_ReadOnly,
                                                                                                            SerialComms.ConnectionStatus.Listening    => Cultures.Resources.Comms_Listening,
                                                                                                            SerialComms.ConnectionStatus.Disconnected => Cultures.Resources.Comms_Disconnected,
                                                                                                            _                                         => "",
                                                                                                        };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
}
