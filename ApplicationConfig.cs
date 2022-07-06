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
        /// Initialise the CTecUtil.ApplicationConfig class.
        /// </summary>
        /// <param name="productName">The software's name ("Quantec Programming Tools", etc.)</param>
        public static void InitConfigSettings(string productName)
        {
            _initialised = true;
            Directory.CreateDirectory(_appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _companyName));
            _configFilePath = Path.Combine(_appDataFolder, productName + TextFile.JsonFileExt);
            readSettings();
        }


        private const  string _companyName = "C-Tec";
        private static bool   _initialised = false;
        private static string _appDataFolder;
        private static string _configFilePath;
        private static ApplicationConfigData _config = new();


        private static void readSettings()
        {
            if (!_initialised)
                notInitialisedError();

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

            if (string.IsNullOrEmpty(_config.SerialPort.PortName))
            {
                //try legacy registry settings (only ever used in pre-release test versions)
                Registry.Initialise("QuantecTools");
                _config.SerialPort = Registry.ReadSerialPortSettings();
            }
        }


        public static void SaveSettings()
        {
            if (!_initialised)
                notInitialisedError();

            TextFile.SaveFile(JsonConvert.SerializeObject(_config, Formatting.Indented), _configFilePath);
        }


        private static void notInitialisedError()
        {
            MessageBox.Show("***Code error***\n\nThe ApplicationConfig class has not been initialised.\nCall ApplicationConfig.InitConfigSettings(<productName>).", _companyName);
            Environment.Exit(0);
        }


        /// <summary>
        /// Main application window's size and position.
        /// </summary>
        public static WindowSizeParams MainWindow { get => _config.MainWindow; }


        /// <summary>
        /// Configurator Monitor window's size and position.
        /// </summary>
        public static WindowSizeParams MonitorWindow { get => _config.MonitorWindow; }


        /// <summary>
        /// Save the main application window's size and position.
        /// </summary>
        public static void UpdateMainWindowParams(Window window, bool saveSettings = false)
        {
            if (_config.MainWindow is null)
                _config.MainWindow = new();

            updateWindowParams(window, _config.MonitorWindow, saveSettings);
        }


        /// <summary>
        /// Save the Configurator Monitor window's size and position.
        /// </summary>
        public static void UpdateMonitorWindowParams(Window window, bool saveSettings = false)
        {
            if (_config.MonitorWindow is null)
                _config.MonitorWindow = new();

            updateWindowParams(window, _config.MonitorWindow, saveSettings);
        }


        private static void updateWindowParams(Window window, WindowSizeParams dimensions, bool saveSettings)
        {
            var newLocation = new Point((int)window.Left, (int)window.Top);
            var newSize     = new Size((int)window.Width, (int)window.Height);

            dimensions.Location    = newLocation;
            dimensions.Size        = newSize;
            dimensions.IsMaximised = window.WindowState == WindowState.Maximized;

            if (saveSettings)
                SaveSettings();
        }


        /// <summary>
        /// Retrieve the main application window's state and set the window's size and position accordingly.
        /// </summary>
        public static WindowState RestoreMainWindowState(Window window) => UI.WindowUtils.SetWindowDimensions(window, _config.MainWindow);


        /// <summary>
        /// Retrieve the Configurator Monitor window's state and set the window's size and position accordingly.
        /// </summary>
        public static WindowState RestoreMonitorWindowState(Window window) => UI.WindowUtils.SetWindowDimensions(window, _config.MonitorWindow);



        public static float ZoomLevel
        {
            get => _config.ZoomLevel;
            set => _config.ZoomLevel = value;
        }


        public static bool IsClassicLayout
        {
            get => _config.Layout == Layouts.Classic;
            set  { _config.Layout = value ? Layouts.Classic : Layouts.Standard; SaveSettings(); }
        }


        public static string Culture
        {
            get => _config.CultureName;
            set  { _config.CultureName = value; SaveSettings(); }
        }


        public static SerialPortSettings SerialPortSettings
        {
            get => _config.SerialPort;
            set  { _config.SerialPort = value; SaveSettings(); }
        }
    }
}
