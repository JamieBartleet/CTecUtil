using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public abstract class ApplicationConfigBase
    {
        public static SupportedApps OwnerApp { get; set; } = SupportedApps.NotSet;
        protected static string productName => OwnerApp switch { SupportedApps.Quantec => "QuantecTools", SupportedApps.XFP => "XfpTools", _ => "ZfpTools" };


        /// <summary>
        /// Initialise the CTecUtil.ApplicationConfigBase class.
        /// </summary>
        /// <param name="productName">The software's name ("Quantec Programming Tools", etc.)</param>
        public void InitConfigSettings(SupportedApps ownerApp)
        {
            OwnerApp = ownerApp;
            _initialised = true;
            var productName = OwnerApp switch { SupportedApps.Quantec => "QuantecTools", SupportedApps.XFP => "XfpTools", _ => "ZfpTools" };
            Directory.CreateDirectory(AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _companyName));
            _configFilePath = Path.Combine(AppDataFolder, productName + TextFile.JsonFileExt);
            readSettings();
        }


        public abstract ApplicationConfigData Data { get; set; }


        protected const string _companyName = "C-Tec";
        protected bool _initialised = false;
        protected string _configFilePath;

        public static string AppDataFolder { get; set; }


        /// <summary>Delegate to send notification when a recent files list has changed</summary>
        public delegate void RecentFileListChangeNotifier();
        public delegate void SettingsSaver();

        /// <summary>Sends notification when the recent panel files list has changed</summary>
        [JsonIgnore]
        public static RecentFileListChangeNotifier RecentPanelFileListHasChanged;

        protected abstract void readSettings();


        public void SaveSettings()
        {
            if (!_initialised)
                notInitialisedError();

            TextFile.SaveFile(JsonConvert.SerializeObject(Data, Formatting.Indented), _configFilePath);
        }


        protected void notInitialisedError()
        {
            MessageBox.Show("***Code error***\n\nThe CTecUtil.ApplicationConfig class has not been initialised.\nCall ApplicationConfig.InitConfigSettings(<ownerApp>).", _companyName + "App Error");
            Application.Current.Shutdown();
        }


        /// <summary>
        /// Main application window's size and position.
        /// </summary>
        public WindowSizeParams MainWindow { get => Data.MainWindow; }


        /// <summary>
        /// Validation window's size and position.
        /// </summary>
        public WindowSizeParams ValidationWindow { get => Data.ValidationWindow; }


        /// <summary>
        /// Save the main application window's size and position.
        /// </summary>
        public void UpdateMainWindowParams(Window window, double zoomLevel, bool saveSettings = false)
        {
            Data.MainWindow ??= new();
            updateWindowParams(window, zoomLevel, Data.MainWindow, saveSettings);
        }


        /// <summary>
        /// Save the Validation window's size and position.
        /// </summary>
        public void UpdateValidationWindowParams(Window window, double scale, bool saveSettings = false)
        {
            Data.ValidationWindow ??= new();
            updateWindowParams(window, scale, Data.ValidationWindow, saveSettings);
        }


        protected void updateWindowParams(Window window, double scale, WindowSizeParams dimensions, bool saveSettings)
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

        public double ZoomStep                       => (MaxZoom - MinZoom) / 16;
        public double ZoomLevel                      { get => Data.ZoomLevel;        set => Data.ZoomLevel = value; }
        public string Culture                        { get => Data.CultureName;      set { Data.CultureName = value; SaveSettings(); } }
        public string Protocol                       { get => Data.Protocol;         set { Data.Protocol = value; SaveSettings(); } }
        public SerialPortSettings SerialPortSettings { get => Data.SerialPort;       set { Data.SerialPort = value; SaveSettings(); } }
        public RecentFilesList    RecentPanelFiles   { get => Data.RecentPanelFiles; set { Data.RecentPanelFiles = value; SaveSettings(); } }
    }
}
