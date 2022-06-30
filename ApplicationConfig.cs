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
        /// <param name="productName">The software's name ("QuantecTools", XFPTools", etc.)</param>
        public static void InitConfigSettings(string productName)
        {
            Directory.CreateDirectory(_appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _companyName));
            _configFilePath = Path.Combine(_appDataFolder, productName + TextFile.JsonFileExt);
            readSettings();
        }


        private const string _companyName = "C-Tec";
        private static string _appDataFolder;
        private static string _configFilePath;
        private static ApplicationConfigData _config = new();


        private static void readSettings()
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


        public static void SaveSettings() => TextFile.SaveFile(JsonConvert.SerializeObject(_config, Formatting.Indented), _configFilePath);


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
        public static void SaveMainWindowState(Window window)
        {
            var newLocation = new Point(window.Left, window.Top);
            var newSize     = new Size(window.Width, window.Height);

            if (_config.MainWindow is null)
                _config.MainWindow = new() { Location = newLocation, Size = newSize, IsMaximised = window.WindowState == WindowState.Maximized };
            else
            {
                _config.MainWindow.Location = newLocation;
                _config.MainWindow.Size = newSize;
            }
        }


        /// <summary>
        /// Save the Configurator Monitor window's size and position.
        /// </summary>
        public static void SaveMonitorWindowState(Window window)
        {
            var newLocation = new Point(window.Left, window.Top);
            var newSize = new Size(window.Width, window.Height);

            if (_config.MonitorWindow is null)
                _config.MonitorWindow = new() { Location = newLocation, Size = newSize, IsMaximised = window.WindowState == WindowState.Maximized };
            else
            {
                _config.MonitorWindow.Location = newLocation;
                _config.MonitorWindow.Size = newSize;
            }
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


        public static bool IsClassicMode
        {
            get => _config.Mode == Modes.Classic;
            set  { _config.Mode = value ? Modes.Classic : Modes.Standard; SaveSettings(); }
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
