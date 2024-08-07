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
        public static SupportedApps OwnerApp { get; private set; } = SupportedApps.NotSet;


        /// <summary>
        /// Initialise the CTecUtil.ApplicationConfig class.
        /// </summary>
        /// <param name="productName">The software's name ("Quantec Programming Tools", etc.)</param>
        public static void InitConfigSettings(SupportedApps ownerApp)
        {
            OwnerApp = ownerApp;
            var productName = OwnerApp switch { SupportedApps.Quantec => "QuantecTools", SupportedApps.XFP => "XfpTools", _ => "ZfpTools" };
            _initialised = true;
            Directory.CreateDirectory(AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _companyName));
            _configFilePath = Path.Combine(AppDataFolder, productName + TextFile.JsonFileExt);
            readSettings();
        }


        private const string _companyName = "C-Tec";
        private static bool _initialised = false;
        private static string _configFilePath;
        private static ApplicationConfigData _config = new();
        
        public static string AppDataFolder { get; set; }


        /// <summary>Delegate to send notification when a recent files list has changed</summary>
        public delegate void RecentFileListChangeNotifier();
        /// <summary>Sends notification when the recent panel files list has changed</summary>
        [JsonIgnore]
        public static RecentFileListChangeNotifier RecentPanelFileListHasChanged;
        /// <summary>Sends notification when the recent configurator files list has changed</summary>
        [JsonIgnore]
        public static RecentFileListChangeNotifier RecentConfiguratorFileListHasChanged;


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
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException ex) { Debug.WriteLine(ex.Message); }
            catch (UnauthorizedAccessException ex) { Debug.WriteLine(ex.Message); }
            catch (IOException ex) { Debug.WriteLine(ex.Message); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }


        public static void SaveSettings()
        {
            if (!_initialised)
                notInitialisedError();

            TextFile.SaveFile(JsonConvert.SerializeObject(_config, Formatting.Indented), _configFilePath);
        }


        private static void notInitialisedError()
        {
            MessageBox.Show("***Code error***\n\nThe CTecUtil.ApplicationConfig class has not been initialised.\nCall ApplicationConfig.InitConfigSettings(<ownerApp>).", _companyName + "App Error");
            Application.Current.Shutdown();
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
        /// Validation window's size and position.
        /// </summary>
        public static WindowSizeParams ValidationWindow { get => _config.ValidationWindow; }


        /// <summary>
        /// Save the main application window's size and position.
        /// </summary>
        public static void UpdateMainWindowParams(Window window, bool saveSettings = false)
        {
            _config.MainWindow ??= new();

            updateWindowParams(window, _config.MainWindow, saveSettings);
        }


        /// <summary>
        /// Save the Configurator Monitor window's size and position.
        /// </summary>
        public static void UpdateMonitorWindowParams(Window window, bool saveSettings = false)
        {
            _config.MonitorWindow ??= new();

            updateWindowParams(window, _config.MonitorWindow, saveSettings);
        }


        /// <summary>
        /// Save the Validation window's size and position.
        /// </summary>
        public static void UpdateValidationWindowParams(Window window, bool saveSettings = false)
        {
            _config.ValidationWindow ??= new();

            updateWindowParams(window, _config.ValidationWindow, saveSettings);
        }


        private static void updateWindowParams(Window window, WindowSizeParams dimensions, bool saveSettings)
        {
            dimensions.Location = new Point((int)window.Left, (int)window.Top);
            dimensions.Size = new Size((int)window.Width, (int)window.Height);
            dimensions.IsMaximised = window.WindowState == WindowState.Maximized;

            if (saveSettings)
                SaveSettings();
        }


        public static float MinZoom = 0.45f;
        public static float MaxZoom = 1.25f;
        public static float ZoomStep { get => (MaxZoom - MinZoom) / 16; }

        public static float ZoomLevel
        {
            get => _config.ZoomLevel;
            set => _config.ZoomLevel = value;
        }

        public static float SerialMonitorZoomLevel
        {
            get => _config.SerialMonitorZoomLevel;
            set => _config.SerialMonitorZoomLevel = value;
        }


        public static string Culture
        {
            get => _config.CultureName;
            set { _config.CultureName = value; SaveSettings(); }
        }


        public static string Protocol
        {
            get => _config.Protocol;
            set { _config.Protocol = value; SaveSettings(); }
        }


        public static SerialPortSettings SerialPortSettings
        {
            get => _config.SerialPort;
            set { _config.SerialPort = value; SaveSettings(); }
        }


        public static RecentFilesList RecentPanelFiles
        {
            get => _config.RecentPanelFiles;
            set { _config.RecentPanelFiles = value; SaveSettings(); }
        }


        public static RecentFilesList RecentConfiguratorFiles
        {
            get => _config.RecentConfiguratorFiles;
            set { _config.RecentConfiguratorFiles = value; SaveSettings(); }
        }
    }
}
