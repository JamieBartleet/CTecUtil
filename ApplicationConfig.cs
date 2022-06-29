using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CTecUtil.IO;
using Newtonsoft.Json;

namespace CTecUtil
{
    public class ApplicationConfig
    {
        /// <summary>
        /// Initialise the CTecUtil.ApplicationData class.
        /// </summary>
        /// <param name="productName">The software's name ("QuantecTools", XFPTools", etc.)</param>
        public ApplicationConfig(string productName)
        {
            Directory.CreateDirectory(_appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _companyName));
            _configFilePath = Path.Combine(_appDataFolder, productName + TextFile.JsonFileExt);
            readSettings();
        }


        private const string _companyName = "C-Tec";
        private string _appDataFolder;
        private string _configFilePath;
        private ApplicationConfigData _config = new();


        private void readSettings()
        {
            try
            {
                using FileStream stream = new(_configFilePath, FileMode.Open, FileAccess.Read);
                if (stream is not null)
                {
                    using StreamReader reader = new(stream);
                    if (reader is not null)
                    {
                        _config = JsonConvert.DeserializeObject<ApplicationConfigData>(reader.ReadToEnd());
                        reader.Close();
                    }
                }
            }
            catch (FileNotFoundException ex) { }
            catch (DirectoryNotFoundException ex) { Debug.WriteLine(ex.Message); }
            catch (UnauthorizedAccessException ex) { Debug.WriteLine(ex.Message); }
            catch (IOException ex) { Debug.WriteLine(ex.Message); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }


        public void SaveSettings() => TextFile.SaveFile(JsonConvert.SerializeObject(_config, Formatting.Indented), _configFilePath);



        /// <summary>
        /// Main application window's size and position.
        /// </summary>
        public WindowSizeParams MainWindow { get => _config.MainWindow; }

        /// <summary>
        /// Save the window's size and position.
        /// </summary>
        public void SetMainWindowState(Window window)
        {
            var newLocation = new Point(window.Left, window.Top);
            var newSize = new Size(window.Width, window.Height);

            if (_config.MainWindow is null)
                _config.MainWindow = new() { Location = newLocation, Size = newSize, IsMaximised = window.WindowState == WindowState.Maximized };
            else
            {
                _config.MainWindow.Location = newLocation;
                _config.MainWindow.Size = newSize;
            }
        }


        ///// <summary>
        ///// Save the main application window's size and position.
        ///// </summary>
        //public void SaveMainWindowState(Window window) => saveWindowState(window, RegistryKeyNames.MainWindowKey);

        ///// <summary>
        ///// Retrieve the main application saved window state and set the window's size and position accordingly.
        ///// </summary>
        //public WindowState RestoreMainWindowState(Window window) => restoreWindowState(window, RegistryKeyNames.MainWindowKey);


        ///// <summary>
        ///// Save the serial monitor window's size and position.
        ///// </summary>
        //public void SaveSerialMonitorWindowState(Window window) => saveWindowState(window, RegistryKeyNames.MonitorKey);

        ///// <summary>
        ///// Retrieve the serial monitor's saved window state and set the window's size and position accordingly.
        ///// </summary>
        //public WindowState RestoreSerialMonitorWindowState(Window window) => restoreWindowState(window, RegistryKeyNames.MonitorKey);




        /// <summary>
        /// Retrieve the window state and set the window's size and position accordingly.
        /// </summary>
        public WindowState restoreWindowState(Window window, string windowKey)
        {
            var prevState = (string)readSubKey(windowKey);
            if (prevState is not null)
            {
                var size_pos = prevState.Split(new char[] { ';' });

                if (size_pos.Length > 0)
                {
                    try
                    {
                        var w_h = parsePoint(size_pos[0]);
                        window.Width = w_h.X;
                        window.Height = w_h.Y;
                    }
                    catch { }
                }

                if (size_pos.Length > 1)
                {
                    try
                    {
                        System.Drawing.Point topLeft;

                        var x_y = parsePoint(size_pos[1]);
                        topLeft = new((int)x_y.X, (int)x_y.Y);

                        //ensure top-left of app screen is visible
                        var loc = UI.WindowUtils.AdjustXY(new(topLeft.X, topLeft.Y), new((int)window.Width, (int)window.Height), 0, 0);

                        window.Top = loc.Y;
                        window.Left = loc.X;
                    }
                    catch { }
                }

                if (size_pos.Length > 2)
                    if (size_pos[2] == _maximised)
                        return WindowState.Maximized;
            }

            return WindowState.Normal;
        }


        public float ZoomLevel
        {
            get => _config.ZoomLevel;
            set => _config.ZoomLevel = value;
        }


        public void SaveMode(bool isClassicMode)
        {
            _config.Mode = isClassicMode ? Modes.Classic : Modes.Standard;
            SaveSettings();
        }
        public bool IsClassicMode
        {
            get => _config.Mode == Modes.Classic;
            set
            {
                _config.Mode = value ? Modes.Classic : Modes.Standard;
                SaveSettings();
            }
        }



        public string Culture
        {
            get => _config.CultureName;
            set
            {
                _config.CultureName = value;
                SaveSettings();
            }
        }


        private const string _portSetting = "port";
        //private const string _baudSetting = "baud";
        //private const string _handshakeSetting = "handshake";
        //private const string _paritySetting = "parity";
        //private const string _dataBitsSetting = "databits";
        //private const string _stopBitsSetting = "stopbits";
        //private const string _readTimeoutSetting = "readtimeout";
        //private const string _writeTimeoutSetting = "writetimeout";

        public void SaveSerialPortSettings(SerialPortSettings settings)
            => writeSubKey(RegistryKeyNames.SerialPortKey, _portSetting + "=" + settings.PortName
                                                        // + ","+ _baudSetting + "=" + settings.BaudRate
                                                        // + ","+ _handshakeSetting + "=" + settings.Handshake
                                                        // + ","+ _paritySetting + "=" + settings.Parity
                                                        // + ","+ _dataBitsSetting + "=" + settings.DataBits
                                                        // + ","+ _stopBitsSetting + "=" + settings.StopBits
                                                        // + ","+ _readTimeoutSetting + "=" + settings.ReadTimeout
                                                        // + ","+ _writeTimeoutSetting + "=" + settings.WriteTimeout
                                                        );

        public SerialPortSettings ReadSerialPortSettings()
        {
            SerialPortSettings result = new();

            var keyData = (string)readSubKey(RegistryKeyNames.SerialPortKey);
            if (!string.IsNullOrEmpty(keyData))
            {
                var settings = keyData.Split(new char[] { ',' });

                foreach (var s in settings)
                {
                    var param = s.Split(new char[] { '=' });
                    if (param.Length > 1)
                    {
                        switch (param?[0].ToLower())
                        {
                            case _portSetting:
                                result.PortName     = param[1];
                                break;
                                //case _baudSetting:         result.BaudRate     = parseInt(param[1], 9600); break;
                                //case _handshakeSetting:    result.Handshake    = parseHandshake(param[1]); break;
                                //case _paritySetting:       result.Parity       = parseParity(param[1]); break;
                                //case _dataBitsSetting:     result.DataBits     = parseInt(param[1], 8); break;
                                //case _stopBitsSetting:     result.StopBits     = parseStopBits(param[1]); break;
                                //case _readTimeoutSetting:  result.ReadTimeout  = parseInt(param[1], 500); break;
                                //case _writeTimeoutSetting: result.WriteTimeout = parseInt(param[1], 500); break;
                        }
                    }
                }
            }

            return result;
        }

        private int parseInt(string value, int defaultValue) { return (int.TryParse(value, out var b)) ? b : defaultValue; }
        private Handshake parseHandshake(string value) { return (Handshake.TryParse(value, out Handshake h)) ? h : Handshake.None; }
        private Parity parseParity(string value) { return (Parity.TryParse(value, out Parity h)) ? h : Parity.None; }
        private StopBits parseStopBits(string value) { return (StopBits.TryParse(value, out StopBits s)) ? s : StopBits.None; }


        private static string _productName;
        private const  string _rootKey   = @"SOFTWARE\CTec\";
        private const  string _maximised = "max";


        /// <summary>
        /// Write registry entry to HKEY_CURRENT_USER.
        /// </summary>
        /// <param name="subkeyName"></param>
        /// <param name="value"></param>
        private void writeSubKey(string subkeyName, object value)
        {
            //try
            //{
            //    //RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(_rootKey + _productName);
            //    key.SetValue(subkeyName, value);
            //    key.Close();
            //}
            //catch { }
        }


        /// <summary>
        /// Read registry entry from HKEY_CURRENT_USER.
        /// </summary>
        /// <param name="subkeyName"></param>
        private object readSubKey(string subkeyName, object defaultValue = null)
        {
            //try
            //{
            //    RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(_rootKey + _productName);
            //    var result = key.GetValue(subkeyName, defaultValue);
            //    key.Close();
            //    return result;
            //}
            //catch
            //{
            //    return null;
            //}
            return null;
        }


        private Point parsePoint(string value)
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
