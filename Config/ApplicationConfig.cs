using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using CTecUtil.IO;
using Newtonsoft.Json;

namespace CTecUtil.Config
{
    public abstract class ApplicationConfig
    {
        protected static string productName => ApplicationConfigData.OwnerApp switch { SupportedApps.Quantec => "QuantecTools", SupportedApps.XFP => "XfpTools", _ => "ZfpTools" };


        /// <summary>
        /// Initialise the CTecUtil.ApplicationConfigBase class.
        /// </summary>
        public void InitConfigSettings(SupportedApps ownerApp)
        {
            ApplicationConfigData.OwnerApp = ownerApp;
            _initialised = true;
            Directory.CreateDirectory(AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _companyName));
            _configFilePath = Path.Combine(AppDataFolder, productName + TextFile.JsonFileExt);
            checkFileExists();
            readSettings();
            InitTimer();
        }


        private static ApplicationConfigData _data;
        protected ApplicationConfigData Data { get => _data; set => _data = value; }


        protected const  string _companyName = "C-Tec";
        protected static bool   _initialised = false;
        protected static string _configFilePath;

        public static string AppDataFolder { get; set; }


        /// <summary>Delegate to send notification when a recent files list has changed</summary>
        public delegate void RecentFileListChangeNotifier();


        /// <summary>Sends notification when the recent files list has changed</summary>
        public static RecentFileListChangeNotifier RecentFileListHasChanged;

        protected void checkFileExists()
        {
            if (!new FileInfo(_configFilePath).Exists)
            {                         
                Data = newConfigDataData();
                saveSettings();
            }
        }

        protected abstract ApplicationConfigData newConfigDataData();
        protected abstract void readSettings();


        protected static void notInitialisedError()
        {
            MessageBox.Show("***Code error***\n\nThe CTecUtil.ApplicationConfig class has not been initialised.\nCall ApplicationConfig.InitConfigSettings(<ownerApp>).", _companyName + "App Error");
            Application.Current.Shutdown();
        }


        /// <summary>Main application window's size and position.</summary>
        public WindowSizeParams MainWindow { get => Data.MainWindow; }


        /// <summary>Validation window's size and position.</summary>
        public WindowSizeParams ValidationWindow { get => Data.ValidationWindow; }


        /// <summary>Save the main application window's size and position.</summary>
        public void UpdateMainWindowParams(Window window, bool saveSettings = false) => UpdateMainWindowParams(window, Data.MainWindow.Scale, saveSettings);
        public void UpdateMainWindowParams(Window window, double scale, bool saveSettings = false)
        {
            Data.MainWindow = new(window, scale);
            Save = true;
        }


        /// <summary>Save the Validation window's size and position.</summary>
        public void UpdateValidationWindowParams(Window window, double scale, bool saveSettings = false)
        {
            Data.ValidationWindow = new(window, scale);
            Save = true;
        }


        protected void updateWindowParams(Window window, double scale, WindowSizeParams @params, bool saveSettings)
        {
            @params.Location    = new Point((int)window.Left, (int)window.Top);
            @params.Size        = new Size((int)window.Width, (int)window.Height);
            @params.IsMaximised = window.WindowState == WindowState.Maximized;
            @params.Scale       = scale;

            Save = true; 
        }


        public void OffsetMainWindowPosition()
        {
            var x = MainWindow.Location.Value.X + 20;
            var y = MainWindow.Location.Value.Y + 30;
            var w = MainWindow.Size.Value.Width;
            var h = MainWindow.Size.Value.Height - 20;
            MainWindow.IsMaximised = false;
            MainWindow.Location = new(x, y);
            MainWindow.Size = new(w, h);
            Save = true;
        }


        public static double MinZoom = 0.45;
        public static double MaxZoom = 1.25;

        public double ZoomStep                       => (MaxZoom - MinZoom) / 16;
        public double ZoomLevel                      { get => Data.ZoomLevel;            set { Data.ZoomLevel = value; Save = true; } }
        public string Culture                        { get => Data.CultureName;          set { Data.CultureName = value; Save = true; } }
        public string Protocol                       { get => Data.Protocol;             set { Data.Protocol = value; Save = true; } }
        public SerialPortSettings SerialPortSettings { get => Data.SerialPort;           set { Data.SerialPort = value; Save = true; } }
        private static string _serialPortName;
        public static string SerialPortName          { get => _data.SerialPort.PortName; set { _data.SerialPort.PortName = value; Save = true; } }
        public RecentFilesList    RecentPanelFiles   { get => Data.RecentPanelFiles;     set { Data.RecentPanelFiles = value; Save = true; } }


        #region save with timer
        internal static void InitTimer()
        {
            _updateTimer = new Timer() { Interval = 7500 };
            _updateTimer.Elapsed += updateTimerTick;
            _updateTimer.Start();
        }

        private static Timer _updateTimer;

        public static bool Save { get; set; }

        private static void updateTimerTick(object sender, EventArgs e)
        {
            if (Save)
            {
                saveSettings();
                Save = false;
            }
        }

        protected static void saveSettings()
        {
            if (!_initialised)
                notInitialisedError();

            TextFile.SaveFile(JsonConvert.SerializeObject(_data, Formatting.Indented), _configFilePath);
        }
        #endregion
    }
}
