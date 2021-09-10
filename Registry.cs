using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
            if (!_initialised) throw new Exception("CTecUtil: the Registry class has not been initialised.");
            writeSubKey(Keys.WindowKey, window.WindowState == WindowState.Maximized ? _maximised : window.Width + "," + window.Height + ":" + window.Left + "," + window.Top);
        }


        /// <summary>
        /// Retrieve the window state and set the window's size and position accordingly.
        /// </summary>
        public static WindowState RestoreWindowState(Window window)
        {
            if (!_initialised) throw new Exception("CTecUtil: the Registry class has not been initialised.");

            var prevState = (string)CTecUtil.Registry.readSubKey(CTecUtil.Registry.Keys.WindowKey);
            if (prevState is not null)
            {
                if (prevState == _maximised)
                {
                    return WindowState.Maximized;
                }
                else
                {
                    var size_pos = prevState.Split(new char[] { ':' });

                    if (size_pos.Length > 0)
                    {
                        var w_h = size_pos[0].Split(new char[] { ',' });
                        if (w_h.Length > 1)
                        {
                            if (int.TryParse(w_h[0], out var w) && int.TryParse(w_h[1], out var h))
                            {
                                window.Width = w;
                                window.Height = h;
                            }
                        }
                    }

                    if (size_pos.Length > 1)
                    {
                        var left_top = size_pos[1].Split(new char[] { ',' });
                        if (left_top.Length > 1)
                        {
                            if (int.TryParse(left_top[0], out var l) && int.TryParse(left_top[1], out var t))
                            {
                                window.Left = l;
                                window.Top = t;
                            }
                        }
                    }
                }
            }

            return WindowState.Normal;
        }


        public static void SaveZoomLevel(float zoomLevel) => writeSubKey(Keys.ZoomKey, zoomLevel);
        public static float? ReadZoomLevel() => float.TryParse((string)readSubKey(Keys.ZoomKey), out float zoomLevel) ? zoomLevel : null;


        public static void SaveCulture(string cultureName) => writeSubKey(Keys.CultureKey, cultureName);
        public static string ReadCulture() => (string)readSubKey(Keys.CultureKey);


        internal class Keys
        {
            public const string ZoomKey    = @"ZoomLevel";
            public const string CultureKey = @"Culture";
            public const string WindowKey  = @"Window";
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
    }
}
