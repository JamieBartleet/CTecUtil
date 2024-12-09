using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CTecUtil.IO;
using Newtonsoft.Json;

namespace CTecUtil.Config
{
    public class ApplicationConfig
    {
        public static SupportedApps OwnerApp { get; set; } = SupportedApps.NotSet;
        protected static string productName => OwnerApp switch { SupportedApps.Quantec => "QuantecTools", SupportedApps.XFP => "XfpTools", _ => "ZfpTools" };


        /// <summary>
        /// Initialise the CTecUtil.ApplicationConfig class.
        /// </summary>
        /// <param name="productName">The software's name ("Quantec Programming Tools", etc.)</param>
        public static void InitConfigSettings(SupportedApps ownerApp, ApplicationConfigData config)
        {
            OwnerApp = ownerApp;
            _config = config;
            _initialised = true;
            var productName = OwnerApp switch { SupportedApps.Quantec => "QuantecTools", SupportedApps.XFP => "XfpTools", _ => "ZfpTools" };
            Directory.CreateDirectory(AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _companyName));
            _configFilePath = Path.Combine(AppDataFolder, productName + TextFile.JsonFileExt);
            readSettings();
        }


        protected const string _companyName = "C-Tec";
        protected static bool _initialised = false;
        protected static string _configFilePath;
        protected static ApplicationConfigData _config;

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


        protected static void notInitialisedError()
        {
            MessageBox.Show("***Code error***\n\nThe CTecUtil.ApplicationConfig class has not been initialised.\nCall ApplicationConfig.InitConfigSettings(<ownerApp>).", _companyName + "App Error");
            Application.Current.Shutdown();
        }


        /// <summary>
        /// Main application window's size and position.
        /// </summary>
        public static WindowSizeParams MainWindow { get => _config.MainWindow; }


        /// <summary>
        /// Save the main application window's size and position.
        /// </summary>
        public static void UpdateMainWindowParams(Window window, double zoomLevel, bool saveSettings = false)
        {
            _config.MainWindow ??= new();

            updateWindowParams(window, zoomLevel, _config.MainWindow, saveSettings);
        }


        protected static void updateWindowParams(Window window, double scale, WindowSizeParams dimensions, bool saveSettings)
        {
            dimensions.Location = new Point((int)window.Left, (int)window.Top);
            dimensions.Size = new Size((int)window.Width, (int)window.Height);
            dimensions.IsMaximised = window.WindowState == WindowState.Maximized;
            dimensions.Scale = scale;

            if (saveSettings)
                SaveSettings();
        }


        public static double MinZoom = 0.45;
        public static double MaxZoom = 1.25;
        public static double ZoomStep { get => (MaxZoom - MinZoom) / 16; }

        public static double ZoomLevel
        {
            get => _config.ZoomLevel;
            set => _config.ZoomLevel = value;
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


        //public static RecentFilesList RecentConfiguratorFiles
        //{
        //    get => _config.RecentConfiguratorFiles;
        //    set { _config.RecentConfiguratorFiles = value; SaveSettings(); }
        //}
    }
}
