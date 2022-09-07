using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using CTecUtil.IO;

namespace CTecUtil
{
    public class ApplicationConfigData
    {
        public ApplicationConfigData()
        {
            RecentPanelFiles.RecentFileListHasChanged        = new(() => ApplicationConfig.RecentPanelFileListHasChanged?.Invoke());
            RecentConfiguratorFiles.RecentFileListHasChanged = new(() => ApplicationConfig.RecentConfiguratorFileListHasChanged?.Invoke());
        }

        public Layouts            Layout;
        public string             CultureName = "en-GB";
        public WindowSizeParams   MainWindow;
        public WindowSizeParams   MonitorWindow;
        public WindowSizeParams   ValidationWindow;
        public float              ZoomLevel = 0.75f;
        public float              SerialMonitorZoomLevel = 0.75f;
        public SerialPortSettings SerialPort = new();
        public string             Protocol = "CAST";
        public RecentFilesList    RecentPanelFiles = new();
        public RecentFilesList    RecentConfiguratorFiles = new();
    }


    public enum Layouts { Standard, Classic };


    public class WindowSizeParams
    {
        public Point? Location;
        public Size?  Size;
        public bool   IsMaximised;
    }
}
