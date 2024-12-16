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
        public ApplicationConfigData() { }


        public static SupportedApps OwnerApp { get; set; } = SupportedApps.NotSet;


        public string             CultureName      { get; set; } = "en-GB";
        public WindowSizeParams   MainWindow       { get; set; } = new();
        public WindowSizeParams   ValidationWindow { get; set; } = new();
        public double             ZoomLevel        { get; set; } = 0.75;
        public SerialPortSettings SerialPort       { get; set; } = new();
        public string             Protocol         { get; set; } = "";
        public RecentFilesList    RecentPanelFiles { get; set; } = new();
    }
}
