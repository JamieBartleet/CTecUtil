using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CTecUtil.IO;

namespace CTecUtil
{
    public class ApplicationConfigData
    {
        public Layouts            Layout;
        public string             CultureName = "en-GB";
        public WindowSizeParams   MainWindow;
        public WindowSizeParams   MonitorWindow;
        public float              ZoomLevel = 0.75f;
        public SerialPortSettings SerialPort = new();
    }


    public enum Layouts { Standard, Classic };


    public class WindowSizeParams
    {
        public Point? Location;
        public Size?  Size;
        public bool   IsMaximised;
    }
}
