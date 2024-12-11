using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using CTecUtil.IO;

namespace CTecUtil.Config
{
    public class ApplicationConfigData
    {
        public ApplicationConfigData() => RecentPanelFiles = new();


        public static SupportedApps OwnerApp { get; set; } = SupportedApps.NotSet;


        public string             CultureName = "en-GB";
        public WindowSizeParams   MainWindow;
        public WindowSizeParams   ValidationWindow;
        public double             ZoomLevel = 0.75;
        public SerialPortSettings SerialPort = new();
        public string             Protocol = "";
        public RecentFilesList    RecentPanelFiles;
    }
}
