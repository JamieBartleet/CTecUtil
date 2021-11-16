using CTecUtil.IO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CTecUtil
{
    public class Registry
    {
        /// <summary>
        /// Initialise the CTecUtil.Registry class.
        /// </summary>
        /// <param name="productName">The software's name ("QuantecTools", XFPTools", etc.)</param>
        public static void Initialise(string productName)
        {
            _productName = productName;
            _initialised = true;
        }


        /// <summary>
        /// Save the window's size and position.
        /// </summary>
        public static void SaveWindowState(Window window)
        {
            if (window.WindowState == WindowState.Maximized)
            {
                var prevState = (string)readSubKey(Keys.WindowKey);
                if (prevState != null && prevState.EndsWith(_maximised))
                    writeSubKey(Keys.WindowKey, prevState);
                else
                    writeSubKey(Keys.WindowKey, prevState + ";" + _maximised);
            }
            else
            {
                writeSubKey(Keys.WindowKey, window.Width + "," + window.Height + ";" + window.Left + "," + window.Top);
            }
        }


        /// <summary>
        /// Retrieve the window state and set the window's size and position accordingly.
        /// </summary>
        public static WindowState RestoreWindowState(Window window)
        {
            var prevState = (string)readSubKey(Keys.WindowKey);
            if (prevState is not null)
            {
                //if (prevState.StartsWith(_maximised))
                //    return WindowState.Maximized;
                
                var size_pos = prevState.Split(new char[] { ';' });

                if (size_pos.Length > 0)
                {
                    var w_h = parsePoint(size_pos[0]);
                    window.Width = w_h.X;
                    window.Height = w_h.Y;
                }

                if (size_pos.Length > 1)
                {
                    var x_y = parsePoint(size_pos[1]);
                    window.Left = x_y.X;
                    window.Top = x_y.Y;
                }

                if (size_pos.Length > 2)
                    if (size_pos[2] == _maximised)
                        return WindowState.Maximized;
            }

            return WindowState.Normal;
        }


        public static void SaveZoomLevel(float zoomLevel) => writeSubKey(Keys.ZoomKey, zoomLevel.ToString("F2", CultureInfo.InvariantCulture));
        public static float? ReadZoomLevel() => float.TryParse((string)readSubKey(Keys.ZoomKey), NumberStyles.Float, CultureInfo.InvariantCulture, out float zoomLevel) ? zoomLevel : null;


        public static void SaveCulture(string cultureName) => writeSubKey(Keys.CultureKey, cultureName);
        public static string ReadCulture() => (string)readSubKey(Keys.CultureKey);

        
        public static void SaveSerialPortSettings(SerialPortSettings settings)
            => writeSubKey(Keys.SerialPortKey, "n=" + settings.PortName
                                            + ",b=" + settings.BaudRate
                                            + ",h=" + settings.Handshake
                                            + ",p=" + settings.Parity
                                            + ",d=" + settings.DataBits
                                            + ",s=" + settings.StopBits
                                            + ",r=" + settings.ReadTimeout
                                            + ",w=" + settings.WriteTimeout);

        public static SerialPortSettings ReadSerialPortSettings()
        {
            SerialPortSettings result = new();

            var keyData = (string)readSubKey(Keys.SerialPortKey);
            if (!string.IsNullOrEmpty(keyData))
            {
                var settings = keyData.Split(new char[] { ',' });
                
                foreach (var s in settings)
                {
                    var param = s.Split(new char[] { '=' });
                    if (param.Length > 1)
                    {
                        switch (param?[0][0])
                        {
                            case 'n': result.PortName     = param[1]; break;
                            case 'b': result.BaudRate     = parseInt(param[1], 9600); break;
                            case 'h': result.Handshake    = parseHandshake(param[1]); break;
                            case 'p': result.Parity       = parseParity(param[1]); break;
                            case 'd': result.DataBits     = parseInt(param[1], 8); break;
                            case 's': result.StopBits     = parseStopBits(param[1]); break;
                            case 'r': result.ReadTimeout  = parseInt(param[1], 500); break;
                            case 'w': result.WriteTimeout = parseInt(param[1], 500); break;
                        }
                    }
                }
            }

            return result;
        }

        private static int       parseInt(string value, int defaultValue) { return (int.TryParse(value, out var b)) ? b : defaultValue; }
        private static Handshake parseHandshake(string value) { return (Handshake.TryParse(value, out Handshake h)) ? h : Handshake.None; }
        private static Parity    parseParity(string value)    { return (Parity.TryParse(value, out Parity h)) ? h : Parity.None; }
        private static StopBits  parseStopBits(string value)  { return (StopBits.TryParse(value, out StopBits s)) ? s : StopBits.None; }


        internal class Keys
        {
            public const string ZoomKey       = @"ZoomLevel";
            public const string CultureKey    = @"Culture";
            public const string WindowKey     = @"Window";
            public const string MessageBoxKey = @"MsgBox";
            public const string SerialPortKey = @"SerialPort";
        }


        private static bool   _initialised;
        private static string _productName;
        private const  string _rootKey   = @"SOFTWARE\CTec\";
        private const  string _maximised = "max";


        /// <summary>
        /// Write registry entry to HKEY_CURRENT_USER.
        /// </summary>
        /// <param name="subkeyName"></param>
        /// <param name="value"></param>
        private static void writeSubKey(string subkeyName, object value)
        {
            if (!_initialised) throw new Exception("CTecUtil: the Registry class has not been initialised.");

            try
            {
                RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(_rootKey + _productName);
                key.SetValue(subkeyName, value);  
                key.Close(); 
            }
            catch { }
        }


        /// <summary>
        /// Read registry entry from HKEY_CURRENT_USER.
        /// </summary>
        /// <param name="subkeyName"></param>
        private static object readSubKey(string subkeyName)
        {
            if (!_initialised) throw new Exception("CTecUtil: the Registry class has not been initialised.");

            try
            {
                RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(_rootKey + _productName);
                var result = key.GetValue(subkeyName);
                key.Close();
                return result;
            }
            catch
            {
                return null;
            }
        }


        private static Point parsePoint(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var pair = value.Split(new char[] { ',' });
                if (pair.Length > 1)
                    if (double.TryParse(pair[0], out var d1) && double.TryParse(pair[1], out var d2))
                        return new Point { X = d1, Y = d2 };
            }
            return new Point();
        }
    }
}
